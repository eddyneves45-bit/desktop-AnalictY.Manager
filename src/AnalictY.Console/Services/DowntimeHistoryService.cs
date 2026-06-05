using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using AnalictY.Console.Models;

namespace AnalictY.Console.Services;

public sealed class DowntimeHistoryService
{
    private static readonly Uri MachinesEndpoint = new("http://127.0.0.1:5000/api/machines");
    private static readonly Uri DowntimesEndpoint = new("http://127.0.0.1:5000/api/downtimes");
    private readonly HttpClient _httpClient;

    public DowntimeHistoryService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<DowntimeHistoryResult> LoadAsync(
        string? machineId,
        string periodKey,
        CancellationToken cancellationToken = default)
    {
        var machines = await LoadMachinesAsync(cancellationToken);
        string selectedMachineId = ResolveMachineId(machineId, machines);
        var selectedMachine = machines.FirstOrDefault(machine => machine.Id == selectedMachineId) ?? machines.First();
        var period = BuildPeriod(periodKey);

        try
        {
            var uriBuilder = new UriBuilder(DowntimesEndpoint);
            var query = new List<string>
            {
                $"from={Uri.EscapeDataString(period.From.ToUniversalTime().ToString("O", CultureInfo.InvariantCulture))}",
                $"to={Uri.EscapeDataString(period.To.ToUniversalTime().ToString("O", CultureInfo.InvariantCulture))}",
                "limit=200"
            };

            if (!string.IsNullOrWhiteSpace(selectedMachineId))
            {
                query.Add($"machine_id={Uri.EscapeDataString(selectedMachineId)}");
            }

            uriBuilder.Query = string.Join("&", query);
            using HttpResponseMessage response = await _httpClient.GetAsync(uriBuilder.Uri, cancellationToken);
            if (response.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
            {
                return CreateFallbackResult(machines, selectedMachineId, period.Label, "Usando dados demonstrativos; histórico de paradas exige autenticação ou permissão.");
            }

            if (!response.IsSuccessStatusCode)
            {
                return CreateFallbackResult(machines, selectedMachineId, period.Label, "Usando dados demonstrativos; servidor não retornou histórico de paradas.");
            }

            using JsonDocument document = await ReadJsonAsync(response, cancellationToken);
            var rows = ExtractArray(document.RootElement, "downtimes", "data", "items", "results")
                .Select(MapRow)
                .ToList();

            if (rows.Count == 0)
            {
                return new DowntimeHistoryResult(
                    machines,
                    Array.Empty<DowntimeHistoryRow>(),
                    selectedMachineId,
                    selectedMachine.DisplayName,
                    period.Label,
                    0,
                    "0s",
                    "Sem paradas no período",
                    "Nenhuma parada encontrada para os filtros atuais.",
                    false);
            }

            var durations = ExtractArray(document.RootElement, "downtimes", "data", "items", "results")
                .Select(ReadDurationSeconds)
                .ToList();

            return new DowntimeHistoryResult(
                machines,
                rows,
                selectedMachineId,
                selectedMachine.DisplayName,
                period.Label,
                rows.Count,
                FormatDuration(durations.Sum()),
                ResolveMainReason(rows),
                "Histórico de paradas carregado do AnalictY Server.",
                false);
        }
        catch (Exception ex) when (ex is HttpRequestException or OperationCanceledException or JsonException)
        {
            return CreateFallbackResult(machines, selectedMachineId, period.Label, "Usando dados demonstrativos; não foi possível carregar o histórico de paradas.");
        }
    }

    private async Task<IReadOnlyList<DowntimeMachineOption>> LoadMachinesAsync(CancellationToken cancellationToken)
    {
        try
        {
            using HttpResponseMessage response = await _httpClient.GetAsync(MachinesEndpoint, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return CreateFallbackMachines();
            }

            using JsonDocument document = await ReadJsonAsync(response, cancellationToken);
            var machines = ExtractArray(document.RootElement, "machines", "data", "items", "results")
                .Select(MapMachine)
                .Where(machine => !string.IsNullOrWhiteSpace(machine.Id) && !string.IsNullOrWhiteSpace(machine.Name))
                .ToList();

            return machines.Count > 0 ? machines : CreateFallbackMachines();
        }
        catch
        {
            return CreateFallbackMachines();
        }
    }

    private static DowntimeHistoryRow MapRow(JsonElement item)
    {
        DateTime? start = ReadDateTime(item, "start_time", "startTime", "inicio_em", "trigger");
        DateTime? end = ReadDateTime(item, "end_time", "endTime", "fim_em", "recovery");
        string reason = ReadString(item, "reason", "motivo", "informed_reason", "motivo_informado", "status_origin_description") ?? "Não classificada";
        string category = ReadString(item, "category", "categoria", "description", "descricao") ?? "Parada";
        double duration = ReadDurationSeconds(item);

        return new DowntimeHistoryRow(
            FormatDateTime(start),
            end is null ? "Em andamento" : FormatDateTime(end),
            reason,
            category,
            FormatDuration(duration));
    }

    private static double ReadDurationSeconds(JsonElement item)
    {
        return ReadDouble(item, "duration_seconds", "durationSeconds", "duracao_segundos", "total_seconds");
    }

    private static string ResolveMainReason(IReadOnlyList<DowntimeHistoryRow> rows)
    {
        return rows
            .GroupBy(row => row.Reason)
            .OrderByDescending(group => group.Count())
            .FirstOrDefault()
            ?.Key ?? "Sem motivo";
    }

    private static DowntimeHistoryResult CreateFallbackResult(
        IReadOnlyList<DowntimeMachineOption> machines,
        string selectedMachineId,
        string periodLabel,
        string message)
    {
        var selectedMachine = machines.FirstOrDefault(machine => machine.Id == selectedMachineId) ?? machines.First();
        var rows = new List<DowntimeHistoryRow>
        {
            new("08:12", "08:24", "Aguardando material", "Ociosa", "12m"),
            new("10:03", "10:18", "Ajuste de operador", "Setup", "15m"),
            new("13:41", "14:05", "Manutenção preventiva", "Manutenção", "24m")
        };

        return new DowntimeHistoryResult(
            machines,
            rows,
            selectedMachine.Id,
            selectedMachine.DisplayName,
            periodLabel,
            rows.Count,
            "51m",
            "Aguardando material",
            message,
            true);
    }

    private static IReadOnlyList<DowntimeMachineOption> CreateFallbackMachines()
    {
        return new List<DowntimeMachineOption>
        {
            new("1", "PJ_08", "PJ_08"),
            new("2", "Morno_01", "Morno_01"),
            new("3", "Morno_02", "Morno_02")
        };
    }

    private static string ResolveMachineId(string? machineId, IReadOnlyList<DowntimeMachineOption> machines)
    {
        if (!string.IsNullOrWhiteSpace(machineId) && machines.Any(machine => machine.Id == machineId))
        {
            return machineId;
        }

        return machines.First().Id;
    }

    private static DowntimeMachineOption MapMachine(JsonElement machine)
    {
        string id = ReadString(machine, "id", "machine_id", "machineId") ?? string.Empty;
        string name = ReadString(machine, "name", "nome", "code", "codigo", "código") ?? id;
        string code = ReadString(machine, "code", "codigo", "código") ?? string.Empty;
        return new DowntimeMachineOption(id, name, code);
    }

    private static PeriodWindow BuildPeriod(string periodKey)
    {
        DateTime now = DateTime.Now;
        return periodKey switch
        {
            "Última hora" => new PeriodWindow(now.AddHours(-1), now, "Última hora"),
            "Mês atual" => new PeriodWindow(new DateTime(now.Year, now.Month, 1), now, "Mês atual"),
            _ => new PeriodWindow(DateTime.Today, DateTime.Today.AddDays(1).AddMinutes(-1), "Hoje")
        };
    }

    private static string FormatDateTime(DateTime? value)
    {
        return value?.ToString("dd/MM/yyyy HH:mm") ?? "-";
    }

    private static string FormatDuration(double seconds)
    {
        if (seconds <= 0)
        {
            return "0s";
        }

        int totalSeconds = (int)Math.Round(seconds);
        int hours = totalSeconds / 3600;
        int minutes = totalSeconds % 3600 / 60;
        int remainingSeconds = totalSeconds % 60;

        if (hours > 0)
        {
            return $"{hours}h {minutes}m";
        }

        return minutes > 0 ? $"{minutes}m {remainingSeconds}s" : $"{remainingSeconds}s";
    }

    private static async Task<JsonDocument> ReadJsonAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        return await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);
    }

    private static IReadOnlyList<JsonElement> ExtractArray(JsonElement root, params string[] propertyNames)
    {
        if (root.ValueKind == JsonValueKind.Array)
        {
            return root.EnumerateArray().Select(item => item.Clone()).ToList();
        }

        if (root.ValueKind != JsonValueKind.Object)
        {
            return Array.Empty<JsonElement>();
        }

        foreach (string propertyName in propertyNames)
        {
            if (TryGetProperty(root, propertyName, out JsonElement value) && value.ValueKind == JsonValueKind.Array)
            {
                return value.EnumerateArray().Select(item => item.Clone()).ToList();
            }
        }

        return Array.Empty<JsonElement>();
    }

    private static string? ReadString(JsonElement element, params string[] propertyNames)
    {
        foreach (string propertyName in propertyNames)
        {
            if (!TryGetProperty(element, propertyName, out JsonElement value))
            {
                continue;
            }

            return value.ValueKind switch
            {
                JsonValueKind.String => value.GetString(),
                JsonValueKind.Number => value.ToString(),
                _ => null
            };
        }

        return null;
    }

    private static double ReadDouble(JsonElement element, params string[] propertyNames)
    {
        foreach (string propertyName in propertyNames)
        {
            if (!TryGetProperty(element, propertyName, out JsonElement value))
            {
                continue;
            }

            if (value.ValueKind == JsonValueKind.Number && value.TryGetDouble(out double number))
            {
                return number;
            }

            if (value.ValueKind == JsonValueKind.String &&
                double.TryParse(value.GetString(), NumberStyles.Any, CultureInfo.InvariantCulture, out double parsed))
            {
                return parsed;
            }
        }

        return 0;
    }

    private static DateTime? ReadDateTime(JsonElement element, params string[] propertyNames)
    {
        string? value = ReadString(element, propertyNames);
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        if (DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out DateTime parsed))
        {
            return parsed.ToLocalTime();
        }

        return null;
    }

    private static bool TryGetProperty(JsonElement element, string propertyName, out JsonElement value)
    {
        if (element.ValueKind != JsonValueKind.Object)
        {
            value = default;
            return false;
        }

        foreach (JsonProperty property in element.EnumerateObject())
        {
            if (string.Equals(property.Name, propertyName, StringComparison.OrdinalIgnoreCase))
            {
                value = property.Value;
                return true;
            }
        }

        value = default;
        return false;
    }

    private sealed record PeriodWindow(DateTime From, DateTime To, string Label);
}

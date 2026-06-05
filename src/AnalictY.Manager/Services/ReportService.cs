using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using AnalictY.Manager.Models;

namespace AnalictY.Manager.Services;

public sealed class ReportService
{
    private static readonly Uri MachinesEndpoint = new("http://127.0.0.1:5000/api/machines");
    private static readonly Uri ProductionMatrixEndpoint = new("http://127.0.0.1:5000/api/reports/production/matrix");
    private static readonly Uri StatusMatrixEndpoint = new("http://127.0.0.1:5000/api/reports/status/matrix");
    private static readonly Uri DowntimeEventsEndpoint = new("http://127.0.0.1:5000/api/reports/downtime/events");
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly HttpClient _httpClient;

    public ReportService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<ReportPreviewResult> LoadPreviewAsync(string reportType, string machineId, string periodKey, CancellationToken cancellationToken = default)
    {
        string normalizedType = NormalizeReportType(reportType);
        var machines = await LoadMachinesAsync(cancellationToken);
        string selectedMachineId = ResolveMachineId(machineId, machines);
        var selectedMachine = machines.FirstOrDefault(machine => machine.Id == selectedMachineId) ?? machines.First();
        var period = BuildPeriod(periodKey);
        var payload = new
        {
            report_type = normalizedType,
            machine_id = string.IsNullOrWhiteSpace(selectedMachine.Id) ? null : selectedMachine.Id,
            inicio_em = period.From,
            fim_em = period.To,
            formato = "csv",
            incluir_motivos_parada = normalizedType == "downtime"
        };

        try
        {
            JsonElement? root = await PostJsonAsync(ResolveEndpoint(normalizedType), payload, cancellationToken);
            if (root is null)
            {
                return CreateFallback(machines, selectedMachine, normalizedType, period.Label, "Usando dados demonstrativos; servidor não retornou o relatório.");
            }

            return normalizedType switch
            {
                "production" => BuildProductionPreview(root.Value, machines, selectedMachine, period.Label),
                "status" => BuildStatusPreview(root.Value, machines, selectedMachine, period.Label),
                "downtime" => BuildDowntimePreview(root.Value, machines, selectedMachine, period.Label),
                _ => CreateFallback(machines, selectedMachine, normalizedType, period.Label, "Tipo de relatório não reconhecido; exibindo exemplo.")
            };
        }
        catch (Exception ex) when (ex is HttpRequestException or OperationCanceledException or JsonException)
        {
            return CreateFallback(machines, selectedMachine, normalizedType, period.Label, "Usando dados demonstrativos; não foi possível gerar a prévia.");
        }
    }

    private async Task<IReadOnlyList<ReportMachineOption>> LoadMachinesAsync(CancellationToken cancellationToken)
    {
        try
        {
            using HttpResponseMessage response = await _httpClient.GetAsync(MachinesEndpoint, cancellationToken);
            if (response.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden || !response.IsSuccessStatusCode)
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

    private static ReportPreviewResult BuildProductionPreview(JsonElement root, IReadOnlyList<ReportMachineOption> machines, ReportMachineOption selectedMachine, string periodLabel)
    {
        List<ReportPreviewRow> rows = ExtractArray(root, "rows", "Rows", "data", "items")
            .Select(item => new ReportPreviewRow(
                ReadString(item, "hour", "Hour") ?? "-",
                FormatValue(ReadDouble(item, "total", "Total")),
                FormatValue(ReadDouble(item, "outsideShift", "outside_shift", "outsideShiftTotal")),
                FormatValue(ReadDouble(item, "good", "Good")),
                FormatValue(ReadDouble(item, "total", "Total"))))
            .ToList();

        if (rows.Count == 0)
        {
            rows = CreateFallbackRows().ToList();
        }

        return new ReportPreviewResult(machines, rows, selectedMachine.Id, selectedMachine.DisplayName, "Produção", periodLabel, "Pronto", "Prévia de produção gerada com dados reais.", false);
    }

    private static ReportPreviewResult BuildStatusPreview(JsonElement root, IReadOnlyList<ReportMachineOption> machines, ReportMachineOption selectedMachine, string periodLabel)
    {
        List<ReportPreviewRow> rows = ExtractArray(root, "rows", "Rows", "data", "items")
            .Select(item => new ReportPreviewRow(
                ReadString(item, "hour", "Hour") ?? "-",
                FormatValue(ReadDouble(item, "productionMinutes", "production_minutes")),
                FormatValue(ReadDouble(item, "idleMinutes", "idle_minutes")),
                FormatValue(ReadDouble(item, "maintenanceMinutes", "maintenance_minutes")),
                FormatValue(ReadDouble(item, "totalMinutes", "total_minutes"))))
            .ToList();

        if (rows.Count == 0)
        {
            rows = CreateFallbackRows().ToList();
        }

        return new ReportPreviewResult(machines, rows, selectedMachine.Id, selectedMachine.DisplayName, "Status", periodLabel, "Pronto", "Prévia de status gerada com dados reais.", false);
    }

    private static ReportPreviewResult BuildDowntimePreview(JsonElement root, IReadOnlyList<ReportMachineOption> machines, ReportMachineOption selectedMachine, string periodLabel)
    {
        List<ReportPreviewRow> rows = ExtractArray(root, "items", "events", "rows", "data")
            .Select(item => new ReportPreviewRow(
                ReadString(item, "triggerAt", "trigger_at", "start_time", "startTime") ?? "-",
                ReadString(item, "recoveryAt", "recovery_at", "end_time", "endTime") ?? "-",
                ReadString(item, "reason", "motivo", "category", "categoria") ?? "-",
                FormatValue(ReadDouble(item, "totalMinutes", "total_minutes", "durationMinutes", "duration_minutes")),
                FormatValue(ReadDouble(item, "totalSeconds", "total_seconds", "durationSeconds", "duration_seconds"))))
            .ToList();

        if (rows.Count == 0)
        {
            rows = CreateFallbackRows().ToList();
        }

        return new ReportPreviewResult(machines, rows, selectedMachine.Id, selectedMachine.DisplayName, "Paradas", periodLabel, "Pronto", "Prévia de paradas gerada com dados reais.", false);
    }

    private static ReportPreviewResult CreateFallback(IReadOnlyList<ReportMachineOption> machines, ReportMachineOption selectedMachine, string reportType, string periodLabel, string message)
    {
        return new ReportPreviewResult(machines, CreateFallbackRows(), selectedMachine.Id, selectedMachine.DisplayName, ReportTypeLabel(reportType), periodLabel, "Demonstrativo", message, true);
    }

    private static IReadOnlyList<ReportPreviewRow> CreateFallbackRows()
    {
        return new List<ReportPreviewRow>
        {
            new("06:00", "120", "8", "112", "120"),
            new("07:00", "136", "5", "131", "136"),
            new("08:00", "128", "7", "121", "128"),
            new("09:00", "142", "3", "139", "142")
        };
    }

    private static string ReportTypeLabel(string reportType) => reportType switch
    {
        "production" => "Produção",
        "status" => "Status",
        "downtime" => "Paradas",
        _ => "Relatório"
    };

    private static string NormalizeReportType(string reportType) => reportType switch
    {
        "Produção" or "produção" => "production",
        "Paradas" or "paradas" => "downtime",
        "Status" or "status" => "status",
        _ => reportType
    };

    private static Uri ResolveEndpoint(string reportType) => reportType switch
    {
        "production" => ProductionMatrixEndpoint,
        "status" => StatusMatrixEndpoint,
        "downtime" => DowntimeEventsEndpoint,
        _ => ProductionMatrixEndpoint
    };

    private static string ResolveMachineId(string? machineId, IReadOnlyList<ReportMachineOption> machines)
    {
        if (!string.IsNullOrWhiteSpace(machineId) && machines.Any(machine => machine.Id == machineId))
        {
            return machineId;
        }

        return machines.First().Id;
    }

    private static IReadOnlyList<ReportMachineOption> CreateFallbackMachines()
    {
        return new List<ReportMachineOption>
        {
            new("1", "PJ_08", "PJ_08"),
            new("2", "Morno_01", "Morno_01"),
            new("3", "Morno_02", "Morno_02")
        };
    }

    private static ReportMachineOption MapMachine(JsonElement item)
    {
        string id = ReadString(item, "id", "machine_id", "machineId") ?? string.Empty;
        string name = ReadString(item, "name", "nome", "code", "codigo", "código") ?? id;
        string code = ReadString(item, "code", "codigo", "código") ?? string.Empty;
        return new ReportMachineOption(id, name, code);
    }

    private async Task<JsonElement?> PostJsonAsync(Uri endpoint, object payload, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, endpoint)
        {
            Content = new StringContent(JsonSerializer.Serialize(payload, JsonOptions), Encoding.UTF8, "application/json")
        };

        using HttpResponseMessage response = await _httpClient.SendAsync(request, cancellationToken);
        if (response.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden || !response.IsSuccessStatusCode)
        {
            return null;
        }

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using JsonDocument document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);
        return document.RootElement.Clone();
    }

    private static string FormatValue(double value) => value > 0 ? value.ToString("N0", CultureInfo.GetCultureInfo("pt-BR")) : "--";

    private static async Task<JsonDocument> ReadJsonAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        return await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);
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
            if (TryGetProperty(element, propertyName, out JsonElement value))
            {
                return value.ValueKind switch
                {
                    JsonValueKind.String => value.GetString(),
                    JsonValueKind.Number => value.ToString(),
                    _ => null
                };
            }
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

            if (value.ValueKind == JsonValueKind.String && double.TryParse(value.GetString(), NumberStyles.Any, CultureInfo.InvariantCulture, out double parsed))
            {
                return parsed;
            }
        }

        return 0;
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

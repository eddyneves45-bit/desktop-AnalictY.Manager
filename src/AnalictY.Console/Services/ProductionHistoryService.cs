using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using AnalictY.Console.Models;

namespace AnalictY.Console.Services;

public sealed class ProductionHistoryService
{
    private static readonly Uri MachinesEndpoint = new("http://127.0.0.1:5000/api/machines");
    private static readonly Uri MatrixEndpoint = new("http://127.0.0.1:5000/api/reports/production/matrix");
    private static readonly Uri DashboardEndpoint = new("http://127.0.0.1:5000/api/reports/machine-dashboard");
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly HttpClient _httpClient;

    public ProductionHistoryService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<IReadOnlyList<ProductionMachineOption>> LoadMachinesAsync(CancellationToken cancellationToken = default)
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
        catch (Exception ex) when (ex is HttpRequestException or OperationCanceledException or JsonException)
        {
            return CreateFallbackMachines();
        }
    }

    public async Task<ProductionHistoryResult> LoadHistoryAsync(
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
            var payload = JsonSerializer.Serialize(new
            {
                report_type = "production",
                machine_id = selectedMachineId,
                inicio_em = period.From,
                fim_em = period.To,
                formato = "csv",
                incluir_motivos_parada = false
            }, JsonOptions);

            using var request = new HttpRequestMessage(HttpMethod.Post, MatrixEndpoint)
            {
                Content = new StringContent(payload, Encoding.UTF8, "application/json")
            };

            using HttpResponseMessage response = await _httpClient.SendAsync(request, cancellationToken);
            if (response.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
            {
                return CreateFallbackResult(machines, selectedMachineId, period.Label, "Usando dados demonstrativos; histórico exige permissão de relatórios.");
            }

            if (!response.IsSuccessStatusCode)
            {
                return CreateFallbackResult(machines, selectedMachineId, period.Label, "Usando dados demonstrativos; servidor não retornou histórico de produção.");
            }

            using JsonDocument matrixDocument = await ReadJsonAsync(response, cancellationToken);
            var producedRows = ReadMatrixRows(matrixDocument.RootElement);
            var dashboard = await LoadDashboardAsync(selectedMachineId, period.From, period.To, cancellationToken);

            double totalProduced = dashboard.TotalProduced > 0 ? dashboard.TotalProduced : producedRows.Sum(row => row.Produced);
            double totalLost = dashboard.TotalLost;
            double totalGood = dashboard.TotalGood > 0 ? dashboard.TotalGood : Math.Max(totalProduced - totalLost, 0);
            var rows = ApplyLossDistribution(producedRows, totalProduced, totalLost);

            return new ProductionHistoryResult(
                machines,
                rows,
                selectedMachineId,
                selectedMachine.DisplayName,
                period.Label,
                totalProduced,
                totalLost,
                totalGood,
                dashboard.HasSummary
                    ? "Histórico carregado do AnalictY Server."
                    : "Histórico carregado; perdas por hora não disponíveis neste endpoint.",
                false);
        }
        catch (Exception ex) when (ex is HttpRequestException or OperationCanceledException or JsonException)
        {
            return CreateFallbackResult(machines, selectedMachineId, period.Label, "Usando dados demonstrativos; não foi possível carregar o histórico de produção.");
        }
    }

    private async Task<DashboardSummary> LoadDashboardAsync(
        string machineId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken)
    {
        try
        {
            var builder = new UriBuilder(DashboardEndpoint);
            builder.Query =
                $"machine_id={Uri.EscapeDataString(machineId)}&from={Uri.EscapeDataString(from.ToString("O", CultureInfo.InvariantCulture))}&to={Uri.EscapeDataString(to.ToString("O", CultureInfo.InvariantCulture))}";

            using HttpResponseMessage response = await _httpClient.GetAsync(builder.Uri, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return new DashboardSummary(0, 0, 0, false);
            }

            using JsonDocument document = await ReadJsonAsync(response, cancellationToken);
            return new DashboardSummary(
                ReadDouble(document.RootElement, "production_total", "productionTotal"),
                ReadDouble(document.RootElement, "loss_total", "lossTotal"),
                ReadDouble(document.RootElement, "good_total", "goodTotal"),
                true);
        }
        catch
        {
            return new DashboardSummary(0, 0, 0, false);
        }
    }

    private static IReadOnlyList<ProductionHistoryRow> ReadMatrixRows(JsonElement root)
    {
        return ExtractArray(root, "rows", "Rows", "data", "items")
            .Select(row =>
            {
                string hour = ReadString(row, "hour", "Hour") ?? ReadHour(row);
                double produced = ReadDouble(row, "total", "Total");
                return new ProductionHistoryRow(hour, produced, 0, produced);
            })
            .Where(row => !string.IsNullOrWhiteSpace(row.Hour))
            .ToList();
    }

    private static IReadOnlyList<ProductionHistoryRow> ApplyLossDistribution(
        IReadOnlyList<ProductionHistoryRow> rows,
        double totalProduced,
        double totalLost)
    {
        if (rows.Count == 0)
        {
            return rows;
        }

        if (totalProduced <= 0 || totalLost <= 0)
        {
            return rows.Select(row => row with { Lost = 0, Good = row.Produced }).ToList();
        }

        return rows
            .Select(row =>
            {
                double lost = Math.Round(totalLost * (row.Produced / totalProduced), 2);
                return row with { Lost = lost, Good = Math.Max(row.Produced - lost, 0) };
            })
            .ToList();
    }

    private static ProductionHistoryResult CreateFallbackResult(
        IReadOnlyList<ProductionMachineOption> machines,
        string selectedMachineId,
        string periodLabel,
        string message)
    {
        var selectedMachine = machines.FirstOrDefault(machine => machine.Id == selectedMachineId) ?? machines.First();
        var rows = new List<ProductionHistoryRow>
        {
            new("06:00", 120, 4, 116),
            new("07:00", 136, 6, 130),
            new("08:00", 128, 3, 125),
            new("09:00", 142, 5, 137),
            new("10:00", 110, 2, 108),
            new("11:00", 96, 1, 95)
        };

        return new ProductionHistoryResult(
            machines,
            rows,
            selectedMachine.Id,
            selectedMachine.DisplayName,
            periodLabel,
            rows.Sum(row => row.Produced),
            rows.Sum(row => row.Lost),
            rows.Sum(row => row.Good),
            message,
            true);
    }

    private static IReadOnlyList<ProductionMachineOption> CreateFallbackMachines()
    {
        return new List<ProductionMachineOption>
        {
            new("1", "PJ_08", "PJ_08"),
            new("2", "Morno_01", "Morno_01"),
            new("3", "Morno_02", "Morno_02")
        };
    }

    private static string ResolveMachineId(string? machineId, IReadOnlyList<ProductionMachineOption> machines)
    {
        if (!string.IsNullOrWhiteSpace(machineId) && machines.Any(machine => machine.Id == machineId))
        {
            return machineId;
        }

        return machines.First().Id;
    }

    private static ProductionMachineOption MapMachine(JsonElement machine)
    {
        string id = ReadString(machine, "id", "machine_id", "machineId") ?? string.Empty;
        string name = ReadString(machine, "name", "nome", "code", "codigo", "código") ?? id;
        string code = ReadString(machine, "code", "codigo", "código") ?? string.Empty;
        return new ProductionMachineOption(id, name, code);
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

    private static string ReadHour(JsonElement row)
    {
        string? hourStart = ReadString(row, "hourStart", "HourStart", "hour_start");
        return DateTime.TryParse(hourStart, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out DateTime date)
            ? date.ToString("HH:00")
            : "-";
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

    private sealed record DashboardSummary(double TotalProduced, double TotalLost, double TotalGood, bool HasSummary);

    private sealed record PeriodWindow(DateTime From, DateTime To, string Label);
}

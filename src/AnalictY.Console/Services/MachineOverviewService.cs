using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Windows.Media;
using AnalictY.Console.Models;

namespace AnalictY.Console.Services;

public sealed class MachineOverviewService
{
    private static readonly Uri MachinesEndpoint = new("http://127.0.0.1:5000/api/machines");
    private static readonly Uri ResolvedStatesEndpoint = new("http://127.0.0.1:5000/api/machines/resolved-state-all");
    private readonly HttpClient _httpClient;

    public MachineOverviewService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<MachineOverviewResult> LoadAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            using HttpResponseMessage machinesResponse = await _httpClient.GetAsync(MachinesEndpoint, cancellationToken);
            if (machinesResponse.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
            {
                return Fallback("Usando dados demonstrativos; a API de máquinas exige autenticação.");
            }

            if (!machinesResponse.IsSuccessStatusCode)
            {
                return Fallback("Usando dados demonstrativos; servidor não retornou máquinas.");
            }

            using JsonDocument machinesDocument = await ReadJsonAsync(machinesResponse, cancellationToken);
            var machineElements = ExtractArray(machinesDocument.RootElement, "machines", "data", "items", "results");
            if (machineElements.Count == 0)
            {
                return Fallback("Usando dados demonstrativos; servidor não retornou máquinas.");
            }

            var resolvedStates = await LoadResolvedStatesAsync(cancellationToken);
            var cards = machineElements
                .Select(machine => MapMachine(machine, resolvedStates))
                .Where(machine => !string.IsNullOrWhiteSpace(machine.Name))
                .ToList();

            if (cards.Count == 0)
            {
                return Fallback("Usando dados demonstrativos; servidor não retornou máquinas válidas.");
            }

            return new MachineOverviewResult(cards, false, $"Máquinas carregadas do AnalictY Server: {cards.Count}.");
        }
        catch (OperationCanceledException)
        {
            return Fallback("Usando dados demonstrativos; tempo esgotado ao consultar máquinas.");
        }
        catch (HttpRequestException)
        {
            return Fallback("Usando dados demonstrativos; servidor não retornou máquinas.");
        }
        catch (JsonException)
        {
            return Fallback("Usando dados demonstrativos; resposta de máquinas não pôde ser lida.");
        }
    }

    public static IReadOnlyList<MachineCard> CreateFallbackMachines()
    {
        return new List<MachineCard>
        {
            CreateMachineCard("PJ_08", "Linha Principal", "Em operação", "OP-2406-118", "92%"),
            CreateMachineCard("Morno_01", "Preparação", "Em operação", "OP-2406-121", "88%"),
            CreateMachineCard("Morno_02", "Preparação", "Ociosa", "Sem ordem", "0%"),
            CreateMachineCard("Injetora_03", "Célula A", "Manutenção", "Bloqueada", "0%"),
            CreateMachineCard("Esteira_01", "Expedição", "Em operação", "OP-2406-102", "95%"),
            CreateMachineCard("Misturador_02", "Célula B", "Ociosa", "Aguardando programação", "0%")
        };
    }

    private async Task<Dictionary<string, JsonElement>> LoadResolvedStatesAsync(CancellationToken cancellationToken)
    {
        try
        {
            using HttpResponseMessage response = await _httpClient.GetAsync(ResolvedStatesEndpoint, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return new Dictionary<string, JsonElement>(StringComparer.OrdinalIgnoreCase);
            }

            using JsonDocument document = await ReadJsonAsync(response, cancellationToken);
            var states = new Dictionary<string, JsonElement>(StringComparer.OrdinalIgnoreCase);

            if (document.RootElement.ValueKind == JsonValueKind.Object)
            {
                foreach (JsonProperty property in document.RootElement.EnumerateObject())
                {
                    if (property.Value.ValueKind is JsonValueKind.Object or JsonValueKind.String or JsonValueKind.Number)
                    {
                        states[property.Name] = property.Value.Clone();
                    }
                }

                foreach (JsonElement item in ExtractArray(document.RootElement, "states", "data", "items", "results"))
                {
                    AddState(states, item);
                }
            }
            else if (document.RootElement.ValueKind == JsonValueKind.Array)
            {
                foreach (JsonElement item in document.RootElement.EnumerateArray())
                {
                    AddState(states, item);
                }
            }

            return states;
        }
        catch
        {
            return new Dictionary<string, JsonElement>(StringComparer.OrdinalIgnoreCase);
        }
    }

    private static void AddState(Dictionary<string, JsonElement> states, JsonElement state)
    {
        var keys = new[]
        {
            ReadString(state, "machine_id", "machineId", "id"),
            ReadString(state, "machine_name", "machineName", "name", "nome"),
            ReadString(state, "code", "codigo", "código")
        };

        foreach (string? key in keys)
        {
            if (!string.IsNullOrWhiteSpace(key))
            {
                states[key] = state.Clone();
            }
        }
    }

    private static MachineCard MapMachine(JsonElement machine, IReadOnlyDictionary<string, JsonElement> states)
    {
        string id = ReadString(machine, "id", "machine_id", "machineId") ?? string.Empty;
        string? code = ReadString(machine, "code", "codigo", "código");
        string name = ReadString(machine, "name", "nome", "code", "codigo", "código") ?? id;
        string area = ReadString(machine, "location", "setor", "sector", "area", "cost_center", "centro_custo", "centroCusto")
            ?? "Sem setor informado";
        string order = ReadString(machine, "current_order", "currentOrder", "ordem", "order", "op", "production_order")
            ?? "Sem ordem";
        string efficiency = ReadEfficiency(machine);

        JsonElement? state = FindState(states, id, name, code);
        string? rawStatus = state is null
            ? ReadString(machine, "status", "state", "estado", "resolved_state", "resolvedState")
            : ReadString(
                state.Value,
                "machine_status",
                "machineStatus",
                "status_maquina",
                "status",
                "state",
                "estado",
                "resolved_state",
                "resolvedState",
                "value",
                "valor");

        string status = NormalizeStatus(rawStatus);
        return CreateMachineCard(name, area, status, order, efficiency);
    }

    private static JsonElement? FindState(IReadOnlyDictionary<string, JsonElement> states, params string?[] keys)
    {
        foreach (string? key in keys)
        {
            if (!string.IsNullOrWhiteSpace(key) && states.TryGetValue(key, out JsonElement state))
            {
                return state;
            }
        }

        return null;
    }

    private static MachineCard CreateMachineCard(string name, string area, string status, string currentOrder, string efficiency)
    {
        var background = status switch
        {
            "Em operação" => new SolidColorBrush(Color.FromRgb(28, 139, 99)),
            "Ociosa" => new SolidColorBrush(Color.FromRgb(100, 116, 139)),
            "Manutenção" => new SolidColorBrush(Color.FromRgb(217, 119, 6)),
            _ => new SolidColorBrush(Color.FromRgb(75, 85, 99))
        };

        return new MachineCard(name, area, status, currentOrder, efficiency, Brushes.White, background);
    }

    private static string NormalizeStatus(string? status)
    {
        string normalized = RemoveDiacritics(status).Trim().ToUpperInvariant();

        return normalized switch
        {
            "RODANDO" or "OPERACAO" or "OPERATION" or "RUNNING" or "ON" or "1" => "Em operação",
            "OCIOSA" or "OCIOSO" or "IDLE" or "2" => "Ociosa",
            "MANUTENCAO" or "MAINTENANCE" or "MAINT" or "3" => "Manutenção",
            "OFFLINE" or "INATIVA" or "INATIVO" or "STOPPED" or "PARADA" or "PARADO" or "OFF" or "0" => "Inativa/Offline",
            _ => "Inativa/Offline"
        };
    }

    private static string ReadEfficiency(JsonElement machine)
    {
        string? value = ReadString(machine, "efficiency", "eficiencia", "eficiência", "oee");
        if (string.IsNullOrWhiteSpace(value))
        {
            return "0%";
        }

        return value.EndsWith('%') ? value : $"{value}%";
    }

    private static MachineOverviewResult Fallback(string message)
    {
        return new MachineOverviewResult(CreateFallbackMachines(), true, message);
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
            if (TryGetProperty(root, propertyName, out JsonElement property) && property.ValueKind == JsonValueKind.Array)
            {
                return property.EnumerateArray().Select(item => item.Clone()).ToList();
            }
        }

        return Array.Empty<JsonElement>();
    }

    private static string? ReadString(JsonElement element, params string[] propertyNames)
    {
        if (element.ValueKind is JsonValueKind.String or JsonValueKind.Number or JsonValueKind.True or JsonValueKind.False)
        {
            return element.ToString();
        }

        if (element.ValueKind != JsonValueKind.Object)
        {
            return null;
        }

        foreach (string propertyName in propertyNames)
        {
            if (TryGetProperty(element, propertyName, out JsonElement value))
            {
                return value.ValueKind switch
                {
                    JsonValueKind.String => value.GetString(),
                    JsonValueKind.Number => value.ToString(),
                    JsonValueKind.True => "1",
                    JsonValueKind.False => "0",
                    _ => value.ToString()
                };
            }
        }

        return null;
    }

    private static bool TryGetProperty(JsonElement element, string propertyName, out JsonElement value)
    {
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

    private static string RemoveDiacritics(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return string.Empty;
        }

        string normalized = text.Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder(normalized.Length);

        foreach (char character in normalized)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(character) != UnicodeCategory.NonSpacingMark)
            {
                builder.Append(character);
            }
        }

        return builder.ToString().Normalize(NormalizationForm.FormC);
    }
}

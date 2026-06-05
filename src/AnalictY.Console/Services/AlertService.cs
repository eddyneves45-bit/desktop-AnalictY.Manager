using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using AnalictY.Console.Models;

namespace AnalictY.Console.Services;

public sealed class AlertService
{
    private static readonly Uri AlertsEndpoint = new("http://127.0.0.1:5000/api/alerts?limit=20");
    private static readonly Uri AlertRulesEndpoint = new("http://127.0.0.1:5000/api/alert-rules");
    private static readonly Uri TelegramStatusEndpoint = new("http://127.0.0.1:5000/api/notifications/telegram/status");
    private static readonly Uri TelegramConnectionsEndpoint = new("http://127.0.0.1:5000/api/notifications/telegram/connections");
    private static readonly Uri TelegramRecipientsEndpoint = new("http://127.0.0.1:5000/api/notifications/telegram/recipients");
    private readonly HttpClient _httpClient;

    public AlertService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<AlertOverviewResult> LoadAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var alertsTask = LoadJsonAsync(AlertsEndpoint, cancellationToken);
            var rulesTask = LoadJsonAsync(AlertRulesEndpoint, cancellationToken);
            var telegramTask = LoadJsonAsync(TelegramStatusEndpoint, cancellationToken);
            var connectionsTask = LoadJsonAsync(TelegramConnectionsEndpoint, cancellationToken);
            var recipientsTask = LoadJsonAsync(TelegramRecipientsEndpoint, cancellationToken);

            await Task.WhenAll(alertsTask, rulesTask, telegramTask, connectionsTask, recipientsTask);

            JsonElement? alertsRoot = alertsTask.Result;
            JsonElement? rulesRoot = rulesTask.Result;
            JsonElement? telegramRoot = telegramTask.Result;
            JsonElement? connectionsRoot = connectionsTask.Result;
            JsonElement? recipientsRoot = recipientsTask.Result;

            if (alertsRoot is null && rulesRoot is null && telegramRoot is null)
            {
                return CreateFallback("Usando dados demonstrativos; servidor não retornou alertas.");
            }

            var alertItems = alertsRoot is null
                ? Array.Empty<JsonElement>()
                : ExtractArray(alertsRoot.Value, "alerts", "data", "items", "results");
            var ruleItems = rulesRoot is null
                ? Array.Empty<JsonElement>()
                : ExtractArray(rulesRoot.Value, "rules", "data", "items", "results");

            var rows = BuildRows(ruleItems, alertItems);
            int activeAlerts = alertItems.Count(item => !ReadBool(item, true, "isAcknowledged", "is_acknowledged", "acknowledged"));
            int criticalAlerts = alertItems.Count(IsCritical);
            string telegramStatus = BuildTelegramStatus(telegramRoot, connectionsRoot, recipientsRoot);
            string lastActivity = ResolveLastActivity(alertItems, ruleItems, connectionsRoot, recipientsRoot);

            return new AlertOverviewResult(
                rows.Count > 0 ? rows : CreateFallbackRows(),
                activeAlerts,
                criticalAlerts,
                telegramStatus,
                lastActivity,
                rows.Count > 0 || alertItems.Count > 0
                    ? "Alertas carregados do AnalictY Server."
                    : "Nenhum alerta ou regra retornado; exibindo exemplos demonstrativos.",
                rows.Count == 0 && alertItems.Count == 0);
        }
        catch (Exception ex) when (ex is HttpRequestException or OperationCanceledException or JsonException)
        {
            return CreateFallback("Usando dados demonstrativos; não foi possível carregar alertas.");
        }
    }

    private async Task<JsonElement?> LoadJsonAsync(Uri endpoint, CancellationToken cancellationToken)
    {
        try
        {
            using HttpResponseMessage response = await _httpClient.GetAsync(endpoint, cancellationToken);
            if (response.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
            {
                return null;
            }

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            using JsonDocument document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);
            return document.RootElement.Clone();
        }
        catch (Exception ex) when (ex is HttpRequestException or OperationCanceledException or JsonException)
        {
            return null;
        }
    }

    private static List<AlertRuleRow> BuildRows(IReadOnlyList<JsonElement> rules, IReadOnlyList<JsonElement> alerts)
    {
        var rows = rules
            .Select(MapRule)
            .Where(row => !string.IsNullOrWhiteSpace(row.Name))
            .ToList();

        rows.AddRange(alerts
            .Take(8)
            .Select(MapAlert)
            .Where(row => !string.IsNullOrWhiteSpace(row.Name)));

        return rows;
    }

    private static AlertRuleRow MapRule(JsonElement item)
    {
        string name = ReadString(item, "name", "nome", "title", "titulo") ?? "Regra de alerta";
        string tagName = ReadString(item, "tagName", "tag_name", "tag", "tagConfigId") ?? "TAG";
        string operatorValue = ReadString(item, "operator", "operador") ?? "";
        string limitValue = ReadString(item, "limitValue", "limit_value", "value", "valor") ?? "";
        string message = ReadString(item, "message", "mensagem", "description", "descricao") ?? "";
        string severity = NormalizeSeverity(ReadString(item, "severity", "severidade", "level", "prioridade"));
        bool active = ReadBool(item, true, "isActive", "is_active", "enabled", "active");
        string condition = string.IsNullOrWhiteSpace(limitValue)
            ? message
            : $"{tagName} {operatorValue} {limitValue}".Trim();

        return new AlertRuleRow(
            name,
            tagName,
            string.IsNullOrWhiteSpace(condition) ? "Condição configurada no servidor" : condition,
            severity,
            active ? "Ativa" : "Inativa");
    }

    private static AlertRuleRow MapAlert(JsonElement item)
    {
        string title = ReadString(item, "title", "titulo", "name", "nome") ?? "Alerta registrado";
        string machine = ReadString(item, "machineId", "machine_id", "machine", "maquina", "scope", "escopo") ?? "Geral";
        string message = ReadString(item, "message", "mensagem", "metadata", "description", "descricao") ?? "Evento registrado pelo servidor";
        string severity = NormalizeSeverity(ReadString(item, "severity", "severidade", "level", "prioridade"));
        bool acknowledged = ReadBool(item, false, "isAcknowledged", "is_acknowledged", "acknowledged");

        return new AlertRuleRow(
            title,
            machine,
            message,
            severity,
            acknowledged ? "Reconhecido" : "Pendente");
    }

    private static string BuildTelegramStatus(JsonElement? statusRoot, JsonElement? connectionsRoot, JsonElement? recipientsRoot)
    {
        if (statusRoot is null)
        {
            return "Indisponível";
        }

        bool enabled = ReadBool(statusRoot.Value, false, "enabled", "is_enabled", "configured");
        int recipients = ReadInt(statusRoot.Value, "recipients", "active_recipients");
        int connections = ReadInt(statusRoot.Value, "dynamicConnections", "dynamic_connections", "connections");
        string? chat = ReadString(statusRoot.Value, "chatId", "chat_id");

        if (connections == 0 && connectionsRoot is not null)
        {
            connections = ExtractArray(connectionsRoot.Value, "connections", "data", "items")
                .Count(item => ReadBool(item, false, "is_active", "isActive", "active"));
        }

        if (recipients == 0 && recipientsRoot is not null)
        {
            recipients = ExtractArray(recipientsRoot.Value, "recipients", "data", "items")
                .Count(item => ReadBool(item, false, "is_active", "isActive", "active"));
        }

        if (!enabled)
        {
            return "Desabilitado";
        }

        if (recipients > 0)
        {
            return $"Ativo, {recipients} destino(s)";
        }

        return connections > 0
            ? $"Ativo, {connections} conexão(ões)"
            : $"Ativo{(string.IsNullOrWhiteSpace(chat) ? string.Empty : $" para {chat}")}";
    }

    private static string ResolveLastActivity(
        IReadOnlyList<JsonElement> alerts,
        IReadOnlyList<JsonElement> rules,
        JsonElement? connectionsRoot,
        JsonElement? recipientsRoot)
    {
        var dates = alerts
            .Concat(rules)
            .Concat(connectionsRoot is null ? [] : ExtractArray(connectionsRoot.Value, "connections", "data", "items"))
            .Concat(recipientsRoot is null ? [] : ExtractArray(recipientsRoot.Value, "recipients", "data", "items"))
            .Select(item => ReadDateTime(item, "createdAt", "created_at", "updatedAt", "updated_at", "acknowledgedAt", "acknowledged_at"))
            .Where(date => date is not null)
            .Select(date => date!.Value)
            .OrderByDescending(date => date)
            .FirstOrDefault();

        return dates == default ? "Sem registro" : dates.ToString("dd/MM/yyyy HH:mm");
    }

    private static AlertOverviewResult CreateFallback(string message)
    {
        return new AlertOverviewResult(
            CreateFallbackRows(),
            2,
            1,
            "Demonstração",
            "Sem registro real",
            message,
            true);
    }

    private static IReadOnlyList<AlertRuleRow> CreateFallbackRows()
    {
        return new List<AlertRuleRow>
        {
            new("Temperatura alta", "PJ_08", "TAG temperatura > 80", "Crítica", "Ativa"),
            new("Máquina ociosa", "Morno_01", "Status em ociosa por mais de 15 min", "Média", "Ativa"),
            new("Manutenção preventiva", "Morno_02", "Horímetro próximo do limite", "Alta", "Inativa")
        };
    }

    private static bool IsCritical(JsonElement item)
    {
        string severity = ReadString(item, "severity", "severidade", "level", "prioridade") ?? "";
        return severity.Equals("critical", StringComparison.OrdinalIgnoreCase)
            || severity.Equals("critica", StringComparison.OrdinalIgnoreCase)
            || severity.Equals("crítica", StringComparison.OrdinalIgnoreCase)
            || severity.Equals("high", StringComparison.OrdinalIgnoreCase)
            || severity.Equals("alta", StringComparison.OrdinalIgnoreCase);
    }

    private static string NormalizeSeverity(string? severity)
    {
        return (severity ?? string.Empty).Trim().ToLowerInvariant() switch
        {
            "critical" or "critica" or "crítica" => "Crítica",
            "high" or "alta" => "Alta",
            "medium" or "media" or "média" => "Média",
            "low" or "baixa" => "Baixa",
            "" => "Não informada",
            var value => CultureInfo.GetCultureInfo("pt-BR").TextInfo.ToTitleCase(value)
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
            if (!TryGetProperty(element, propertyName, out JsonElement value))
            {
                continue;
            }

            return value.ValueKind switch
            {
                JsonValueKind.String => value.GetString(),
                JsonValueKind.Number => value.ToString(),
                JsonValueKind.True => "true",
                JsonValueKind.False => "false",
                _ => null
            };
        }

        return null;
    }

    private static int ReadInt(JsonElement element, params string[] propertyNames)
    {
        foreach (string propertyName in propertyNames)
        {
            if (!TryGetProperty(element, propertyName, out JsonElement value))
            {
                continue;
            }

            if (value.ValueKind == JsonValueKind.Number && value.TryGetInt32(out int number))
            {
                return number;
            }

            if (value.ValueKind == JsonValueKind.String && int.TryParse(value.GetString(), out int parsed))
            {
                return parsed;
            }
        }

        return 0;
    }

    private static bool ReadBool(JsonElement element, bool defaultValue, params string[] propertyNames)
    {
        foreach (string propertyName in propertyNames)
        {
            if (!TryGetProperty(element, propertyName, out JsonElement value))
            {
                continue;
            }

            if (value.ValueKind is JsonValueKind.True or JsonValueKind.False)
            {
                return value.GetBoolean();
            }

            if (value.ValueKind == JsonValueKind.String && bool.TryParse(value.GetString(), out bool parsed))
            {
                return parsed;
            }
        }

        return defaultValue;
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
}

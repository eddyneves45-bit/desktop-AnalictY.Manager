namespace AnalictY.Manager.Models;

// OPC UA
public sealed class OpcUaConnectionsResult
{
    public List<OpcUaConnection> Connections { get; set; } = new();
    public string? Error { get; set; }
}

public sealed class OpcUaConnection
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string ServerUrl { get; set; } = string.Empty;
    public string SecurityPolicy { get; set; } = "None";
    public string SecurityMode { get; set; } = "None";
    public string Username { get; set; } = string.Empty;
    public string CertificatePath { get; set; } = string.Empty;
    public string PrivateKeyPath { get; set; } = string.Empty;
    public string UpdateInterval { get; set; } = "1000";
    public bool IsActive { get; set; } = true;
    public string Status { get; set; } = "Desconhecido";
}

public sealed class OpcUaConnectionRequest
{
    public string Name { get; set; } = string.Empty;
    public string ServerUrl { get; set; } = string.Empty;
    public string SecurityPolicy { get; set; } = "None";
    public string SecurityMode { get; set; } = "None";
    public string? Username { get; set; }
    public string? Password { get; set; }
    public string? CertificatePath { get; set; }
    public string? PrivateKeyPath { get; set; }
    public string UpdateInterval { get; set; } = "1000";
    public bool IsActive { get; set; } = true;
}

public sealed class OpcUaBrowseResult
{
    public List<OpcUaNode> Nodes { get; set; } = new();
    public string? Error { get; set; }
}

public sealed class OpcUaNode
{
    public string NodeId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Quality { get; set; } = "Desconhecido";
    public bool HasChildren { get; set; }
}

// MQTT
public sealed class MqttConnectionsResult
{
    public List<MqttConnection> Connections { get; set; } = new();
    public string? Error { get; set; }
}

public sealed class MqttConnection
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string ClientId { get; set; } = string.Empty;
    public string BrokerHost { get; set; } = string.Empty;
    public string BrokerPort { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public bool TlsEnabled { get; set; }
    public string CaCertPath { get; set; } = string.Empty;
    public string ClientCertPath { get; set; } = string.Empty;
    public string ClientKeyPath { get; set; } = string.Empty;
    public string Topics { get; set; } = string.Empty;
    public string Qos { get; set; } = "0";
    public bool IsActive { get; set; } = true;
    public string Status { get; set; } = "Desconhecido";
}

public sealed class MqttConnectionRequest
{
    public string Name { get; set; } = string.Empty;
    public string ClientId { get; set; } = string.Empty;
    public string BrokerHost { get; set; } = string.Empty;
    public string BrokerPort { get; set; } = string.Empty;
    public string? Username { get; set; }
    public string? Password { get; set; }
    public bool TlsEnabled { get; set; }
    public string? CaCertPath { get; set; }
    public string? ClientCertPath { get; set; }
    public string? ClientKeyPath { get; set; }
    public string Topics { get; set; } = string.Empty;
    public string Qos { get; set; } = "0";
    public bool IsActive { get; set; } = true;
}

public sealed class MqttTopicsResult
{
    public List<MqttTopic> Topics { get; set; } = new();
    public string? Error { get; set; }
}

public sealed class MqttTopic
{
    public string Topic { get; set; } = string.Empty;
    public string Qos { get; set; } = "0";
    public string Subscribers { get; set; } = "0";
}

public sealed class MqttClientsResult
{
    public List<MqttClient> Clients { get; set; } = new();
    public string? Error { get; set; }
}

public sealed class MqttClient
{
    public string ClientId { get; set; } = string.Empty;
    public string Ip { get; set; } = string.Empty;
    public string Connected { get; set; } = "Não";
    public string Topics { get; set; } = "0";
}

// MySQL
public sealed class MysqlConnectionsResult
{
    public List<MysqlConnection> Connections { get; set; } = new();
    public string? Error { get; set; }
}

public sealed class MysqlConnection
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Host { get; set; } = string.Empty;
    public string Port { get; set; } = string.Empty;
    public string Database { get; set; } = string.Empty;
    public string Provider { get; set; } = "MySQL";
    public string Username { get; set; } = string.Empty;
    public string PoolSize { get; set; } = "10";
    public bool IsActive { get; set; } = true;
    public bool IsPrimary { get; set; }
    public bool IsLocal { get; set; }
    public bool IsRemote { get; set; }
    public string Status { get; set; } = "Desconhecido";
}

public sealed class MysqlConnectionRequest
{
    public string Name { get; set; } = string.Empty;
    public string Host { get; set; } = string.Empty;
    public string Port { get; set; } = string.Empty;
    public string Database { get; set; } = string.Empty;
    public string Provider { get; set; } = "MySQL";
    public string? Username { get; set; }
    public string? Password { get; set; }
    public string PoolSize { get; set; } = "10";
    public bool IsActive { get; set; } = true;
    public bool IsPrimary { get; set; }
    public bool IsLocal { get; set; }
}

// Tags
public sealed class TagsResult
{
    public List<Tag> Tags { get; set; } = new();
    public string? Error { get; set; }
}

public sealed class Tag
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string DataType { get; set; } = string.Empty;
    public string ScanRate { get; set; } = string.Empty;
}

public sealed class MachineFoldersResult
{
    public List<MachineFolder> Folders { get; set; } = new();
    public string? Error { get; set; }
}

// Shifts
public sealed class ShiftsResult
{
    public List<Shift> Shifts { get; set; } = new();
    public string? Error { get; set; }
}

public sealed class Shift
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string StartTime { get; set; } = string.Empty;
    public string EndTime { get; set; } = string.Empty;
    public string? DaysOfWeek { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; }
}

public sealed class ShiftRequest
{
    public string Name { get; set; } = string.Empty;
    public string StartTime { get; set; } = string.Empty;
    public string EndTime { get; set; } = string.Empty;
    public string? DaysOfWeek { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; }
}

// Telegram
public sealed class TelegramStatusResult
{
    public bool Enabled { get; set; }
    public string BotToken { get; set; } = string.Empty;
    public string ChatId { get; set; } = string.Empty;
    public string? Error { get; set; }
}

// FTP Export
public sealed class FtpExportResult
{
    public bool Enabled { get; set; }
    public string Name { get; set; } = "Exportacao FTP/SFTP";
    public string Protocol { get; set; } = "SFTP";
    public string Host { get; set; } = string.Empty;
    public string Port { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public bool PasswordConfigured { get; set; }
    public string PrivateKeyPath { get; set; } = string.Empty;
    public string Directory { get; set; } = string.Empty;
    public string Frequency { get; set; } = "manual";
    public string DataType { get; set; } = "production";
    public string FileFormat { get; set; } = "CSV";
    public string? Error { get; set; }
}

public sealed class FtpExportRequest
{
    public string Name { get; set; } = "Exportacao FTP/SFTP";
    public bool Enabled { get; set; } = true;
    public string Protocol { get; set; } = "SFTP";
    public string Host { get; set; } = string.Empty;
    public string Port { get; set; } = "22";
    public string? Username { get; set; }
    public string? Password { get; set; }
    public string? PrivateKeyPath { get; set; }
    public string Directory { get; set; } = "/";
    public string Frequency { get; set; } = "manual";
    public string DataType { get; set; } = "production";
    public string FileFormat { get; set; } = "CSV";
}

// Tag Request
public sealed class TagRequest
{
    public string TagName { get; set; } = string.Empty;
    public string DataType { get; set; } = "Double";
    public string DriverType { get; set; } = "OPCUA";
    public string Address { get; set; } = string.Empty;
    public int OpcUaConnectionId { get; set; }
    public int PollIntervalMs { get; set; } = 1000;
    public bool IsActive { get; set; } = true;
}

// Database Browser Models
public sealed class DatabaseConnection
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Provider { get; set; } = string.Empty;
    public string Host { get; set; } = string.Empty;
    public string Port { get; set; } = string.Empty;
    public string Database { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public bool IsPrimary { get; set; }
    public bool IsLocal { get; set; }
}

public sealed class DatabaseInfo
{
    public string Name { get; set; } = string.Empty;
    public bool IsDefault { get; set; }
}

public sealed class TableInfo
{
    public string Schema { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public int? Rows { get; set; }
}

public sealed class ColumnInfo
{
    public string Name { get; set; } = string.Empty;
    public string DataType { get; set; } = string.Empty;
    public string FullType { get; set; } = string.Empty;
    public bool Nullable { get; set; }
    public string Key { get; set; } = string.Empty;
    public int Ordinal { get; set; }
}

public sealed class DatabaseConnectionsResult
{
    public List<DatabaseConnection> Connections { get; set; } = new();
    public string? Error { get; set; }
}

public sealed class DatabasesResult
{
    public List<DatabaseInfo> Databases { get; set; } = new();
    public string? Error { get; set; }
}

public sealed class TablesResult
{
    public List<TableInfo> Tables { get; set; } = new();
    public string? Error { get; set; }
}

public sealed class ColumnsResult
{
    public List<ColumnInfo> Columns { get; set; } = new();
    public string? Error { get; set; }
}

public sealed class RowsResult
{
    public List<string> Columns { get; set; } = new();
    public List<Dictionary<string, object>> Rows { get; set; } = new();
    public string? Error { get; set; }
}

// Operation Result
public sealed class OperationResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;

    public static OperationResult CreateSuccess(string message) => new() { Success = true, Message = message };
    public static OperationResult CreateFailed(string message) => new() { Success = false, Message = message };
}

// Machine
public sealed class Machine
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string CostCenter { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string FolderId { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
}

// Machine Folder
public sealed class MachineFolder
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int? ParentFolderId { get; set; }
    public bool IsSector { get; set; } = false;
}

// Machine Request
public sealed class MachineRequest
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string CostCenter { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public int? FolderId { get; set; }
    public bool IsActive { get; set; } = true;
}

// Machine Folder Request
public sealed class MachineFolderRequest
{
    public string Name { get; set; } = string.Empty;
    public int? ParentFolderId { get; set; }
    public bool IsSector { get; set; } = false;
}

// Weintek Config
public sealed class WeintekConfig
{
    public string Name { get; set; } = string.Empty;
    public string Gateway { get; set; } = string.Empty;
    public string FhdxIp { get; set; } = string.Empty;
    public string EndpointPath { get; set; } = string.Empty;
    public bool Enabled { get; set; } = true;
    public bool EnforceSourceIp { get; set; } = false;
    public bool GatewayTokenRequired { get; set; } = false;
    public bool TokenConfigured { get; set; } = false;
    public string? TokenPrefix { get; set; }
    public string? TokenCreatedAt { get; set; }
    public string? LastAccessAt { get; set; }
    public string? LastSourceIp { get; set; }
}

// Discovered Tag
public sealed class DiscoveredTag
{
    public string Gateway { get; set; } = string.Empty;
    public string CostCenter { get; set; } = string.Empty;
    public string Machine { get; set; } = string.Empty;
    public string Tag { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public object? Value { get; set; }
    public string DataType { get; set; } = string.Empty;
    public string FirstSeen { get; set; } = string.Empty;
    public string LastSeen { get; set; } = string.Empty;
    public string SourceIp { get; set; } = string.Empty;
    public bool Created { get; set; } = false;
}

// Weintek Browser Result
public sealed class WeintekBrowserResult
{
    public List<DiscoveredTag> Tags { get; set; } = new();
    public string? Error { get; set; }
}

// Alert Item
public sealed class AlertItem
{
    public int Id { get; set; }
    public string AlertType { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? MachineId { get; set; }
    public string? Metadata { get; set; }
    public bool IsAcknowledged { get; set; }
    public string? AcknowledgedBy { get; set; }
    public string? AcknowledgedAt { get; set; }
    public string CreatedAt { get; set; } = string.Empty;
}

// Alert Rule
public sealed class AlertRule
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int TagConfigId { get; set; }
    public string? TagName { get; set; }
    public string Operator { get; set; } = ">";
    public double LimitValue { get; set; }
    public string Severity { get; set; } = "medium";
    public string Message { get; set; } = string.Empty;
    public int? TelegramConnectionId { get; set; }
    public List<int> TelegramRecipientIds { get; set; } = new();
    public bool IsActive { get; set; } = true;
}

// Alert Rule Request
public sealed class AlertRuleRequest
{
    public string Name { get; set; } = string.Empty;
    public int TagConfigId { get; set; }
    public string Operator { get; set; } = ">";
    public double LimitValue { get; set; }
    public string Severity { get; set; } = "medium";
    public string Message { get; set; } = string.Empty;
    public int? TelegramConnectionId { get; set; }
    public List<int> TelegramRecipientIds { get; set; } = new();
    public bool IsActive { get; set; } = true;
}

// Alerts Result
public sealed class AlertsResult
{
    public List<AlertItem> Alerts { get; set; } = new();
    public string? Error { get; set; }
}

// Alert Rules Result
public sealed class AlertRulesResult
{
    public List<AlertRule> Rules { get; set; } = new();
    public string? Error { get; set; }
}

// Telegram Connection
public sealed class TelegramConnection
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool BotTokenConfigured { get; set; }
    public string? BotTokenMasked { get; set; }
    public string? DefaultChatId { get; set; }
    public bool IsActive { get; set; }
    public int CooldownMinutes { get; set; }
    public int Recipients { get; set; }
    public int ActiveRecipients { get; set; }
}

// Telegram Recipient
public sealed class TelegramRecipient
{
    public int Id { get; set; }
    public int ConnectionId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ChatId { get; set; } = string.Empty;
    public string DestinationType { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}

// Telegram Connection Request
public sealed class TelegramConnectionRequest
{
    public string Name { get; set; } = string.Empty;
    public string? BotToken { get; set; }
    public string? DefaultChatId { get; set; }
    public int CooldownMinutes { get; set; } = 15;
    public bool IsActive { get; set; } = true;
}

// Telegram Recipient Request
public sealed class TelegramRecipientRequest
{
    public int ConnectionId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ChatId { get; set; } = string.Empty;
    public string DestinationType { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
}

// Telegram Connections Result
public sealed class TelegramConnectionsResult
{
    public List<TelegramConnection> Connections { get; set; } = new();
    public string? Error { get; set; }
}

// Telegram Recipients Result
public sealed class TelegramRecipientsResult
{
    public List<TelegramRecipient> Recipients { get; set; } = new();
    public string? Error { get; set; }
}

// User
public sealed class User
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = "user";
    public List<string> Permissions { get; set; } = new();
    public bool IsActive { get; set; }
    public bool MfaRequired { get; set; }
    public bool MfaEnabled { get; set; }
}

// User Request
public sealed class UserRequest
{
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Password { get; set; }
    public string Role { get; set; } = "user";
    public List<string> Permissions { get; set; } = new();
    public bool IsActive { get; set; } = true;
    public bool MfaRequired { get; set; } = false;
}

// Users Result
public sealed class UsersResult
{
    public List<User> Users { get; set; } = new();
    public string? Error { get; set; }
}

// Audit Log
public sealed class AuditLog
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public string? EntityType { get; set; }
    public string? EntityId { get; set; }
    public int StatusCode { get; set; }
    public string? IpAddress { get; set; }
    public string CreatedAt { get; set; } = string.Empty;
}

// Audit Logs Result
public sealed class AuditLogsResult
{
    public List<AuditLog> Logs { get; set; } = new();
    public string? Error { get; set; }
}

// Recent Log
public sealed class RecentLog
{
    public string Timestamp { get; set; } = string.Empty;
    public string Level { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? Exception { get; set; }
}

// Recent Logs Result
public sealed class RecentLogsResult
{
    public List<RecentLog> Logs { get; set; } = new();
    public string? Error { get; set; }
}

// Downtime
public sealed class Downtime
{
    public int Id { get; set; }
    public int? DowntimeId { get; set; }
    public string MachineId { get; set; } = string.Empty;
    public string? MachineName { get; set; }
    public string? MachineCode { get; set; }
    public string StartTime { get; set; } = string.Empty;
    public string? EndTime { get; set; }
    public int? DurationSeconds { get; set; }
    public int? StatusOrigin { get; set; }
    public string? StatusOriginDescription { get; set; }
    public bool CanClassify { get; set; }
    public int? ReasonId { get; set; }
    public string? Reason { get; set; }
    public string? Category { get; set; }
    public string? InformedReason { get; set; }
    public string? Observation { get; set; }
    public string? AcknowledgedBy { get; set; }
}

// Downtime Reason
public sealed class DowntimeReason
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? Category { get; set; }
}

// Downtimes Result
public sealed class DowntimesResult
{
    public List<Downtime> Downtimes { get; set; } = new();
    public string? Error { get; set; }
}

// Downtime Reasons Result
public sealed class DowntimeReasonsResult
{
    public List<DowntimeReason> Reasons { get; set; } = new();
    public string? Error { get; set; }
}

// Dashboards
public sealed class DashboardWidget
{
    public string Id { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Metric { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Color { get; set; }
    public int? X { get; set; }
    public int? Y { get; set; }
    public int? W { get; set; }
    public int? H { get; set; }
}

public sealed class DashboardConfig
{
    public int? Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string MachineId { get; set; } = string.Empty;
    public string PeriodPreset { get; set; } = "today";
    public string RefreshInterval { get; set; } = "10";
    public bool IsDefault { get; set; }
    public List<DashboardWidget> Widgets { get; set; } = new();
}

public sealed class DashboardConfigRequest
{
    public int? Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string MachineId { get; set; } = string.Empty;
    public string PeriodPreset { get; set; } = "today";
    public string RefreshInterval { get; set; } = "10";
    public bool IsDefault { get; set; }
    public bool IsActive { get; set; }
    public List<DashboardWidget> Widgets { get; set; } = new();
}

public sealed class DashboardConfigsResult
{
    public List<DashboardConfig> Configs { get; set; } = new();
    public string? Error { get; set; }
}

// Production Diagnostics
public sealed class DiagnosticTag
{
    public string Alias { get; set; } = string.Empty;
    public int TagId { get; set; }
    public string? Name { get; set; }
    public string? Address { get; set; }
    public string? Driver { get; set; }
    public string? PersistenceMode { get; set; }
    public string? Value { get; set; }
    public string? Quality { get; set; }
    public string? SourceTimestamp { get; set; }
    public string? LastPersistedAt { get; set; }
}

public sealed class DiagnosticSnapshot
{
    public string GeneratedAt { get; set; } = string.Empty;
    public string? MachineId { get; set; }
    public Dictionary<string, object> Window { get; set; } = new();
    public List<Machine> Machines { get; set; } = new();
    public List<Dictionary<string, object>> Pipeline { get; set; } = new();
    public Dictionary<string, int> Queues { get; set; } = new();
    public DiagnosticSqlite Sqlite { get; set; } = new();
    public DiagnosticMysql Mysql { get; set; } = new();
}

public sealed class DiagnosticSqlite
{
    public List<DiagnosticTag> Tags { get; set; } = new();
    public List<Dictionary<string, object>> PendingEnvelopes { get; set; } = new();
    public int PendingCount { get; set; }
    public int ProcessedCount { get; set; }
    public int FailedCount { get; set; }
}

public sealed class DiagnosticMysql
{
    public bool Available { get; set; }
    public string? Message { get; set; }
    public DiagnosticTotals? Totals { get; set; }
    public List<Dictionary<string, object>> ProductionEvents { get; set; } = new();
    public List<Dictionary<string, object>> LossEvents { get; set; } = new();
    public List<Dictionary<string, object>> StatusEvents { get; set; } = new();
    public List<Dictionary<string, object>> DowntimeEvents { get; set; } = new();
    public List<Dictionary<string, object>> HourlySummary { get; set; } = new();
}

public sealed class DiagnosticTotals
{
    public double Produced { get; set; }
    public double Losses { get; set; }
    public double Good { get; set; }
    public double QualityPercent { get; set; }
    public int StatusEvents { get; set; }
    public int DowntimeEvents { get; set; }
}

public sealed class DiagnosticResult
{
    public DiagnosticSnapshot? Snapshot { get; set; }
    public string? Error { get; set; }
}

public sealed class SystemTimezoneResult
{
    public string TimeZoneId { get; set; } = "America/Sao_Paulo";
    public string Label { get; set; } = "Brasil - Brasília (GMT-3)";
    public string? Error { get; set; }
}

// Simulator
public sealed class VirtualMachineSummary
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string CostCenter { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}

public sealed class VirtualMachineRuntime
{
    public int MachineId { get; set; }
    public int Status { get; set; }
    public int DowntimeReasonCode { get; set; }
    public int ProductionCounter { get; set; }
    public int LossCounter { get; set; }
    public int PiecesPerMinute { get; set; }
    public bool Running { get; set; }
}

public sealed class VirtualConsole
{
    public Machine Machine { get; set; } = new();
    public Dictionary<string, VirtualTag> Tags { get; set; } = new();
    public List<VirtualReason> Reasons { get; set; } = new();
    public VirtualMachineRuntime Simulator { get; set; } = new();
}

public sealed class VirtualTag
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
}

public sealed class VirtualReason
{
    public int Code { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? Category { get; set; }
}

public sealed class VirtualMachineRequest
{
    public string Name { get; set; } = "Máquina Virtual 1";
    public string Code { get; set; } = "VIRTUAL-001";
    public string CostCenter { get; set; } = "Simulacao";
    public string Location { get; set; } = "Laboratorio";
    public int? FolderId { get; set; }
}

public sealed class VirtualMachinePublishRequest
{
    public int Status { get; set; }
    public int DowntimeReasonCode { get; set; }
    public int ProductionCounter { get; set; }
    public int LossCounter { get; set; }
}

public sealed class VirtualMachineStartRequest
{
    public int PiecesPerMinute { get; set; } = 60;
}

public sealed class VirtualMachinesResult
{
    public List<VirtualMachineSummary> Machines { get; set; } = new();
    public string? Error { get; set; }
}

public sealed class VirtualConsoleResult
{
    public VirtualConsole? Console { get; set; }
    public string? Error { get; set; }
}

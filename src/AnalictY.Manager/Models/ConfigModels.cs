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
    public string Endpoint { get; set; } = string.Empty;
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
    public string Endpoint { get; set; } = string.Empty;
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
    public string Host { get; set; } = string.Empty;
    public string Port { get; set; } = string.Empty;
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
    public string Host { get; set; } = string.Empty;
    public string Port { get; set; } = string.Empty;
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
    public string Type { get; set; } = "MySQL";
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
    public string Type { get; set; } = "MySQL";
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
    public List<string> Folders { get; set; } = new();
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

// Operation Result
public sealed class OperationResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;

    public static OperationResult CreateSuccess(string message) => new() { Success = true, Message = message };
    public static OperationResult CreateFailed(string message) => new() { Success = false, Message = message };
}

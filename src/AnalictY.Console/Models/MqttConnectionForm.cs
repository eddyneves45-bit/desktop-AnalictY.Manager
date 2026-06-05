namespace AnalictY.Console.Models
{
    public class MqttConnectionForm
    {
        public int? Id { get; set; }
        public string Name { get; set; } = "";
        public string ClientId { get; set; } = "";
        public string BrokerHost { get; set; } = "";
        public string BrokerPort { get; set; } = "";
        public string Username { get; set; } = "";
        public string Password { get; set; } = "";
        public bool TlsEnabled { get; set; }
        public string CaCertPath { get; set; } = "";
        public string ClientCertPath { get; set; } = "";
        public string ClientKeyPath { get; set; } = "";
        public string Topics { get; set; } = "";
        public string Qos { get; set; } = "";
        public bool IsActive { get; set; }
    }
}

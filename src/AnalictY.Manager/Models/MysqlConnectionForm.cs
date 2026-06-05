namespace AnalictY.Manager.Models
{
    public class MysqlConnectionForm
    {
        public int? Id { get; set; }
        public string Name { get; set; } = "";
        public string Host { get; set; } = "";
        public string Port { get; set; } = "";
        public string User { get; set; } = "";
        public string Password { get; set; } = "";
        public string Database { get; set; } = "";
        public string PoolSize { get; set; } = "";
        public bool IsActive { get; set; }
    }
}

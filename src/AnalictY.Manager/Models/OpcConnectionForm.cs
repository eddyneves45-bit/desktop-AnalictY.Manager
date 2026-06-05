namespace AnalictY.Manager.Models
{
    public class OpcConnectionForm
    {
        public int? Id { get; set; }
        public string Name { get; set; } = "";
        public string ServerUrl { get; set; } = "";
        public string SecurityPolicy { get; set; } = "";
        public string SecurityMode { get; set; } = "";
        public string Username { get; set; } = "";
        public string Password { get; set; } = "";
        public string CertificatePath { get; set; } = "";
        public string PrivateKeyPath { get; set; } = "";
        public string UpdateInterval { get; set; } = "";
        public bool IsActive { get; set; }
    }
}

namespace AnalictY.Console.Models
{
    public class ConfigCard
    {
        public string Id { get; set; } = "";
        public string Title { get; set; } = "";
        public string Description { get; set; } = "";
        public string Icon { get; set; } = "";
        public string IconPath { get; set; } = "";
        public string Count { get; set; } = "";
        public string Category { get; set; } = "";
        public string ColorHex { get; set; } = "";
        public string HoverColorHex { get; set; } = "";
        public string IconColorHex { get; set; } = "";
        public bool IsButton { get; set; }
        public string NavigationTarget { get; set; } = "";
    }
}

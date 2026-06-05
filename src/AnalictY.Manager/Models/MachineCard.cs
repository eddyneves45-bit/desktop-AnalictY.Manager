using System.Windows.Media;

namespace AnalictY.Manager.Models;

public sealed record MachineCard(
    string Name,
    string Area,
    string Status,
    string CurrentOrder,
    string Efficiency,
    Brush StatusBrush,
    Brush StatusBackground);

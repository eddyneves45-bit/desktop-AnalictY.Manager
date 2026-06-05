using System.Collections.ObjectModel;
using AnalictY.Manager.Infrastructure;

namespace AnalictY.Manager.ViewModels;

public sealed class TagsViewModel : ObservableObject
{
    private string _selectedMachine = "Todas";
    private string _selectedDriver = "Todos";
    private string _searchText = string.Empty;
    private string _totalTags = "156";
    private string _activeTags = "142";
    private string _staleTags = "8";
    private string _errorTags = "6";

    public TagsViewModel()
    {
        Machines = new ObservableCollection<string> { "Todas", "Máquina 01", "Máquina 02", "Prensa Joelho 08", "Célula A" };
        Drivers = new ObservableCollection<string> { "Todos", "OPC UA", "MQTT", "HTTP", "Modbus" };

        TagRows = new ObservableCollection<TagRow>
        {
            new("M01_Producao_Contador", "Máquina 01", "OPC UA", "ns=2;s=Producao.Contador", "1.250", "Good", "2s atrás"),
            new("M01_Status_Maquina", "Máquina 01", "OPC UA", "ns=2;s=Status.Maquina", "1 (Ligado)", "Good", "2s atrás"),
            new("M02_Producao_Contador", "Máquina 02", "MQTT", "fabrica/m02/producao", "980", "Good", "3s atrás"),
            new("PJ08_Ciclo", "Prensa Joelho 08", "HTTP", "/api/weintek/pj08", "56", "Good", "1s atrás"),
            new("CA_Temperatura_Zona1", "Célula A", "Modbus", "40001", "72.5", "Good", "4s atrás"),
            new("M01_Pressao_Sistema", "Máquina 01", "OPC UA", "ns=2;s=Pressao.Sistema", "4.2", "Good", "2s atrás"),
            new("M02_Velocidade_Rotacao", "Máquina 02", "MQTT", "fabrica/m02/velocidade", "1450", "Good", "3s atrás"),
            new("PJ08_Force_Ciclo", "Prensa Joelho 08", "HTTP", "/api/weintek/pj08/force", "85", "Good", "1s atrás")
        };
    }

    public string SelectedMachine
    {
        get => _selectedMachine;
        set => SetProperty(ref _selectedMachine, value);
    }

    public string SelectedDriver
    {
        get => _selectedDriver;
        set => SetProperty(ref _selectedDriver, value);
    }

    public string SearchText
    {
        get => _searchText;
        set => SetProperty(ref _searchText, value);
    }

    public string TotalTags
    {
        get => _totalTags;
        set => SetProperty(ref _totalTags, value);
    }

    public string ActiveTags
    {
        get => _activeTags;
        set => SetProperty(ref _activeTags, value);
    }

    public string StaleTags
    {
        get => _staleTags;
        set => SetProperty(ref _staleTags, value);
    }

    public string ErrorTags
    {
        get => _errorTags;
        set => SetProperty(ref _errorTags, value);
    }

    public ObservableCollection<string> Machines { get; }
    public ObservableCollection<string> Drivers { get; }
    public ObservableCollection<TagRow> TagRows { get; }
}

public sealed record TagRow(string Name, string Machine, string Driver, string Address, string Value, string Quality, string UpdatedAt);

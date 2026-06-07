using System.Collections.ObjectModel;
using System.Net.Http;
using System.Text.Json;
using System.Windows;
using System.Windows.Input;
using AnalictY.Manager.Infrastructure;

namespace AnalictY.Manager.ViewModels;

public sealed class RuntimeViewModel : ObservableObject
{
    private static readonly Uri RuntimeEndpoint = new("http://127.0.0.1:5000/api/runtime/state");
    private readonly HttpClient _httpClient;
    private string _status = "Carregando...";
    private string _uptime = "-";
    private string _version = "-";
    private string _queuedEvents = "-";
    private string _processingRate = "-";
    private string _lastCycle = "-";
    private string _totalCycles = "-";
    private bool _isLoading;

    public RuntimeViewModel(HttpClient httpClient)
    {
        _httpClient = httpClient;
        RefreshCommand = new RelayCommand(async _ => await LoadAsync());
        RestartCommand = new RelayCommand(_ =>
        {
            MessageBox.Show("Reinício do Runtime exigirá confirmação futura e integração com o serviço Windows.", "AnalictY Manager", MessageBoxButton.OK, MessageBoxImage.Information);
            return Task.CompletedTask;
        });

        InternalServices = new ObservableCollection<RuntimeServiceRow>();
        RecentCycles = new ObservableCollection<RuntimeCycleRow>();
        
        _ = LoadAsync();
    }

    public string Status
    {
        get => _status;
        set => SetProperty(ref _status, value);
    }

    public string Uptime
    {
        get => _uptime;
        set => SetProperty(ref _uptime, value);
    }

    public string Version
    {
        get => _version;
        set => SetProperty(ref _version, value);
    }

    public string QueuedEvents
    {
        get => _queuedEvents;
        set => SetProperty(ref _queuedEvents, value);
    }

    public string ProcessingRate
    {
        get => _processingRate;
        set => SetProperty(ref _processingRate, value);
    }

    public string LastCycle
    {
        get => _lastCycle;
        set => SetProperty(ref _lastCycle, value);
    }

    public string TotalCycles
    {
        get => _totalCycles;
        set => SetProperty(ref _totalCycles, value);
    }

    public bool IsLoading
    {
        get => _isLoading;
        set => SetProperty(ref _isLoading, value);
    }

    public ICommand RefreshCommand { get; }
    public ICommand RestartCommand { get; }
    public ObservableCollection<RuntimeServiceRow> InternalServices { get; }
    public ObservableCollection<RuntimeCycleRow> RecentCycles { get; }

    public async Task LoadAsync()
    {
        IsLoading = true;
        try
        {
            using HttpResponseMessage response = await _httpClient.GetAsync(RuntimeEndpoint);
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                Status = "Requer login";
                return;
            }

            if (!response.IsSuccessStatusCode)
            {
                Status = $"Erro: {response.StatusCode}";
                return;
            }

            string content = await response.Content.ReadAsStringAsync();
            using JsonDocument doc = JsonDocument.Parse(content);
            
            Status = "Em execução";
            Uptime = ReadString(doc.RootElement, "uptime") ?? "-";
            Version = ReadString(doc.RootElement, "version") ?? "-";
            
            int tagCount = CountRuntimeItems(doc.RootElement);
            QueuedEvents = tagCount.ToString();
            ProcessingRate = $"{tagCount} tags";
        }
        catch (Exception ex)
        {
            Status = $"Erro: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private static string? ReadString(JsonElement element, params string[] names)
    {
        foreach (string name in names)
        {
            if (element.TryGetProperty(name, out JsonElement value))
            {
                return value.ValueKind == JsonValueKind.String ? value.GetString() : value.ToString();
            }
        }
        return null;
    }

    private static int CountRuntimeItems(JsonElement root)
    {
        if (root.ValueKind == JsonValueKind.Array)
        {
            return root.GetArrayLength();
        }

        if (root.ValueKind != JsonValueKind.Object)
        {
            return 0;
        }

        foreach (string name in new[] { "tags", "states", "data", "items", "results" })
        {
            if (root.TryGetProperty(name, out JsonElement value) && value.ValueKind == JsonValueKind.Array)
            {
                return value.GetArrayLength();
            }
        }

        return root.EnumerateObject().Count();
    }
}

public sealed record RuntimeServiceRow(string Name, string Status, string Rate, string Errors);
public sealed record RuntimeCycleRow(string Time, string Events, string Errors, string Duration);

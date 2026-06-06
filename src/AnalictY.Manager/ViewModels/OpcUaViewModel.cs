using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using AnalictY.Manager.Infrastructure;
using AnalictY.Manager.Models;
using AnalictY.Manager.Services;

namespace AnalictY.Manager.ViewModels;

public sealed class OpcUaViewModel : ObservableObject
{
    private readonly ConfigService _configService;
    private bool _loading = true;
    private string _statusMessage = "Carregando conexões...";

    public OpcUaViewModel(ConfigService configService)
    {
        _configService = configService;
        TestConnectionCommand = new RelayCommand(_ => TestConnectionAsync());
        ReconnectCommand = new RelayCommand(_ => ReconnectAsync());
        BrowseNodesCommand = new RelayCommand(_ => BrowseNodesAsync());

        ServerRows = new ObservableCollection<OpcServerRow>();
        NodeRows = new ObservableCollection<OpcNodeRow>();

        _ = LoadConnectionsAsync();
    }

    public bool Loading
    {
        get => _loading;
        set => SetProperty(ref _loading, value);
    }

    public string StatusMessage
    {
        get => _statusMessage;
        set => SetProperty(ref _statusMessage, value);
    }

    public ICommand TestConnectionCommand { get; }
    public ICommand ReconnectCommand { get; }
    public ICommand BrowseNodesCommand { get; }
    public ObservableCollection<OpcServerRow> ServerRows { get; }
    public ObservableCollection<OpcNodeRow> NodeRows { get; }

    private async Task LoadConnectionsAsync()
    {
        Loading = true;
        StatusMessage = "Carregando conexões...";
        ServerRows.Clear();

        try
        {
            var result = await _configService.GetOpcUaConnectionsAsync();
            if (result.Error != null)
            {
                StatusMessage = $"Erro ao carregar conexões: {result.Error}";
                Loading = false;
                return;
            }

            if (result.Connections.Count == 0)
            {
                StatusMessage = "Nenhuma conexão OPC UA cadastrada no servidor.";
                Loading = false;
                return;
            }

            foreach (var conn in result.Connections)
            {
                ServerRows.Add(new OpcServerRow(
                    conn.Name,
                    conn.Status,
                    conn.Endpoint,
                    conn.Id
                ));
            }

            StatusMessage = $"{result.Connections.Count} conexão(ões) encontrada(s).";
        }
        catch
        {
            StatusMessage = "Erro ao conectar ao servidor.";
        }
        finally
        {
            Loading = false;
        }
    }

    private async Task TestConnectionAsync()
    {
        MessageBox.Show("Teste de conexão será conectado ao servidor OPC UA quando o endpoint estiver disponível.", "AnalictY Manager", MessageBoxButton.OK, MessageBoxImage.Information);
        await Task.CompletedTask;
    }

    private async Task ReconnectAsync()
    {
        MessageBox.Show("Reconexão será conectada ao servidor OPC UA quando o endpoint estiver disponível.", "AnalictY Manager", MessageBoxButton.OK, MessageBoxImage.Information);
        await Task.CompletedTask;
    }

    private async Task BrowseNodesAsync()
    {
        MessageBox.Show("Navegação de nós será conectada ao servidor OPC UA quando o endpoint estiver disponível.", "AnalictY Manager", MessageBoxButton.OK, MessageBoxImage.Information);
        await Task.CompletedTask;
    }
}

public sealed record OpcServerRow(string Name, string Status, string Endpoint, string Id);
public sealed record OpcNodeRow(string NodeId, string DataType, string Value, string Quality, string LastRead);

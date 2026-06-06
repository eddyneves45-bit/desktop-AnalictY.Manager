using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using AnalictY.Manager.Infrastructure;
using AnalictY.Manager.Services;

namespace AnalictY.Manager.ViewModels;

public sealed class OpcUaViewModel : ObservableObject
{
    private readonly ConfigService _configService;
    private bool _loading = true;
    private string _statusMessage = "Carregando conexoes...";
    private OpcServerRow? _selectedServer;

    public OpcUaViewModel(ConfigService configService)
    {
        _configService = configService;
        TestConnectionCommand = new RelayCommand(TestConnectionAsync);
        ReconnectCommand = new RelayCommand(ReconnectAsync);
        BrowseNodesCommand = new RelayCommand(BrowseNodesAsync);

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

    public OpcServerRow? SelectedServer
    {
        get => _selectedServer;
        set => SetProperty(ref _selectedServer, value);
    }

    public ICommand TestConnectionCommand { get; }
    public ICommand ReconnectCommand { get; }
    public ICommand BrowseNodesCommand { get; }
    public ObservableCollection<OpcServerRow> ServerRows { get; }
    public ObservableCollection<OpcNodeRow> NodeRows { get; }

    private async Task LoadConnectionsAsync()
    {
        Loading = true;
        StatusMessage = "Carregando conexoes...";
        ServerRows.Clear();

        try
        {
            var result = await _configService.GetOpcUaConnectionsAsync();
            if (!string.IsNullOrWhiteSpace(result.Error))
            {
                StatusMessage = $"Erro ao carregar conexoes: {result.Error}";
                return;
            }

            foreach (var conn in result.Connections)
            {
                ServerRows.Add(new OpcServerRow(
                    conn.Name,
                    conn.IsActive ? conn.Status : "Desativado",
                    conn.ServerUrl,
                    conn.Id));
            }

            SelectedServer = ServerRows.FirstOrDefault();
            StatusMessage = ServerRows.Count == 0
                ? "Nenhuma conexao OPC UA cadastrada no servidor."
                : $"{ServerRows.Count} conexao(oes) encontrada(s).";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Erro ao conectar ao servidor: {ex.Message}";
        }
        finally
        {
            Loading = false;
        }
    }

    private async Task TestConnectionAsync()
    {
        if (SelectedServer is null)
        {
            MessageBox.Show("Selecione uma conexao OPC UA para testar.", "AnalictY Manager", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var result = await _configService.TestOpcUaEndpointAsync(SelectedServer.Endpoint);
        MessageBox.Show(result.Message, "Teste OPC UA", MessageBoxButton.OK, result.Success ? MessageBoxImage.Information : MessageBoxImage.Warning);
    }

    private async Task ReconnectAsync()
    {
        if (SelectedServer is null)
        {
            MessageBox.Show("Selecione uma conexao OPC UA para reconectar.", "AnalictY Manager", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var result = await _configService.ConnectOpcUaAsync(SelectedServer.Id);
        MessageBox.Show(result.Message, "Reconectar OPC UA", MessageBoxButton.OK, result.Success ? MessageBoxImage.Information : MessageBoxImage.Warning);
    }

    private async Task BrowseNodesAsync()
    {
        if (SelectedServer is null)
        {
            MessageBox.Show("Selecione uma conexao OPC UA para procurar nos.", "AnalictY Manager", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        Loading = true;
        StatusMessage = $"Procurando nos em {SelectedServer.Name}...";
        NodeRows.Clear();

        try
        {
            var result = await _configService.BrowseOpcUaAsync(int.Parse(SelectedServer.Id));
            if (!string.IsNullOrWhiteSpace(result.Error))
            {
                StatusMessage = $"Erro ao procurar nos: {result.Error}";
                return;
            }

            foreach (var node in result.Nodes)
            {
                NodeRows.Add(new OpcNodeRow(
                    node.NodeId,
                    string.IsNullOrWhiteSpace(node.Type) ? "-" : node.Type,
                    string.IsNullOrWhiteSpace(node.Name) ? "-" : node.Name,
                    node.Quality,
                    DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")));
            }

            StatusMessage = $"{NodeRows.Count} no(s) carregado(s) de {SelectedServer.Name}.";
        }
        finally
        {
            Loading = false;
        }
    }
}

public sealed record OpcServerRow(string Name, string Status, string Endpoint, string Id);
public sealed record OpcNodeRow(string NodeId, string DataType, string Value, string Quality, string LastRead);

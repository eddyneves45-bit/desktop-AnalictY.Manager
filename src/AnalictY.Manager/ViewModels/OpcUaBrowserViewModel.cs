using System.Collections.ObjectModel;
using System.Windows.Input;
using AnalictY.Manager.Infrastructure;
using AnalictY.Manager.Services;
using AnalictY.Manager.Models;

namespace AnalictY.Manager.ViewModels;

public sealed class OpcUaBrowserViewModel : ObservableObject
{
    private readonly ConfigService _configService;
    private bool _isLoading;
    private string _statusMessage = "Carregando conexões OPC UA...";
    private string _errorMessage = string.Empty;
    private string _currentNodeId = string.Empty;
    private int? _selectedConnectionId;
    private OpcUaNode? _selectedNode;
    private readonly List<string> _history = new();
    private readonly List<string> _breadcrumb = new();

    public OpcUaBrowserViewModel(ConfigService configService)
    {
        _configService = configService;
        RefreshCommand = new RelayCommand(LoadAsync);
        Connections = new ObservableCollection<OpcUaConnectionDisplay>();
        Nodes = new ObservableCollection<OpcUaNode>();
        Breadcrumb = new ObservableCollection<string>();
    }

    public ObservableCollection<OpcUaConnectionDisplay> Connections { get; }
    public ObservableCollection<OpcUaNode> Nodes { get; }
    public ObservableCollection<string> Breadcrumb { get; }

    public ICommand RefreshCommand { get; }

    public bool IsLoading
    {
        get => _isLoading;
        set => SetProperty(ref _isLoading, value);
    }

    public string StatusMessage
    {
        get => _statusMessage;
        set => SetProperty(ref _statusMessage, value);
    }

    public string ErrorMessage
    {
        get => _errorMessage;
        set
        {
            if (SetProperty(ref _errorMessage, value))
            {
                OnPropertyChanged(nameof(HasError));
            }
        }
    }

    public bool HasError => !string.IsNullOrWhiteSpace(ErrorMessage);

    public string CurrentNodeId
    {
        get => _currentNodeId;
        set => SetProperty(ref _currentNodeId, value);
    }

    public int? SelectedConnectionId
    {
        get => _selectedConnectionId;
        set
        {
            if (SetProperty(ref _selectedConnectionId, value) && value.HasValue)
            {
                GoToRoot();
            }
        }
    }

    public OpcUaNode? SelectedNode
    {
        get => _selectedNode;
        set => SetProperty(ref _selectedNode, value);
    }

    public bool CanGoBack => _history.Count > 0;

    public async Task LoadAsync()
    {
        IsLoading = true;
        ErrorMessage = string.Empty;
        StatusMessage = "Carregando conexões OPC UA...";

        try
        {
            var result = await _configService.GetOpcUaConnectionsAsync();
            if (!string.IsNullOrWhiteSpace(result.Error))
            {
                ErrorMessage = result.Error;
                StatusMessage = "Erro ao carregar conexões.";
                return;
            }

            Connections.Clear();
            foreach (var conn in result.Connections)
            {
                Connections.Add(new OpcUaConnectionDisplay
                {
                    Id = int.Parse(conn.Id),
                    Name = conn.Name,
                    ServerUrl = conn.ServerUrl,
                    DisplayName = $"{conn.Name} ({conn.ServerUrl})"
                });
            }

            if (Connections.Count > 0)
            {
                SelectedConnectionId = Connections[0].Id;
            }

            StatusMessage = Connections.Count == 0
                ? "Nenhuma conexão OPC UA configurada."
                : $"{Connections.Count} conexão(ões) OPC UA carregada(s).";
        }
        finally
        {
            IsLoading = false;
        }
    }

    public async Task LoadNodesAsync()
    {
        if (!_selectedConnectionId.HasValue)
        {
            return;
        }

        IsLoading = true;
        ErrorMessage = string.Empty;
        StatusMessage = "Carregando nós OPC UA...";

        try
        {
            var result = await _configService.BrowseOpcUaAsync(_selectedConnectionId.Value, _currentNodeId);
            if (!string.IsNullOrWhiteSpace(result.Error))
            {
                ErrorMessage = result.Error;
                StatusMessage = "Erro ao carregar nós.";
                return;
            }

            Nodes.Clear();
            foreach (var node in result.Nodes)
            {
                Nodes.Add(node);
            }

            if (Nodes.Count > 0)
            {
                SelectedNode = Nodes[0];
            }

            UpdateBreadcrumb();
            StatusMessage = $"{Nodes.Count} nó(s) encontrado(s).";
        }
        finally
        {
            IsLoading = false;
        }
    }

    public void GoBack()
    {
        if (_history.Count == 0)
        {
            return;
        }

        var previousNode = _history[^1];
        _history.RemoveAt(_history.Count - 1);
        CurrentNodeId = previousNode;
        OnPropertyChanged(nameof(CanGoBack));
        _ = LoadNodesAsync();
    }

    public void GoToRoot()
    {
        _history.Clear();
        CurrentNodeId = string.Empty;
        OnPropertyChanged(nameof(CanGoBack));
        _ = LoadNodesAsync();
    }

    public void OpenNode(string nodeId)
    {
        _history.Add(_currentNodeId);
        CurrentNodeId = nodeId;
        OnPropertyChanged(nameof(CanGoBack));
        _ = LoadNodesAsync();
    }

    public string AutoTagName(OpcUaNode node)
    {
        var displayName = node.Name ?? "NewTag";
        return displayName
            .Replace(" ", "_")
            .Replace("/", "_")
            .Replace("\\", "_")
            .Replace(".", "_")
            .Replace(",", "_")
            .Replace(";", "_")
            .Replace(":", "_")
            .Replace("-", "_")
            .Replace("+", "_")
            .Replace("(", "_")
            .Replace(")", "_")
            .Replace("[", "_")
            .Replace("]", "_")
            .Replace("{", "_")
            .Replace("}", "_")
            .Replace("__", "_")
            .Trim('_');
    }

    public async Task CreateTagAsync(string tagName, string dataType)
    {
        if (string.IsNullOrWhiteSpace(tagName))
        {
            ErrorMessage = "Nome da TAG é obrigatório.";
            return;
        }

        if (SelectedNode == null)
        {
            ErrorMessage = "Selecione um nó para criar a TAG.";
            return;
        }

        if (!_selectedConnectionId.HasValue)
        {
            ErrorMessage = "Selecione uma conexão OPC UA.";
            return;
        }

        var request = new TagRequest
        {
            TagName = tagName,
            DataType = dataType,
            DriverType = "OPCUA",
            Address = SelectedNode.NodeId,
            OpcUaConnectionId = _selectedConnectionId.Value,
            PollIntervalMs = 1000,
            IsActive = true
        };

        var result = await _configService.CreateTagAsync(request);
        if (result.Success)
        {
            StatusMessage = $"TAG '{tagName}' criada com sucesso.";
        }
        else
        {
            ErrorMessage = result.Message ?? "Erro ao criar TAG.";
        }
    }

    private void UpdateBreadcrumb()
    {
        Breadcrumb.Clear();
        var path = new List<string> { _currentNodeId };
        path.AddRange(_history);
        path = path.Distinct().ToList();

        foreach (var item in path)
        {
            Breadcrumb.Add(string.IsNullOrWhiteSpace(item) ? "Root" : item);
        }
    }
}

public sealed record OpcUaConnectionDisplay
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string ServerUrl { get; init; } = string.Empty;
    public string DisplayName { get; init; } = string.Empty;
}

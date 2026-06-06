using System.Collections.ObjectModel;
using System.Windows.Input;
using AnalictY.Manager.Infrastructure;
using AnalictY.Manager.Services;
using AnalictY.Manager.Models;

namespace AnalictY.Manager.ViewModels;

public sealed class DatabaseBrowserViewModel : ObservableObject
{
    private readonly ConfigService _configService;
    private bool _isLoading;
    private string _statusMessage = "Carregando conexões...";
    private string _errorMessage = string.Empty;
    private int? _selectedConnectionId;
    private string _selectedDatabase = string.Empty;
    private TableInfo? _selectedTable;

    // Filters
    private string _search = string.Empty;
    private string _dateFrom = string.Empty;
    private string _dateTo = string.Empty;
    private int _limit = 100;
    private int _offset = 0;

    public DatabaseBrowserViewModel(ConfigService configService)
    {
        _configService = configService;
        RefreshCommand = new RelayCommand(LoadAsync);
        LoadDataCommand = new RelayCommand(async () => await LoadTableDetailsAsync());
        PreviousPageCommand = new RelayCommand(async () => await LoadTableDetailsAsync(Math.Max(_offset - _limit, 0)));
        NextPageCommand = new RelayCommand(async () => await LoadTableDetailsAsync(_offset + _limit));

        Connections = new ObservableCollection<DatabaseConnectionDisplay>();
        Databases = new ObservableCollection<DatabaseInfo>();
        Tables = new ObservableCollection<TableInfo>();
        Columns = new ObservableCollection<ColumnInfo>();
        Rows = new ObservableCollection<Dictionary<string, object>>();
    }

    public ObservableCollection<DatabaseConnectionDisplay> Connections { get; }
    public ObservableCollection<DatabaseInfo> Databases { get; }
    public ObservableCollection<TableInfo> Tables { get; }
    public ObservableCollection<ColumnInfo> Columns { get; }
    public ObservableCollection<Dictionary<string, object>> Rows { get; }

    public ICommand RefreshCommand { get; }
    public ICommand LoadDataCommand { get; }
    public ICommand PreviousPageCommand { get; }
    public ICommand NextPageCommand { get; }

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
        set => SetProperty(ref _errorMessage, value);
    }

    public int? SelectedConnectionId
    {
        get => _selectedConnectionId;
        set
        {
            if (SetProperty(ref _selectedConnectionId, value) && value.HasValue)
            {
                _ = LoadDatabasesAsync(value.Value);
            }
        }
    }

    public string SelectedDatabase
    {
        get => _selectedDatabase;
        set
        {
            if (SetProperty(ref _selectedDatabase, value) && !string.IsNullOrWhiteSpace(value) && _selectedConnectionId.HasValue)
            {
                _ = LoadTablesAsync(_selectedConnectionId.Value, value);
            }
        }
    }

    public TableInfo? SelectedTable
    {
        get => _selectedTable;
        set
        {
            if (SetProperty(ref _selectedTable, value) && value != null)
            {
                _offset = 0;
                _ = LoadTableDetailsAsync();
            }
        }
    }

    public string Search
    {
        get => _search;
        set => SetProperty(ref _search, value);
    }

    public string DateFrom
    {
        get => _dateFrom;
        set => SetProperty(ref _dateFrom, value);
    }

    public string DateTo
    {
        get => _dateTo;
        set => SetProperty(ref _dateTo, value);
    }

    public int Limit
    {
        get => _limit;
        set => SetProperty(ref _limit, value);
    }

    public int Offset
    {
        get => _offset;
        private set => SetProperty(ref _offset, value);
    }

    public bool CanGoPrevious => _offset > 0;
    public bool CanGoNext => Rows.Count >= _limit;

    public async Task LoadAsync()
    {
        IsLoading = true;
        ErrorMessage = string.Empty;
        StatusMessage = "Carregando conexões de banco...";

        try
        {
            var result = await _configService.GetDatabaseConnectionsAsync();
            if (!string.IsNullOrWhiteSpace(result.Error))
            {
                ErrorMessage = result.Error;
                StatusMessage = "Erro ao carregar conexões.";
                return;
            }

            Connections.Clear();
            foreach (var conn in result.Connections)
            {
                Connections.Add(new DatabaseConnectionDisplay
                {
                    Id = conn.Id,
                    Name = conn.Name,
                    Provider = conn.Provider,
                    Host = conn.Host,
                    Port = conn.Port,
                    Database = conn.Database,
                    DisplayName = $"{conn.Name} ({conn.Provider})"
                });
            }

            if (Connections.Count > 0)
            {
                var primary = result.Connections.FirstOrDefault(c => c.IsPrimary) ?? result.Connections[0];
                SelectedConnectionId = primary.Id;
            }

            StatusMessage = Connections.Count == 0
                ? "Nenhuma conexão de banco configurada."
                : $"{Connections.Count} conexão(ões) carregada(s).";
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task LoadDatabasesAsync(int connectionId)
    {
        IsLoading = true;
        ErrorMessage = string.Empty;
        StatusMessage = "Carregando bancos...";

        try
        {
            var result = await _configService.GetDatabasesAsync(connectionId);
            if (!string.IsNullOrWhiteSpace(result.Error))
            {
                ErrorMessage = result.Error;
                StatusMessage = "Erro ao carregar bancos.";
                return;
            }

            Databases.Clear();
            foreach (var db in result.Databases)
            {
                Databases.Add(db);
            }

            if (Databases.Count > 0)
            {
                var defaultDb = result.Databases.FirstOrDefault(d => d.IsDefault) ?? result.Databases[0];
                SelectedDatabase = defaultDb.Name;
            }

            StatusMessage = $"{Databases.Count} banco(s) encontrado(s).";
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task LoadTablesAsync(int connectionId, string database)
    {
        IsLoading = true;
        ErrorMessage = string.Empty;
        StatusMessage = "Carregando tabelas...";

        try
        {
            var result = await _configService.GetTablesAsync(connectionId, database);
            if (!string.IsNullOrWhiteSpace(result.Error))
            {
                ErrorMessage = result.Error;
                StatusMessage = "Erro ao carregar tabelas.";
                return;
            }

            Tables.Clear();
            foreach (var table in result.Tables)
            {
                Tables.Add(table);
            }

            SelectedTable = null;
            Columns.Clear();
            Rows.Clear();

            StatusMessage = $"{Tables.Count} tabela(s) encontrada(s).";
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task LoadTableDetailsAsync(int nextOffset = 0)
    {
        if (!_selectedConnectionId.HasValue || string.IsNullOrWhiteSpace(_selectedDatabase) || _selectedTable == null)
        {
            ErrorMessage = "Selecione conexão, banco e tabela.";
            return;
        }

        IsLoading = true;
        ErrorMessage = string.Empty;
        StatusMessage = "Carregando dados...";

        try
        {
            // Load columns
            var columnsResult = await _configService.GetColumnsAsync(
                _selectedConnectionId.Value,
                _selectedDatabase,
                _selectedTable.Schema,
                _selectedTable.Name);

            if (!string.IsNullOrWhiteSpace(columnsResult.Error))
            {
                ErrorMessage = columnsResult.Error;
                return;
            }

            Columns.Clear();
            foreach (var col in columnsResult.Columns)
            {
                Columns.Add(col);
            }

            // Load rows
            var rowsResult = await _configService.GetRowsAsync(
                _selectedConnectionId.Value,
                _selectedDatabase,
                _selectedTable.Schema,
                _selectedTable.Name,
                _limit,
                nextOffset,
                _search,
                null,
                null,
                null,
                _dateFrom,
                _dateTo);

            if (!string.IsNullOrWhiteSpace(rowsResult.Error))
            {
                ErrorMessage = rowsResult.Error;
                return;
            }

            Rows.Clear();
            foreach (var row in rowsResult.Rows)
            {
                Rows.Add(row);
            }

            Offset = nextOffset;
            OnPropertyChanged(nameof(CanGoPrevious));
            OnPropertyChanged(nameof(CanGoNext));

            StatusMessage = $"{Rows.Count} linha(s) carregada(s). Offset: {Offset}.";
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
        finally
        {
            IsLoading = false;
        }
    }

    public void ExportCsv()
    {
        if (!_selectedConnectionId.HasValue || string.IsNullOrWhiteSpace(_selectedDatabase) || _selectedTable == null)
        {
            ErrorMessage = "Selecione conexão, banco e tabela.";
            return;
        }

        var queryParams = new List<string>
        {
            $"database={Uri.EscapeDataString(_selectedDatabase)}",
            $"schema={Uri.EscapeDataString(_selectedTable.Schema)}",
            $"table={Uri.EscapeDataString(_selectedTable.Name)}"
        };

        if (!string.IsNullOrWhiteSpace(_search))
            queryParams.Add($"q={Uri.EscapeDataString(_search)}");
        if (!string.IsNullOrWhiteSpace(_dateFrom))
            queryParams.Add($"date_from={Uri.EscapeDataString(_dateFrom)}");
        if (!string.IsNullOrWhiteSpace(_dateTo))
            queryParams.Add($"date_to={Uri.EscapeDataString(_dateTo)}");

        var url = $"http://127.0.0.1:5000/api/database-browser/connections/{_selectedConnectionId.Value}/export.csv?{string.Join('&', queryParams)}";
        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(url) { UseShellExecute = true });
    }
}

public sealed record DatabaseConnectionDisplay
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Provider { get; init; } = string.Empty;
    public string Host { get; init; } = string.Empty;
    public string Port { get; init; } = string.Empty;
    public string Database { get; init; } = string.Empty;
    public string DisplayName { get; init; } = string.Empty;
}

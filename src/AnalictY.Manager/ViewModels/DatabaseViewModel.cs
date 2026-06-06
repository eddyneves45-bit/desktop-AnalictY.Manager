using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using AnalictY.Manager.Infrastructure;
using AnalictY.Manager.Models;
using AnalictY.Manager.Services;

namespace AnalictY.Manager.ViewModels;

public sealed class DatabaseViewModel : ObservableObject
{
    private readonly DatabaseService _databaseService;
    private readonly ConfigService _configService;
    private bool _isLoading;
    private bool _isTestingConnection;
    private string _databaseType = "-";
    private string _status = "-";
    private string _path = "-";
    private string _size = "-";
    private string _tables = "-";
    private string _records = "-";
    private string _lastWrite = "-";
    private string _integrity = "-";
    private string _lastBackup = "-";
    private string _backupSize = "-";
    private string _statusMessage = "Clique em Testar Conexao para verificar o banco de dados.";
    private string _errorMessage = string.Empty;

    public DatabaseViewModel(DatabaseService databaseService, ConfigService configService)
    {
        _databaseService = databaseService;
        _configService = configService;
        TestConnectionCommand = new RelayCommand(async () => await TestConnectionAsync());
        LoadTablesCommand = new RelayCommand(async () => await LoadTablesAsync());
        TestMysqlCommand = new RelayCommand(async () => await TestMysqlAsync());
        SetPrimaryCommand = new RelayCommand(async (id) => await SetPrimaryAsync(id?.ToString() ?? string.Empty));
        SetLocalCommand = new RelayCommand(async (id) => await SetLocalAsync(id?.ToString() ?? string.Empty));
        SetRemoteCommand = new RelayCommand(async (id) => await SetRemoteAsync(id?.ToString() ?? string.Empty));
        InitMysqlCommand = new RelayCommand(async (id) => await InitMysqlAsync(id?.ToString() ?? string.Empty));
        BackupNowCommand = new RelayCommand(_ =>
        {
            MessageBox.Show("Backup imediato sera conectado ao endpoint quando disponivel.", "AnalictY Manager", MessageBoxButton.OK, MessageBoxImage.Information);
            return Task.CompletedTask;
        });
        OptimizeCommand = new RelayCommand(_ =>
        {
            MessageBox.Show("Otimizacao do banco sera conectada ao endpoint quando disponivel.", "AnalictY Manager", MessageBoxButton.OK, MessageBoxImage.Information);
            return Task.CompletedTask;
        });

        DatabaseTables = new ObservableCollection<string>();
        BackupRows = new ObservableCollection<BackupRow>();
        MysqlConnections = new ObservableCollection<MysqlConnectionRow>();
    }

    public bool IsLoading { get => _isLoading; set => SetProperty(ref _isLoading, value); }
    public bool IsTestingConnection { get => _isTestingConnection; set => SetProperty(ref _isTestingConnection, value); }
    public string DatabaseType { get => _databaseType; set => SetProperty(ref _databaseType, value); }
    public string Status { get => _status; set => SetProperty(ref _status, value); }
    public string Path { get => _path; set => SetProperty(ref _path, value); }
    public string Size { get => _size; set => SetProperty(ref _size, value); }
    public string Tables { get => _tables; set => SetProperty(ref _tables, value); }
    public string Records { get => _records; set => SetProperty(ref _records, value); }
    public string LastWrite { get => _lastWrite; set => SetProperty(ref _lastWrite, value); }
    public string Integrity { get => _integrity; set => SetProperty(ref _integrity, value); }
    public string LastBackup { get => _lastBackup; set => SetProperty(ref _lastBackup, value); }
    public string BackupSize { get => _backupSize; set => SetProperty(ref _backupSize, value); }
    public string StatusMessage { get => _statusMessage; set => SetProperty(ref _statusMessage, value); }
    public string ErrorMessage { get => _errorMessage; set => SetProperty(ref _errorMessage, value); }
    public ObservableCollection<string> DatabaseTables { get; }
    public ObservableCollection<BackupRow> BackupRows { get; }
    public ObservableCollection<MysqlConnectionRow> MysqlConnections { get; }
    public ICommand TestConnectionCommand { get; }
    public ICommand LoadTablesCommand { get; }
    public ICommand TestMysqlCommand { get; }
    public ICommand SetPrimaryCommand { get; }
    public ICommand SetLocalCommand { get; }
    public ICommand SetRemoteCommand { get; }
    public ICommand InitMysqlCommand { get; }
    public ICommand BackupNowCommand { get; }
    public ICommand OptimizeCommand { get; }

    public async Task LoadAsync()
    {
        await TestConnectionAsync();
        await LoadTablesAsync();
        await LoadMysqlConnectionsAsync();
    }

    private async Task TestConnectionAsync()
    {
        IsTestingConnection = true;
        StatusMessage = "Testando conexao...";
        ErrorMessage = string.Empty;

        try
        {
            var dbStatus = await _databaseService.GetStatusAsync();
            if (!string.IsNullOrEmpty(dbStatus.Error))
            {
                ErrorMessage = dbStatus.Error;
                StatusMessage = "Nao foi possivel conectar ao banco de dados.";
                DatabaseType = "Desconhecido";
                Status = "Offline";
                Path = "Desconhecido";
                return;
            }

            DatabaseType = dbStatus.Type;
            Status = dbStatus.Status;
            Path = dbStatus.Path;
            Size = string.IsNullOrWhiteSpace(dbStatus.Size) ? "-" : dbStatus.Size;
            Tables = string.IsNullOrWhiteSpace(dbStatus.Tables) ? "-" : dbStatus.Tables;
            Records = string.IsNullOrWhiteSpace(dbStatus.Records) ? "-" : dbStatus.Records;
            LastWrite = string.IsNullOrWhiteSpace(dbStatus.LastWrite) ? "-" : dbStatus.LastWrite;
            Integrity = Status.Equals("Conectado", StringComparison.OrdinalIgnoreCase) ? "OK" : "-";
            StatusMessage = "Conexao estabelecida com sucesso.";
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            StatusMessage = "Erro ao testar conexao.";
        }
        finally
        {
            IsTestingConnection = false;
        }
    }

    private async Task LoadTablesAsync()
    {
        IsLoading = true;
        StatusMessage = "Carregando conexoes de banco...";
        ErrorMessage = string.Empty;
        DatabaseTables.Clear();

        try
        {
            var result = await _databaseService.GetTablesAsync();
            if (!string.IsNullOrEmpty(result.Error))
            {
                ErrorMessage = result.Error;
                StatusMessage = "Nao foi possivel carregar as conexoes.";
                return;
            }

            foreach (var table in result.Tables)
            {
                DatabaseTables.Add(table);
            }

            StatusMessage = $"{result.Tables.Count} conexoes carregadas.";
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            StatusMessage = "Erro ao carregar conexoes.";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task LoadMysqlConnectionsAsync()
    {
        IsLoading = true;
        ErrorMessage = string.Empty;
        MysqlConnections.Clear();

        try
        {
            var result = await _configService.GetMysqlConnectionsAsync();
            if (!string.IsNullOrEmpty(result.Error))
            {
                ErrorMessage = result.Error;
                return;
            }

            foreach (var connection in result.Connections)
            {
                MysqlConnections.Add(new MysqlConnectionRow
                {
                    Id = connection.Id,
                    Name = connection.Name,
                    Host = connection.Host,
                    Port = connection.Port,
                    Database = connection.Database,
                    Type = connection.Type,
                    IsPrimary = connection.IsPrimary ? "Sim" : "Não",
                    IsLocal = connection.IsLocal ? "Sim" : "Não",
                    IsRemote = connection.IsRemote ? "Sim" : "Não",
                    Status = connection.Status
                });
            }
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

    private async Task TestMysqlAsync()
    {
        try
        {
            var result = await _configService.TestMysqlAsync();
            if (result.Success)
            {
                MessageBox.Show(result.Message, "AnalictY Manager", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show(result.Message, "AnalictY Manager", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erro: {ex.Message}", "AnalictY Manager", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async Task SetPrimaryAsync(string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            MessageBox.Show("ID da conexão não informado.", "AnalictY Manager", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        try
        {
            var result = await _configService.SetMysqlPrimaryAsync(id);
            if (result.Success)
            {
                MessageBox.Show(result.Message, "AnalictY Manager", MessageBoxButton.OK, MessageBoxImage.Information);
                await LoadMysqlConnectionsAsync();
            }
            else
            {
                MessageBox.Show(result.Message, "AnalictY Manager", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erro: {ex.Message}", "AnalictY Manager", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async Task SetLocalAsync(string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            MessageBox.Show("ID da conexão não informado.", "AnalictY Manager", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        try
        {
            var result = await _configService.SetMysqlLocalAsync(id);
            if (result.Success)
            {
                MessageBox.Show(result.Message, "AnalictY Manager", MessageBoxButton.OK, MessageBoxImage.Information);
                await LoadMysqlConnectionsAsync();
            }
            else
            {
                MessageBox.Show(result.Message, "AnalictY Manager", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erro: {ex.Message}", "AnalictY Manager", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async Task SetRemoteAsync(string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            MessageBox.Show("ID da conexão não informado.", "AnalictY Manager", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        try
        {
            var result = await _configService.SetMysqlRemoteAsync(id);
            if (result.Success)
            {
                MessageBox.Show(result.Message, "AnalictY Manager", MessageBoxButton.OK, MessageBoxImage.Information);
                await LoadMysqlConnectionsAsync();
            }
            else
            {
                MessageBox.Show(result.Message, "AnalictY Manager", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erro: {ex.Message}", "AnalictY Manager", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async Task InitMysqlAsync(string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            MessageBox.Show("ID da conexão não informado.", "AnalictY Manager", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        try
        {
            var result = await _configService.InitMysqlAsync(id);
            if (result.Success)
            {
                MessageBox.Show(result.Message, "AnalictY Manager", MessageBoxButton.OK, MessageBoxImage.Information);
                await LoadMysqlConnectionsAsync();
            }
            else
            {
                MessageBox.Show(result.Message, "AnalictY Manager", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erro: {ex.Message}", "AnalictY Manager", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}

public sealed class MysqlConnectionRow
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Host { get; set; } = string.Empty;
    public string Port { get; set; } = string.Empty;
    public string Database { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string IsPrimary { get; set; } = "Não";
    public string IsLocal { get; set; } = "Não";
    public string IsRemote { get; set; } = "Não";
    public string Status { get; set; } = "Desconhecido";
}

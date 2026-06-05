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
    private string _statusMessage = "Clique em Testar Conexão para verificar o banco de dados.";
    private string _errorMessage = string.Empty;

    public DatabaseViewModel(DatabaseService databaseService)
    {
        _databaseService = databaseService;
        TestConnectionCommand = new RelayCommand(async () => await TestConnectionAsync());
        LoadTablesCommand = new RelayCommand(async () => await LoadTablesAsync());
        BackupNowCommand = new RelayCommand(_ =>
        {
            MessageBox.Show("Backup imediato será conectado ao endpoint quando disponível.", "AnalictY Manager", MessageBoxButton.OK, MessageBoxImage.Information);
            return Task.CompletedTask;
        });
        OptimizeCommand = new RelayCommand(_ =>
        {
            MessageBox.Show("Otimização do banco será conectada ao endpoint quando disponível.", "AnalictY Manager", MessageBoxButton.OK, MessageBoxImage.Information);
            return Task.CompletedTask;
        });
        DatabaseTables = new ObservableCollection<string>();
        BackupRows = new ObservableCollection<BackupRow>
        {
            new("analicty_backup_20250605_0200.db", "42.8 MB", "Hoje 02:00", "Completo"),
            new("analicty_backup_20250604_0200.db", "42.5 MB", "Ontem 02:00", "Completo"),
            new("analicty_backup_20250603_0200.db", "42.3 MB", "03/06 02:00", "Completo"),
            new("analicty_backup_20250602_0200.db", "42.1 MB", "02/06 02:00", "Completo"),
            new("analicty_backup_20250601_0200.db", "41.9 MB", "01/06 02:00", "Completo")
        };
    }

    public bool IsLoading
    {
        get => _isLoading;
        set => SetProperty(ref _isLoading, value);
    }

    public bool IsTestingConnection
    {
        get => _isTestingConnection;
        set => SetProperty(ref _isTestingConnection, value);
    }

    public string DatabaseType
    {
        get => _databaseType;
        set => SetProperty(ref _databaseType, value);
    }

    public string Status
    {
        get => _status;
        set => SetProperty(ref _status, value);
    }

    public string Path
    {
        get => _path;
        set => SetProperty(ref _path, value);
    }

    public string Size
    {
        get => _size;
        set => SetProperty(ref _size, value);
    }

    public string Tables
    {
        get => _tables;
        set => SetProperty(ref _tables, value);
    }

    public string Records
    {
        get => _records;
        set => SetProperty(ref _records, value);
    }

    public string LastWrite
    {
        get => _lastWrite;
        set => SetProperty(ref _lastWrite, value);
    }

    public string Integrity
    {
        get => _integrity;
        set => SetProperty(ref _integrity, value);
    }

    public string LastBackup
    {
        get => _lastBackup;
        set => SetProperty(ref _lastBackup, value);
    }

    public string BackupSize
    {
        get => _backupSize;
        set => SetProperty(ref _backupSize, value);
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

    public ObservableCollection<string> DatabaseTables { get; }
    public ObservableCollection<BackupRow> BackupRows { get; }

    public ICommand TestConnectionCommand { get; }
    public ICommand LoadTablesCommand { get; }
    public ICommand BackupNowCommand { get; }
    public ICommand OptimizeCommand { get; }

    public async Task LoadAsync()
    {
        await TestConnectionAsync();
    }

    private async Task TestConnectionAsync()
    {
        IsTestingConnection = true;
        StatusMessage = "Testando conexão...";
        ErrorMessage = string.Empty;
        DatabaseTables.Clear();

        try
        {
            var dbStatus = await _databaseService.GetStatusAsync();

            if (!string.IsNullOrEmpty(dbStatus.Error))
            {
                ErrorMessage = dbStatus.Error;
                StatusMessage = "Não foi possível conectar ao banco de dados. Verifique se o AnalictY Server está em execução.";
                DatabaseType = "Desconhecido";
                Status = "Offline";
                Path = "Desconhecido";
            }
            else
            {
                DatabaseType = dbStatus.Type;
                Status = dbStatus.Status;
                Path = dbStatus.Path;
                StatusMessage = "Conexão estabelecida com sucesso.";
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            StatusMessage = "Erro ao testar conexão.";
        }
        finally
        {
            IsTestingConnection = false;
        }
    }

    private async Task LoadTablesAsync()
    {
        IsLoading = true;
        StatusMessage = "Carregando tabelas...";
        ErrorMessage = string.Empty;
        DatabaseTables.Clear();

        try
        {
            var tablesResult = await _databaseService.GetTablesAsync();

            if (!string.IsNullOrEmpty(tablesResult.Error))
            {
                ErrorMessage = tablesResult.Error;
                StatusMessage = "Não foi possível carregar as tabelas.";
            }
            else
            {
                foreach (var table in tablesResult.Tables)
                {
                    DatabaseTables.Add(table);
                }
                StatusMessage = $"{tablesResult.Tables.Count} tabelas carregadas.";
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            StatusMessage = "Erro ao carregar tabelas.";
        }
        finally
        {
            IsLoading = false;
        }
    }
}

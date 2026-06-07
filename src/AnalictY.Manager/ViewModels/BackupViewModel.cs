using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using AnalictY.Manager.Infrastructure;
using AnalictY.Manager.Services;

namespace AnalictY.Manager.ViewModels;

public sealed class BackupViewModel : ObservableObject
{
    private readonly BackupService _backupService;
    private readonly AuthService _authService;
    private bool _isLoading;
    private string _statusMessage = "Carregando backups...";
    private string _errorMessage = string.Empty;
    private string _lastBackupStatus = "-";
    private string _lastBackupDate = "-";
    private string _destination = "-";
    private string _lastBackupSize = "-";
    private string _schedule = "Não configurado";

    public BackupViewModel(BackupService backupService, AuthService authService)
    {
        _backupService = backupService;
        _authService = authService;
        
        BackupNowCommand = new RelayCommand(async _ => await CreateBackupAsync());
        RestoreCommand = new RelayCommand(async param => 
        {
            if (param is string backupId)
            {
                var result = MessageBox.Show(
                    $"RESTAURAÇÃO REQUER CONFIRMAÇÃO: Esta ação substituirá o banco de dados atual com o backup '{backupId}'. Deseja continuar?",
                    "AnalictY Manager - AVISO",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);
                
                if (result == MessageBoxResult.Yes)
                {
                    await RestoreBackupAsync(backupId);
                }
            }
        });
        OpenFolderCommand = new RelayCommand(_ =>
        {
            MessageBox.Show("Abertura da pasta de backup não implementada nesta versão.", "AnalictY Manager", MessageBoxButton.OK, MessageBoxImage.Information);
            return Task.CompletedTask;
        });

        BackupRows = new ObservableCollection<BackupRow>();
        
        _ = LoadAsync();
    }

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

    public string LastBackupStatus
    {
        get => _lastBackupStatus;
        set => SetProperty(ref _lastBackupStatus, value);
    }

    public string LastBackupDate
    {
        get => _lastBackupDate;
        set => SetProperty(ref _lastBackupDate, value);
    }

    public string Destination
    {
        get => _destination;
        set => SetProperty(ref _destination, value);
    }

    public string LastBackupSize
    {
        get => _lastBackupSize;
        set => SetProperty(ref _lastBackupSize, value);
    }

    public string Schedule
    {
        get => _schedule;
        set => SetProperty(ref _schedule, value);
    }

    public ICommand BackupNowCommand { get; }
    public ICommand RestoreCommand { get; }
    public ICommand OpenFolderCommand { get; }
    public ObservableCollection<BackupRow> BackupRows { get; }

    public async Task LoadAsync()
    {
        IsLoading = true;
        ErrorMessage = string.Empty;
        BackupRows.Clear();

        try
        {
            var result = await _backupService.GetBackupsAsync();
            
            if (!string.IsNullOrEmpty(result.Error))
            {
                ErrorMessage = result.Error;
                StatusMessage = "Erro ao carregar";
                return;
            }

            foreach (var backup in result.Backups)
            {
                BackupRows.Add(new BackupRow(
                    backup.FileName,
                    backup.Size,
                    FormatDate(backup.CreatedAt),
                    "Completo"
                ));
            }

            if (BackupRows.Count > 0)
            {
                LastBackupStatus = "Completo";
                LastBackupDate = FormatDate(BackupRows[0].Date);
                LastBackupSize = BackupRows[0].Size;
            }

            StatusMessage = $"Carregado {BackupRows.Count} backups";
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Erro ao carregar backups: {ex.Message}";
            StatusMessage = "Erro ao carregar";
        }
        finally
        {
            IsLoading = false;
        }
    }

    public async Task CreateBackupAsync()
    {
        if (_authService.CurrentSession == null)
        {
            MessageBox.Show("Faça login como administrador para executar esta ação.", "AnalictY Manager - Autenticação Necessária", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        IsLoading = true;
        StatusMessage = "Criando backup...";

        try
        {
            var result = await _backupService.CreateBackupAsync();
            
            if (result.Success)
            {
                MessageBox.Show($"Backup criado com sucesso: {result.BackupId}", "AnalictY Manager", MessageBoxButton.OK, MessageBoxImage.Information);
                await LoadAsync();
            }
            else
            {
                MessageBox.Show($"Erro ao criar backup: {result.Message}", "AnalictY Manager - Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erro ao criar backup: {ex.Message}", "AnalictY Manager - Erro", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsLoading = false;
        }
    }

    public async Task RestoreBackupAsync(string backupId)
    {
        if (_authService.CurrentSession == null)
        {
            MessageBox.Show("Faça login como administrador para executar esta ação.", "AnalictY Manager - Autenticação Necessária", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        IsLoading = true;
        StatusMessage = "Restaurando backup...";

        try
        {
            var result = await _backupService.RestoreBackupAsync(backupId);
            
            if (result.Success)
            {
                MessageBox.Show($"Backup restaurado com sucesso. O servidor pode precisar ser reiniciado.", "AnalictY Manager", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show($"Erro ao restaurar backup: {result.Message}", "AnalictY Manager - Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erro ao restaurar backup: {ex.Message}", "AnalictY Manager - Erro", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsLoading = false;
        }
    }

    private static string FormatDate(string isoDate)
    {
        if (DateTime.TryParse(isoDate, out var date))
        {
            return date.ToString("dd/MM/yyyy HH:mm");
        }
        return isoDate;
    }
}

public sealed record BackupRow(string FileName, string Size, string Date, string Status);

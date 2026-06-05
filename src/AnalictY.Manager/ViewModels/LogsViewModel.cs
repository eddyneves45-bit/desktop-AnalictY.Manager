using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Input;
using AnalictY.Manager.Infrastructure;
using AnalictY.Manager.Models;
using AnalictY.Manager.Services;

namespace AnalictY.Manager.ViewModels;

public sealed class LogsViewModel : ObservableObject
{
    private readonly LogsService _logsService;
    private bool _isLoading;
    private string _selectedLevel = "Todos";
    private string _searchText = string.Empty;
    private string _statusMessage = "Clique em Carregar para visualizar os logs.";
    private string _errorMessage = string.Empty;
    private string _totalLogs = "0";
    private string _errorLogs = "0";
    private string _warningLogs = "0";

    public LogsViewModel(LogsService logsService)
    {
        _logsService = logsService;
        LoadCommand = new RelayCommand(async () => await LoadLogsAsync());
        OpenLogsFolderCommand = new RelayCommand(_ => { OpenLogsFolder(); return Task.CompletedTask; });
        LogLevels = new ObservableCollection<string> { "Todos", "Debug", "Info", "Warning", "Error", "Critical" };
        LogEntries = new ObservableCollection<LogEntry>();
    }

    public bool IsLoading
    {
        get => _isLoading;
        set => SetProperty(ref _isLoading, value);
    }

    public string SelectedLevel
    {
        get => _selectedLevel;
        set
        {
            if (SetProperty(ref _selectedLevel, value))
            {
                _ = LoadLogsAsync();
            }
        }
    }

    public string SearchText
    {
        get => _searchText;
        set => SetProperty(ref _searchText, value);
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

    public string TotalLogs
    {
        get => _totalLogs;
        set => SetProperty(ref _totalLogs, value);
    }

    public string ErrorLogs
    {
        get => _errorLogs;
        set => SetProperty(ref _errorLogs, value);
    }

    public string WarningLogs
    {
        get => _warningLogs;
        set => SetProperty(ref _warningLogs, value);
    }

    public ObservableCollection<string> LogLevels { get; }
    public ObservableCollection<LogEntry> LogEntries { get; }

    public ICommand LoadCommand { get; }
    public ICommand OpenLogsFolderCommand { get; }

    public async Task LoadAsync()
    {
        await LoadLogsAsync();
    }

    private async Task LoadLogsAsync()
    {
        IsLoading = true;
        StatusMessage = "Carregando logs...";
        ErrorMessage = string.Empty;
        LogEntries.Clear();

        try
        {
            var levelFilter = SelectedLevel == "Todos" ? null : SelectedLevel;
            var result = await _logsService.GetRecentLogsAsync(100, levelFilter);

            if (!string.IsNullOrEmpty(result.Error))
            {
                ErrorMessage = result.Error;
                StatusMessage = "Não foi possível carregar os logs. Verifique se o AnalictY Server está em execução.";
            }
            else
            {
                foreach (var entry in result.Entries)
                {
                    LogEntries.Add(entry);
                }
                TotalLogs = result.Entries.Count.ToString();
                ErrorLogs = result.Entries.Count(e => e.Level == "Error" || e.Level == "Critical").ToString();
                WarningLogs = result.Entries.Count(e => e.Level == "Warning").ToString();
                StatusMessage = $"{result.Entries.Count} logs carregados.";
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            StatusMessage = "Erro ao carregar logs.";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void OpenLogsFolder()
    {
        try
        {
            var logsPath = Path.Combine(AppContext.BaseDirectory, "logs");
            if (Directory.Exists(logsPath))
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = logsPath,
                    UseShellExecute = true
                });
            }
            else
            {
                MessageBox.Show("Pasta de logs não encontrada.", "Informação", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Não foi possível abrir a pasta de logs: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}

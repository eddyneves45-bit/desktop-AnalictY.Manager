using System.Windows.Input;
using AnalictY.Manager.Infrastructure;
using AnalictY.Manager.Models;
using AnalictY.Manager.Services;

namespace AnalictY.Manager.ViewModels;

public sealed class UpdatesViewModel : ObservableObject
{
    private readonly UpdatesService _updatesService;
    private readonly VersionService _versionService;
    private bool _isLoading;
    private bool _isChecking;
    private string _currentVersion = "-";
    private string _channel = "-";
    private string _latestVersion = "-";
    private string _statusMessage = "Clique em Verificar Atualizações para buscar novas versões.";
    private string _releaseNotes = string.Empty;
    private bool _hasUpdate;

    public UpdatesViewModel(UpdatesService updatesService, VersionService versionService)
    {
        _updatesService = updatesService;
        _versionService = versionService;
        CheckUpdatesCommand = new RelayCommand(async () => await CheckUpdatesAsync());
    }

    public bool IsLoading
    {
        get => _isLoading;
        set => SetProperty(ref _isLoading, value);
    }

    public bool IsChecking
    {
        get => _isChecking;
        set => SetProperty(ref _isChecking, value);
    }

    public string CurrentVersion
    {
        get => _currentVersion;
        set => SetProperty(ref _currentVersion, value);
    }

    public string Channel
    {
        get => _channel;
        set => SetProperty(ref _channel, value);
    }

    public string LatestVersion
    {
        get => _latestVersion;
        set => SetProperty(ref _latestVersion, value);
    }

    public string StatusMessage
    {
        get => _statusMessage;
        set => SetProperty(ref _statusMessage, value);
    }

    public string ReleaseNotes
    {
        get => _releaseNotes;
        set => SetProperty(ref _releaseNotes, value);
    }

    public bool HasUpdate
    {
        get => _hasUpdate;
        set => SetProperty(ref _hasUpdate, value);
    }

    public ICommand CheckUpdatesCommand { get; }

    public async Task LoadAsync()
    {
        IsLoading = true;
        try
        {
            var version = await _versionService.GetVersionAsync();
            CurrentVersion = version.Version;
            Channel = version.Channel;
        }
        catch (Exception ex)
        {
            StatusMessage = $"Erro ao carregar versão: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task CheckUpdatesAsync()
    {
        IsChecking = true;
        StatusMessage = "Verificando atualizações...";
        ReleaseNotes = string.Empty;
        HasUpdate = false;

        try
        {
            var result = await _updatesService.CheckForUpdatesAsync();

            if (!string.IsNullOrEmpty(result.Error))
            {
                StatusMessage = $"Erro: {result.Error}";
            }
            else
            {
                LatestVersion = result.LatestVersion;
                HasUpdate = result.HasUpdate;
                ReleaseNotes = result.ReleaseNotes ?? "Notas de lançamento não disponíveis.";

                if (result.HasUpdate)
                {
                    StatusMessage = "Nova versão disponível!";
                }
                else
                {
                    StatusMessage = "Você já está na versão mais recente.";
                }
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Erro ao verificar atualizações: {ex.Message}";
        }
        finally
        {
            IsChecking = false;
        }
    }
}

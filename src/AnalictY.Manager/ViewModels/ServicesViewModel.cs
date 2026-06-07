using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using AnalictY.Manager.Infrastructure;
using AnalictY.Manager.Services;

namespace AnalictY.Manager.ViewModels;

public sealed class ServicesViewModel : ObservableObject
{
    private readonly AdminApiService _adminApiService;
    private bool _isLoading;
    private string _statusMessage = "Carregando serviços...";
    private string _errorMessage = string.Empty;

    public ServicesViewModel(AdminApiService adminApiService)
    {
        _adminApiService = adminApiService;
        
        StartCommand = new RelayCommand(_ =>
        {
            MessageBox.Show("Início de serviço exigirá confirmação futura e integração com o serviço Windows.", "AnalictY Manager", MessageBoxButton.OK, MessageBoxImage.Information);
            return Task.CompletedTask;
        });
        StopCommand = new RelayCommand(_ =>
        {
            MessageBox.Show("Parada de serviço exigirá confirmação futura e integração com o serviço Windows.", "AnalictY Manager", MessageBoxButton.OK, MessageBoxImage.Information);
            return Task.CompletedTask;
        });
        RestartCommand = new RelayCommand(_ =>
        {
            MessageBox.Show("Reinício de serviço exigirá confirmação futura e integração com o serviço Windows.", "AnalictY Manager", MessageBoxButton.OK, MessageBoxImage.Information);
            return Task.CompletedTask;
        });

        ServiceRows = new ObservableCollection<ServiceRow>();
        
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

    public ICommand StartCommand { get; }
    public ICommand StopCommand { get; }
    public ICommand RestartCommand { get; }
    public ObservableCollection<ServiceRow> ServiceRows { get; }

    public async Task LoadAsync()
    {
        IsLoading = true;
        ErrorMessage = string.Empty;
        ServiceRows.Clear();

        try
        {
            var services = await _adminApiService.GetServicesAsync();
            
            foreach (var service in services)
            {
                ServiceRows.Add(new ServiceRow(
                    service.Name,
                    service.Status,
                    service.Uptime,
                    "-",
                    "-",
                    service.Status
                ));
            }

            StatusMessage = $"Carregado {ServiceRows.Count} serviços";
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Erro ao carregar serviços: {ex.Message}";
            StatusMessage = "Erro ao carregar";
        }
        finally
        {
            IsLoading = false;
        }
    }
}

public sealed record ServiceRow(string Name, string Status, string Uptime, string Port, string Pid, string State);

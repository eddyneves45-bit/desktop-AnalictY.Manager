using System.Collections.ObjectModel;
using System.Windows.Input;
using AnalictY.Manager.Infrastructure;
using AnalictY.Manager.Services;
using AnalictY.Manager.Models;

namespace AnalictY.Manager.ViewModels;

public sealed class DowntimeReasonsViewModel : ObservableObject
{
    private readonly ConfigService _configService;
    private bool _isLoading;
    private string _statusMessage = "Carregando paradas...";
    private string _errorMessage = string.Empty;
    private Downtime? _selectedDowntime;
    private string _filterMachineId = string.Empty;
    private string _filterFromDate = string.Empty;
    private string _filterToDate = string.Empty;
    private int _retentionDays = 1;

    // Classification form fields
    private int? _selectedReasonId;
    private string _informedReason = string.Empty;
    private string _observation = string.Empty;

    public DowntimeReasonsViewModel(ConfigService configService)
    {
        _configService = configService;
        RefreshCommand = new RelayCommand(Refresh);
        ApplyFiltersCommand = new RelayCommand(ApplyFilters);
        ClearFiltersCommand = new RelayCommand(ClearFilters);
        ClassifyCommand = new RelayCommand(Classify);
        SaveRetentionCommand = new RelayCommand(SaveRetention);

        Downtimes = new ObservableCollection<Downtime>();
        Reasons = new ObservableCollection<DowntimeReason>();
    }

    public ObservableCollection<Downtime> Downtimes { get; }
    public ObservableCollection<DowntimeReason> Reasons { get; }

    public ICommand RefreshCommand { get; }
    public ICommand ApplyFiltersCommand { get; }
    public ICommand ClearFiltersCommand { get; }
    public ICommand ClassifyCommand { get; }
    public ICommand SaveRetentionCommand { get; }

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

    public Downtime? SelectedDowntime
    {
        get => _selectedDowntime;
        set => SetProperty(ref _selectedDowntime, value);
    }

    public string FilterMachineId
    {
        get => _filterMachineId;
        set => SetProperty(ref _filterMachineId, value);
    }

    public string FilterFromDate
    {
        get => _filterFromDate;
        set => SetProperty(ref _filterFromDate, value);
    }

    public string FilterToDate
    {
        get => _filterToDate;
        set => SetProperty(ref _filterToDate, value);
    }

    public int RetentionDays
    {
        get => _retentionDays;
        set => SetProperty(ref _retentionDays, value);
    }

    public int? SelectedReasonId
    {
        get => _selectedReasonId;
        set => SetProperty(ref _selectedReasonId, value);
    }

    public string InformedReason
    {
        get => _informedReason;
        set => SetProperty(ref _informedReason, value);
    }

    public string Observation
    {
        get => _observation;
        set => SetProperty(ref _observation, value);
    }

    public int OpenCount => Downtimes.Count(d => d.CanClassify && string.IsNullOrWhiteSpace(d.EndTime));

    private async Task OnRefreshAsync()
    {
        await LoadAsync();
    }

    public Task Refresh() => OnRefreshAsync();

    public async Task LoadAsync()
    {
        IsLoading = true;
        ErrorMessage = string.Empty;
        StatusMessage = "Carregando paradas...";

        try
        {
            var downtimesResult = await _configService.GetDowntimesAsync(
                string.IsNullOrWhiteSpace(FilterMachineId) ? null : FilterMachineId,
                string.IsNullOrWhiteSpace(FilterFromDate) ? null : FilterFromDate,
                string.IsNullOrWhiteSpace(FilterToDate) ? null : FilterToDate,
                200);

            if (!string.IsNullOrWhiteSpace(downtimesResult.Error))
            {
                ErrorMessage = downtimesResult.Error;
                return;
            }

            Downtimes.Clear();
            foreach (var downtime in downtimesResult.Downtimes)
            {
                Downtimes.Add(downtime);
            }

            var reasonsResult = await _configService.GetDowntimeReasonsCatalogAsync();
            if (!string.IsNullOrWhiteSpace(reasonsResult.Error))
            {
                ErrorMessage = reasonsResult.Error;
                return;
            }

            Reasons.Clear();
            foreach (var reason in reasonsResult.Reasons)
            {
                Reasons.Add(reason);
            }

            StatusMessage = $"{Downtimes.Count} parada(s) carregada(s). {OpenCount} parada(s) aberta(s) aguardando classificação.";
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

    public Task ApplyFilters()
    {
        return LoadAsync();
    }

    public Task ClearFilters()
    {
        FilterMachineId = string.Empty;
        FilterFromDate = string.Empty;
        FilterToDate = string.Empty;
        return LoadAsync();
    }

    public Task OpenClassify(Downtime downtime)
    {
        SelectedDowntime = downtime;
        SelectedReasonId = downtime.ReasonId;
        InformedReason = downtime.InformedReason ?? string.Empty;
        Observation = downtime.Observation ?? string.Empty;
        return Task.CompletedTask;
    }

    public async Task Classify()
    {
        if (SelectedDowntime == null || SelectedDowntime.DowntimeId == null)
        {
            ErrorMessage = "Selecione uma parada para classificar.";
            return;
        }

        IsLoading = true;
        ErrorMessage = string.Empty;

        try
        {
            var result = await _configService.ClassifyDowntimeAsync(
                SelectedDowntime.DowntimeId.Value,
                SelectedReasonId,
                string.IsNullOrWhiteSpace(InformedReason) ? null : InformedReason,
                string.IsNullOrWhiteSpace(Observation) ? null : Observation,
                "admin");

            if (result.Success)
            {
                StatusMessage = "Parada classificada com sucesso.";
                SelectedDowntime = null;
                SelectedReasonId = null;
                InformedReason = string.Empty;
                Observation = string.Empty;
                await LoadAsync();
            }
            else
            {
                ErrorMessage = result.Message ?? "Erro ao classificar parada.";
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

    public async Task SaveRetention()
    {
        IsLoading = true;
        ErrorMessage = string.Empty;

        try
        {
            var result = await _configService.UpdateDowntimeRetentionAsync(RetentionDays);
            if (result.Success)
            {
                StatusMessage = "Retenção de paradas atualizada com sucesso.";
            }
            else
            {
                ErrorMessage = result.Message ?? "Erro ao atualizar retenção.";
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
}

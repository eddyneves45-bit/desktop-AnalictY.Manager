using System.Collections.ObjectModel;
using System.Windows.Input;
using AnalictY.Manager.Infrastructure;
using AnalictY.Manager.Services;
using AnalictY.Manager.Models;

namespace AnalictY.Manager.ViewModels;

public sealed class ShiftsViewModel : ObservableObject
{
    private readonly ConfigService _configService;
    private bool _isLoading;
    private string _statusMessage = "Carregando turnos...";
    private string _errorMessage = string.Empty;
    private Shift? _selectedShift;

    // Form fields
    private string _shiftName = string.Empty;
    private string _startTime = string.Empty;
    private string _endTime = string.Empty;
    private string _daysOfWeek = "0,1,2,3,4";
    private string _description = string.Empty;
    private bool _isActive = true;

    public ShiftsViewModel(ConfigService configService)
    {
        _configService = configService;
        RefreshCommand = new RelayCommand(Refresh);
        CreateCommand = new RelayCommand(OpenCreateModal);
        EditCommand = new RelayCommand(OpenEditModal);
        DeleteCommand = new RelayCommand(DeleteShift);
        SaveCommand = new RelayCommand(SaveShift);

        Shifts = new ObservableCollection<Shift>();
    }

    public ObservableCollection<Shift> Shifts { get; }

    public ICommand RefreshCommand { get; }
    public ICommand CreateCommand { get; }
    public ICommand EditCommand { get; }
    public ICommand DeleteCommand { get; }
    public ICommand SaveCommand { get; }

    private async Task OnRefreshAsync()
    {
        await LoadAsync();
    }

    public Task Refresh() => OnRefreshAsync();

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

    public Shift? SelectedShift
    {
        get => _selectedShift;
        set => SetProperty(ref _selectedShift, value);
    }

    // Form fields
    public string ShiftName
    {
        get => _shiftName;
        set => SetProperty(ref _shiftName, value);
    }

    public string StartTime
    {
        get => _startTime;
        set => SetProperty(ref _startTime, value);
    }

    public string EndTime
    {
        get => _endTime;
        set => SetProperty(ref _endTime, value);
    }

    public string DaysOfWeek
    {
        get => _daysOfWeek;
        set => SetProperty(ref _daysOfWeek, value);
    }

    public string Description
    {
        get => _description;
        set => SetProperty(ref _description, value);
    }

    public bool IsActive
    {
        get => _isActive;
        set => SetProperty(ref _isActive, value);
    }

    public bool IsEditing => SelectedShift != null;
    public string FormTitle => IsEditing ? "Editar Turno" : "Novo Turno";

    public async Task LoadAsync()
    {
        IsLoading = true;
        ErrorMessage = string.Empty;
        StatusMessage = "Carregando turnos...";

        try
        {
            var result = await _configService.GetShiftsAsync();
            if (!string.IsNullOrWhiteSpace(result.Error))
            {
                ErrorMessage = result.Error;
                StatusMessage = "Erro ao carregar turnos.";
                return;
            }

            Shifts.Clear();
            foreach (var shift in result.Shifts)
            {
                Shifts.Add(shift);
            }

            StatusMessage = $"{Shifts.Count} turno(s) carregado(s).";
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

    public Task OpenCreateModal()
    {
        SelectedShift = null;
        ShiftName = string.Empty;
        StartTime = string.Empty;
        EndTime = string.Empty;
        DaysOfWeek = "0,1,2,3,4";
        Description = string.Empty;
        IsActive = true;

        OnPropertyChanged(nameof(IsEditing));
        OnPropertyChanged(nameof(FormTitle));
        return Task.CompletedTask;
    }

    public Task OpenEditModal()
    {
        if (SelectedShift == null) return Task.CompletedTask;

        ShiftName = SelectedShift.Name ?? string.Empty;
        StartTime = SelectedShift.StartTime ?? string.Empty;
        EndTime = SelectedShift.EndTime ?? string.Empty;
        DaysOfWeek = SelectedShift.DaysOfWeek ?? "0,1,2,3,4";
        Description = SelectedShift.Description ?? string.Empty;
        IsActive = SelectedShift.IsActive;

        OnPropertyChanged(nameof(IsEditing));
        OnPropertyChanged(nameof(FormTitle));
        return Task.CompletedTask;
    }

    public async Task SaveShift()
    {
        if (string.IsNullOrWhiteSpace(ShiftName) || string.IsNullOrWhiteSpace(StartTime) || string.IsNullOrWhiteSpace(EndTime))
        {
            ErrorMessage = "Preencha nome, início e fim.";
            return;
        }

        IsLoading = true;
        ErrorMessage = string.Empty;

        try
        {
            var request = new ShiftRequest
            {
                Name = ShiftName,
                StartTime = StartTime,
                EndTime = EndTime,
                DaysOfWeek = DaysOfWeek,
                Description = Description,
                IsActive = IsActive
            };

            OperationResult result;
            if (IsEditing && SelectedShift != null)
            {
                result = await _configService.UpdateShiftAsync(SelectedShift.Id, request);
            }
            else
            {
                result = await _configService.CreateShiftAsync(request);
            }

            if (result.Success)
            {
                StatusMessage = IsEditing ? "Turno atualizado com sucesso." : "Turno criado com sucesso.";
                await LoadAsync();
                await OpenCreateModal();
            }
            else
            {
                ErrorMessage = result.Message ?? "Erro ao salvar turno.";
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

    public async Task DeleteShift()
    {
        if (SelectedShift == null)
        {
            ErrorMessage = "Selecione um turno para excluir.";
            return;
        }

        IsLoading = true;
        ErrorMessage = string.Empty;

        try
        {
            var result = await _configService.DeleteShiftAsync(SelectedShift.Id);
            if (result.Success)
            {
                StatusMessage = "Turno excluído com sucesso.";
                await LoadAsync();
            }
            else
            {
                ErrorMessage = result.Message ?? "Erro ao excluir turno.";
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

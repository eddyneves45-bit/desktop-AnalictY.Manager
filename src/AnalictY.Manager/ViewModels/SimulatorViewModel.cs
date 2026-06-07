using System.Collections.ObjectModel;
using System.Windows.Input;
using AnalictY.Manager.Infrastructure;
using AnalictY.Manager.Services;
using AnalictY.Manager.Models;

namespace AnalictY.Manager.ViewModels;

public sealed class SimulatorViewModel : ObservableObject
{
    private readonly ConfigService _configService;
    private bool _isLoading;
    private string _statusMessage = "Carregando simulador...";
    private string _errorMessage = string.Empty;
    private VirtualConsole? _console;
    private string _machineName = "Máquina Virtual 1";
    private string _machineCode = "VIRTUAL-001";
    private string _costCenter = "Simulacao";
    private string _location = "Laboratorio";
    private string _status = "0";
    private string _reason = "0";
    private string _productionCounter = "0";
    private string _lossCounter = "0";
    private string _piecesPerMinute = "60";
    private bool _running;
    private int _selectedMachineId;

    public SimulatorViewModel(ConfigService configService)
    {
        _configService = configService;
        RefreshCommand = new RelayCommand(Refresh);
        CreateMachineCommand = new RelayCommand(CreateMachine);
        OpenMachineCommand = new RelayCommand(OpenMachine);
        SetStatusCommand = new RelayCommand(SetStatus);
        ProduceOneCommand = new RelayCommand(ProduceOne);
        AddLossCommand = new RelayCommand(AddLoss);
        PublishCommand = new RelayCommand(Publish);
        StartAutomaticCommand = new RelayCommand(StartAutomatic);
        StopAutomaticCommand = new RelayCommand(StopAutomatic);
        ChooseReasonCommand = new RelayCommand(ChooseReason);
        BackToMachinesCommand = new RelayCommand(BackToMachines);

        Machines = new ObservableCollection<VirtualMachineSummary>();
    }

    public ObservableCollection<VirtualMachineSummary> Machines { get; }

    public int SelectedMachineId
    {
        get => _selectedMachineId;
        set => SetProperty(ref _selectedMachineId, value);
    }

    public ICommand RefreshCommand { get; }
    public ICommand CreateMachineCommand { get; }
    public ICommand OpenMachineCommand { get; }
    public ICommand SetStatusCommand { get; }
    public ICommand ProduceOneCommand { get; }
    public ICommand AddLossCommand { get; }
    public ICommand PublishCommand { get; }
    public ICommand StartAutomaticCommand { get; }
    public ICommand StopAutomaticCommand { get; }
    public ICommand ChooseReasonCommand { get; }
    public ICommand BackToMachinesCommand { get; }

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

    public VirtualConsole? Console
    {
        get => _console;
        set => SetProperty(ref _console, value);
    }

    public bool HasConsole => Console != null;

    // Form fields
    public string MachineName
    {
        get => _machineName;
        set => SetProperty(ref _machineName, value);
    }

    public string MachineCode
    {
        get => _machineCode;
        set => SetProperty(ref _machineCode, value);
    }

    public string CostCenter
    {
        get => _costCenter;
        set => SetProperty(ref _costCenter, value);
    }

    public string Location
    {
        get => _location;
        set => SetProperty(ref _location, value);
    }

    // Console fields
    public string Status
    {
        get => _status;
        set => SetProperty(ref _status, value);
    }

    public string Reason
    {
        get => _reason;
        set => SetProperty(ref _reason, value);
    }

    public string ProductionCounter
    {
        get => _productionCounter;
        set => SetProperty(ref _productionCounter, value);
    }

    public string LossCounter
    {
        get => _lossCounter;
        set => SetProperty(ref _lossCounter, value);
    }

    public string PiecesPerMinute
    {
        get => _piecesPerMinute;
        set => SetProperty(ref _piecesPerMinute, value);
    }

    public bool Running
    {
        get => _running;
        set => SetProperty(ref _running, value);
    }

    public string StatusLabel => int.TryParse(Status, out var s) ? s switch
    {
        0 => "Inativa",
        1 => "Produção",
        2 => "Ociosa",
        3 => "Manutenção",
        _ => "Desconhecido"
    } : "Desconhecido";

    private async Task OnRefreshAsync()
    {
        await LoadMachinesAsync();
    }

    public Task Refresh() => OnRefreshAsync();

    public async Task LoadMachinesAsync()
    {
        IsLoading = true;
        ErrorMessage = string.Empty;
        StatusMessage = "Carregando máquinas...";

        try
        {
            var result = await _configService.GetVirtualMachinesAsync();
            if (!string.IsNullOrWhiteSpace(result.Error))
            {
                ErrorMessage = result.Error;
                StatusMessage = "Erro ao carregar máquinas.";
                return;
            }

            Machines.Clear();
            foreach (var machine in result.Machines)
            {
                Machines.Add(machine);
            }

            StatusMessage = $"{Machines.Count} máquina(s) virtual(is) carregada(s).";
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

    public async Task CreateMachine()
    {
        IsLoading = true;
        ErrorMessage = string.Empty;

        try
        {
            var request = new VirtualMachineRequest
            {
                Name = MachineName,
                Code = MachineCode,
                CostCenter = CostCenter,
                Location = Location,
                FolderId = null
            };

            var result = await _configService.CreateVirtualMachineAsync(request);
            if (result.Success)
            {
                StatusMessage = "Máquina virtual criada com sucesso.";
                await LoadMachinesAsync();
            }
            else
            {
                ErrorMessage = result.Message ?? "Erro ao criar máquina virtual.";
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

    public async Task OpenMachine(object? parameter = null)
    {
        if (parameter is int idParameter)
        {
            SelectedMachineId = idParameter;
        }
        else if (parameter is VirtualMachineSummary machine)
        {
            SelectedMachineId = machine.Id;
        }

        if (SelectedMachineId == 0) return;

        IsLoading = true;
        ErrorMessage = string.Empty;

        try
        {
            var result = await _configService.GetVirtualConsoleAsync(SelectedMachineId);
            if (!string.IsNullOrWhiteSpace(result.Error))
            {
                ErrorMessage = result.Error;
                return;
            }

            if (result.Console != null)
            {
                Console = result.Console;
                Status = result.Console.Simulator.Status.ToString();
                Reason = result.Console.Simulator.DowntimeReasonCode.ToString();
                ProductionCounter = result.Console.Simulator.ProductionCounter.ToString();
                LossCounter = result.Console.Simulator.LossCounter.ToString();
                PiecesPerMinute = result.Console.Simulator.PiecesPerMinute.ToString();
                Running = result.Console.Simulator.Running;
                StatusMessage = "Bancada aberta.";
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

    public async Task Publish(object? next = null)
    {
        if (Console == null) return;

        var payload = new VirtualMachinePublishRequest
        {
            Status = int.TryParse(Status, out var s) ? s : 0,
            DowntimeReasonCode = int.TryParse(Reason, out var r) ? r : 0,
            ProductionCounter = int.TryParse(ProductionCounter, out var pc) ? pc : 0,
            LossCounter = int.TryParse(LossCounter, out var lc) ? lc : 0
        };

        var result = await _configService.PublishVirtualMachineAsync(int.TryParse(Console.Machine.Id, out var id) ? id : 0, payload);
        if (result.Success)
        {
            StatusMessage = $"Publicado: status {payload.Status}, produção {payload.ProductionCounter}, perdas {payload.LossCounter}";
        }
        else
        {
            ErrorMessage = result.Message ?? "Erro ao publicar valores.";
        }
    }

    public async Task SetStatus()
    {
        // Will be called from XAML via binding to Status property
        await Publish();
    }

    public async Task ProduceOne()
    {
        if (int.TryParse(ProductionCounter, out var current))
        {
            var next = current + 1;
            ProductionCounter = next.ToString();
            await Publish();
        }
    }

    public async Task AddLoss()
    {
        if (int.TryParse(LossCounter, out var current))
        {
            var next = current + 1;
            LossCounter = next.ToString();
            await Publish();
        }
    }

    public async Task StartAutomatic()
    {
        if (Console == null || Running) return;

        IsLoading = true;
        ErrorMessage = string.Empty;

        try
        {
            var request = new VirtualMachineStartRequest { PiecesPerMinute = int.TryParse(PiecesPerMinute, out var ppm) ? ppm : 60 };
            var result = await _configService.StartVirtualMachineAsync(int.TryParse(Console.Machine.Id, out var id) ? id : 0, request);
            if (result.Success)
            {
                Running = true;
                StatusMessage = "Produção automática iniciada.";
                await RefreshConsole();
            }
            else
            {
                ErrorMessage = result.Message ?? "Erro ao iniciar simulação.";
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

    public async Task StopAutomatic()
    {
        if (Console == null || !Running) return;

        IsLoading = true;
        ErrorMessage = string.Empty;

        try
        {
            var result = await _configService.StopVirtualMachineAsync(int.TryParse(Console.Machine.Id, out var id) ? id : 0);
            if (result.Success)
            {
                Running = false;
                StatusMessage = "Produção automática parada.";
                await RefreshConsole();
            }
            else
            {
                ErrorMessage = result.Message ?? "Erro ao parar simulação.";
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

    public async Task ChooseReason()
    {
        // Will be called from XAML via binding to SelectedReason property
        await Publish();
    }

    public Task BackToMachines()
    {
        Console = null;
        StatusMessage = "Voltar para bancadas.";
        return Task.CompletedTask;
    }

    private async Task RefreshConsole()
    {
        if (Console == null) return;
        var machineId = Console.Machine.Id;
        if (int.TryParse(machineId, out var id))
        {
            SelectedMachineId = id;
            await OpenMachine();
        }
    }
}

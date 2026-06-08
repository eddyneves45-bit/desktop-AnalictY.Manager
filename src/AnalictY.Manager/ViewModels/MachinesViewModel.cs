using System.Collections.ObjectModel;
using System.Windows.Input;
using AnalictY.Manager.Infrastructure;
using AnalictY.Manager.Services;
using AnalictY.Manager.Models;

namespace AnalictY.Manager.ViewModels;

public sealed class MachinesViewModel : ObservableObject
{
    private readonly ConfigService _configService;
    private bool _isLoading;
    private string _statusMessage = "Carregando máquinas...";
    private string _errorMessage = string.Empty;
    private string _search = string.Empty;
    private MachineFolder? _selectedFolder;
    private Machine? _selectedMachine;

    // Machine form fields
    private string _machineName = string.Empty;
    private string _machineCode = string.Empty;
    private string _costCenter = string.Empty;
    private string _location = string.Empty;
    private int? _folderId;
    private bool _isActive = true;
    private bool _showMachineModal;
    private bool _showFolderModal;
    private bool _isEditingFolder;
    private ObservableCollection<MachineFolder> _breadcrumbs = new();

    // Folder form fields
    private string _folderName = string.Empty;
    private int? _parentFolderId;
    private bool _isSector = false;

    public MachinesViewModel(ConfigService configService)
    {
        _configService = configService;
        RefreshCommand = new RelayCommand(Refresh);
        CreateMachineCommand = new RelayCommand(OpenCreateMachineModal);
        EditMachineCommand = new RelayCommand(OpenEditMachineModal);
        DeleteMachineCommand = new RelayCommand(DeleteMachine);
        SaveMachineCommand = new RelayCommand(SaveMachine);
        
        CreateFolderCommand = new RelayCommand(OpenCreateFolderModal);
        EditFolderCommand = new RelayCommand(OpenEditFolderModal);
        DeleteFolderCommand = new RelayCommand(DeleteFolder);
        SaveFolderCommand = new RelayCommand(SaveFolder);
        
        CloseModalsCommand = new RelayCommand(() => { ShowMachineModal = false; ShowFolderModal = false; });
        SelectFolderCommand = new RelayCommand<MachineFolder?>(SelectFolder);

        Machines = new ObservableCollection<Machine>();
        Folders = new ObservableCollection<MachineFolder>();
        FilteredMachines = new ObservableCollection<Machine>();
        RootFolders = new ObservableCollection<MachineFolder>();
    }

    public ObservableCollection<Machine> Machines { get; }
    public ObservableCollection<MachineFolder> Folders { get; }
    public ObservableCollection<MachineFolder> RootFolders { get; }
    public ObservableCollection<Machine> FilteredMachines { get; }
    public ObservableCollection<MachineFolder> Breadcrumbs => _breadcrumbs;

    public ICommand RefreshCommand { get; }
    public ICommand CreateMachineCommand { get; }
    public ICommand EditMachineCommand { get; }
    public ICommand DeleteMachineCommand { get; }
    public ICommand SaveMachineCommand { get; }
    public ICommand CreateFolderCommand { get; }
    public ICommand EditFolderCommand { get; }
    public ICommand DeleteFolderCommand { get; }
    public ICommand SaveFolderCommand { get; }
    public ICommand CloseModalsCommand { get; }
    public ICommand SelectFolderCommand { get; }

    public bool ShowMachineModal
    {
        get => _showMachineModal;
        set => SetProperty(ref _showMachineModal, value);
    }

    public bool ShowFolderModal
    {
        get => _showFolderModal;
        set => SetProperty(ref _showFolderModal, value);
    }

    public bool IsEditingFolder
    {
        get => _isEditingFolder;
        set => SetProperty(ref _isEditingFolder, value);
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

    public string Search
    {
        get => _search;
        set
        {
            if (SetProperty(ref _search, value))
            {
                FilterMachines();
            }
        }
    }

    public MachineFolder? SelectedFolder
    {
        get => _selectedFolder;
        set
        {
            if (SetProperty(ref _selectedFolder, value))
            {
                FilterMachines();
            }
        }
    }

    public Machine? SelectedMachine
    {
        get => _selectedMachine;
        set => SetProperty(ref _selectedMachine, value);
    }

    // Machine form fields
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

    public int? FolderId
    {
        get => _folderId;
        set => SetProperty(ref _folderId, value);
    }

    public bool IsActive
    {
        get => _isActive;
        set => SetProperty(ref _isActive, value);
    }

    // Folder form fields
    public string FolderName
    {
        get => _folderName;
        set => SetProperty(ref _folderName, value);
    }

    public int? ParentFolderId
    {
        get => _parentFolderId;
        set => SetProperty(ref _parentFolderId, value);
    }

    public bool IsSector
    {
        get => _isSector;
        set => SetProperty(ref _isSector, value);
    }

    public bool IsEditingMachine => SelectedMachine != null;

    private async Task OnRefreshAsync()
    {
        await LoadAsync();
    }

    public Task Refresh() => OnRefreshAsync();

    public async Task LoadAsync()
    {
        IsLoading = true;
        ErrorMessage = string.Empty;
        StatusMessage = "Carregando máquinas e pastas...";

        try
        {
            var foldersResult = await _configService.GetMachineFoldersAsync();
            if (!string.IsNullOrWhiteSpace(foldersResult.Error))
            {
                ErrorMessage = foldersResult.Error;
                StatusMessage = "Erro ao carregar pastas.";
                return;
            }

            var machinesResult = await _configService.GetMachinesAsync();
            if (!string.IsNullOrWhiteSpace(machinesResult.Error))
            {
                ErrorMessage = machinesResult.Error;
                StatusMessage = "Erro ao carregar máquinas.";
                return;
            }

            Folders.Clear();
            foreach (var folder in foldersResult.Folders)
            {
                Folders.Add(folder);
            }

            Machines.Clear();
            foreach (var machine in machinesResult.Machines)
            {
                Machines.Add(machine);
            }

            BuildFolderTree();
            FilterMachines();
            StatusMessage = $"{Machines.Count} máquina(s), {Folders.Count} pasta(s) carregada(s).";
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

    private void BuildFolderTree()
    {
        RootFolders.Clear();
        var folderMap = Folders.ToDictionary(f => f.Id);
        
        foreach (var folder in Folders)
        {
            folder.SubFolders.Clear();
        }

        foreach (var folder in Folders)
        {
            if (folder.ParentFolderId.HasValue && folderMap.TryGetValue(folder.ParentFolderId.Value, out var parent))
            {
                parent.SubFolders.Add(folder);
            }
            else
            {
                RootFolders.Add(folder);
            }
        }
    }

    private void SelectFolder(MachineFolder? folder)
    {
        foreach (var f in Folders) f.IsSelected = false;
        if (folder != null) folder.IsSelected = true;
        
        SelectedFolder = folder;
        UpdateBreadcrumbs();
    }

    private void UpdateBreadcrumbs()
    {
        _breadcrumbs.Clear();
        if (SelectedFolder == null) return;

        var path = new List<MachineFolder>();
        var cursor = SelectedFolder;
        while (cursor != null)
        {
            path.Insert(0, cursor);
            cursor = Folders.FirstOrDefault(f => f.Id == cursor.ParentFolderId);
        }

        foreach (var folder in path) _breadcrumbs.Add(folder);
    }

    private void FilterMachines()
    {
        var filtered = Machines.ToList();

        if (SelectedFolder != null)
        {
            filtered = filtered.Where(m => m.FolderId == SelectedFolder.Id.ToString()).ToList();
        }

        if (!string.IsNullOrWhiteSpace(_search))
        {
            filtered = filtered.Where(m =>
                (m.Name?.Contains(_search, StringComparison.OrdinalIgnoreCase) ?? false) ||
                (m.Code?.Contains(_search, StringComparison.OrdinalIgnoreCase) ?? false) ||
                (m.Location?.Contains(_search, StringComparison.OrdinalIgnoreCase) ?? false))
            .ToList();
        }

        FilteredMachines.Clear();
        foreach (var machine in filtered)
        {
            FilteredMachines.Add(machine);
        }
    }

    public Task OpenCreateMachineModal()
    {
        SelectedMachine = null;
        MachineName = string.Empty;
        MachineCode = string.Empty;
        CostCenter = string.Empty;
        Location = string.Empty;
        FolderId = SelectedFolder?.Id;
        IsActive = true;
        ShowMachineModal = true;

        OnPropertyChanged(nameof(IsEditingMachine));
        return Task.CompletedTask;
    }

    public Task OpenEditMachineModal()
    {
        if (SelectedMachine == null) return Task.CompletedTask;

        MachineName = SelectedMachine.Name ?? string.Empty;
        MachineCode = SelectedMachine.Code ?? string.Empty;
        CostCenter = SelectedMachine.CostCenter ?? string.Empty;
        Location = SelectedMachine.Location ?? string.Empty;
        FolderId = int.TryParse(SelectedMachine.FolderId, out var fid) ? fid : null;
        IsActive = SelectedMachine.IsActive;
        ShowMachineModal = true;

        OnPropertyChanged(nameof(IsEditingMachine));
        return Task.CompletedTask;
    }

    public async Task SaveMachine()
    {
        if (string.IsNullOrWhiteSpace(MachineName) || string.IsNullOrWhiteSpace(MachineCode))
        {
            ErrorMessage = "Preencha nome e código.";
            return;
        }

        IsLoading = true;
        ErrorMessage = string.Empty;

        try
        {
            var request = new MachineRequest
            {
                Name = MachineName,
                Code = MachineCode,
                CostCenter = CostCenter,
                Location = Location,
                FolderId = FolderId,
                IsActive = IsActive
            };

            OperationResult result;
            if (IsEditingMachine && SelectedMachine != null)
            {
                result = await _configService.UpdateMachineAsync(int.Parse(SelectedMachine.Id), request);
            }
            else
            {
                result = await _configService.CreateMachineAsync(request);
            }

            if (result.Success)
            {
                StatusMessage = IsEditingMachine ? "Máquina atualizada com sucesso." : "Máquina criada com sucesso.";
                ShowMachineModal = false;
                await LoadAsync();
            }
            else
            {
                ErrorMessage = result.Message ?? "Erro ao salvar máquina.";
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

    public async Task DeleteMachine()
    {
        if (SelectedMachine == null)
        {
            ErrorMessage = "Selecione uma máquina para excluir.";
            return;
        }

        IsLoading = true;
        ErrorMessage = string.Empty;

        try
        {
            var result = await _configService.DeleteMachineAsync(int.Parse(SelectedMachine.Id));
            if (result.Success)
            {
                StatusMessage = "Máquina excluída com sucesso.";
                SelectedMachine = null;
                await LoadAsync();
            }
            else
            {
                ErrorMessage = result.Message ?? "Erro ao excluir máquina.";
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

    public Task OpenCreateFolderModal()
    {
        IsEditingFolder = false;
        FolderName = string.Empty;
        ParentFolderId = SelectedFolder?.Id;
        IsSector = false;
        ShowFolderModal = true;
        return Task.CompletedTask;
    }

    public Task OpenEditFolderModal()
    {
        if (SelectedFolder == null) return Task.CompletedTask;
        
        IsEditingFolder = true;
        FolderName = SelectedFolder.Name;
        ParentFolderId = SelectedFolder.ParentFolderId;
        IsSector = SelectedFolder.IsSector;
        ShowFolderModal = true;
        return Task.CompletedTask;
    }

    public async Task DeleteFolder()
    {
        if (SelectedFolder == null) return;

        IsLoading = true;
        ErrorMessage = string.Empty;

        try
        {
            var result = await _configService.DeleteMachineFolderAsync(SelectedFolder.Id);
            if (result.Success)
            {
                StatusMessage = "Pasta excluída com sucesso.";
                SelectedFolder = null;
                await LoadAsync();
            }
            else
            {
                ErrorMessage = result.Message ?? "Erro ao excluir pasta.";
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

    public async Task SaveFolder()
    {
        if (string.IsNullOrWhiteSpace(FolderName))
        {
            ErrorMessage = "Preencha o nome da pasta.";
            return;
        }

        IsLoading = true;
        ErrorMessage = string.Empty;

        try
        {
            var request = new MachineFolderRequest
            {
                Name = FolderName,
                ParentFolderId = ParentFolderId,
                IsSector = IsSector
            };

            OperationResult result;
            if (IsEditingFolder && SelectedFolder != null)
            {
                result = await _configService.UpdateMachineFolderAsync(SelectedFolder.Id, request);
            }
            else
            {
                result = await _configService.CreateMachineFolderAsync(request);
            }

            if (result.Success)
            {
                StatusMessage = IsEditingFolder ? "Pasta atualizada com sucesso." : "Pasta criada com sucesso.";
                ShowFolderModal = false;
                await LoadAsync();
            }
            else
            {
                ErrorMessage = result.Message ?? "Erro ao salvar pasta.";
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

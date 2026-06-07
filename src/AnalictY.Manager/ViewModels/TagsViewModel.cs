using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using AnalictY.Manager.Infrastructure;
using AnalictY.Manager.Models;
using AnalictY.Manager.Services;

namespace AnalictY.Manager.ViewModels;

public sealed class TagsViewModel : ObservableObject
{
    private readonly ConfigService _configService;
    private bool _isLoading;
    private string _statusMessage = "Carregando TAGs...";
    private string _errorMessage = string.Empty;
    private string _search = string.Empty;
    private Tag? _selectedTag;
    private int? _selectedFilterFolderId;
    private string _tagName = string.Empty;
    private string _dataType = "Double";
    private string _driverType = "OPCUA";
    private string _address = string.Empty;
    private int? _opcuaConnectionId;
    private int? _mqttConnectionId;
    private int? _selectedFolderId;
    private int _pollIntervalMs = 1000;
    private bool _isActive = true;
    private string _newFolderName = string.Empty;
    private int? _newFolderParentId;
    private MachineFolderOption? _selectedFolderOption;
    private MachineFolderOption? _editingFolder;
    private bool _isTagEditorOpen;
    private bool _isFolderEditorOpen;
    private bool _isDeleteModalOpen;
    private string _folderModalTitle = "Nova pasta";
    private string _deleteModalTitle = string.Empty;
    private string _deleteModalMessage = string.Empty;
    private string _deleteTarget = string.Empty;

    public TagsViewModel(ConfigService configService)
    {
        _configService = configService;

        RefreshCommand = new RelayCommand(Refresh);
        CreateCommand = new RelayCommand(OpenCreateModal);
        EditCommand = new RelayCommand(parameter => OpenEditModal(parameter as Tag));
        DeleteCommand = new RelayCommand(parameter => OpenDeleteTagModal(parameter as Tag));
        SaveCommand = new RelayCommand(SaveTag);
        CreateFolderCommand = new RelayCommand(OpenCreateFolderModal);
        EditFolderCommand = new RelayCommand(OpenEditFolderModal);
        DeleteFolderCommand = new RelayCommand(OpenDeleteFolderModal);
        SaveFolderCommand = new RelayCommand(SaveFolder);
        ConfirmDeleteCommand = new RelayCommand(ConfirmDelete);
        CloseModalCommand = new RelayCommand(CloseModals);

        Tags = new ObservableCollection<Tag>();
        FilteredTags = new ObservableCollection<Tag>();
        Folders = new ObservableCollection<MachineFolderOption>();
        FolderFilterOptions = new ObservableCollection<MachineFolderOption>();
        DataTypes = new ObservableCollection<string>(new[] { "Double", "Float", "Int32", "Int64", "String", "Boolean" });
        DriverTypes = new ObservableCollection<string>(new[] { "OPCUA", "MQTT" });
    }

    public ObservableCollection<Tag> Tags { get; }
    public ObservableCollection<Tag> FilteredTags { get; }
    public ObservableCollection<MachineFolderOption> Folders { get; }
    public ObservableCollection<MachineFolderOption> FolderFilterOptions { get; }
    public ObservableCollection<string> DataTypes { get; }
    public ObservableCollection<string> DriverTypes { get; }

    public ICommand RefreshCommand { get; }
    public ICommand CreateCommand { get; }
    public ICommand EditCommand { get; }
    public ICommand DeleteCommand { get; }
    public ICommand SaveCommand { get; }
    public ICommand CreateFolderCommand { get; }
    public ICommand EditFolderCommand { get; }
    public ICommand DeleteFolderCommand { get; }
    public ICommand SaveFolderCommand { get; }
    public ICommand ConfirmDeleteCommand { get; }
    public ICommand CloseModalCommand { get; }

    public Task Refresh() => LoadAsync();

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
                FilterTags();
            }
        }
    }

    public int? SelectedFilterFolderId
    {
        get => _selectedFilterFolderId;
        set
        {
            if (SetProperty(ref _selectedFilterFolderId, value))
            {
                FilterTags();
            }
        }
    }

    public Tag? SelectedTag
    {
        get => _selectedTag;
        set => SetProperty(ref _selectedTag, value);
    }

    public string TagName
    {
        get => _tagName;
        set => SetProperty(ref _tagName, value);
    }

    public string DataType
    {
        get => _dataType;
        set => SetProperty(ref _dataType, value);
    }

    public string DriverType
    {
        get => _driverType;
        set => SetProperty(ref _driverType, value);
    }

    public string Address
    {
        get => _address;
        set => SetProperty(ref _address, value);
    }

    public int? OpcUaConnectionId
    {
        get => _opcuaConnectionId;
        set => SetProperty(ref _opcuaConnectionId, value);
    }

    public int? MqttConnectionId
    {
        get => _mqttConnectionId;
        set => SetProperty(ref _mqttConnectionId, value);
    }

    public int? SelectedFolderId
    {
        get => _selectedFolderId;
        set => SetProperty(ref _selectedFolderId, value);
    }

    public int PollIntervalMs
    {
        get => _pollIntervalMs;
        set => SetProperty(ref _pollIntervalMs, Math.Max(100, value));
    }

    public bool IsActive
    {
        get => _isActive;
        set => SetProperty(ref _isActive, value);
    }

    public string NewFolderName
    {
        get => _newFolderName;
        set => SetProperty(ref _newFolderName, value);
    }

    public int? NewFolderParentId
    {
        get => _newFolderParentId;
        set => SetProperty(ref _newFolderParentId, value);
    }

    public MachineFolderOption? SelectedFolderOption
    {
        get => _selectedFolderOption;
        set
        {
            if (SetProperty(ref _selectedFolderOption, value))
            {
                SelectedFilterFolderId = value?.Id;
            }
        }
    }

    public bool IsTagEditorOpen
    {
        get => _isTagEditorOpen;
        set
        {
            if (SetProperty(ref _isTagEditorOpen, value))
            {
                OnPropertyChanged(nameof(TagEditorVisibility));
            }
        }
    }

    public bool IsFolderEditorOpen
    {
        get => _isFolderEditorOpen;
        set
        {
            if (SetProperty(ref _isFolderEditorOpen, value))
            {
                OnPropertyChanged(nameof(FolderEditorVisibility));
            }
        }
    }

    public bool IsDeleteModalOpen
    {
        get => _isDeleteModalOpen;
        set
        {
            if (SetProperty(ref _isDeleteModalOpen, value))
            {
                OnPropertyChanged(nameof(DeleteModalVisibility));
            }
        }
    }

    public Visibility TagEditorVisibility => IsTagEditorOpen ? Visibility.Visible : Visibility.Collapsed;
    public Visibility FolderEditorVisibility => IsFolderEditorOpen ? Visibility.Visible : Visibility.Collapsed;
    public Visibility DeleteModalVisibility => IsDeleteModalOpen ? Visibility.Visible : Visibility.Collapsed;

    public string FolderModalTitle
    {
        get => _folderModalTitle;
        set => SetProperty(ref _folderModalTitle, value);
    }

    public string DeleteModalTitle
    {
        get => _deleteModalTitle;
        set => SetProperty(ref _deleteModalTitle, value);
    }

    public string DeleteModalMessage
    {
        get => _deleteModalMessage;
        set => SetProperty(ref _deleteModalMessage, value);
    }

    public bool IsEditing => SelectedTag != null;
    public string FormTitle => IsEditing ? "Editar TAG" : "Nova TAG";

    public async Task LoadAsync()
    {
        IsLoading = true;
        ErrorMessage = string.Empty;
        StatusMessage = "Carregando TAGs...";

        try
        {
            var foldersTask = _configService.GetMachineFoldersAsync();
            var tagsTask = _configService.GetTagsAsync();
            var runtimeTask = _configService.GetTagRuntimeStatesAsync();
            await Task.WhenAll(foldersTask, tagsTask, runtimeTask);

            LoadFolders(foldersTask.Result.Folders);

            var result = tagsTask.Result;
            if (!string.IsNullOrWhiteSpace(result.Error))
            {
                ErrorMessage = result.Error;
                StatusMessage = "Erro ao carregar TAGs.";
                return;
            }

            Tags.Clear();
            var runtimeById = runtimeTask.Result
                .Where(r => !string.IsNullOrWhiteSpace(r.TagId))
                .GroupBy(r => r.TagId)
                .ToDictionary(g => g.Key, g => g.First());
            var runtimeByName = runtimeTask.Result
                .Where(r => !string.IsNullOrWhiteSpace(r.TagName))
                .GroupBy(r => r.TagName, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

            foreach (var tag in result.Tags)
            {
                tag.FolderName = GetFolderName(tag.FolderId);

                if ((runtimeById.TryGetValue(tag.Id, out var runtime) || runtimeByName.TryGetValue(tag.Name, out runtime)) && runtime is not null)
                {
                    tag.CurrentValue = runtime.Value;
                    tag.Quality = runtime.Quality;
                    tag.LastTimestamp = runtime.Timestamp;
                    tag.RuntimeConnected = runtime.Connected;
                }

                Tags.Add(tag);
            }

            FilterTags();
            StatusMessage = $"{Tags.Count} TAG(s) carregada(s).";
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            StatusMessage = "Erro ao carregar TAGs.";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void LoadFolders(IReadOnlyCollection<MachineFolder> folders)
    {
        var ordered = BuildFolderOptions(folders);

        Folders.Clear();
        Folders.Add(MachineFolderOption.None("Sem pasta"));
        foreach (var folder in ordered)
        {
            Folders.Add(folder);
        }

        FolderFilterOptions.Clear();
        FolderFilterOptions.Add(MachineFolderOption.None("Todas as TAGs"));
        FolderFilterOptions.Add(MachineFolderOption.RootOnly());
        foreach (var folder in ordered)
        {
            FolderFilterOptions.Add(folder);
        }
    }

    private static List<MachineFolderOption> BuildFolderOptions(IReadOnlyCollection<MachineFolder> folders)
    {
        var childrenByParent = folders
            .GroupBy(f => f.ParentFolderId ?? 0)
            .ToDictionary(g => g.Key, g => g.OrderBy(f => f.Name).ToList());

        var result = new List<MachineFolderOption>();
        AddChildren(0, 0);
        return result;

        void AddChildren(int parentId, int depth)
        {
            if (!childrenByParent.TryGetValue(parentId, out var children))
            {
                return;
            }

            foreach (var child in children)
            {
                result.Add(new MachineFolderOption(child.Id, child.Name, child.ParentFolderId, depth));
                AddChildren(child.Id, depth + 1);
            }
        }
    }

    private void FilterTags()
    {
        IEnumerable<Tag> query = Tags;

        if (!string.IsNullOrWhiteSpace(_search))
        {
            query = query.Where(t =>
                t.Name.Contains(_search, StringComparison.OrdinalIgnoreCase) ||
                t.Address.Contains(_search, StringComparison.OrdinalIgnoreCase) ||
                t.DataType.Contains(_search, StringComparison.OrdinalIgnoreCase) ||
                t.DriverType.Contains(_search, StringComparison.OrdinalIgnoreCase) ||
                t.Status.Contains(_search, StringComparison.OrdinalIgnoreCase));
        }

        if (_selectedFilterFolderId.HasValue)
        {
            query = _selectedFilterFolderId.Value == 0
                ? query.Where(t => !t.FolderId.HasValue)
                : query.Where(t => t.FolderId == _selectedFilterFolderId.Value);
        }

        FilteredTags.Clear();
        foreach (var tag in query.OrderBy(t => t.Name))
        {
            FilteredTags.Add(tag);
        }
    }

    public Task OpenCreateModal()
    {
        SelectedTag = null;
        TagName = string.Empty;
        DataType = "Double";
        DriverType = "OPCUA";
        Address = string.Empty;
        OpcUaConnectionId = null;
        MqttConnectionId = null;
        SelectedFolderId = SelectedFilterFolderId > 0 ? SelectedFilterFolderId : null;
        PollIntervalMs = 1000;
        IsActive = true;
        IsTagEditorOpen = true;

        OnPropertyChanged(nameof(IsEditing));
        OnPropertyChanged(nameof(FormTitle));
        return Task.CompletedTask;
    }

    public Task OpenEditModal(Tag? tag = null)
    {
        SelectedTag = tag ?? SelectedTag;
        if (SelectedTag == null)
        {
            ErrorMessage = "Selecione uma TAG para editar.";
            return Task.CompletedTask;
        }

        TagName = SelectedTag.Name;
        DataType = string.IsNullOrWhiteSpace(SelectedTag.DataType) ? "Double" : SelectedTag.DataType;
        DriverType = string.IsNullOrWhiteSpace(SelectedTag.DriverType) ? "OPCUA" : SelectedTag.DriverType;
        Address = SelectedTag.Address;
        OpcUaConnectionId = SelectedTag.OpcUaConnectionId;
        MqttConnectionId = SelectedTag.MqttConnectionId;
        SelectedFolderId = SelectedTag.FolderId;
        PollIntervalMs = int.TryParse(SelectedTag.ScanRate, out var scanRate) ? scanRate : 1000;
        IsActive = SelectedTag.IsActive;
        IsTagEditorOpen = true;

        OnPropertyChanged(nameof(IsEditing));
        OnPropertyChanged(nameof(FormTitle));
        return Task.CompletedTask;
    }

    public async Task SaveTag()
    {
        if (string.IsNullOrWhiteSpace(TagName) || string.IsNullOrWhiteSpace(Address))
        {
            ErrorMessage = "Preencha nome e endereco.";
            return;
        }

        IsLoading = true;
        ErrorMessage = string.Empty;

        try
        {
            var request = new TagRequest
            {
                TagName = TagName.Trim(),
                DataType = DataType,
                DriverType = DriverType,
                Address = Address.Trim(),
                OpcUaConnectionId = DriverType.Equals("OPCUA", StringComparison.OrdinalIgnoreCase) ? OpcUaConnectionId : null,
                MqttConnectionId = DriverType.Equals("MQTT", StringComparison.OrdinalIgnoreCase) ? MqttConnectionId : null,
                FolderId = SelectedFolderId,
                PollIntervalMs = PollIntervalMs,
                IsActive = IsActive,
                PersistenceMode = SelectedTag?.PersistenceMode
            };

            OperationResult result;
            if (IsEditing && SelectedTag != null && int.TryParse(SelectedTag.Id, out var tagId))
            {
                result = await _configService.UpdateTagAsync(tagId, request);
            }
            else
            {
                result = await _configService.CreateTagAsync(request);
            }

            if (result.Success)
            {
                StatusMessage = IsEditing ? "TAG atualizada com sucesso." : "TAG criada com sucesso.";
                await LoadAsync();
                await CloseModals();
            }
            else
            {
                ErrorMessage = result.Message ?? "Erro ao salvar TAG.";
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

    public Task OpenDeleteTagModal(Tag? tag = null)
    {
        SelectedTag = tag ?? SelectedTag;
        if (SelectedTag == null)
        {
            ErrorMessage = "Selecione uma TAG para excluir.";
            return Task.CompletedTask;
        }

        _deleteTarget = "tag";
        DeleteModalTitle = "Excluir TAG";
        DeleteModalMessage = $"Excluir a TAG \"{SelectedTag.Name}\"?";
        IsDeleteModalOpen = true;
        return Task.CompletedTask;
    }

    private async Task DeleteTag()
    {
        if (SelectedTag == null)
        {
            ErrorMessage = "Selecione uma TAG para excluir.";
            return;
        }

        if (!int.TryParse(SelectedTag.Id, out var tagId))
        {
            ErrorMessage = "ID da TAG invalido.";
            return;
        }

        IsLoading = true;
        ErrorMessage = string.Empty;

        try
        {
            var result = await _configService.DeleteTagAsync(tagId);
            if (result.Success)
            {
                StatusMessage = "TAG excluida com sucesso.";
                await LoadAsync();
                await CloseModals();
            }
            else
            {
                ErrorMessage = result.Message ?? "Erro ao excluir TAG.";
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
        _editingFolder = null;
        FolderModalTitle = "Nova pasta";
        NewFolderName = string.Empty;
        NewFolderParentId = SelectedFilterFolderId > 0 ? SelectedFilterFolderId : null;
        IsFolderEditorOpen = true;
        return Task.CompletedTask;
    }

    public Task OpenEditFolderModal()
    {
        var folder = SelectedFolderOption;
        if (folder?.Id is null or 0)
        {
            ErrorMessage = "Selecione uma pasta para editar.";
            return Task.CompletedTask;
        }

        _editingFolder = folder;
        FolderModalTitle = "Editar pasta";
        NewFolderName = folder.Name;
        NewFolderParentId = folder.ParentFolderId;
        IsFolderEditorOpen = true;
        return Task.CompletedTask;
    }

    public Task OpenDeleteFolderModal()
    {
        var folder = SelectedFolderOption;
        if (folder?.Id is null or 0)
        {
            ErrorMessage = "Selecione uma pasta para excluir.";
            return Task.CompletedTask;
        }

        _editingFolder = folder;
        _deleteTarget = "folder";
        DeleteModalTitle = "Excluir pasta";
        DeleteModalMessage = $"Excluir a pasta \"{folder.Name}\"?";
        IsDeleteModalOpen = true;
        return Task.CompletedTask;
    }

    public async Task SaveFolder()
    {
        if (string.IsNullOrWhiteSpace(NewFolderName))
        {
            ErrorMessage = "Informe o nome da pasta.";
            return;
        }

        IsLoading = true;
        ErrorMessage = string.Empty;

        try
        {
            var request = new MachineFolderRequest
            {
                Name = NewFolderName.Trim(),
                ParentFolderId = NewFolderParentId,
                IsSector = false
            };

            var result = _editingFolder?.Id is > 0
                ? await _configService.UpdateMachineFolderAsync(_editingFolder.Id.Value, request)
                : await _configService.CreateMachineFolderAsync(request);

            if (result.Success)
            {
                StatusMessage = _editingFolder?.Id is > 0 ? "Pasta atualizada com sucesso." : "Pasta criada com sucesso.";
                NewFolderName = string.Empty;
                NewFolderParentId = null;
                await LoadAsync();
                await CloseModals();
            }
            else
            {
                ErrorMessage = result.Message ?? "Erro ao criar pasta.";
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

    public async Task ConfirmDelete()
    {
        if (_deleteTarget == "tag")
        {
            await DeleteTag();
            return;
        }

        if (_deleteTarget == "folder")
        {
            await DeleteFolder();
        }
    }

    private async Task DeleteFolder()
    {
        if (_editingFolder?.Id is not > 0)
        {
            ErrorMessage = "Selecione uma pasta para excluir.";
            return;
        }

        IsLoading = true;
        ErrorMessage = string.Empty;

        try
        {
            var result = await _configService.DeleteMachineFolderAsync(_editingFolder.Id.Value);
            if (result.Success)
            {
                StatusMessage = "Pasta excluida com sucesso.";
                SelectedFilterFolderId = null;
                await LoadAsync();
                await CloseModals();
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

    public Task CloseModals()
    {
        IsTagEditorOpen = false;
        IsFolderEditorOpen = false;
        IsDeleteModalOpen = false;
        _deleteTarget = string.Empty;
        return Task.CompletedTask;
    }

    public async Task MoveTagToFolderAsync(Tag tag, int? folderId)
    {
        if (!int.TryParse(tag.Id, out var tagId))
        {
            ErrorMessage = "ID da TAG invalido.";
            return;
        }

        IsLoading = true;
        ErrorMessage = string.Empty;

        try
        {
            var request = new TagRequest
            {
                TagName = tag.Name,
                DataType = string.IsNullOrWhiteSpace(tag.DataType) ? "Double" : tag.DataType,
                DriverType = string.IsNullOrWhiteSpace(tag.DriverType) ? "OPCUA" : tag.DriverType,
                Address = tag.Address,
                OpcUaConnectionId = tag.OpcUaConnectionId,
                MqttConnectionId = tag.MqttConnectionId,
                FolderId = folderId,
                PollIntervalMs = int.TryParse(tag.ScanRate, out var scanRate) ? scanRate : 1000,
                IsActive = tag.IsActive,
                PersistenceMode = tag.PersistenceMode
            };

            var result = await _configService.UpdateTagAsync(tagId, request);
            if (result.Success)
            {
                StatusMessage = folderId.HasValue
                    ? $"TAG movida para {GetFolderName(folderId)}."
                    : "TAG movida para Sem pasta.";
                await LoadAsync();
                SelectedFilterFolderId = folderId;
            }
            else
            {
                ErrorMessage = result.Message ?? "Erro ao mover TAG.";
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

    public string GetFolderName(int? folderId)
    {
        if (!folderId.HasValue)
        {
            return "Sem pasta";
        }

        return Folders.FirstOrDefault(f => f.Id == folderId.Value)?.DisplayName ?? $"Pasta {folderId.Value}";
    }
}

public sealed class MachineFolderOption
{
    public MachineFolderOption(int? id, string name, int? parentFolderId, int depth)
    {
        Id = id;
        Name = name;
        ParentFolderId = parentFolderId;
        Depth = depth;
    }

    public int? Id { get; }
    public string Name { get; }
    public int? ParentFolderId { get; }
    public int Depth { get; }
    public string DisplayName => Depth <= 0 ? Name : $"{new string(' ', Depth * 4)}{Name}";

    public static MachineFolderOption None(string label) => new(null, label, null, 0);
    public static MachineFolderOption RootOnly() => new(0, "Sem pasta", null, 0);
}

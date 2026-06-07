using System.Collections.ObjectModel;
using System.Windows.Input;
using AnalictY.Manager.Infrastructure;
using AnalictY.Manager.Services;
using AnalictY.Manager.Models;

namespace AnalictY.Manager.ViewModels;

public sealed class TagsViewModel : ObservableObject
{
    private readonly ConfigService _configService;
    private bool _isLoading;
    private string _statusMessage = "Carregando TAGs...";
    private string _errorMessage = string.Empty;
    private string _search = string.Empty;
    private Tag? _selectedTag;

    // Form fields
    private string _tagName = string.Empty;
    private string _dataType = "Double";
    private string _driverType = "OPCUA";
    private string _address = string.Empty;
    private int _opcuaConnectionId;
    private int _pollIntervalMs = 1000;
    private bool _isActive = true;

    public TagsViewModel(ConfigService configService)
    {
        _configService = configService;
        RefreshCommand = new RelayCommand(Refresh);
        CreateCommand = new RelayCommand(OpenCreateModal);
        EditCommand = new RelayCommand(OpenEditModal);
        DeleteCommand = new RelayCommand(DeleteTag);
        SaveCommand = new RelayCommand(SaveTag);

        Tags = new ObservableCollection<Tag>();
        FilteredTags = new ObservableCollection<Tag>();
    }

    public ObservableCollection<Tag> Tags { get; }
    public ObservableCollection<Tag> FilteredTags { get; }

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

    public Tag? SelectedTag
    {
        get => _selectedTag;
        set => SetProperty(ref _selectedTag, value);
    }

    // Form fields
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

    public int OpcUaConnectionId
    {
        get => _opcuaConnectionId;
        set => SetProperty(ref _opcuaConnectionId, value);
    }

    public int PollIntervalMs
    {
        get => _pollIntervalMs;
        set => SetProperty(ref _pollIntervalMs, value);
    }

    public bool IsActive
    {
        get => _isActive;
        set => SetProperty(ref _isActive, value);
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
            var result = await _configService.GetTagsAsync();
            if (!string.IsNullOrWhiteSpace(result.Error))
            {
                ErrorMessage = result.Error;
                StatusMessage = "Erro ao carregar TAGs.";
                return;
            }

            Tags.Clear();
            foreach (var tag in result.Tags)
            {
                Tags.Add(tag);
            }

            FilterTags();
            StatusMessage = $"{Tags.Count} TAG(s) carregada(s).";
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

    private void FilterTags()
    {
        var filtered = string.IsNullOrWhiteSpace(_search)
            ? Tags.ToList()
            : Tags.Where(t =>
                (t.Name?.Contains(_search, StringComparison.OrdinalIgnoreCase) ?? false) ||
                (t.Address?.Contains(_search, StringComparison.OrdinalIgnoreCase) ?? false) ||
                (t.DataType?.Contains(_search, StringComparison.OrdinalIgnoreCase) ?? false))
            .ToList();

        FilteredTags.Clear();
        foreach (var tag in filtered)
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
        OpcUaConnectionId = 0;
        PollIntervalMs = 1000;
        IsActive = true;

        OnPropertyChanged(nameof(IsEditing));
        OnPropertyChanged(nameof(FormTitle));
        return Task.CompletedTask;
    }

    public Task OpenEditModal()
    {
        if (SelectedTag == null) return Task.CompletedTask;

        TagName = SelectedTag.Name ?? string.Empty;
        DataType = SelectedTag.DataType ?? "Double";
        DriverType = "OPCUA";
        Address = SelectedTag.Address ?? string.Empty;
        OpcUaConnectionId = 0;
        PollIntervalMs = 1000;
        IsActive = true;

        OnPropertyChanged(nameof(IsEditing));
        OnPropertyChanged(nameof(FormTitle));
        return Task.CompletedTask;
    }

    public async Task SaveTag()
    {
        if (string.IsNullOrWhiteSpace(TagName) || string.IsNullOrWhiteSpace(Address))
        {
            ErrorMessage = "Preencha nome e endereço.";
            return;
        }

        IsLoading = true;
        ErrorMessage = string.Empty;

        try
        {
            var request = new TagRequest
            {
                TagName = TagName,
                DataType = DataType,
                DriverType = DriverType,
                Address = Address,
                OpcUaConnectionId = OpcUaConnectionId,
                PollIntervalMs = PollIntervalMs,
                IsActive = IsActive
            };

            OperationResult result;
            if (IsEditing && SelectedTag != null)
            {
                result = await _configService.UpdateTagAsync(int.Parse(SelectedTag.Id), request);
            }
            else
            {
                result = await _configService.CreateTagAsync(request);
            }

            if (result.Success)
            {
                StatusMessage = IsEditing ? "TAG atualizada com sucesso." : "TAG criada com sucesso.";
                await LoadAsync();
                await OpenCreateModal();
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

    public async Task DeleteTag()
    {
        if (SelectedTag == null)
        {
            ErrorMessage = "Selecione uma TAG para excluir.";
            return;
        }

        IsLoading = true;
        ErrorMessage = string.Empty;

        try
        {
            var result = await _configService.DeleteTagAsync(int.Parse(SelectedTag.Id));
            if (result.Success)
            {
                StatusMessage = "TAG excluída com sucesso.";
                await LoadAsync();
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
}

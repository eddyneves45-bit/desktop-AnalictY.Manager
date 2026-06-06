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
    private string _selectedMachine = "Todas";
    private string _selectedDriver = "Todos";
    private string _searchText = string.Empty;
    private string _totalTags = "-";
    private string _activeTags = "-";
    private string _staleTags = "-";
    private string _errorTags = "-";
    private string _statusMessage = "Carregando tags...";
    private string _errorMessage = string.Empty;

    public TagsViewModel(ConfigService configService)
    {
        _configService = configService;
        LoadTagsCommand = new RelayCommand(async _ => await LoadTagsAsync());
        RefreshCommand = new RelayCommand(async _ => await LoadTagsAsync());

        Machines = new ObservableCollection<string> { "Todas" };
        Drivers = new ObservableCollection<string> { "Todos", "OPC UA", "MQTT", "HTTP", "Modbus" };
        TagRows = new ObservableCollection<TagRow>();

        _ = LoadTagsAsync();
    }

    public bool IsLoading
    {
        get => _isLoading;
        set => SetProperty(ref _isLoading, value);
    }

    public string SelectedMachine
    {
        get => _selectedMachine;
        set => SetProperty(ref _selectedMachine, value);
    }

    public string SelectedDriver
    {
        get => _selectedDriver;
        set => SetProperty(ref _selectedDriver, value);
    }

    public string SearchText
    {
        get => _searchText;
        set => SetProperty(ref _searchText, value);
    }

    public string TotalTags
    {
        get => _totalTags;
        set => SetProperty(ref _totalTags, value);
    }

    public string ActiveTags
    {
        get => _activeTags;
        set => SetProperty(ref _activeTags, value);
    }

    public string StaleTags
    {
        get => _staleTags;
        set => SetProperty(ref _staleTags, value);
    }

    public string ErrorTags
    {
        get => _errorTags;
        set => SetProperty(ref _errorTags, value);
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

    public ObservableCollection<string> Machines { get; }
    public ObservableCollection<string> Drivers { get; }
    public ObservableCollection<TagRow> TagRows { get; }
    public ICommand LoadTagsCommand { get; }
    public ICommand RefreshCommand { get; }

    private async Task LoadTagsAsync()
    {
        IsLoading = true;
        StatusMessage = "Carregando tags...";
        ErrorMessage = string.Empty;

        try
        {
            // Load machine folders
            var foldersResult = await _configService.GetMachineFoldersAsync();
            if (foldersResult.Error == null)
            {
                Machines.Clear();
                Machines.Add("Todas");
                foreach (var folder in foldersResult.Folders)
                {
                    Machines.Add(folder);
                }
            }

            // Load tags
            var tagsResult = await _configService.GetTagsAsync();
            if (tagsResult.Error != null)
            {
                ErrorMessage = tagsResult.Error;
                StatusMessage = "Erro ao carregar tags.";
                return;
            }

            TagRows.Clear();
            foreach (var tag in tagsResult.Tags)
            {
                TagRows.Add(new TagRow
                {
                    Name = tag.Name,
                    Machine = "Desconhecido",
                    Driver = "Desconhecido",
                    Address = tag.Address,
                    Value = "-",
                    Quality = "Good",
                    UpdatedAt = "-"
                });
            }

            TotalTags = tagsResult.Tags.Count.ToString();
            ActiveTags = tagsResult.Tags.Count.ToString();
            StaleTags = "0";
            ErrorTags = "0";
            StatusMessage = $"{tagsResult.Tags.Count} tags carregadas.";
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            StatusMessage = "Erro ao carregar tags.";
        }
        finally
        {
            IsLoading = false;
        }
    }
}

public sealed class TagRow
{
    public string Name { get; set; } = string.Empty;
    public string Machine { get; set; } = string.Empty;
    public string Driver { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string Quality { get; set; } = string.Empty;
    public string UpdatedAt { get; set; } = string.Empty;
}

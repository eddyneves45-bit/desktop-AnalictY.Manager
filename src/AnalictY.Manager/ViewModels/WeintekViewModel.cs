using System.Collections.ObjectModel;
using System.Windows.Input;
using AnalictY.Manager.Infrastructure;
using AnalictY.Manager.Services;
using AnalictY.Manager.Models;

namespace AnalictY.Manager.ViewModels;

public sealed class WeintekViewModel : ObservableObject
{
    private readonly ConfigService _configService;
    private bool _isLoading;
    private string _statusMessage = "Carregando configuração Weintek...";
    private string _errorMessage = string.Empty;
    private string _search = string.Empty;
    private DiscoveredTag? _selectedTag;
    private string _generatedToken = string.Empty;

    // Config fields
    private string _name = "FHDX Weintek";
    private string _gateway = "FHDX_01";
    private string _fhdxIp = string.Empty;
    private string _endpointPath = "/api/weintek/ingest";
    private bool _enabled = true;
    private bool _enforceSourceIp = false;
    private bool _gatewayTokenRequired = false;

    public WeintekViewModel(ConfigService configService)
    {
        _configService = configService;
        RefreshCommand = new RelayCommand(Refresh);
        SaveConfigCommand = new RelayCommand(SaveConfig);
        GenerateTokenCommand = new RelayCommand(GenerateToken);
        RevokeTokenCommand = new RelayCommand(RevokeToken);
        CreateTagCommand = new RelayCommand(CreateTag);

        Tags = new ObservableCollection<DiscoveredTag>();
        FilteredTags = new ObservableCollection<DiscoveredTag>();
    }

    public ObservableCollection<DiscoveredTag> Tags { get; }
    public ObservableCollection<DiscoveredTag> FilteredTags { get; }

    public ICommand RefreshCommand { get; }
    public ICommand SaveConfigCommand { get; }
    public ICommand GenerateTokenCommand { get; }
    public ICommand RevokeTokenCommand { get; }
    public ICommand CreateTagCommand { get; }

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

    public DiscoveredTag? SelectedTag
    {
        get => _selectedTag;
        set => SetProperty(ref _selectedTag, value);
    }

    // Config fields
    public string Name
    {
        get => _name;
        set => SetProperty(ref _name, value);
    }

    public string Gateway
    {
        get => _gateway;
        set => SetProperty(ref _gateway, value);
    }

    public string FhdxIp
    {
        get => _fhdxIp;
        set => SetProperty(ref _fhdxIp, value);
    }

    public string EndpointPath
    {
        get => _endpointPath;
        set => SetProperty(ref _endpointPath, value);
    }

    public bool Enabled
    {
        get => _enabled;
        set => SetProperty(ref _enabled, value);
    }

    public bool EnforceSourceIp
    {
        get => _enforceSourceIp;
        set => SetProperty(ref _enforceSourceIp, value);
    }

    public bool GatewayTokenRequired
    {
        get => _gatewayTokenRequired;
        set => SetProperty(ref _gatewayTokenRequired, value);
    }

    public string GeneratedToken
    {
        get => _generatedToken;
        set => SetProperty(ref _generatedToken, value);
    }

    public WeintekConfig Config { get; private set; } = new();

    private async Task OnRefreshAsync()
    {
        await LoadAsync();
    }

    public Task Refresh() => OnRefreshAsync();

    public async Task LoadAsync()
    {
        IsLoading = true;
        ErrorMessage = string.Empty;
        StatusMessage = "Carregando configuração Weintek...";

        try
        {
            var config = await _configService.GetWeintekConfigAsync();
            Config = config;
            Name = config.Name;
            Gateway = config.Gateway;
            FhdxIp = config.FhdxIp;
            EndpointPath = config.EndpointPath;
            Enabled = config.Enabled;
            EnforceSourceIp = config.EnforceSourceIp;
            GatewayTokenRequired = config.GatewayTokenRequired;

            await LoadBrowserAsync();
            StatusMessage = "Configuração Weintek carregada.";
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

    public async Task LoadBrowserAsync()
    {
        try
        {
            var result = await _configService.GetWeintekBrowserAsync();
            if (!string.IsNullOrWhiteSpace(result.Error))
            {
                ErrorMessage = result.Error;
                return;
            }

            Tags.Clear();
            foreach (var tag in result.Tags)
            {
                Tags.Add(tag);
            }

            FilterTags();
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
    }

    private void FilterTags()
    {
        var filtered = string.IsNullOrWhiteSpace(_search)
            ? Tags.ToList()
            : Tags.Where(t =>
                (t.Tag?.Contains(_search, StringComparison.OrdinalIgnoreCase) ?? false) ||
                (t.Address?.Contains(_search, StringComparison.OrdinalIgnoreCase) ?? false) ||
                (t.Machine?.Contains(_search, StringComparison.OrdinalIgnoreCase) ?? false))
            .ToList();

        FilteredTags.Clear();
        foreach (var tag in filtered)
        {
            FilteredTags.Add(tag);
        }
    }

    public async Task SaveConfig()
    {
        IsLoading = true;
        ErrorMessage = string.Empty;

        try
        {
            var config = new WeintekConfig
            {
                Name = Name,
                Gateway = Gateway,
                FhdxIp = FhdxIp,
                EndpointPath = EndpointPath,
                Enabled = Enabled,
                EnforceSourceIp = EnforceSourceIp,
                GatewayTokenRequired = GatewayTokenRequired
            };

            var result = await _configService.UpdateWeintekConfigAsync(config);
            if (result.Success)
            {
                StatusMessage = "Configuração Weintek salva com sucesso.";
                await LoadAsync();
            }
            else
            {
                ErrorMessage = result.Message ?? "Erro ao salvar configuração.";
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

    public async Task GenerateToken()
    {
        IsLoading = true;
        ErrorMessage = string.Empty;
        GeneratedToken = string.Empty;

        try
        {
            var result = await _configService.GenerateWeintekTokenAsync();
            if (result.Success)
            {
                StatusMessage = "Token gerado com sucesso.";
                await LoadAsync();
            }
            else
            {
                ErrorMessage = result.Message ?? "Erro ao gerar token.";
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

    public async Task RevokeToken()
    {
        IsLoading = true;
        ErrorMessage = string.Empty;

        try
        {
            var result = await _configService.RevokeWeintekTokenAsync();
            if (result.Success)
            {
                StatusMessage = "Token revogado com sucesso.";
                await LoadAsync();
            }
            else
            {
                ErrorMessage = result.Message ?? "Erro ao revogar token.";
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

    public async Task CreateTag()
    {
        if (SelectedTag == null)
        {
            ErrorMessage = "Selecione uma TAG para criar.";
            return;
        }

        IsLoading = true;
        ErrorMessage = string.Empty;

        try
        {
            var tagName = SelectedTag.Tag.Replace(" ", "_");
            var result = await _configService.CreateWeintekTagAsync(tagName, SelectedTag.Address, SelectedTag.DataType);
            if (result.Success)
            {
                StatusMessage = "TAG criada com sucesso.";
                await LoadBrowserAsync();
            }
            else
            {
                ErrorMessage = result.Message ?? "Erro ao criar TAG.";
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

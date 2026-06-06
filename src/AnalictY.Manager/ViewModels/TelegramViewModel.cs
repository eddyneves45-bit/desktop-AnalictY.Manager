using System.Collections.ObjectModel;
using System.Windows.Input;
using AnalictY.Manager.Infrastructure;
using AnalictY.Manager.Services;
using AnalictY.Manager.Models;

namespace AnalictY.Manager.ViewModels;

public sealed class TelegramViewModel : ObservableObject
{
    private readonly ConfigService _configService;
    private bool _isLoading;
    private string _statusMessage = "Carregando Telegram...";
    private string _errorMessage = string.Empty;
    private TelegramConnection? _selectedConnection;
    private TelegramRecipient? _selectedRecipient;

    // Connection form fields
    private string _connectionName = "Telegram Produção";
    private string _botToken = string.Empty;
    private string _defaultChatId = string.Empty;
    private int _cooldownMinutes = 15;
    private bool _isActive = true;

    // Recipient form fields
    private int _connectionId;
    private string _recipientName = string.Empty;
    private string _chatId = string.Empty;
    private string _destinationType = "user";
    private bool _recipientIsActive = true;

    public TelegramViewModel(ConfigService configService)
    {
        _configService = configService;
        RefreshCommand = new RelayCommand(Refresh);
        CreateConnectionCommand = new RelayCommand(OpenCreateConnectionModal);
        EditConnectionCommand = new RelayCommand(OpenEditConnectionModal);
        DeleteConnectionCommand = new RelayCommand(DeleteConnection);
        SaveConnectionCommand = new RelayCommand(SaveConnection);
        CreateRecipientCommand = new RelayCommand(OpenCreateRecipientModal);
        EditRecipientCommand = new RelayCommand(OpenEditRecipientModal);
        DeleteRecipientCommand = new RelayCommand(DeleteRecipient);
        SaveRecipientCommand = new RelayCommand(SaveRecipient);
        TestTelegramCommand = new RelayCommand(TestTelegram);

        Connections = new ObservableCollection<TelegramConnection>();
        Recipients = new ObservableCollection<TelegramRecipient>();
    }

    public ObservableCollection<TelegramConnection> Connections { get; }
    public ObservableCollection<TelegramRecipient> Recipients { get; }

    public ICommand RefreshCommand { get; }
    public ICommand CreateConnectionCommand { get; }
    public ICommand EditConnectionCommand { get; }
    public ICommand DeleteConnectionCommand { get; }
    public ICommand SaveConnectionCommand { get; }
    public ICommand CreateRecipientCommand { get; }
    public ICommand EditRecipientCommand { get; }
    public ICommand DeleteRecipientCommand { get; }
    public ICommand SaveRecipientCommand { get; }
    public ICommand TestTelegramCommand { get; }

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

    public TelegramConnection? SelectedConnection
    {
        get => _selectedConnection;
        set => SetProperty(ref _selectedConnection, value);
    }

    public TelegramRecipient? SelectedRecipient
    {
        get => _selectedRecipient;
        set => SetProperty(ref _selectedRecipient, value);
    }

    // Connection form fields
    public string ConnectionName
    {
        get => _connectionName;
        set => SetProperty(ref _connectionName, value);
    }

    public string BotToken
    {
        get => _botToken;
        set => SetProperty(ref _botToken, value);
    }

    public string DefaultChatId
    {
        get => _defaultChatId;
        set => SetProperty(ref _defaultChatId, value);
    }

    public int CooldownMinutes
    {
        get => _cooldownMinutes;
        set => SetProperty(ref _cooldownMinutes, value);
    }

    public bool IsActive
    {
        get => _isActive;
        set => SetProperty(ref _isActive, value);
    }

    // Recipient form fields
    public int ConnectionId
    {
        get => _connectionId;
        set => SetProperty(ref _connectionId, value);
    }

    public string RecipientName
    {
        get => _recipientName;
        set => SetProperty(ref _recipientName, value);
    }

    public string ChatId
    {
        get => _chatId;
        set => SetProperty(ref _chatId, value);
    }

    public string DestinationType
    {
        get => _destinationType;
        set => SetProperty(ref _destinationType, value);
    }

    public bool RecipientIsActive
    {
        get => _recipientIsActive;
        set => SetProperty(ref _recipientIsActive, value);
    }

    public bool IsEditingConnection => SelectedConnection != null;
    public bool IsEditingRecipient => SelectedRecipient != null;

    private async Task OnRefreshAsync()
    {
        await LoadAsync();
    }

    public Task Refresh() => OnRefreshAsync();

    public async Task LoadAsync()
    {
        IsLoading = true;
        ErrorMessage = string.Empty;
        StatusMessage = "Carregando Telegram...";

        try
        {
            var connectionsResult = await _configService.GetTelegramConnectionsAsync();
            if (!string.IsNullOrWhiteSpace(connectionsResult.Error))
            {
                ErrorMessage = connectionsResult.Error;
                return;
            }

            Connections.Clear();
            foreach (var connection in connectionsResult.Connections)
            {
                Connections.Add(connection);
            }

            var recipientsResult = await _configService.GetTelegramRecipientsAsync();
            if (!string.IsNullOrWhiteSpace(recipientsResult.Error))
            {
                ErrorMessage = recipientsResult.Error;
                return;
            }

            Recipients.Clear();
            foreach (var recipient in recipientsResult.Recipients)
            {
                Recipients.Add(recipient);
            }

            StatusMessage = $"{Connections.Count} conexão(ões), {Recipients.Count} destinatário(s) carregado(s).";
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

    public Task OpenCreateConnectionModal()
    {
        SelectedConnection = null;
        ConnectionName = "Telegram Produção";
        BotToken = string.Empty;
        DefaultChatId = string.Empty;
        CooldownMinutes = 15;
        IsActive = true;

        OnPropertyChanged(nameof(IsEditingConnection));
        return Task.CompletedTask;
    }

    public Task OpenEditConnectionModal()
    {
        if (SelectedConnection == null) return Task.CompletedTask;

        ConnectionName = SelectedConnection.Name;
        BotToken = string.Empty;
        DefaultChatId = SelectedConnection.DefaultChatId ?? string.Empty;
        CooldownMinutes = SelectedConnection.CooldownMinutes;
        IsActive = SelectedConnection.IsActive;

        OnPropertyChanged(nameof(IsEditingConnection));
        return Task.CompletedTask;
    }

    public async Task SaveConnection()
    {
        if (string.IsNullOrWhiteSpace(ConnectionName))
        {
            ErrorMessage = "Preencha o nome da conexão.";
            return;
        }

        IsLoading = true;
        ErrorMessage = string.Empty;

        try
        {
            var request = new TelegramConnectionRequest
            {
                Name = ConnectionName,
                BotToken = string.IsNullOrWhiteSpace(BotToken) ? null : BotToken,
                DefaultChatId = string.IsNullOrWhiteSpace(DefaultChatId) ? null : DefaultChatId,
                CooldownMinutes = CooldownMinutes,
                IsActive = IsActive
            };

            OperationResult result;
            if (IsEditingConnection && SelectedConnection != null)
            {
                result = await _configService.UpdateTelegramConnectionAsync(SelectedConnection.Id, request);
            }
            else
            {
                result = await _configService.CreateTelegramConnectionAsync(request);
            }

            if (result.Success)
            {
                StatusMessage = IsEditingConnection ? "Conexão atualizada com sucesso." : "Conexão criada com sucesso.";
                await OpenCreateConnectionModal();
                await LoadAsync();
            }
            else
            {
                ErrorMessage = result.Message ?? "Erro ao salvar conexão.";
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

    public async Task DeleteConnection()
    {
        if (SelectedConnection == null)
        {
            ErrorMessage = "Selecione uma conexão para excluir.";
            return;
        }

        IsLoading = true;
        ErrorMessage = string.Empty;

        try
        {
            var result = await _configService.DeleteTelegramConnectionAsync(SelectedConnection.Id);
            if (result.Success)
            {
                StatusMessage = "Conexão excluída com sucesso.";
                SelectedConnection = null;
                await LoadAsync();
            }
            else
            {
                ErrorMessage = result.Message ?? "Erro ao excluir conexão.";
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

    public Task OpenCreateRecipientModal()
    {
        SelectedRecipient = null;
        ConnectionId = Connections.FirstOrDefault()?.Id ?? 0;
        RecipientName = string.Empty;
        ChatId = string.Empty;
        DestinationType = "user";
        RecipientIsActive = true;

        OnPropertyChanged(nameof(IsEditingRecipient));
        return Task.CompletedTask;
    }

    public Task OpenEditRecipientModal()
    {
        if (SelectedRecipient == null) return Task.CompletedTask;

        ConnectionId = SelectedRecipient.ConnectionId;
        RecipientName = SelectedRecipient.Name;
        ChatId = SelectedRecipient.ChatId;
        DestinationType = SelectedRecipient.DestinationType;
        RecipientIsActive = SelectedRecipient.IsActive;

        OnPropertyChanged(nameof(IsEditingRecipient));
        return Task.CompletedTask;
    }

    public async Task SaveRecipient()
    {
        if (string.IsNullOrWhiteSpace(RecipientName) || string.IsNullOrWhiteSpace(ChatId))
        {
            ErrorMessage = "Preencha nome e chat_id.";
            return;
        }

        IsLoading = true;
        ErrorMessage = string.Empty;

        try
        {
            var request = new TelegramRecipientRequest
            {
                ConnectionId = ConnectionId,
                Name = RecipientName,
                ChatId = ChatId,
                DestinationType = DestinationType,
                IsActive = RecipientIsActive
            };

            OperationResult result;
            if (IsEditingRecipient && SelectedRecipient != null)
            {
                result = await _configService.UpdateTelegramRecipientAsync(SelectedRecipient.Id, request);
            }
            else
            {
                result = await _configService.CreateTelegramRecipientAsync(request);
            }

            if (result.Success)
            {
                StatusMessage = IsEditingRecipient ? "Destinatário atualizado com sucesso." : "Destinatário criado com sucesso.";
                await OpenCreateRecipientModal();
                await LoadAsync();
            }
            else
            {
                ErrorMessage = result.Message ?? "Erro ao salvar destinatário.";
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

    public async Task DeleteRecipient()
    {
        if (SelectedRecipient == null)
        {
            ErrorMessage = "Selecione um destinatário para excluir.";
            return;
        }

        IsLoading = true;
        ErrorMessage = string.Empty;

        try
        {
            var result = await _configService.DeleteTelegramRecipientAsync(SelectedRecipient.Id);
            if (result.Success)
            {
                StatusMessage = "Destinatário excluído com sucesso.";
                SelectedRecipient = null;
                await LoadAsync();
            }
            else
            {
                ErrorMessage = result.Message ?? "Erro ao excluir destinatário.";
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

    public async Task TestTelegram()
    {
        IsLoading = true;
        ErrorMessage = string.Empty;

        try
        {
            var connectionId = SelectedConnection?.Id;
            var recipientId = SelectedRecipient?.Id;

            var result = await _configService.TestTelegramAsync(connectionId, recipientId);
            if (result.Success)
            {
                StatusMessage = "Teste Telegram enviado com sucesso.";
            }
            else
            {
                ErrorMessage = result.Message ?? "Erro ao enviar teste.";
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

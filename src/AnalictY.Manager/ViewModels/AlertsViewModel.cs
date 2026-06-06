using System.Collections.ObjectModel;
using System.Windows.Input;
using AnalictY.Manager.Infrastructure;
using AnalictY.Manager.Services;
using AnalictY.Manager.Models;

namespace AnalictY.Manager.ViewModels;

public sealed class AlertsViewModel : ObservableObject
{
    private readonly ConfigService _configService;
    private bool _isLoading;
    private string _statusMessage = "Carregando alertas...";
    private string _errorMessage = string.Empty;
    private string _search = string.Empty;
    private AlertItem? _selectedAlert;
    private AlertRule? _selectedRule;
    private int _retentionDays = 1;

    // Rule form fields
    private string _ruleName = string.Empty;
    private int _tagConfigId;
    private string _operator = ">";
    private double _limitValue;
    private string _severity = "medium";
    private string _message = string.Empty;
    private int? _telegramConnectionId;
    private bool _isActive = true;

    public AlertsViewModel(ConfigService configService)
    {
        _configService = configService;
        RefreshCommand = new RelayCommand(Refresh);
        CreateRuleCommand = new RelayCommand(OpenCreateRuleModal);
        EditRuleCommand = new RelayCommand(OpenEditRuleModal);
        DeleteRuleCommand = new RelayCommand(DeleteRule);
        SaveRuleCommand = new RelayCommand(SaveRule);
        AcknowledgeAlertCommand = new RelayCommand(AcknowledgeAlert);

        Alerts = new ObservableCollection<AlertItem>();
        FilteredAlerts = new ObservableCollection<AlertItem>();
        Rules = new ObservableCollection<AlertRule>();
    }

    public ObservableCollection<AlertItem> Alerts { get; }
    public ObservableCollection<AlertItem> FilteredAlerts { get; }
    public ObservableCollection<AlertRule> Rules { get; }

    public ICommand RefreshCommand { get; }
    public ICommand CreateRuleCommand { get; }
    public ICommand EditRuleCommand { get; }
    public ICommand DeleteRuleCommand { get; }
    public ICommand SaveRuleCommand { get; }
    public ICommand AcknowledgeAlertCommand { get; }

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
                FilterAlerts();
            }
        }
    }

    public AlertItem? SelectedAlert
    {
        get => _selectedAlert;
        set => SetProperty(ref _selectedAlert, value);
    }

    public AlertRule? SelectedRule
    {
        get => _selectedRule;
        set => SetProperty(ref _selectedRule, value);
    }

    public int RetentionDays
    {
        get => _retentionDays;
        set => SetProperty(ref _retentionDays, value);
    }

    // Rule form fields
    public string RuleName
    {
        get => _ruleName;
        set => SetProperty(ref _ruleName, value);
    }

    public int TagConfigId
    {
        get => _tagConfigId;
        set => SetProperty(ref _tagConfigId, value);
    }

    public string Operator
    {
        get => _operator;
        set => SetProperty(ref _operator, value);
    }

    public double LimitValue
    {
        get => _limitValue;
        set => SetProperty(ref _limitValue, value);
    }

    public string Severity
    {
        get => _severity;
        set => SetProperty(ref _severity, value);
    }

    public string Message
    {
        get => _message;
        set => SetProperty(ref _message, value);
    }

    public int? TelegramConnectionId
    {
        get => _telegramConnectionId;
        set => SetProperty(ref _telegramConnectionId, value);
    }

    public bool IsActive
    {
        get => _isActive;
        set => SetProperty(ref _isActive, value);
    }

    public bool IsEditingRule => SelectedRule != null;

    private async Task OnRefreshAsync()
    {
        await LoadAsync();
    }

    public Task Refresh() => OnRefreshAsync();

    public async Task LoadAsync()
    {
        IsLoading = true;
        ErrorMessage = string.Empty;
        StatusMessage = "Carregando alertas e regras...";

        try
        {
            var alertsResult = await _configService.GetAlertsAsync(20);
            if (!string.IsNullOrWhiteSpace(alertsResult.Error))
            {
                ErrorMessage = alertsResult.Error;
                return;
            }

            Alerts.Clear();
            foreach (var alert in alertsResult.Alerts)
            {
                Alerts.Add(alert);
            }

            var rulesResult = await _configService.GetAlertRulesAsync();
            if (!string.IsNullOrWhiteSpace(rulesResult.Error))
            {
                ErrorMessage = rulesResult.Error;
                return;
            }

            Rules.Clear();
            foreach (var rule in rulesResult.Rules)
            {
                Rules.Add(rule);
            }

            FilterAlerts();
            StatusMessage = $"{Alerts.Count} alerta(s), {Rules.Count} regra(s) carregada(s).";
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

    private void FilterAlerts()
    {
        var filtered = string.IsNullOrWhiteSpace(_search)
            ? Alerts.ToList()
            : Alerts.Where(a =>
                (a.Title?.Contains(_search, StringComparison.OrdinalIgnoreCase) ?? false) ||
                (a.Message?.Contains(_search, StringComparison.OrdinalIgnoreCase) ?? false) ||
                (a.Severity?.Contains(_search, StringComparison.OrdinalIgnoreCase) ?? false))
            .ToList();

        FilteredAlerts.Clear();
        foreach (var alert in filtered)
        {
            FilteredAlerts.Add(alert);
        }
    }

    public Task OpenCreateRuleModal()
    {
        SelectedRule = null;
        RuleName = string.Empty;
        TagConfigId = 0;
        Operator = ">";
        LimitValue = 0;
        Severity = "medium";
        Message = string.Empty;
        TelegramConnectionId = null;
        IsActive = true;

        OnPropertyChanged(nameof(IsEditingRule));
        return Task.CompletedTask;
    }

    public Task OpenEditRuleModal()
    {
        if (SelectedRule == null) return Task.CompletedTask;

        RuleName = SelectedRule.Name;
        TagConfigId = SelectedRule.TagConfigId;
        Operator = SelectedRule.Operator;
        LimitValue = SelectedRule.LimitValue;
        Severity = SelectedRule.Severity;
        Message = SelectedRule.Message;
        TelegramConnectionId = SelectedRule.TelegramConnectionId;
        IsActive = SelectedRule.IsActive;

        OnPropertyChanged(nameof(IsEditingRule));
        return Task.CompletedTask;
    }

    public async Task SaveRule()
    {
        if (string.IsNullOrWhiteSpace(RuleName) || TagConfigId == 0)
        {
            ErrorMessage = "Preencha nome e TAG.";
            return;
        }

        IsLoading = true;
        ErrorMessage = string.Empty;

        try
        {
            var request = new AlertRuleRequest
            {
                Name = RuleName,
                TagConfigId = TagConfigId,
                Operator = Operator,
                LimitValue = LimitValue,
                Severity = Severity,
                Message = Message,
                TelegramConnectionId = TelegramConnectionId,
                TelegramRecipientIds = new List<int>(),
                IsActive = IsActive
            };

            OperationResult result;
            if (IsEditingRule && SelectedRule != null)
            {
                result = await _configService.UpdateAlertRuleAsync(SelectedRule.Id, request);
            }
            else
            {
                result = await _configService.CreateAlertRuleAsync(request);
            }

            if (result.Success)
            {
                StatusMessage = IsEditingRule ? "Regra atualizada com sucesso." : "Regra criada com sucesso.";
                await OpenCreateRuleModal();
                await LoadAsync();
            }
            else
            {
                ErrorMessage = result.Message ?? "Erro ao salvar regra.";
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

    public async Task DeleteRule()
    {
        if (SelectedRule == null)
        {
            ErrorMessage = "Selecione uma regra para excluir.";
            return;
        }

        IsLoading = true;
        ErrorMessage = string.Empty;

        try
        {
            var result = await _configService.DeleteAlertRuleAsync(SelectedRule.Id);
            if (result.Success)
            {
                StatusMessage = "Regra excluída com sucesso.";
                SelectedRule = null;
                await LoadAsync();
            }
            else
            {
                ErrorMessage = result.Message ?? "Erro ao excluir regra.";
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

    public async Task AcknowledgeAlert()
    {
        if (SelectedAlert == null)
        {
            ErrorMessage = "Selecione um alerta para reconhecer.";
            return;
        }

        IsLoading = true;
        ErrorMessage = string.Empty;

        try
        {
            var result = await _configService.AcknowledgeAlertAsync(SelectedAlert.Id, "admin");
            if (result.Success)
            {
                StatusMessage = "Alerta reconhecido com sucesso.";
                await LoadAsync();
            }
            else
            {
                ErrorMessage = result.Message ?? "Erro ao reconhecer alerta.";
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

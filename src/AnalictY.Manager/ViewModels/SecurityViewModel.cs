using System.Windows.Input;
using AnalictY.Manager.Infrastructure;
using AnalictY.Manager.Services;

namespace AnalictY.Manager.ViewModels;

public sealed class SecurityViewModel : ObservableObject
{
    private readonly SecurityService _securityService;
    private bool _isLoading;
    private bool _isMfaEnabled;
    private bool _hasSetup;
    private string _statusMessage = "Carregue o status de seguranca para consultar o MFA.";
    private string _errorMessage = string.Empty;
    private string _secret = string.Empty;
    private string _otpAuthUrl = string.Empty;
    private string _mfaCode = string.Empty;

    public SecurityViewModel(SecurityService securityService)
    {
        _securityService = securityService;
        RefreshCommand = new RelayCommand(_ => LoadAsync());
        StartSetupCommand = new RelayCommand(_ => StartSetupAsync());
        EnableCommand = new RelayCommand(_ => EnableAsync());
        DisableCommand = new RelayCommand(_ => DisableAsync());
    }

    public bool IsLoading { get => _isLoading; private set => SetProperty(ref _isLoading, value); }
    public bool IsMfaEnabled { get => _isMfaEnabled; private set => SetProperty(ref _isMfaEnabled, value); }
    public bool HasSetup { get => _hasSetup; private set => SetProperty(ref _hasSetup, value); }
    public string StatusMessage { get => _statusMessage; private set => SetProperty(ref _statusMessage, value); }
    public string ErrorMessage { get => _errorMessage; private set => SetProperty(ref _errorMessage, value); }
    public string Secret { get => _secret; private set => SetProperty(ref _secret, value); }
    public string OtpAuthUrl { get => _otpAuthUrl; private set => SetProperty(ref _otpAuthUrl, value); }

    public string MfaCode
    {
        get => _mfaCode;
        set
        {
            var normalized = new string((value ?? string.Empty).Where(char.IsDigit).Take(6).ToArray());
            SetProperty(ref _mfaCode, normalized);
        }
    }

    public string MfaStateLabel => IsMfaEnabled ? "Ativo" : "Desativado";

    public ICommand RefreshCommand { get; }
    public ICommand StartSetupCommand { get; }
    public ICommand EnableCommand { get; }
    public ICommand DisableCommand { get; }

    public async Task LoadAsync()
    {
        IsLoading = true;
        ErrorMessage = string.Empty;
        StatusMessage = "Consultando MFA...";
        try
        {
            var result = await _securityService.GetMfaStatusAsync();
            if (!result.Success)
            {
                ErrorMessage = result.Error;
                StatusMessage = "Nao foi possivel carregar o status MFA.";
                return;
            }

            IsMfaEnabled = result.Enabled;
            HasSetup = false;
            Secret = string.Empty;
            OtpAuthUrl = string.Empty;
            MfaCode = string.Empty;
            OnPropertyChanged(nameof(MfaStateLabel));
            StatusMessage = IsMfaEnabled ? "MFA ativo para o usuario atual." : "MFA desativado para o usuario atual.";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task StartSetupAsync()
    {
        IsLoading = true;
        ErrorMessage = string.Empty;
        StatusMessage = "Gerando chave MFA...";
        try
        {
            var result = await _securityService.StartMfaSetupAsync();
            if (!result.Success)
            {
                ErrorMessage = result.Error;
                StatusMessage = "Nao foi possivel iniciar o MFA.";
                return;
            }

            HasSetup = true;
            Secret = result.Secret;
            OtpAuthUrl = result.OtpAuthUrl;
            MfaCode = string.Empty;
            StatusMessage = "Adicione a chave no aplicativo autenticador e informe o codigo gerado.";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task EnableAsync()
    {
        IsLoading = true;
        ErrorMessage = string.Empty;
        try
        {
            var result = await _securityService.EnableMfaAsync(MfaCode);
            if (!result.Success)
            {
                ErrorMessage = result.Message;
                return;
            }

            StatusMessage = result.Message;
            await LoadAsync();
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task DisableAsync()
    {
        IsLoading = true;
        ErrorMessage = string.Empty;
        try
        {
            var result = await _securityService.DisableMfaAsync(MfaCode);
            if (!result.Success)
            {
                ErrorMessage = result.Message;
                return;
            }

            StatusMessage = result.Message;
            await LoadAsync();
        }
        finally
        {
            IsLoading = false;
        }
    }
}

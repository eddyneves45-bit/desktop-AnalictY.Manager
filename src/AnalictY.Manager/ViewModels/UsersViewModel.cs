using System.Collections.ObjectModel;
using System.Windows.Input;
using AnalictY.Manager.Infrastructure;
using AnalictY.Manager.Services;
using AnalictY.Manager.Models;

namespace AnalictY.Manager.ViewModels;

public sealed class UsersViewModel : ObservableObject
{
    private readonly ConfigService _configService;
    private bool _isLoading;
    private string _statusMessage = "Carregando usuários...";
    private string _errorMessage = string.Empty;
    private User? _selectedUser;

    // User form fields
    private string _username = string.Empty;
    private string _email = string.Empty;
    private string _password = string.Empty;
    private string _role = "user";
    private bool _isActive = true;
    private bool _mfaRequired = false;

    // Permissions
    private bool _permGoalsManage = false;
    private bool _permReportsDownload = false;
    private bool _permAlertRulesManage = false;
    private bool _permUsersManage = false;
    private bool _permAuditView = false;

    public UsersViewModel(ConfigService configService)
    {
        _configService = configService;
        RefreshCommand = new RelayCommand(Refresh);
        CreateUserCommand = new RelayCommand(OpenCreateUserModal);
        EditUserCommand = new RelayCommand(OpenEditUserModal);
        DeleteUserCommand = new RelayCommand(DeleteUser);
        SaveUserCommand = new RelayCommand(SaveUser);

        Users = new ObservableCollection<User>();
    }

    public ObservableCollection<User> Users { get; }

    public ICommand RefreshCommand { get; }
    public ICommand CreateUserCommand { get; }
    public ICommand EditUserCommand { get; }
    public ICommand DeleteUserCommand { get; }
    public ICommand SaveUserCommand { get; }

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

    public User? SelectedUser
    {
        get => _selectedUser;
        set => SetProperty(ref _selectedUser, value);
    }

    // User form fields
    public string Username
    {
        get => _username;
        set => SetProperty(ref _username, value);
    }

    public string Email
    {
        get => _email;
        set => SetProperty(ref _email, value);
    }

    public string Password
    {
        get => _password;
        set => SetProperty(ref _password, value);
    }

    public string Role
    {
        get => _role;
        set => SetProperty(ref _role, value);
    }

    public bool IsActive
    {
        get => _isActive;
        set => SetProperty(ref _isActive, value);
    }

    public bool MfaRequired
    {
        get => _mfaRequired;
        set => SetProperty(ref _mfaRequired, value);
    }

    // Permissions
    public bool PermGoalsManage
    {
        get => _permGoalsManage;
        set => SetProperty(ref _permGoalsManage, value);
    }

    public bool PermReportsDownload
    {
        get => _permReportsDownload;
        set => SetProperty(ref _permReportsDownload, value);
    }

    public bool PermAlertRulesManage
    {
        get => _permAlertRulesManage;
        set => SetProperty(ref _permAlertRulesManage, value);
    }

    public bool PermUsersManage
    {
        get => _permUsersManage;
        set => SetProperty(ref _permUsersManage, value);
    }

    public bool PermAuditView
    {
        get => _permAuditView;
        set => SetProperty(ref _permAuditView, value);
    }

    public bool IsEditingUser => SelectedUser != null;
    public bool ShowPermissions => Role == "custom";

    private async Task OnRefreshAsync()
    {
        await LoadAsync();
    }

    public Task Refresh() => OnRefreshAsync();

    public async Task LoadAsync()
    {
        IsLoading = true;
        ErrorMessage = string.Empty;
        StatusMessage = "Carregando usuários...";

        try
        {
            var result = await _configService.GetUsersAsync();
            if (!string.IsNullOrWhiteSpace(result.Error))
            {
                ErrorMessage = result.Error;
                return;
            }

            Users.Clear();
            foreach (var user in result.Users)
            {
                Users.Add(user);
            }

            StatusMessage = $"{Users.Count} usuário(s) carregado(s).";
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

    public Task OpenCreateUserModal()
    {
        SelectedUser = null;
        Username = string.Empty;
        Email = string.Empty;
        Password = string.Empty;
        Role = "user";
        IsActive = true;
        MfaRequired = false;
        ClearPermissions();

        OnPropertyChanged(nameof(IsEditingUser));
        OnPropertyChanged(nameof(ShowPermissions));
        return Task.CompletedTask;
    }

    public Task OpenEditUserModal()
    {
        if (SelectedUser == null) return Task.CompletedTask;

        Username = SelectedUser.Username;
        Email = SelectedUser.Email;
        Password = string.Empty;
        Role = SelectedUser.Role;
        IsActive = SelectedUser.IsActive;
        MfaRequired = SelectedUser.MfaRequired;

        ClearPermissions();
        foreach (var perm in SelectedUser.Permissions)
        {
            switch (perm)
            {
                case "goals.manage": PermGoalsManage = true; break;
                case "reports.download": PermReportsDownload = true; break;
                case "alert-rules.manage": PermAlertRulesManage = true; break;
                case "users.manage": PermUsersManage = true; break;
                case "audit.view": PermAuditView = true; break;
            }
        }

        OnPropertyChanged(nameof(IsEditingUser));
        OnPropertyChanged(nameof(ShowPermissions));
        return Task.CompletedTask;
    }

    private void ClearPermissions()
    {
        PermGoalsManage = false;
        PermReportsDownload = false;
        PermAlertRulesManage = false;
        PermUsersManage = false;
        PermAuditView = false;
    }

    private List<string> GetPermissions()
    {
        var permissions = new List<string>();
        if (PermGoalsManage) permissions.Add("goals.manage");
        if (PermReportsDownload) permissions.Add("reports.download");
        if (PermAlertRulesManage) permissions.Add("alert-rules.manage");
        if (PermUsersManage) permissions.Add("users.manage");
        if (PermAuditView) permissions.Add("audit.view");
        return permissions;
    }

    public async Task SaveUser()
    {
        if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Email))
        {
            ErrorMessage = "Preencha usuário e email.";
            return;
        }

        if (!IsEditingUser && string.IsNullOrWhiteSpace(Password))
        {
            ErrorMessage = "Preencha a senha para novo usuário.";
            return;
        }

        IsLoading = true;
        ErrorMessage = string.Empty;

        try
        {
            var request = new UserRequest
            {
                Username = Username,
                Email = Email,
                Password = string.IsNullOrWhiteSpace(Password) ? null : Password,
                Role = Role,
                IsActive = IsActive,
                MfaRequired = MfaRequired,
                Permissions = Role == "custom" ? GetPermissions() : new List<string>()
            };

            OperationResult result;
            if (IsEditingUser && SelectedUser != null)
            {
                result = await _configService.UpdateUserAsync(SelectedUser.Id, request);
            }
            else
            {
                result = await _configService.CreateUserAsync(request);
            }

            if (result.Success)
            {
                StatusMessage = IsEditingUser ? "Usuário atualizado com sucesso." : "Usuário criado com sucesso.";
                await OpenCreateUserModal();
                await LoadAsync();
            }
            else
            {
                ErrorMessage = result.Message ?? "Erro ao salvar usuário.";
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

    public async Task DeleteUser()
    {
        if (SelectedUser == null)
        {
            ErrorMessage = "Selecione um usuário para excluir.";
            return;
        }

        if (SelectedUser.Role == "admin")
        {
            ErrorMessage = "Não é possível excluir usuários admin.";
            return;
        }

        IsLoading = true;
        ErrorMessage = string.Empty;

        try
        {
            var result = await _configService.DeleteUserAsync(SelectedUser.Id);
            if (result.Success)
            {
                StatusMessage = "Usuário excluído com sucesso.";
                SelectedUser = null;
                await LoadAsync();
            }
            else
            {
                ErrorMessage = result.Message ?? "Erro ao excluir usuário.";
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

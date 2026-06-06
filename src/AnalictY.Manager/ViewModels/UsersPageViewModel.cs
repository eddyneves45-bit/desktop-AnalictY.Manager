using System.Collections.ObjectModel;
using System.Windows.Input;
using AnalictY.Manager.Infrastructure;
using AnalictY.Manager.Services;

namespace AnalictY.Manager.ViewModels;

public sealed class UsersPageViewModel : ObservableObject
{
    private readonly UserAdminService _service;
    private string _statusMessage = "Aguardando leitura.";
    private string _errorMessage = string.Empty;
    private string _totalUsers = "0";

    public UsersPageViewModel(UserAdminService service)
    {
        _service = service;
        Users = new ObservableCollection<UserAdminRow>();
        LoadCommand = new RelayCommand(async () => await LoadAsync());
    }

    public ObservableCollection<UserAdminRow> Users { get; }
    public ICommand LoadCommand { get; }
    public string StatusMessage { get => _statusMessage; set => SetProperty(ref _statusMessage, value); }
    public string ErrorMessage { get => _errorMessage; set => SetProperty(ref _errorMessage, value); }
    public string TotalUsers { get => _totalUsers; set => SetProperty(ref _totalUsers, value); }

    public async Task LoadAsync()
    {
        StatusMessage = "Carregando usuarios...";
        ErrorMessage = string.Empty;
        Users.Clear();

        try
        {
            var users = await _service.GetUsersAsync();
            foreach (var user in users)
            {
                Users.Add(user);
            }

            TotalUsers = users.Count.ToString();
            StatusMessage = $"{users.Count} usuarios carregados.";
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            StatusMessage = "Nao foi possivel carregar usuarios.";
        }
    }
}

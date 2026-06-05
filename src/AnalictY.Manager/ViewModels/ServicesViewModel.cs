using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using AnalictY.Manager.Infrastructure;

namespace AnalictY.Manager.ViewModels;

public sealed class ServicesViewModel : ObservableObject
{
    public ServicesViewModel()
    {
        StartCommand = new RelayCommand(_ =>
        {
            MessageBox.Show("Início de serviço será conectado ao serviço Windows quando o endpoint estiver disponível.", "AnalictY Manager", MessageBoxButton.OK, MessageBoxImage.Information);
            return Task.CompletedTask;
        });
        StopCommand = new RelayCommand(_ =>
        {
            MessageBox.Show("Parada de serviço exigirá confirmação futura e integração com o serviço Windows.", "AnalictY Manager", MessageBoxButton.OK, MessageBoxImage.Information);
            return Task.CompletedTask;
        });
        RestartCommand = new RelayCommand(_ =>
        {
            MessageBox.Show("Reinício de serviço exigirá confirmação futura e integração com o serviço Windows.", "AnalictY Manager", MessageBoxButton.OK, MessageBoxImage.Information);
            return Task.CompletedTask;
        });

        ServiceRows = new ObservableCollection<ServiceRow>
        {
            new("AnalictY API", "Em execução", "2d 14h 32m", "5000", "12345", "Running"),
            new("Runtime Service", "Em execução", "2d 14h 30m", "-", "12346", "Running"),
            new("MQTT Service", "Em execução", "2d 14h 28m", "1883", "12347", "Running"),
            new("OPC UA Service", "Em execução", "2d 14h 25m", "4840", "12348", "Running"),
            new("Database Service", "Em execução", "2d 14h 20m", "-", "12349", "Running"),
            new("WebSocket Server", "Em execução", "2d 14h 15m", "3000", "12350", "Running")
        };
    }

    public ICommand StartCommand { get; }
    public ICommand StopCommand { get; }
    public ICommand RestartCommand { get; }
    public ObservableCollection<ServiceRow> ServiceRows { get; }
}

public sealed record ServiceRow(string Name, string Status, string Uptime, string Port, string Pid, string State);

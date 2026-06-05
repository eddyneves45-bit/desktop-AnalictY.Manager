using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using AnalictY.Manager.Infrastructure;

namespace AnalictY.Manager.ViewModels;

public sealed class EventsViewModel : ObservableObject
{
    private string _eventsToday = "45";
    private string _warnings = "8";
    private string _errors = "3";
    private string _acknowledged = "38";

    public EventsViewModel()
    {
        AcknowledgeCommand = new RelayCommand(_ =>
        {
            MessageBox.Show("Reconhecimento de eventos será conectado ao endpoint quando disponível.", "AnalictY Manager", MessageBoxButton.OK, MessageBoxImage.Information);
            return Task.CompletedTask;
        });

        EventRows = new ObservableCollection<EventRow>
        {
            new("05/06/2025 14:32:15", "Error", "Runtime", "Falha na conexão OPC UA"),
            new("05/06/2025 14:30:42", "Warning", "MQTT", "Cliente desconectado inesperadamente"),
            new("05/06/2025 14:28:10", "Info", "Database", "Backup automático concluído"),
            new("05/06/2025 14:25:33", "Error", "Runtime", "Timeout na leitura da TAG M01_Producao_Contador"),
            new("05/06/2025 14:22:18", "Warning", "System", "Uso de CPU acima de 80%"),
            new("05/06/2025 14:20:05", "Info", "API", "Nova sessão iniciada"),
            new("05/06/2025 14:18:42", "Error", "Runtime", "Falha na escrita no banco de dados"),
            new("05/06/2025 14:15:30", "Info", "MQTT", "Novo cliente conectado"),
            new("05/06/2025 14:12:15", "Warning", "OPC UA", "Qualidade degradada em nó monitorado"),
            new("05/06/2025 14:10:00", "Info", "System", "Serviço iniciado com sucesso")
        };
    }

    public string EventsToday
    {
        get => _eventsToday;
        set => SetProperty(ref _eventsToday, value);
    }

    public string Warnings
    {
        get => _warnings;
        set => SetProperty(ref _warnings, value);
    }

    public string Errors
    {
        get => _errors;
        set => SetProperty(ref _errors, value);
    }

    public string Acknowledged
    {
        get => _acknowledged;
        set => SetProperty(ref _acknowledged, value);
    }

    public ICommand AcknowledgeCommand { get; }
    public ObservableCollection<EventRow> EventRows { get; }
}

public sealed record EventRow(string DateTime, string Severity, string Source, string Message);

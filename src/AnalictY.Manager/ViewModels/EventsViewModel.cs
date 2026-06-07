using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using AnalictY.Manager.Infrastructure;
using AnalictY.Manager.Services;

namespace AnalictY.Manager.ViewModels;

public sealed class EventsViewModel : ObservableObject
{
    private readonly EventsService _eventsService;
    private bool _isLoading;
    private string _statusMessage = "Carregando eventos...";
    private string _errorMessage = string.Empty;
    private string _eventsToday = "0";
    private string _warnings = "0";
    private string _errors = "0";
    private string _acknowledged = "0";

    public EventsViewModel(EventsService eventsService)
    {
        _eventsService = eventsService;
        
        AcknowledgeCommand = new RelayCommand(_ =>
        {
            MessageBox.Show("Reconhecimento de eventos não implementado nesta versão.", "AnalictY Manager", MessageBoxButton.OK, MessageBoxImage.Information);
            return Task.CompletedTask;
        });

        EventRows = new ObservableCollection<EventRow>();
        
        _ = LoadAsync();
    }

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

    public async Task LoadAsync()
    {
        IsLoading = true;
        ErrorMessage = string.Empty;
        EventRows.Clear();

        try
        {
            var result = await _eventsService.GetEventsAsync(50);
            
            if (!string.IsNullOrEmpty(result.Error))
            {
                ErrorMessage = result.Error;
                StatusMessage = "Erro ao carregar";
                return;
            }

            foreach (var evt in result.Events)
            {
                EventRows.Add(new EventRow(
                    FormatDate(evt.Timestamp),
                    evt.Level,
                    evt.Source,
                    evt.Message
                ));
            }

            EventsToday = result.Total.ToString();
            Warnings = result.Events.Count(e => e.Level == "warning").ToString();
            Errors = result.Events.Count(e => e.Level == "error").ToString();
            Acknowledged = result.Events.Count(e => e.Acknowledged).ToString();

            StatusMessage = $"Carregado {EventRows.Count} eventos";
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Erro ao carregar eventos: {ex.Message}";
            StatusMessage = "Erro ao carregar";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private static string FormatDate(string isoDate)
    {
        if (DateTime.TryParse(isoDate, out var date))
        {
            return date.ToString("dd/MM/yyyy HH:mm:ss");
        }
        return isoDate;
    }
}

public sealed record EventRow(string DateTime, string Severity, string Source, string Message);

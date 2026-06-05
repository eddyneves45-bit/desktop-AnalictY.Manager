using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using AnalictY.Manager.Infrastructure;

namespace AnalictY.Manager.ViewModels;

public sealed class BackupViewModel : ObservableObject
{
    private string _lastBackupStatus = "Completo";
    private string _lastBackupDate = "Hoje 02:00";
    private string _destination = "C:\\AnalictY\\backups";
    private string _lastBackupSize = "42.8 MB";
    private string _schedule = "Diário às 02:00";

    public BackupViewModel()
    {
        BackupNowCommand = new RelayCommand(_ =>
        {
            MessageBox.Show("Backup imediato será conectado ao endpoint quando disponível.", "AnalictY Manager", MessageBoxButton.OK, MessageBoxImage.Information);
            return Task.CompletedTask;
        });
        RestoreCommand = new RelayCommand(_ =>
        {
            MessageBox.Show("RESTAURAÇÃO REQUER CONFIRMAÇÃO: Esta ação substituirá o banco de dados atual. Implementação futura exigirá confirmação explícita do usuário.", "AnalictY Manager - AVISO", MessageBoxButton.OK, MessageBoxImage.Warning);
            return Task.CompletedTask;
        });
        OpenFolderCommand = new RelayCommand(_ =>
        {
            MessageBox.Show("Abertura da pasta de backup será conectada ao endpoint quando disponível.", "AnalictY Manager", MessageBoxButton.OK, MessageBoxImage.Information);
            return Task.CompletedTask;
        });

        BackupRows = new ObservableCollection<BackupRow>
        {
            new("analicty_backup_20250605_0200.db", "42.8 MB", "Hoje 02:00", "Completo"),
            new("analicty_backup_20250604_0200.db", "42.5 MB", "Ontem 02:00", "Completo"),
            new("analicty_backup_20250603_0200.db", "42.3 MB", "03/06 02:00", "Completo"),
            new("analicty_backup_20250602_0200.db", "42.1 MB", "02/06 02:00", "Completo"),
            new("analicty_backup_20250601_0200.db", "41.9 MB", "01/06 02:00", "Completo"),
            new("analicty_backup_20250531_0200.db", "41.7 MB", "31/05 02:00", "Completo"),
            new("analicty_backup_20250530_0200.db", "41.5 MB", "30/05 02:00", "Completo")
        };
    }

    public string LastBackupStatus
    {
        get => _lastBackupStatus;
        set => SetProperty(ref _lastBackupStatus, value);
    }

    public string LastBackupDate
    {
        get => _lastBackupDate;
        set => SetProperty(ref _lastBackupDate, value);
    }

    public string Destination
    {
        get => _destination;
        set => SetProperty(ref _destination, value);
    }

    public string LastBackupSize
    {
        get => _lastBackupSize;
        set => SetProperty(ref _lastBackupSize, value);
    }

    public string Schedule
    {
        get => _schedule;
        set => SetProperty(ref _schedule, value);
    }

    public ICommand BackupNowCommand { get; }
    public ICommand RestoreCommand { get; }
    public ICommand OpenFolderCommand { get; }
    public ObservableCollection<BackupRow> BackupRows { get; }
}

public sealed record BackupRow(string FileName, string Size, string Date, string Status);

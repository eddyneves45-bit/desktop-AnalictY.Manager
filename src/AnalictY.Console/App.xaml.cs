using System.IO;
using System.Windows.Threading;
using System.Windows;

namespace AnalictY.Console;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        try
        {
            DispatcherUnhandledException += App_DispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;

            ShutdownMode = ShutdownMode.OnMainWindowClose;
            WriteStartupLog("Startup iniciado.");

            var mainWindow = new MainWindow();
            MainWindow = mainWindow;
            mainWindow.Show();
            mainWindow.Activate();

            WriteStartupLog("MainWindow exibida.");
        }
        catch (Exception ex)
        {
            WriteStartupLog(ex.ToString());
            MessageBox.Show(
                ex.ToString(),
                "Erro ao iniciar AnalictY Console",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            Shutdown(-1);
        }
    }

    private static void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        WriteStartupLog($"DispatcherUnhandledException: {e.Exception}");
        MessageBox.Show(
            e.Exception.ToString(),
            "Erro no AnalictY Console",
            MessageBoxButton.OK,
            MessageBoxImage.Error);
        e.Handled = true;
    }

    private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        WriteStartupLog($"UnhandledException: {e.ExceptionObject}");
    }

    private static void TaskScheduler_UnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
    {
        WriteStartupLog($"UnobservedTaskException: {e.Exception}");
        e.SetObserved();
    }

    private static void WriteStartupLog(string message)
    {
        try
        {
            File.AppendAllText(
                Path.Combine(AppContext.BaseDirectory, "startup.log"),
                $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} {message}{Environment.NewLine}");
        }
        catch
        {
        }
    }
}


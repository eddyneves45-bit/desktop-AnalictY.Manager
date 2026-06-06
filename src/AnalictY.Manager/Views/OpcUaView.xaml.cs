using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using AnalictY.Manager.Infrastructure;
using AnalictY.Manager.Services;
using AnalictY.Manager.ViewModels;

namespace AnalictY.Manager.Views;

public partial class OpcUaView : UserControl
{
    private OpcUaBrowserViewModel? _viewModel;

    public OpcUaView()
    {
        InitializeComponent();
        _viewModel = new OpcUaBrowserViewModel(new ConfigService(AppServices.HttpClient));
        DataContext = _viewModel;
        Loaded += OpcUaView_Loaded;
    }

    private async void OpcUaView_Loaded(object sender, RoutedEventArgs e)
    {
        if (_viewModel != null)
        {
            await _viewModel.LoadAsync();
        }
    }

    private void NavigateBackButton_Click(object sender, RoutedEventArgs e)
    {
        var parent = VisualTreeHelper.GetParent(this);
        while (parent != null && parent is not ConfigPage)
        {
            parent = VisualTreeHelper.GetParent(parent);
        }

        if (parent is ConfigPage configPage)
        {
            configPage.ReturnToCards();
        }
    }

    private void ConnectionComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_viewModel != null && sender is ComboBox comboBox)
        {
            if (comboBox.SelectedValue is int connectionId)
            {
                _viewModel.SelectedConnectionId = connectionId;
            }
        }
    }

    private void BackButton_Click(object sender, RoutedEventArgs e)
    {
        _viewModel?.GoBack();
    }

    private void RootButton_Click(object sender, RoutedEventArgs e)
    {
        _viewModel?.GoToRoot();
    }

    private void CreateTag_Click(object sender, RoutedEventArgs e)
    {
        if (_viewModel?.SelectedNode != null)
        {
            TagNameTextBox.Text = _viewModel.AutoTagName(_viewModel.SelectedNode);
            TagModalOverlay.Visibility = Visibility.Visible;
        }
    }

    private void CancelTag_Click(object sender, RoutedEventArgs e)
    {
        TagModalOverlay.Visibility = Visibility.Collapsed;
        TagNameTextBox.Text = string.Empty;
    }

    private async void ConfirmTag_Click(object sender, RoutedEventArgs e)
    {
        if (_viewModel == null)
        {
            return;
        }

        ErrorBorder.Visibility = Visibility.Collapsed;
        _viewModel.ErrorMessage = string.Empty;

        var tagName = TagNameTextBox.Text.Trim();
        var dataType = DataTypeComboBox.SelectedItem?.ToString() ?? "Double";

        await _viewModel.CreateTagAsync(tagName, dataType);

        if (string.IsNullOrWhiteSpace(_viewModel.ErrorMessage))
        {
            TagModalOverlay.Visibility = Visibility.Collapsed;
            ShowNotification("TAG Criada", _viewModel.StatusMessage);
        }
        else
        {
            ShowError(_viewModel.ErrorMessage);
            ShowNotification("Falha ao criar TAG", _viewModel.ErrorMessage);
        }
    }

    private void CloseNotification_Click(object sender, RoutedEventArgs e)
    {
        NotificationOverlay.Visibility = Visibility.Collapsed;
    }

    private void ShowError(string message)
    {
        ErrorText.Text = message;
        ErrorBorder.Visibility = Visibility.Visible;
    }

    private void ShowNotification(string title, string message)
    {
        NotificationTitle.Text = title;
        NotificationMessage.Text = message;
        NotificationOverlay.Visibility = Visibility.Visible;
    }
}

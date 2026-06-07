using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using AnalictY.Manager.Infrastructure;
using AnalictY.Manager.Models;
using AnalictY.Manager.Services;
using AnalictY.Manager.ViewModels;

namespace AnalictY.Manager.Views;

public partial class TagsPage : UserControl
{
    private TagsViewModel? _viewModel;
    private Point _dragStartPoint;

    public TagsPage()
    {
        InitializeComponent();
        _viewModel = new TagsViewModel(new ConfigService(AppServices.HttpClient));
        DataContext = _viewModel;
        Loaded += TagsPage_Loaded;
    }

    private async void TagsPage_Loaded(object sender, RoutedEventArgs e)
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

    private void TagsDataGrid_PreviewMouseMove(object sender, MouseEventArgs e)
    {
        if (e.LeftButton != MouseButtonState.Pressed)
        {
            _dragStartPoint = e.GetPosition(null);
            return;
        }

        var currentPosition = e.GetPosition(null);
        if (Math.Abs(currentPosition.X - _dragStartPoint.X) < SystemParameters.MinimumHorizontalDragDistance &&
            Math.Abs(currentPosition.Y - _dragStartPoint.Y) < SystemParameters.MinimumVerticalDragDistance)
        {
            return;
        }

        if (TagsDataGrid.SelectedItem is not Tag tag)
        {
            return;
        }

        DragDrop.DoDragDrop(TagsDataGrid, new DataObject(typeof(Tag), tag), DragDropEffects.Move);
    }

    private void FoldersList_DragOver(object sender, DragEventArgs e)
    {
        e.Effects = e.Data.GetDataPresent(typeof(Tag)) ? DragDropEffects.Move : DragDropEffects.None;
        e.Handled = true;
    }

    private async void FoldersList_Drop(object sender, DragEventArgs e)
    {
        if (_viewModel == null || !e.Data.GetDataPresent(typeof(Tag)))
        {
            return;
        }

        var tag = e.Data.GetData(typeof(Tag)) as Tag;
        var target = FindAncestor<ListBoxItem>(e.OriginalSource as DependencyObject)?.DataContext as MachineFolderOption;
        if (tag == null || target == null)
        {
            return;
        }

        if (target.Id == null)
        {
            return;
        }

        var folderId = target.Id == 0 ? null : target.Id;
        await _viewModel.MoveTagToFolderAsync(tag, folderId);
    }

    private static T? FindAncestor<T>(DependencyObject? current) where T : DependencyObject
    {
        while (current != null)
        {
            if (current is T match)
            {
                return match;
            }

            current = VisualTreeHelper.GetParent(current);
        }

        return null;
    }
}

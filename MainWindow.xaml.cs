namespace LiveryManager;

using System.IO;
using System.Windows;
using LiveryManager.ViewModels;

/// <summary>
/// Interaction logic for MainWindow.xaml.
/// </summary>
public partial class MainWindow : Window
{
    private MainViewModel? ViewModel => DataContext as MainViewModel;

    public MainWindow()
    {
        InitializeComponent();
    }

    private void Window_PreviewDragOver(object sender, DragEventArgs e)
    {
        e.Effects = TryGetSingleZipFile(e.Data, out _) && ViewModel?.CanAcceptZipDrop() == true
            ? DragDropEffects.Copy
            : DragDropEffects.None;

        e.Handled = true;
    }

    private async void Window_PreviewDrop(object sender, DragEventArgs e)
    {
        e.Handled = true;

        if (ViewModel is null)
        {
            return;
        }

        if (!TryGetSingleZipFile(e.Data, out var zipFilePath))
        {
            ViewModel.ReportDropStatus("Drop a single ZIP file to install a livery.");
            return;
        }

        await ViewModel.InstallDroppedLiveryAsync(zipFilePath);
    }

    private static bool TryGetSingleZipFile(IDataObject data, out string zipFilePath)
    {
        zipFilePath = string.Empty;

        if (!data.GetDataPresent(DataFormats.FileDrop))
        {
            return false;
        }

        if (data.GetData(DataFormats.FileDrop) is not string[] droppedPaths || droppedPaths.Length != 1)
        {
            return false;
        }

        var filePath = droppedPaths[0];
        if (!File.Exists(filePath))
        {
            return false;
        }

        if (!Path.GetExtension(filePath).Equals(".zip", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        zipFilePath = filePath;
        return true;
    }
}

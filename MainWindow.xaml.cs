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
        e.Effects = TryGetDroppedZipFiles(e.Data, out _)
            && ViewModel?.CanAcceptZipDrop() == true
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

        if (!TryGetDroppedZipFiles(e.Data, out var zipFilePaths))
        {
            ViewModel.ReportDropStatus("Drop one or more ZIP files to install liveries.");
            return;
        }

        await ViewModel.InstallDroppedLiveriesAsync(zipFilePaths);
    }

    private static bool TryGetDroppedZipFiles(IDataObject data, out string[] zipFilePaths)
    {
        zipFilePaths = [];

        if (!data.GetDataPresent(DataFormats.FileDrop))
        {
            return false;
        }

        if (data.GetData(DataFormats.FileDrop) is not string[] droppedPaths || droppedPaths.Length == 0)
        {
            return false;
        }

        zipFilePaths = droppedPaths
            .Where(File.Exists)
            .Where(path => Path.GetExtension(path).Equals(".zip", StringComparison.OrdinalIgnoreCase))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        return zipFilePaths.Length > 0;
    }
}

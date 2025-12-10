using System.Windows;

using NAMStudio.ViewModels;

namespace NAMStudio;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainViewModel();
    }
}

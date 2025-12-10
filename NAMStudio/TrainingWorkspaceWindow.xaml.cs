using System.Windows;
using NAMStudio.ViewModels;

namespace NAMStudio;

public partial class TrainingWorkspaceWindow : Window
{
    public TrainingWorkspaceWindow(MainViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}

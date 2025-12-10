using System.Windows;

using NAMStudio.Models;

namespace NAMStudio;

public partial class TrainingRunDetailsWindow : Window
{
    public TrainingRunDetailsWindow(TrainingRun run)
    {
        InitializeComponent();
        DataContext = run;
    }
}

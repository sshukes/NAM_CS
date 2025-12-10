using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace NAMStudio.Models;

public class TrainingMetadata : INotifyPropertyChanged
{
    private string _modelName = string.Empty;
    private string _artist = string.Empty;
    private string _notes = string.Empty;
    private string _genre = string.Empty;

    public string ModelName
    {
        get => _modelName;
        set => SetField(ref _modelName, value);
    }

    public string Artist
    {
        get => _artist;
        set => SetField(ref _artist, value);
    }

    public string Notes
    {
        get => _notes;
        set => SetField(ref _notes, value);
    }

    public string Genre
    {
        get => _genre;
        set => SetField(ref _genre, value);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (Equals(field, value))
        {
            return false;
        }

        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}

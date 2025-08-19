using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace LogisticsPro.UI.Models;

public class User : INotifyPropertyChanged
{
    public int Id { get; set; }
    public string? Username { get; set; }
    public string? Password { get; set; }
    public string? Role { get; set; }
    public string? Name { get; set; }
    public string? LastName { get; set; }
    public string? Department { get; set; }
    public int DepartmentId { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string Status { get; set; } = "Active";

    // Computed properties for UI
    public string FullName => $"{Name} {LastName}".Trim();
    public string InitialLetter => !string.IsNullOrEmpty(Name) ? Name.Substring(0, 1).ToUpper() : "?";

    // For selection in bulk operations
    public bool IsSelected { get; set; }

    // Display ID for ordered list (1, 2, 3, 4...)
    private int _displayId;
    public int DisplayId
    {
        get => _displayId;
        set => SetProperty(ref _displayId, value);
    }

    // INotifyPropertyChanged implementation
    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}
using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace LogisticsPro.UI.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    private readonly Action _navigateToLogin;

    
    public MainWindowViewModel(Action navigateToLogin)
    {
        _navigateToLogin = navigateToLogin;
    }

    [RelayCommand]
    private void NavigateToLogin()
    {
        _navigateToLogin?.Invoke();
    }
    
    // Default constructor for design-time
    public MainWindowViewModel() : this(() => { }) { }

}
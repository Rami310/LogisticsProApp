using System;
using System.Timers;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LogisticsPro.UI.Infrastructure;

namespace LogisticsPro.UI.ViewModels
{
    public abstract partial class BaseDashboardViewModel : ObservableObject
    {
        private readonly Action _navigateToLogin;
        
        private Timer _timer;
        
        [ObservableProperty] 
        private string _welcomeMessage;
        
        [ObservableProperty]
        private string _username;
        
        [ObservableProperty]
        private string _usernameInitial;
        
        [ObservableProperty]
        private DateTime _currentDate = DateTime.Now;
        
        [ObservableProperty]
        private string _currentTime = DateTime.Now.ToString("h:mm tt");
        
        [ObservableProperty]
        private bool _isSidebarExpanded = true;

        [RelayCommand]
        private void ToggleSidebar()
        {
            IsSidebarExpanded = !IsSidebarExpanded;
            Console.WriteLine($"🔄 Sidebar toggled - Expanded: {IsSidebarExpanded}");
        }
        
        protected BaseDashboardViewModel(Action navigateToLogin, string username, string dashboardName = "Dashboard")
        {
            _navigateToLogin = navigateToLogin;
            Username = username;
            UsernameInitial = !string.IsNullOrEmpty(username) ? username[0].ToString().ToUpper() : "U";
            WelcomeMessage = $"Welcome to {dashboardName}, {username}!";
            
            // Start timer for updating time
            _timer = new Timer(30000); // Update every 30 seconds
            _timer.Elapsed += OnTimerElapsed;
            _timer.Start();
        }
        
        private void OnTimerElapsed(object sender, ElapsedEventArgs e)
        {
            Dispatcher.UIThread.InvokeAsync(() => {
                CurrentTime = DateTime.Now.ToString("h:mm tt");
                
                // Update date if it changes
                if (CurrentDate.Day != DateTime.Now.Day)
                {
                    CurrentDate = DateTime.Now;
                }
            });
        }

        [RelayCommand]
        public void Logout()
        {
            // Stop the timer to prevent memory leaks
            _timer?.Stop();
            _timer?.Dispose();
    
            _navigateToLogin?.Invoke();
        }
        
        protected void LogError(string context, Exception ex)
        {
            // Use our centralized error handler instead of custom implementation
            ErrorHandler.LogError(context, ex);
        }
    }
}
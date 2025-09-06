using System;
using System.IO;
using System.Timers;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LogisticsPro.UI.Infrastructure;
using LogisticsPro.UI.Services;

namespace LogisticsPro.UI.ViewModels
{
    public abstract partial class BaseDashboardViewModel : LocalizedViewModelBase
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
        
        [ObservableProperty]
        private string _dashboardText = "";
    
        [ObservableProperty]
        private string _reportsText = "";
    
        [ObservableProperty]
        private string _logoutText = "";
    
        [ObservableProperty]
        private string _languageText = "";
        
        [RelayCommand]
        private void ToggleLanguage()
        {
            var currentLang = _localization.CurrentLanguage;
            var newLang = currentLang == "en" ? "he" : "en";
            _localization.SetLanguage(newLang);
            
            // Save the preference
            SaveLanguagePreference(newLang);
        }
        
        [RelayCommand]
        private void ToggleSidebar()
        {
            IsSidebarExpanded = !IsSidebarExpanded;
            Console.WriteLine($"🔄 Sidebar toggled - Expanded: {IsSidebarExpanded}");
        }
        
        protected BaseDashboardViewModel(Action navigateToLogin, string username, string dashboardName = "Dashboard")
            : base() 
        {
            _navigateToLogin = navigateToLogin;
            Username = username;
            UsernameInitial = !string.IsNullOrEmpty(username) ? username[0].ToString().ToUpper() : "U";
            WelcomeMessage = $"Welcome to {dashboardName}, {username}!";
            
            // Initialize localized texts
            UpdateLocalizedTexts();
            
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
        
        protected override void OnLanguageChanged(object sender, EventArgs e)
        {
            UpdateLocalizedTexts();
            base.OnLanguageChanged(sender, e);
        }

        private void UpdateLocalizedTexts()
        {
            DashboardText = Localize("Dashboard");
            ReportsText = Localize("Reports");
            LogoutText = Localize("Logout");
            LanguageText = Localize("Language");
        
            // Update welcome message based on role
            var role = GetType().Name.Replace("DashboardViewModel", "");
            WelcomeMessage = Localize($"Welcome{role}", Username);
        }
        
        private void SaveLanguagePreference(string languageCode)
        {
            try
            {
                var appDataPath = GetAppDataPath();
                var languageFile = Path.Combine(appDataPath, "language.txt");
                File.WriteAllText(languageFile, languageCode);
                Console.WriteLine($"Language preference saved: {languageCode}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to save language preference: {ex.Message}");
            }
        }
        
        private string GetAppDataPath()
        {
            var appName = "LogisticsPro";
            string appDataPath;
    
            if (OperatingSystem.IsWindows())
            {
                appDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), appName);
            }
            else if (OperatingSystem.IsMacOS())
            {
                appDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), appName);
            }
            else
            {
                var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                appDataPath = Path.Combine(home, $".{appName.ToLower()}");
            }
    
            Directory.CreateDirectory(appDataPath);
            return appDataPath;
        }
    }
}
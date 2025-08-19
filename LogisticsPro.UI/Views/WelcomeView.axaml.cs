using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using LogisticsPro.UI.ViewModels;

namespace LogisticsPro.UI.Views
{
    public partial class WelcomeView : UserControl
    {
        private bool _hasNavigated = false;
        
        public WelcomeView()
        {
            InitializeComponent();
            Loaded += OnLoaded;
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
        
        private async void OnLoaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            // Start auto-navigation timer
            await StartAutoNavigationAsync();
        }
        
        private async Task StartAutoNavigationAsync()
        {
            // Wait 1.5 seconds before auto-navigation
            await Task.Delay(1000);
    
            if (!_hasNavigated && DataContext is MainWindowViewModel viewModel)
            {
                await StartFadeOutAndNavigateAsync(viewModel);
            }
        }
        
        private async Task StartFadeOutAndNavigateAsync(MainWindowViewModel viewModel)
        {
            _hasNavigated = true;
            
            // fade-out class to trigger animation
            Classes.Add("fade-out");
            
            // Wait for fade animation to complete
            await Task.Delay(800);
            
            // Navigate to login
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                viewModel.NavigateToLoginCommand.Execute(null);
            });
        }
    }
}
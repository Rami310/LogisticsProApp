using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Platform;
using LogisticsPro.UI.Infrastructure;
using LogisticsPro.UI.Services;
using LogisticsPro.UI.Views;

namespace LogisticsPro.UI;

public partial class App : Application
{
    private NavigationService _navigationService;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);

        // Initialize service locator
        ServiceLocator.RegisterServices();
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            ErrorHandler.TrySafe("AppStartup", () =>
            {
                // Set global application icon for all windows
                var iconUri = new Uri("avares://LogisticsPro.UI/Assets/logistics pro text as a logo.jpg");
                var icon = new WindowIcon(AssetLoader.Open(iconUri));

                // Create the main window
                var mainWindow = new MainWindow();

                // Set the icon after creating the window
                mainWindow.Icon = icon;
                
                // Initialize navigation service with the main window
                _navigationService = new NavigationService(mainWindow);

                // Register navigation service with the service locator
                ServiceLocator.Register<NavigationService>(_navigationService);

                // Set main window
                desktop.MainWindow = mainWindow;

                // Navigate to welcome screen
                _navigationService.NavigateTo("welcome");

                Console.WriteLine("Application started successfully");
            });
        }

        base.OnFrameworkInitializationCompleted();
    }

    // Utility method to access the navigation service from anywhere
    public static NavigationService GetNavigationService()
    {
        return ServiceLocator.Get<NavigationService>();
    }
}
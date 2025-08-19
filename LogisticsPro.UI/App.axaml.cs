using System;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
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
                // Create the main window
                var mainWindow = new MainWindow();

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
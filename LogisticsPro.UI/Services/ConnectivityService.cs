using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;

namespace LogisticsPro.UI.Services
{
    public static class ConnectivityService
    {
        private static bool _hasShownOfflineNotification = false;
        
        public static async Task<bool> CheckAndNotifyConnectionAsync(Window? parentWindow = null)
        {
            var isConnected = await ApiConfiguration.IsApiAvailableAsync();
            
            if (!isConnected && !_hasShownOfflineNotification)
            {
                await Dispatcher.UIThread.InvokeAsync(async () =>
                {
                    await ShowOfflineNotificationAsync(parentWindow);
                });
                _hasShownOfflineNotification = true;
            }
            
            return isConnected;
        }

        public static async Task ShowOfflineNotificationAsync(Window? parentWindow = null)
        {
            if (!Dispatcher.UIThread.CheckAccess())
            {
                await Dispatcher.UIThread.InvokeAsync(async () => await ShowOfflineNotificationAsync(parentWindow));
                return;
            }

            if (parentWindow == null)
            {
                if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
                {
                    parentWindow = desktop.MainWindow;
                }
            }

            if (parentWindow == null)
            {
                Console.WriteLine("Cannot show modal dialog - no parent window available");
                return;
            }

            var dialog = new Window
            {
                Title = "Connection Status",
                Width = 400,
                Height = 290,
                MinWidth = 400,
                MinHeight = 290,
                MaxWidth = 400,
                MaxHeight = 290,
                CanResize = false,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Background = Brushes.White,
                ShowInTaskbar = false,
                SystemDecorations = SystemDecorations.BorderOnly
            };

            var mainPanel = new StackPanel
            {
                Spacing = 21,
                Margin = new Thickness(30, 18, 30, 25),
                Background = Brushes.White
            };

            var headerSection = new StackPanel
            {
                Spacing = 12,
                HorizontalAlignment = HorizontalAlignment.Center,
                Background = Brushes.White
            };

            var statusIndicator = new Ellipse
            {
                Width = 52,
                Height = 52,
                Fill = new SolidColorBrush(Color.FromRgb(255, 193, 7)),
                HorizontalAlignment = HorizontalAlignment.Center
            };

            var iconPanel = new Grid
            {
                Background = Brushes.White
            };
            iconPanel.Children.Add(statusIndicator);
            iconPanel.Children.Add(new TextBlock
            {
                Text = "⚠",
                FontSize = 24,
                FontWeight = FontWeight.Bold,
                Foreground = Brushes.White,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                TextAlignment = TextAlignment.Center
            });

            headerSection.Children.Add(iconPanel);

            headerSection.Children.Add(new TextBlock
            {
                Text = "Connection Lost",
                FontSize = 22,
                FontWeight = FontWeight.SemiBold,
                Foreground = new SolidColorBrush(Color.FromRgb(31, 41, 55)),
                HorizontalAlignment = HorizontalAlignment.Center,
                TextAlignment = TextAlignment.Center
            });

            headerSection.Children.Add(new TextBlock
            {
                Text = "Using offline demo data",
                FontSize = 14,
                Foreground = new SolidColorBrush(Color.FromRgb(107, 114, 128)),
                HorizontalAlignment = HorizontalAlignment.Center,
                TextAlignment = TextAlignment.Center
            });

            mainPanel.Children.Add(headerSection);

            var warningBox = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(255, 251, 235)),
                CornerRadius = new CornerRadius(10),
                Padding = new Thickness(20, 10),
                BorderBrush = new SolidColorBrush(Color.FromRgb(252, 211, 77)),
                BorderThickness = new Thickness(1),
                HorizontalAlignment = HorizontalAlignment.Stretch
            };

            warningBox.Child = new TextBlock
            {
                Text = "Changes won't be saved",
                FontSize = 13,
                Foreground = new SolidColorBrush(Color.FromRgb(120, 53, 15)),
                HorizontalAlignment = HorizontalAlignment.Center,
                TextAlignment = TextAlignment.Center,
                FontWeight = FontWeight.Medium
            };

            mainPanel.Children.Add(warningBox);

            var buttonSection = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Center,
                Spacing = 12,
                Margin = new Thickness(0, 5, 0, 0),
                Background = Brushes.White
            };

            var retryButton = new Button
            {
                Content = "↻ Retry",
                Width = 100,
                Height = 40,
                Background = Brushes.White,
                Foreground = new SolidColorBrush(Color.FromRgb(59, 130, 246)),
                BorderBrush = new SolidColorBrush(Color.FromRgb(59, 130, 246)),
                BorderThickness = new Thickness(2),
                CornerRadius = new CornerRadius(10),
                FontSize = 13,
                FontWeight = FontWeight.SemiBold,
                HorizontalContentAlignment = HorizontalAlignment.Center,
                VerticalContentAlignment = VerticalAlignment.Center
            };

            var continueButton = new Button
            {
                Content = "Continue",
                Width = 130,
                Height = 40,
                Background = new SolidColorBrush(Color.FromRgb(59, 130, 246)),
                Foreground = Brushes.White,
                BorderThickness = new Thickness(0),
                CornerRadius = new CornerRadius(10),
                FontSize = 13,
                FontWeight = FontWeight.SemiBold,
                HorizontalContentAlignment = HorizontalAlignment.Center,
                VerticalContentAlignment = VerticalAlignment.Center
            };

            continueButton.PointerEntered += (s, e) =>
            {
                continueButton.Background = new SolidColorBrush(Color.FromRgb(37, 99, 235));
            };
            continueButton.PointerExited += (s, e) =>
            {
                continueButton.Background = new SolidColorBrush(Color.FromRgb(59, 130, 246));
            };

            retryButton.PointerEntered += (s, e) =>
            {
                retryButton.Background = new SolidColorBrush(Color.FromRgb(59, 130, 246));
                retryButton.Foreground = Brushes.White;
            };
            retryButton.PointerExited += (s, e) =>
            {
                retryButton.Background = Brushes.White;
                retryButton.Foreground = new SolidColorBrush(Color.FromRgb(59, 130, 246));
            };

            continueButton.Click += (s, e) => dialog.Close();

            retryButton.Click += async (s, e) =>
            {
                var originalContent = retryButton.Content;
                retryButton.Content = "⟳ Checking...";
                retryButton.IsEnabled = false;

                try
                {
                    ApiConfiguration.ResetApiAvailabilityCheck();
                    var isConnected = await ApiConfiguration.IsApiAvailableAsync();

                    if (isConnected)
                    {
                        retryButton.Content = "✓ Connected!";
                        retryButton.Background = new SolidColorBrush(Color.FromRgb(16, 185, 129));
                        retryButton.Foreground = Brushes.White;
                        retryButton.BorderBrush = new SolidColorBrush(Color.FromRgb(16, 185, 129));
                        await Task.Delay(800);
                        _hasShownOfflineNotification = false;
                        dialog.Close();
                        return;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Retry connection error: {ex.Message}");
                }

                retryButton.Content = "✗ Still offline";
                retryButton.Background = new SolidColorBrush(Color.FromRgb(239, 68, 68));
                retryButton.Foreground = Brushes.White;
                retryButton.BorderBrush = new SolidColorBrush(Color.FromRgb(239, 68, 68));
                await Task.Delay(1200);

                retryButton.Content = originalContent;
                retryButton.Background = Brushes.White;
                retryButton.Foreground = new SolidColorBrush(Color.FromRgb(59, 130, 246));
                retryButton.BorderBrush = new SolidColorBrush(Color.FromRgb(59, 130, 246));
                retryButton.IsEnabled = true;
            };

            buttonSection.Children.Add(retryButton);
            buttonSection.Children.Add(continueButton);
            mainPanel.Children.Add(buttonSection);

            dialog.Content = mainPanel;

            dialog.KeyDown += (s, e) =>
            {
                switch (e.Key)
                {
                    case Key.Escape:
                    case Key.Enter:
                        dialog.Close();
                        e.Handled = true;
                        break;
                    case Key.R when e.KeyModifiers == KeyModifiers.None:
                        if (retryButton.IsEnabled)
                        {
                            retryButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                        }

                        e.Handled = true;
                        break;
                }
            };

            dialog.Opened += (s, e) =>
            {
                Dispatcher.UIThread.Post(() => continueButton.Focus(), DispatcherPriority.Loaded);
            };

            try
            {
                Console.WriteLine("Showing ultimate modal connectivity dialog...");
                await dialog.ShowDialog(parentWindow);
                Console.WriteLine("Modal dialog closed by user");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error showing connectivity dialog: {ex.Message}");
                dialog.Show();
            }
        }
        
    }
}
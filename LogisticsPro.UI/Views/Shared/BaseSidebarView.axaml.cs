using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using LogisticsPro.UI.ViewModels;

namespace LogisticsPro.UI.Views.Shared;

public partial class BaseSidebarView : UserControl
{
    public BaseSidebarView()
    {
        InitializeComponent();

        this.DataContextChanged += OnDataContextChanged;
    }

    private void OnDataContextChanged(object? sender, System.EventArgs e)
    {
        if (DataContext is BaseDashboardViewModel viewModel)
        {
            // Bind user info to controls
            var usernameControl = this.FindControl<TextBlock>("Username");
            var usernameInitialControl = this.FindControl<TextBlock>("UsernameInitial");
            var logoutButton = this.FindControl<Button>("LogoutButton");

            if (usernameControl != null)
                usernameControl.Text = viewModel.Username;

            if (usernameInitialControl != null)
                usernameInitialControl.Text = viewModel.UsernameInitial;

            if (logoutButton != null)
                logoutButton.Command = viewModel.LogoutCommand;
        }
    }

    protected void SetMenuItems(Control menuItems)
    {
        var placeholder = this.FindControl<ContentControl>("MenuItemsPlaceholder");
        if (placeholder != null)
        {
            placeholder.Content = menuItems;
        }
    }

    protected void SetUserRole(string role)
    {
        var roleControl = this.FindControl<TextBlock>("UserRole");
        if (roleControl != null)
        {
            roleControl.Text = role;
        }
    }
    
    public virtual void SetCollapsedState(bool isExpanded)
    {
        // Find text elements that should hide/show
        var logoText = this.FindControl<TextBlock>("LogoText");
        var userInfo = this.FindControl<StackPanel>("UserInfo");
    
        // Hide/show text elements based on expanded state
        if (logoText != null) logoText.IsVisible = isExpanded;
        if (userInfo != null) userInfo.IsVisible = isExpanded;
    
        // Hide/show menu text in buttons
        HideShowMenuText(isExpanded);
    }

    private void HideShowMenuText(bool isExpanded)
    {
        // This will hide text in menu buttons when collapsed
        // You can override this in specific sidebar implementations
    }
    
    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
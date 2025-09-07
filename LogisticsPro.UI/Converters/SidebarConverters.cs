/*
 * UI Layout Converters
 * 
 * Purpose: Avalonia UI data binding converters for layout and toggle controls
 * Dependencies: Avalonia.Data.Converters, Avalonia framework
 * 
 * Converters:
 * - BoolToToggleIconConverter - Converts bool to toggle menu icons (hamburger)
 * - BoolToWidthConverter - Converts bool to width values (250/0 for sidebar)
 * - BoolToMarginConverter - Converts bool to margin values for layout spacing
 * 
 * Features: One-way binding support, sidebar control, responsive layout
 * Note: BoolToToggleIconConverter currently shows same icon for both states
 */

using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia;

namespace LogisticsPro.UI.Converters
{
    public class BoolToToggleIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // When expanded (true): Show close/X icon
            // When collapsed (false): Show hamburger menu icon
            return (bool)value ? "☰" : "☰";
        }
        
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class BoolToWidthConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (bool)value ? 250.0 : 0.0;
        }
    
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    
    public class BoolToMarginConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // When sidebar is collapsed (!IsSidebarExpanded = true), add left margin
            return (bool)value ? new Thickness(10, 0, 0, 0) : new Thickness(0);
        }
    
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
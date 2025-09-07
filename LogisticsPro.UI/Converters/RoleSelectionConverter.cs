/*
 * RoleSelectionConverter.cs
 *
 * Purpose: Avalonia UI data binding converter for role-based selection logic
 * Dependencies: Avalonia.Data.Converters
 *
 * Converter:
 * - RoleSelectionConverter - Checks if a role list contains a specific role
 *
 * Features: Role validation, checkbox binding support, user permission checks
 * Usage: Bind to List<string> roles with string parameter for role checking
 */

using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia.Data.Converters;

namespace LogisticsPro.UI.Converters
{
    public class RoleSelectionConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is List<string> roles && parameter is string role)
            {
                return roles.Contains(role);
            }
            return false;
        }
        
        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
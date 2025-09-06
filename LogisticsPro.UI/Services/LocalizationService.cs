using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia.Media;

namespace LogisticsPro.UI.Services
{
    public class LocalizationService
    {
        private static LocalizationService _instance;
        private Dictionary<string, Dictionary<string, string>> _translations;
        private string _currentLanguage = "en";

        public static LocalizationService Instance => _instance ??= new LocalizationService();

        public event EventHandler LanguageChanged;

        private LocalizationService()
        {
            InitializeTranslations();
        }

        private void InitializeTranslations()
        {
            _translations = new Dictionary<string, Dictionary<string, string>>
            {
                ["en"] = new Dictionary<string, string>
                {
                    ["Dashboard"] = "Dashboard",
                    ["Reports"] = "Reports",
                    ["Logout"] = "Logout",
                    ["Language"] = "Language",
                    ["TotalStock"] = "Total Stock",
                    ["PendingRequests"] = "Pending",
                    ["LowStock"] = "Low Stock",
                    ["Approved"] = "Approved",
                    ["Available"] = "Available",
                    ["WarehouseOperations"] = "Warehouse Operations",
                    ["LogisticsOperations"] = "Logistics Operations",
                    ["QuickActions"] = "Quick Actions",
                    ["WelcomeAdmin"] = "Welcome to Admin Dashboard, {0}!",
                    ["AdministratorFinancialOverview"] = "Administrator Financial Overview",
                    ["ThisMonth"] = "This Month",
                    ["MonthlySpending"] = "Monthly spending",
                    ["TotalSpent"] = "Total Spent", 
                    ["AllTimeSpending"] = "All time spending (without a profit)",
                    ["AvailableBudget"] = "Available Budget",
                    ["BudgetUtilization"] = "Budget Utilization",
                    ["OrderProgress"] = "Order Progress",
                    ["ReadyForShipment"] = "Ready for Shipment",
                    ["InInventory"] = "In Inventory", 
                    ["TotalRequests"] = "Total Requests",
                    ["ViewSystemReports"] = "View System Reports",
                    ["AccessComprehensive"] = "Access comprehensive system analytics and reports",
                    ["Used"] = "Used",
                    ["Of"] = "of",
                    ["TotalBudget"] = "total budget"
                },
                ["he"] = new Dictionary<string, string>
                {
                    ["Dashboard"] = "לוח בקרה",
                    ["Reports"] = "דוחות",
                    ["Logout"] = "התנתקות",
                    ["Language"] = "שפה",
                    ["TotalStock"] = "מלאי כולל",
                    ["PendingRequests"] = "ממתין",
                    ["LowStock"] = "מלאי נמוך",
                    ["Approved"] = "מאושר",
                    ["Available"] = "זמין",
                    ["WarehouseOperations"] = "פעולות מחסן",
                    ["LogisticsOperations"] = "פעולות לוגיסטיות",
                    ["QuickActions"] = "פעולות מהירות",
                    ["WelcomeAdmin"] = "ברוך הבא ללוח הבקרה של המנהל, {0}!",
                    ["AdministratorFinancialOverview"] = "סקירה פיננסית של מנהל המערכת",
                    ["ThisMonth"] = "החודש",
                    ["MonthlySpending"] = "הוצאות חודשיות",
                    ["TotalSpent"] = "סה״כ הוצאות",
                    ["AllTimeSpending"] = "כל ההוצאות (ללא רווח)",
                    ["AvailableBudget"] = "תקציב זמין", 
                    ["BudgetUtilization"] = "ניצול תקציב",
                    ["OrderProgress"] = "התקדמות הזמנות",
                    ["ReadyForShipment"] = "מוכן למשלוח",
                    ["InInventory"] = "במלאי",
                    ["TotalRequests"] = "סה״כ בקשות", 
                    ["ViewSystemReports"] = "צפה בדוחות המערכת",
                    ["AccessComprehensive"] = "גישה לניתוח מערכת מקיף ודוחות",
                    ["Used"] = "נוצל",
                    ["Of"] = "מתוך",
                    ["TotalBudget"] = "תקציב כולל"
                }
            };
        }

        public string GetString(string key)
        {
            if (_translations.ContainsKey(_currentLanguage) && 
                _translations[_currentLanguage].ContainsKey(key))
            {
                return _translations[_currentLanguage][key];
            }
            return key; // Fallback to key
        }

        public string GetString(string key, params object[] args)
        {
            var format = GetString(key);
            try
            {
                return string.Format(format, args);
            }
            catch
            {
                return key;
            }
        }

        public void SetLanguage(string languageCode)
        {
            _currentLanguage = languageCode;
            var culture = new CultureInfo(languageCode);
            CultureInfo.CurrentCulture = culture;
            CultureInfo.CurrentUICulture = culture;
            
            LanguageChanged?.Invoke(this, EventArgs.Empty);
            Console.WriteLine($"Language changed to: {languageCode}");
        }

        public string CurrentLanguage => _currentLanguage;

        public List<Language> GetAvailableLanguages()
        {
            return new List<Language>
            {
                new Language { Code = "en", Name = "English", NativeName = "English" },
                new Language { Code = "he", Name = "Hebrew", NativeName = "עברית" }
            };
        }
        
        public bool IsRTL => _currentLanguage == "he";

        public FlowDirection GetFlowDirection()
        {
            return IsRTL ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;
        }
    }

    public class Language
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public string NativeName { get; set; }
    }
}
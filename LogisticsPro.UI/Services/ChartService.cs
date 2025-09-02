using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using LogisticsPro.UI.Models.Revenue;
using SkiaSharp;

namespace LogisticsPro.UI.Services
{
    public class ChartService : IChartService
    {
        /// <summary>
        /// Initialize default chart properties to prevent crashes
        /// </summary>
        public (ISeries[] Series, Axis[] XAxes, Axis[] YAxes) InitializeDefaultChart()
        {
            Console.WriteLine("📊 Initializing default chart properties...");
            
            var series = new ISeries[]
            {
                new LineSeries<double>
                {
                    Values = new double[] { 0, 0, 0, 0, 0, 0 }, // 6 months of zero values
                    Name = "Monthly Profit",
                    Stroke = new SolidColorPaint(SKColors.Green) { StrokeThickness = 3 },
                    Fill = new SolidColorPaint(SKColor.Parse("#059669").WithAlpha(50)),
                    GeometryStroke = new SolidColorPaint(SKColors.Green) { StrokeThickness = 3 },
                    GeometryFill = new SolidColorPaint(SKColors.White),
                    GeometrySize = 8
                }
            };
            
            var xAxes = new Axis[]
            {
                new Axis
                {
                    Labels = new string[] { "Loading...", "", "", "", "", "" },
                    LabelsRotation = 45,
                    TextSize = 12,
                    SeparatorsPaint = new SolidColorPaint(SKColor.Parse("#E5E7EB")),
                    LabelsPaint = new SolidColorPaint(SKColor.Parse("#64748B"))
                }
            };
            
            var yAxes = new Axis[]
            {
                new Axis
                {
                    TextSize = 12,
                    SeparatorsPaint = new SolidColorPaint(SKColor.Parse("#E5E7EB")),
                    LabelsPaint = new SolidColorPaint(SKColor.Parse("#64748B")),
                    Labeler = value => $"${value:N0}"
                }
            };
            
            Console.WriteLine("✅ Default chart properties initialized");
            return (series, xAxes, yAxes);
        }
        
        /// <summary>
        /// Prepare monthly profit chart data from API
        /// </summary>
        public async Task<(ISeries[] Series, Axis[] XAxes, Axis[] YAxes)> PrepareMonthlyProfitChartAsync()
        {
            Console.WriteLine("📈 Preparing monthly profit chart from API...");
            
            try
            {
                // Get all transactions
                var response = await ApiConfiguration.HttpClient.GetAsync("Revenue/transactions");
                
                if (response.IsSuccessStatusCode)
                {
                    var responseJson = await response.Content.ReadAsStringAsync();
                    var transactions = JsonSerializer.Deserialize<List<RevenueTransactionDto>>(responseJson, ApiConfiguration.JsonOptions);
                    
                    if (transactions != null)
                    {
                        //Use ALL transactions, no role filtering for profit charts
                        return CreateProfitChart(transactions);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error preparing chart from API: {ex.Message}");
            }
            
            // Fallback to sample data
            Console.WriteLine("🔄 Using sample chart data");
            return CreateSampleProfitChart();
        }
        
        /// <summary>
        /// Prepare chart for specific user role
        /// </summary>
        public async Task<(ISeries[] Series, Axis[] XAxes, Axis[] YAxes)> PrepareMonthlyProfitChartForRoleAsync(string userRole, string username = null)
        {
            Console.WriteLine($"📈 Preparing monthly profit chart for role: {userRole} (showing ALL company transactions)");
            
            try
            {
                var response = await ApiConfiguration.HttpClient.GetAsync("Revenue/transactions");
                
                if (response.IsSuccessStatusCode)
                {
                    var responseJson = await response.Content.ReadAsStringAsync();
                    var transactions = JsonSerializer.Deserialize<List<RevenueTransactionDto>>(responseJson, ApiConfiguration.JsonOptions);
                    
                    if (transactions != null)
                    {
                        // Profit charts should show total company performance, not individual user performance
                        Console.WriteLine($"📊 Using ALL {transactions.Count} transactions for profit chart (no user filtering)");
                        return CreateProfitChart(transactions);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error preparing role-specific chart: {ex.Message}");
            }
            
            // Fallback
            return CreateSampleProfitChart();
        }
        

        /// <summary>
        /// Prepare sales volume chart (bar chart)
        /// </summary>
        public async Task<(ISeries[] Series, Axis[] XAxes, Axis[] YAxes)> PrepareSalesVolumeChartAsync()
        {
            Console.WriteLine("📊 Preparing sales volume chart...");
            
            try
            {
                var response = await ApiConfiguration.HttpClient.GetAsync("Revenue/transactions");
                
                if (response.IsSuccessStatusCode)
                {
                    var responseJson = await response.Content.ReadAsStringAsync();
                    var transactions = JsonSerializer.Deserialize<List<RevenueTransactionDto>>(responseJson, ApiConfiguration.JsonOptions);
                    
                    if (transactions != null)
                    {
                        return CreateSalesVolumeChart(transactions);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error preparing sales volume chart: {ex.Message}");
            }
            
            return CreateSampleSalesChart();
        }
        
        // ========================================
        // PRIVATE HELPER METHODS
        // ========================================
        
        private (ISeries[] Series, Axis[] XAxes, Axis[] YAxes) CreateProfitChart(List<RevenueTransactionDto> transactions)
        {
            Console.WriteLine($"📊 Creating profit chart with {transactions.Count} total transactions");
            
            var chartData = new List<double>();
            var monthLabels = new List<string>();
            
            for (int i = 5; i >= 0; i--)
            {
                var targetDate = DateTime.Now.AddMonths(-i);
                var monthName = targetDate.ToString("MMM yyyy", System.Globalization.CultureInfo.InvariantCulture);
                
                var monthlyTransactions = transactions
                    .Where(t => t.TransactionType == "DELIVERY_CONFIRMED" && 
                               t.Amount > 0 &&
                               t.CreatedDate.Month == targetDate.Month && 
                               t.CreatedDate.Year == targetDate.Year)
                    .ToList();
                
                var monthlyProfit = monthlyTransactions.Sum(t => (double)t.Amount);
                
                chartData.Add(monthlyProfit);
                monthLabels.Add(monthName);
                
                Console.WriteLine($"📊 {monthName}: ${monthlyProfit:F2} (from {monthlyTransactions.Count} transactions)");
                
                // 🔧 DEBUG: Show some transaction details for August
                if (monthName.Contains("Aug"))
                {
                    Console.WriteLine($"📅 August transactions debug:");
                    foreach (var tx in monthlyTransactions.Take(3))
                    {
                        Console.WriteLine($"   💰 {tx.CreatedDate:MM/dd/yyyy} - ${tx.Amount} - {tx.CreatedBy} - {tx.Description}");
                    }
                }
            }
            
            var series = new ISeries[]
            {
                new LineSeries<double>
                {
                    Values = chartData,
                    Name = "Monthly Profit",
                    Stroke = new SolidColorPaint(SKColors.Green) { StrokeThickness = 3 },
                    Fill = new SolidColorPaint(SKColor.Parse("#059669").WithAlpha(50)),
                    GeometryStroke = new SolidColorPaint(SKColors.Green) { StrokeThickness = 3 },
                    GeometryFill = new SolidColorPaint(SKColors.White),
                    GeometrySize = 8
                }
            };
            
            var xAxes = new Axis[]
            {
                new Axis
                {
                    Labels = monthLabels,
                    LabelsRotation = 45,
                    TextSize = 12,
                    SeparatorsPaint = new SolidColorPaint(SKColor.Parse("#E5E7EB")),
                    LabelsPaint = new SolidColorPaint(SKColor.Parse("#64748B"))
                }
            };
            
            var yAxes = new Axis[]
            {
                new Axis
                {
                    TextSize = 12,
                    SeparatorsPaint = new SolidColorPaint(SKColor.Parse("#E5E7EB")),
                    LabelsPaint = new SolidColorPaint(SKColor.Parse("#64748B")),
                    Labeler = value => $"${value:N0}"
                }
            };
            
            return (series, xAxes, yAxes);
        }
        
        //FilterTransactionsByRole method
        
        private (ISeries[] Series, Axis[] XAxes, Axis[] YAxes) CreateSampleProfitChart()
        {
            var sampleData = new double[] { 0, 500, 300, 750, 750, 1200 };
            var sampleLabels = new string[] { "Mar 2025", "Apr 2025", "May 2025", "Jun 2025", "Jul 2025", "Aug 2025" };
            
            var series = new ISeries[]
            {
                new LineSeries<double>
                {
                    Values = sampleData,
                    Name = "Monthly Profit",
                    Stroke = new SolidColorPaint(SKColors.Green) { StrokeThickness = 3 },
                    Fill = new SolidColorPaint(SKColor.Parse("#059669").WithAlpha(50)),
                    GeometryStroke = new SolidColorPaint(SKColors.Green) { StrokeThickness = 3 },
                    GeometryFill = new SolidColorPaint(SKColors.White),
                    GeometrySize = 8
                }
            };
            
            var xAxes = new Axis[]
            {
                new Axis
                {
                    Labels = sampleLabels,
                    LabelsRotation = 45,
                    TextSize = 12,
                    SeparatorsPaint = new SolidColorPaint(SKColor.Parse("#E5E7EB")),
                    LabelsPaint = new SolidColorPaint(SKColor.Parse("#64748B"))
                }
            };
            
            var yAxes = new Axis[]
            {
                new Axis
                {
                    TextSize = 12,
                    SeparatorsPaint = new SolidColorPaint(SKColor.Parse("#E5E7EB")),
                    LabelsPaint = new SolidColorPaint(SKColor.Parse("#64748B")),
                    Labeler = value => $"${value:N0}"
                }
            };
            
            return (series, xAxes, yAxes);
        }
        
        private (ISeries[] Series, Axis[] XAxes, Axis[] YAxes) CreateSalesVolumeChart(List<RevenueTransactionDto> transactions)
        {
            var chartData = new List<double>();
            var monthLabels = new List<string>();
            
            for (int i = 5; i >= 0; i--)
            {
                var targetDate = DateTime.Now.AddMonths(-i);
                var monthName = targetDate.ToString("MMM yyyy", System.Globalization.CultureInfo.InvariantCulture);
                
                var salesCount = transactions
                    .Where(t => t.TransactionType == "DELIVERY_CONFIRMED" && 
                               t.Amount > 0 &&
                               t.CreatedDate.Month == targetDate.Month && 
                               t.CreatedDate.Year == targetDate.Year)
                    .Count();
                
                chartData.Add(salesCount);
                monthLabels.Add(monthName);
            }
            
            var series = new ISeries[]
            {
                new ColumnSeries<double>
                {
                    Values = chartData,
                    Name = "Monthly Sales",
                    Fill = new SolidColorPaint(SKColor.Parse("#3B82F6"))
                }
            };
            
            var xAxes = new Axis[]
            {
                new Axis
                {
                    Labels = monthLabels,
                    LabelsRotation = 45,
                    TextSize = 12,
                    SeparatorsPaint = new SolidColorPaint(SKColor.Parse("#E5E7EB")),
                    LabelsPaint = new SolidColorPaint(SKColor.Parse("#64748B"))
                }
            };
            
            var yAxes = new Axis[]
            {
                new Axis
                {
                    TextSize = 12,
                    SeparatorsPaint = new SolidColorPaint(SKColor.Parse("#E5E7EB")),
                    LabelsPaint = new SolidColorPaint(SKColor.Parse("#64748B")),
                    Labeler = value => $"{value:N0}"
                }
            };
            
            return (series, xAxes, yAxes);
        }
        
        /// <summary>
        /// Create spending chart from transactions (DEDUCT type)
        /// </summary>
        public (ISeries[] Series, Axis[] XAxes, Axis[] YAxes) CreateSpendingChartFromTransactions(List<RevenueTransactionDto> transactions)
        {
            Console.WriteLine($"💰 Creating spending chart with {transactions.Count} total transactions");
            
            var chartData = new List<double>();
            var monthLabels = new List<string>();
            
            for (int i = 5; i >= 0; i--)
            {
                var targetDate = DateTime.Now.AddMonths(-i);
                var monthName = targetDate.ToString("MMM yyyy", System.Globalization.CultureInfo.InvariantCulture);
                
                var monthlySpendingTransactions = transactions
                    .Where(t => t.TransactionType == "ORDER_PLACED" && 
                               t.Amount > 0 &&
                               t.CreatedDate.Month == targetDate.Month && 
                               t.CreatedDate.Year == targetDate.Year)
                    .ToList();
                
                var monthlySpending = monthlySpendingTransactions.Sum(t => (double)t.Amount);
                
                chartData.Add(monthlySpending);
                monthLabels.Add(monthName);
                
                Console.WriteLine($"💰 {monthName}: ${monthlySpending:F2} spent (from {monthlySpendingTransactions.Count} deductions)");
            }
            
            var series = new ISeries[]
            {
                new LineSeries<double>
                {
                    Values = chartData,
                    Name = "Monthly Spending",
                    Stroke = new SolidColorPaint(SKColors.Red) { StrokeThickness = 3 },
                    Fill = new SolidColorPaint(SKColor.Parse("#EF4444").WithAlpha(50)),
                    GeometryStroke = new SolidColorPaint(SKColors.Red) { StrokeThickness = 3 },
                    GeometryFill = new SolidColorPaint(SKColors.White),
                    GeometrySize = 8
                }
            };
            
            var xAxes = new Axis[]
            {
                new Axis
                {
                    Labels = monthLabels,
                    LabelsRotation = 45,
                    TextSize = 12,
                    SeparatorsPaint = new SolidColorPaint(SKColor.Parse("#E5E7EB")),
                    LabelsPaint = new SolidColorPaint(SKColor.Parse("#64748B"))
                }
            };
            
            var yAxes = new Axis[]
            {
                new Axis
                {
                    TextSize = 12,
                    SeparatorsPaint = new SolidColorPaint(SKColor.Parse("#E5E7EB")),
                    LabelsPaint = new SolidColorPaint(SKColor.Parse("#64748B")),
                    Labeler = value => $"${value:N0}"
                }
            };
            
            return (series, xAxes, yAxes);
        }
        
        
        private (ISeries[] Series, Axis[] XAxes, Axis[] YAxes) CreateSampleSalesChart()
        {
            var sampleData = new double[] { 0, 2, 1, 3, 1, 2 };
            var sampleLabels = new string[] { "Mar 2025", "Apr 2025", "May 2025", "Jun 2025", "Jul 2025", "Aug 2025" };
            
            var series = new ISeries[]
            {
                new ColumnSeries<double>
                {
                    Values = sampleData,
                    Name = "Monthly Sales",
                    Fill = new SolidColorPaint(SKColor.Parse("#3B82F6"))
                }
            };
            
            var xAxes = new Axis[] 
            {
                new Axis 
                {
                    Labels = sampleLabels,
                    LabelsRotation = 45,
                    TextSize = 12
                }
            };
            
            var yAxes = new Axis[]
            {
                new Axis
                {
                    TextSize = 12,
                    Labeler = value => $"{value:N0}"
                }
            };
            
            return (series, xAxes, yAxes);
        }
    }
}
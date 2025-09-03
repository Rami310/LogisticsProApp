
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LogisticsPro.UI.Models.Revenue;

namespace LogisticsPro.UI.Services
{
    public interface IChartService
    {
        /// <summary>
        /// Initialize default chart properties to prevent crashes
        /// </summary>
        (ISeries[] Series, Axis[] XAxes, Axis[] YAxes) InitializeDefaultChart();
        
        /// <summary>
        /// Prepare monthly profit chart data from API
        /// </summary>
        Task<(ISeries[] Series, Axis[] XAxes, Axis[] YAxes)> PrepareMonthlyProfitChartAsync();
        
        /// <summary>
        /// Prepare sales volume chart data from API
        /// </summary>
        Task<(ISeries[] Series, Axis[] XAxes, Axis[] YAxes)> PrepareSalesVolumeChartAsync();
        
        /// <summary>
        /// Prepare chart for specific user role (filters data appropriately)
        /// </summary>
        Task<(ISeries[] Series, Axis[] XAxes, Axis[] YAxes)> PrepareMonthlyProfitChartForRoleAsync(string userRole, string username = null);
        
        (ISeries[] Series, Axis[] XAxes, Axis[] YAxes) CreateCleanProfitChart(List<RevenueTransactionDto> transactions);
        
        (ISeries[] Series, Axis[] XAxes, Axis[] YAxes) CreateSpendingChartFromTransactions(List<RevenueTransactionDto> transactions);
    }
}
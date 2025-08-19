using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace LogisticsPro.UI.Models
{
    public class ProductRequest : INotifyPropertyChanged
    {
        private int _displayId;
        private int _requestedQuantity = 1;
        private decimal _totalCost;
        private string _notes = "";
        private string _requestStatus = "Pending";

        public bool IsPending => RequestStatus == "Pending";
        public int Id { get; set; }
        public string RequestedBy { get; set; } = "";
        public DateTime RequestDate { get; set; }
        public int ProductId { get; set; }
        public Product Product { get; set; } = new Product();
        
        public int DisplayId
        {
            get => _displayId;
            set
            {
                if (_displayId != value)
                {
                    _displayId = value;
                    OnPropertyChanged();
                }
            }
        }
        
        public int RequestedQuantity
        {
            get => _requestedQuantity;
            set
            {
                if (_requestedQuantity != value)
                {
                    _requestedQuantity = value;
                    OnPropertyChanged();
                }
            }
        }

        public decimal TotalCost
        {
            get => _totalCost;
            set
            {
                if (_totalCost != value)
                {
                    _totalCost = value;
                    OnPropertyChanged();
                }
            }
        }

        public string Notes
        {
            get => _notes;
            set
            {
                if (_notes != value)
                {
                    _notes = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(EmployeeName));
                    OnPropertyChanged(nameof(QuantityWithEmployee));
                    OnPropertyChanged(nameof(QuantityWithAbortedBy));
                }
            }
        }

        public string RequestStatus
        {
            get => _requestStatus;
            set
            {
                if (_requestStatus != value)
                {
                    _requestStatus = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(IsPending));
                }
            }
        }        
        
        public string ApprovedByUsername 
        { 
            get 
            {
                if (RequestStatus != "Approved" || string.IsNullOrEmpty(ApprovedBy))
                    return "-";
                
                return ApprovedBy;
            } 
        }

        public DateTime? ApprovalDate { get; set; }
        public string? ApprovedBy { get; set; }
        public string? RejectionReason { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? ReceivedDate { get; set; }
        public string? ReceivedBy { get; set; }
        public DateTime? RejectedDate { get; set; }
        public string? RejectedBy { get; set; }

        public string EmployeeName 
        { 
            get 
            { 
                if (string.IsNullOrEmpty(Notes)) return "Employee";
        
                if (Notes.Contains("[Aborted by "))
                {
                    var start = Notes.IndexOf("[Aborted by ") + 12;
                    var end = Notes.IndexOf(" on ", start);
                    if (end > start)
                    {
                        return Notes.Substring(start, end - start).Trim();
                    }
                }
        
                if (Notes.Contains("confirmed by ") && Notes.Contains(" on "))
                {
                    var start = Notes.IndexOf("confirmed by ") + 13;
                    var end = Notes.IndexOf(" on ", start);
                    if (end > start)
                    {
                        return Notes.Substring(start, end - start).Trim();
                    }
                }
        
                if (Notes.Contains("confirmed by logistics employee"))
                {
                    return "log_emp1";
                }
        
                return "Employee";
            }
        }
    
        public string QuantityWithEmployee => $"Qty: {RequestedQuantity} • Confirmed by {EmployeeName}";
        public string QuantityWithAbortedBy => $"Qty: {RequestedQuantity} • Aborted by {EmployeeName}";
        
        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
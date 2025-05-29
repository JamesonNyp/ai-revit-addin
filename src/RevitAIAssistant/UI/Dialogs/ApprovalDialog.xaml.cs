using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using RevitAIAssistant.Models;
using RevitAIAssistant.UI.Commands;

namespace RevitAIAssistant.UI.Dialogs
{
    /// <summary>
    /// Professional Engineer approval dialog for critical engineering decisions
    /// </summary>
    public partial class ApprovalDialog : Window
    {
        private ApprovalViewModel _viewModel;

        public ApprovalDialog(ApprovalPoint approvalPoint, EngineerInfo reviewingEngineer)
        {
            InitializeComponent();
            
            _viewModel = new ApprovalViewModel(approvalPoint, reviewingEngineer);
            DataContext = _viewModel;
            
            _viewModel.ApprovalCompleted += OnApprovalCompleted;
        }

        public ApprovalStatus Result => _viewModel.ApprovalStatus;
        public string Comments => _viewModel.Comments;

        private void OnApprovalCompleted(object? sender, EventArgs e)
        {
            DialogResult = _viewModel.ApprovalStatus == ApprovalStatus.Approved;
            Close();
        }
    }

    /// <summary>
    /// View model for approval dialog
    /// </summary>
    public class ApprovalViewModel : INotifyPropertyChanged
    {
        private readonly ApprovalPoint _approvalPoint;
        private string _comments = string.Empty;
        private ApprovalStatus _approvalStatus = ApprovalStatus.Pending;

        public event EventHandler? ApprovalCompleted;

        public ApprovalViewModel(ApprovalPoint approvalPoint, EngineerInfo reviewingEngineer)
        {
            _approvalPoint = approvalPoint;
            ReviewingEngineer = reviewingEngineer;
            
            // Initialize collections
            CalculationResults = new ObservableCollection<CalculationResultItem>();
            CodeReferences = new ObservableCollection<string>();
            RequiredActions = new ObservableCollection<RequiredAction>();
            
            // Populate data from approval point
            PopulateData();
            
            // Initialize commands
            ApproveCommand = new RelayCommand(Approve, () => CanApprove);
            RejectCommand = new RelayCommand(Reject);
            RequestInfoCommand = new RelayCommand(RequestInfo);
        }

        #region Properties

        public string ApprovalType => _approvalPoint.ApprovalType;
        public string Description => _approvalPoint.Description;
        public string LiabilityStatement => _approvalPoint.LiabilityStatement;
        public EngineerInfo ReviewingEngineer { get; }
        
        public ObservableCollection<CalculationResultItem> CalculationResults { get; }
        public ObservableCollection<string> CodeReferences { get; }
        public ObservableCollection<RequiredAction> RequiredActions { get; }

        public string Methodology { get; private set; } = string.Empty;

        public string Comments
        {
            get => _comments;
            set
            {
                _comments = value;
                OnPropertyChanged();
            }
        }

        public ApprovalStatus ApprovalStatus
        {
            get => _approvalStatus;
            private set
            {
                _approvalStatus = value;
                OnPropertyChanged();
            }
        }

        public bool CanApprove => RequiredActions.All(a => a.IsCompleted);

        #endregion

        #region Commands

        public ICommand ApproveCommand { get; }
        public ICommand RejectCommand { get; }
        public ICommand RequestInfoCommand { get; }

        #endregion

        #region Methods

        private void PopulateData()
        {
            // Populate calculation results (would come from approval point data)
            CalculationResults.Add(new CalculationResultItem 
            { 
                Name = "Total Connected Load", 
                Value = 847.5, 
                Units = "kVA",
                IsCritical = true
            });
            
            CalculationResults.Add(new CalculationResultItem 
            { 
                Name = "Demand Load", 
                Value = 678.0, 
                Units = "kVA",
                IsCritical = true
            });
            
            CalculationResults.Add(new CalculationResultItem 
            { 
                Name = "Service Size", 
                Value = 800, 
                Units = "A",
                IsCritical = true
            });
            
            CalculationResults.Add(new CalculationResultItem 
            { 
                Name = "Voltage Drop", 
                Value = 2.3, 
                Units = "%",
                IsCritical = false
            });

            // Populate code references
            foreach (var reference in _approvalPoint.RequiredDocuments)
            {
                CodeReferences.Add(reference);
            }
            
            // Add default references if none provided
            if (CodeReferences.Count == 0)
            {
                CodeReferences.Add("NEC Article 220 - Load Calculations");
                CodeReferences.Add("NEC Article 230 - Services");
                CodeReferences.Add("NEC Table 310.15(B)(16) - Conductor Sizing");
            }

            // Populate required actions
            RequiredActions.Add(new RequiredAction 
            { 
                Description = "Verify all load calculations against connected equipment schedules",
                IsCompleted = false
            });
            
            RequiredActions.Add(new RequiredAction 
            { 
                Description = "Confirm demand factors comply with NEC Article 220",
                IsCompleted = false
            });
            
            RequiredActions.Add(new RequiredAction 
            { 
                Description = "Review voltage drop calculations for compliance",
                IsCompleted = false
            });
            
            RequiredActions.Add(new RequiredAction 
            { 
                Description = "Validate conductor and protection device coordination",
                IsCompleted = false
            });

            // Set methodology
            Methodology = @"Load Calculation Methodology:
1. Connected loads extracted from equipment schedules
2. Demand factors applied per NEC Table 220.42
3. Largest motor load increased by 25% per NEC 430.24
4. Service sized at 125% of continuous load plus non-continuous
5. Voltage drop calculated using actual circuit lengths
6. Future expansion factor of 20% included per owner requirements";

            // Monitor required actions for approval eligibility
            foreach (var action in RequiredActions)
            {
                action.PropertyChanged += (s, e) =>
                {
                    if (e.PropertyName == nameof(RequiredAction.IsCompleted))
                    {
                        OnPropertyChanged(nameof(CanApprove));
                    }
                };
            }
        }

        private void Approve()
        {
            if (!CanApprove) return;
            
            ApprovalStatus = ApprovalStatus.Approved;
            ApprovalCompleted?.Invoke(this, EventArgs.Empty);
        }

        private void Reject()
        {
            if (string.IsNullOrWhiteSpace(Comments))
            {
                MessageBox.Show(
                    "Please provide comments explaining the reason for rejection.",
                    "Comments Required",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }
            
            ApprovalStatus = ApprovalStatus.Rejected;
            ApprovalCompleted?.Invoke(this, EventArgs.Empty);
        }

        private void RequestInfo()
        {
            ApprovalStatus = ApprovalStatus.ConditionallyApproved;
            Comments = "Additional information requested: " + Comments;
            ApprovalCompleted?.Invoke(this, EventArgs.Empty);
        }

        #endregion

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }

    #region Supporting Classes

    public class CalculationResultItem
    {
        public string Name { get; set; } = string.Empty;
        public double Value { get; set; }
        public string Units { get; set; } = string.Empty;
        public bool IsCritical { get; set; }
    }

    public class RequiredAction : INotifyPropertyChanged
    {
        private bool _isCompleted;

        public string Description { get; set; } = string.Empty;
        
        public bool IsCompleted
        {
            get => _isCompleted;
            set
            {
                _isCompleted = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    #endregion
}
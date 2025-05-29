using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using RevitAIAssistant.Models;
using RevitAIAssistant.UI.Themes;

namespace RevitAIAssistant.UI.Controls
{
    /// <summary>
    /// Control for presenting rich engineering content in chat messages
    /// </summary>
    public class RichContentPresenter : ContentControl
    {
        static RichContentPresenter()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(RichContentPresenter), 
                new FrameworkPropertyMetadata(typeof(RichContentPresenter)));
        }

        protected override void OnContentChanged(object oldContent, object newContent)
        {
            base.OnContentChanged(oldContent, newContent);

            if (newContent == null)
            {
                Content = null;
                return;
            }

            Content = newContent switch
            {
                ExecutionPlanContent planContent => CreateExecutionPlanView(planContent),
                CalculationResultsContent calcContent => CreateCalculationResultsView(calcContent),
                DocumentationContent docContent => CreateDocumentationView(docContent),
                _ => new TextBlock { Text = newContent.ToString() ?? string.Empty }
            };
        }

        private UIElement CreateExecutionPlanView(ExecutionPlanContent content)
        {
            var theme = ThemeManager.CurrentTheme;
            var stackPanel = new StackPanel { Margin = new Thickness(0, 8, 0, 0) };

            // Title
            var titleBlock = new TextBlock
            {
                Text = "Engineering Task Execution Plan",
                FontSize = 16,
                FontWeight = FontWeights.SemiBold,
                Margin = new Thickness(0, 0, 0, 8),
                Foreground = new SolidColorBrush(theme.PrimaryText)
            };
            stackPanel.Children.Add(titleBlock);

            // Plan summary
            var summaryBorder = new Border
            {
                Background = new SolidColorBrush(Color.FromArgb(25, theme.AccentColor.R, theme.AccentColor.G, theme.AccentColor.B)),
                CornerRadius = new CornerRadius(8),
                Padding = new Thickness(12),
                Margin = new Thickness(0, 0, 0, 12)
            };

            var summaryPanel = new StackPanel();
            summaryBorder.Child = summaryPanel;

            // Objective
            var objectiveBlock = new TextBlock
            {
                Text = content.Plan.Objective,
                FontWeight = FontWeights.SemiBold,
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 0, 0, 8),
                Foreground = new SolidColorBrush(theme.PrimaryText)
            };
            summaryPanel.Children.Add(objectiveBlock);

            // Details grid
            var detailsGrid = new Grid();
            detailsGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            detailsGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            detailsGrid.RowDefinitions.Add(new RowDefinition());
            detailsGrid.RowDefinitions.Add(new RowDefinition());
            detailsGrid.RowDefinitions.Add(new RowDefinition());

            // Complexity
            AddDetailRow(detailsGrid, 0, "Complexity:", content.Plan.Complexity.ToString(), theme);
            AddDetailRow(detailsGrid, 1, "Estimated Time:", content.Plan.EstimatedDuration, theme);
            AddDetailRow(detailsGrid, 2, "Steps:", content.Plan.Tasks.Count.ToString(), theme);

            summaryPanel.Children.Add(detailsGrid);
            stackPanel.Children.Add(summaryBorder);

            // Execution strategy
            if (content.Plan.ExecutionStrategy != null)
            {
                var strategyBlock = new TextBlock
                {
                    Text = "Execution Strategy",
                    FontWeight = FontWeights.SemiBold,
                    Margin = new Thickness(0, 0, 0, 4),
                    Foreground = new SolidColorBrush(theme.PrimaryText)
                };
                stackPanel.Children.Add(strategyBlock);

                var strategyText = new TextBlock
                {
                    Text = content.Plan.ExecutionStrategy.Approach,
                    TextWrapping = TextWrapping.Wrap,
                    Margin = new Thickness(0, 0, 0, 12),
                    Foreground = new SolidColorBrush(theme.SecondaryText)
                };
                stackPanel.Children.Add(strategyText);
            }

            // Tasks
            var tasksBlock = new TextBlock
            {
                Text = "Execution Steps",
                FontWeight = FontWeights.SemiBold,
                Margin = new Thickness(0, 0, 0, 8),
                Foreground = new SolidColorBrush(theme.PrimaryText)
            };
            stackPanel.Children.Add(tasksBlock);

            foreach (var task in content.Plan.Tasks)
            {
                var taskBorder = new Border
                {
                    BorderBrush = new SolidColorBrush(theme.BorderColor),
                    BorderThickness = new Thickness(1),
                    CornerRadius = new CornerRadius(6),
                    Margin = new Thickness(0, 0, 0, 8),
                    Padding = new Thickness(12)
                };

                var taskPanel = new StackPanel();
                taskBorder.Child = taskPanel;

                // Task header
                var taskHeader = new StackPanel { Orientation = Orientation.Horizontal };
                
                var stepNumber = new TextBlock
                {
                    Text = $"Step {task.StepNumber}:",
                    FontWeight = FontWeights.SemiBold,
                    Margin = new Thickness(0, 0, 8, 0),
                    Foreground = new SolidColorBrush(theme.AccentColor)
                };
                taskHeader.Children.Add(stepNumber);

                var taskTitle = new TextBlock
                {
                    Text = task.Title,
                    FontWeight = FontWeights.SemiBold,
                    Foreground = new SolidColorBrush(theme.PrimaryText)
                };
                taskHeader.Children.Add(taskTitle);

                if (task.RequiresPEApproval)
                {
                    var approvalBadge = new Border
                    {
                        Background = new SolidColorBrush(theme.ApprovalRequiredColor),
                        CornerRadius = new CornerRadius(10),
                        Padding = new Thickness(8, 2),
                        Margin = new Thickness(8, 0, 0, 0)
                    };
                    
                    var approvalText = new TextBlock
                    {
                        Text = "PE Approval Required",
                        FontSize = 11,
                        Foreground = Brushes.White
                    };
                    approvalBadge.Child = approvalText;
                    taskHeader.Children.Add(approvalBadge);
                }

                taskPanel.Children.Add(taskHeader);

                // Task details
                var purposeBlock = new TextBlock
                {
                    Text = task.Purpose,
                    TextWrapping = TextWrapping.Wrap,
                    Margin = new Thickness(0, 4, 0, 0),
                    Foreground = new SolidColorBrush(theme.SecondaryText)
                };
                taskPanel.Children.Add(purposeBlock);

                // Estimated time
                var timeBlock = new TextBlock
                {
                    Text = $"Estimated time: {task.EstimatedTime}",
                    FontSize = 12,
                    Margin = new Thickness(0, 4, 0, 0),
                    Foreground = new SolidColorBrush(theme.SecondaryText),
                    Opacity = 0.8
                };
                taskPanel.Children.Add(timeBlock);

                stackPanel.Children.Add(taskBorder);
            }

            // Approval buttons
            var buttonPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(0, 12, 0, 0)
            };

            var approveButton = new Button
            {
                Content = "Approve & Execute",
                Padding = new Thickness(16, 8),
                Margin = new Thickness(0, 0, 8, 0),
                Background = new SolidColorBrush(theme.SuccessColor),
                Foreground = Brushes.White,
                BorderThickness = new Thickness(0)
            };
            approveButton.Click += (s, e) => content.OnApprove?.Invoke();
            buttonPanel.Children.Add(approveButton);

            var rejectButton = new Button
            {
                Content = "Cancel",
                Padding = new Thickness(16, 8),
                Background = new SolidColorBrush(theme.SecondaryButtonBackground),
                Foreground = new SolidColorBrush(theme.SecondaryButtonText),
                BorderThickness = new Thickness(0)
            };
            rejectButton.Click += (s, e) => content.OnReject?.Invoke();
            buttonPanel.Children.Add(rejectButton);

            stackPanel.Children.Add(buttonPanel);

            return stackPanel;
        }

        private UIElement CreateCalculationResultsView(CalculationResultsContent content)
        {
            var theme = ThemeManager.CurrentTheme;
            var stackPanel = new StackPanel { Margin = new Thickness(0, 8, 0, 0) };

            // Title
            var titleBlock = new TextBlock
            {
                Text = "Calculation Results",
                FontSize = 16,
                FontWeight = FontWeights.SemiBold,
                Margin = new Thickness(0, 0, 0, 8),
                Foreground = new SolidColorBrush(theme.PrimaryText)
            };
            stackPanel.Children.Add(titleBlock);

            foreach (var calc in content.Calculations)
            {
                var calcBorder = new Border
                {
                    Background = new SolidColorBrush(theme.CalculationBackground),
                    BorderBrush = new SolidColorBrush(theme.BorderColor),
                    BorderThickness = new Thickness(1),
                    CornerRadius = new CornerRadius(8),
                    Padding = new Thickness(12),
                    Margin = new Thickness(0, 0, 0, 12)
                };

                var calcPanel = new StackPanel();
                calcBorder.Child = calcPanel;

                // Calculation name
                var nameBlock = new TextBlock
                {
                    Text = calc.Name,
                    FontWeight = FontWeights.SemiBold,
                    Margin = new Thickness(0, 0, 0, 4),
                    Foreground = new SolidColorBrush(theme.PrimaryText)
                };
                calcPanel.Children.Add(nameBlock);

                // Description
                if (!string.IsNullOrEmpty(calc.Description))
                {
                    var descBlock = new TextBlock
                    {
                        Text = calc.Description,
                        TextWrapping = TextWrapping.Wrap,
                        Margin = new Thickness(0, 0, 0, 8),
                        Foreground = new SolidColorBrush(theme.SecondaryText)
                    };
                    calcPanel.Children.Add(descBlock);
                }

                // Calculation steps
                foreach (var step in calc.Steps)
                {
                    var stepPanel = new StackPanel { Margin = new Thickness(0, 0, 0, 8) };

                    // Step description
                    var stepDesc = new TextBlock
                    {
                        Text = $"{step.StepNumber}. {step.Description}",
                        Margin = new Thickness(0, 0, 0, 2),
                        Foreground = new SolidColorBrush(theme.PrimaryText)
                    };
                    stepPanel.Children.Add(stepDesc);

                    // Formula
                    var formulaBorder = new Border
                    {
                        Background = new SolidColorBrush(theme.CodeBackground),
                        CornerRadius = new CornerRadius(4),
                        Padding = new Thickness(8, 4),
                        Margin = new Thickness(16, 2, 0, 2)
                    };

                    var formulaBlock = new TextBlock
                    {
                        Text = step.Formula,
                        FontFamily = new FontFamily("Consolas, Courier New"),
                        Foreground = new SolidColorBrush(theme.FormulaHighlight)
                    };
                    formulaBorder.Child = formulaBlock;
                    stepPanel.Children.Add(formulaBorder);

                    // Result
                    var resultBlock = new TextBlock
                    {
                        Text = $"= {step.Result:F2} {step.Units}",
                        FontWeight = FontWeights.SemiBold,
                        Margin = new Thickness(16, 2, 0, 0),
                        Foreground = new SolidColorBrush(theme.AccentColor)
                    };
                    stepPanel.Children.Add(resultBlock);

                    calcPanel.Children.Add(stepPanel);
                }

                // Final results
                var resultsBorder = new Border
                {
                    Background = new SolidColorBrush(Color.FromArgb(25, theme.SuccessColor.R, theme.SuccessColor.G, theme.SuccessColor.B)),
                    CornerRadius = new CornerRadius(4),
                    Padding = new Thickness(8),
                    Margin = new Thickness(0, 8, 0, 0)
                };

                var resultsPanel = new StackPanel();
                resultsBorder.Child = resultsPanel;

                var resultsTitle = new TextBlock
                {
                    Text = "Results:",
                    FontWeight = FontWeights.SemiBold,
                    Margin = new Thickness(0, 0, 0, 4),
                    Foreground = new SolidColorBrush(theme.PrimaryText)
                };
                resultsPanel.Children.Add(resultsTitle);

                foreach (var result in calc.Results)
                {
                    var resultText = new TextBlock
                    {
                        Text = $"{result.Key}: {result.Value:F2} {calc.Units}",
                        Foreground = new SolidColorBrush(theme.PrimaryText)
                    };
                    resultsPanel.Children.Add(resultText);
                }

                calcPanel.Children.Add(resultsBorder);

                // Code references
                if (calc.CodeReferences.Count > 0)
                {
                    var refsBlock = new TextBlock
                    {
                        Text = $"References: {string.Join(", ", calc.CodeReferences)}",
                        FontSize = 12,
                        Margin = new Thickness(0, 8, 0, 0),
                        Foreground = new SolidColorBrush(theme.SecondaryText),
                        Opacity = 0.8
                    };
                    calcPanel.Children.Add(refsBlock);
                }

                stackPanel.Children.Add(calcBorder);
            }

            return stackPanel;
        }

        private UIElement CreateDocumentationView(DocumentationContent content)
        {
            var theme = ThemeManager.CurrentTheme;
            var stackPanel = new StackPanel { Margin = new Thickness(0, 8, 0, 0) };

            // Title
            var titleBlock = new TextBlock
            {
                Text = content.Documentation.Title,
                FontSize = 16,
                FontWeight = FontWeights.SemiBold,
                Margin = new Thickness(0, 0, 0, 8),
                Foreground = new SolidColorBrush(theme.PrimaryText)
            };
            stackPanel.Children.Add(titleBlock);

            // Document type badge
            var typeBorder = new Border
            {
                Background = new SolidColorBrush(theme.AccentColor),
                CornerRadius = new CornerRadius(10),
                Padding = new Thickness(8, 2),
                HorizontalAlignment = HorizontalAlignment.Left,
                Margin = new Thickness(0, 0, 0, 8)
            };
            
            var typeText = new TextBlock
            {
                Text = content.Documentation.Type.ToString().Replace("_", " "),
                FontSize = 12,
                Foreground = Brushes.White
            };
            typeBorder.Child = typeText;
            stackPanel.Children.Add(typeBorder);

            // Preview of content
            var previewBorder = new Border
            {
                Background = new SolidColorBrush(theme.CodeBackground),
                BorderBrush = new SolidColorBrush(theme.BorderColor),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(4),
                Padding = new Thickness(12),
                MaxHeight = 200
            };

            var scrollViewer = new ScrollViewer
            {
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto
            };

            var contentBlock = new TextBlock
            {
                Text = content.Documentation.Content,
                TextWrapping = TextWrapping.Wrap,
                FontFamily = new FontFamily("Consolas, Courier New"),
                FontSize = 12,
                Foreground = new SolidColorBrush(theme.CodeText)
            };
            scrollViewer.Content = contentBlock;
            previewBorder.Child = scrollViewer;
            stackPanel.Children.Add(previewBorder);

            // PE seal notice
            if (content.Documentation.RequiresPESeal)
            {
                var sealNotice = new Border
                {
                    Background = new SolidColorBrush(theme.ApprovalRequiredColor),
                    CornerRadius = new CornerRadius(4),
                    Padding = new Thickness(8),
                    Margin = new Thickness(0, 8, 0, 8)
                };

                var sealText = new TextBlock
                {
                    Text = "⚠️ This document requires Professional Engineer seal before use",
                    Foreground = Brushes.White,
                    FontWeight = FontWeights.SemiBold
                };
                sealNotice.Child = sealText;
                stackPanel.Children.Add(sealNotice);
            }

            // Export buttons
            var exportPanel = new WrapPanel
            {
                Margin = new Thickness(0, 8, 0, 0)
            };

            var exportFormats = new[] { "PDF", "Word", "Excel", "CSV" };
            foreach (var format in exportFormats)
            {
                var exportButton = new Button
                {
                    Content = $"Export as {format}",
                    Padding = new Thickness(12, 6),
                    Margin = new Thickness(0, 0, 8, 8),
                    Background = new SolidColorBrush(theme.SecondaryButtonBackground),
                    Foreground = new SolidColorBrush(theme.SecondaryButtonText),
                    BorderThickness = new Thickness(1),
                    BorderBrush = new SolidColorBrush(theme.BorderColor)
                };
                var fmt = format; // Capture for lambda
                exportButton.Click += (s, e) => content.OnExport?.Invoke(fmt);
                exportPanel.Children.Add(exportButton);
            }

            stackPanel.Children.Add(exportPanel);

            return stackPanel;
        }

        private void AddDetailRow(Grid grid, int row, string label, string value, ThemeColors theme)
        {
            var labelBlock = new TextBlock
            {
                Text = label,
                Margin = new Thickness(0, 0, 8, 2),
                Foreground = new SolidColorBrush(theme.SecondaryText)
            };
            Grid.SetRow(labelBlock, row);
            Grid.SetColumn(labelBlock, 0);
            grid.Children.Add(labelBlock);

            var valueBlock = new TextBlock
            {
                Text = value,
                FontWeight = FontWeights.SemiBold,
                Foreground = new SolidColorBrush(theme.PrimaryText)
            };
            Grid.SetRow(valueBlock, row);
            Grid.SetColumn(valueBlock, 1);
            grid.Children.Add(valueBlock);
        }
    }

    #region Content Types

    public class ExecutionPlanContent
    {
        public EngineeringExecutionPlan Plan { get; set; } = new();
        public Action? OnApprove { get; set; }
        public Action? OnReject { get; set; }
    }

    public class CalculationResultsContent
    {
        public List<CalculationResult> Calculations { get; set; } = new();
    }

    public class DocumentationContent
    {
        public EngineeringDocumentation Documentation { get; set; } = new();
        public Action<string>? OnExport { get; set; }
    }

    #endregion
}
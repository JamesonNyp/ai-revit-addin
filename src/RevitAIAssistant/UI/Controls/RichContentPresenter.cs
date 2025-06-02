using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using RevitAIAssistant.Models;
using RevitAIAssistant.Services;
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
                return;
            }

            // Avoid infinite recursion by checking if we're already processing a rich content type
            if (newContent is UIElement)
            {
                return;
            }

            var visualContent = newContent switch
            {
                ExecutionPlanContent planContent => CreateExecutionPlanView(planContent),
                CalculationResultsContent calcContent => CreateCalculationResultsView(calcContent),
                DocumentationContent docContent => CreateDocumentationView(docContent),
                OrchestrationProgressContent orchContent => CreateOrchestrationProgressView(orchContent),
                OrchestrationResultsContent resultsContent => CreateOrchestrationResultsView(resultsContent),
                _ => new TextBlock { Text = newContent.ToString() ?? string.Empty }
            };

            // Use the template to display the content instead of setting Content directly
            if (Template != null)
            {
                var contentPresenter = Template.FindName("PART_ContentPresenter", this) as ContentPresenter;
                if (contentPresenter != null)
                {
                    contentPresenter.Content = visualContent;
                    return;
                }
            }

            // Fallback: replace the template with our custom content
            Template = new ControlTemplate(typeof(RichContentPresenter))
            {
                VisualTree = new FrameworkElementFactory(typeof(ContentPresenter))
                {
                    Name = "PART_ContentPresenter"
                }
            };
            ApplyTemplate();
            
            var presenter = Template.FindName("PART_ContentPresenter", this) as ContentPresenter;
            if (presenter != null)
            {
                presenter.Content = visualContent;
            }
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
                Foreground = Brushes.White
            };
            stackPanel.Children.Add(titleBlock);

            // Plan summary
            var summaryBorder = new Border
            {
                Background = new SolidColorBrush(Color.FromArgb(255, 50, 58, 71)), // Slightly lighter than parent #3B4453
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
                Foreground = Brushes.White
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
                    Foreground = Brushes.White
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
                Foreground = Brushes.White
            };
            stackPanel.Children.Add(tasksBlock);

            foreach (var task in content.Plan.Tasks)
            {
                var taskBorder = new Border
                {
                    Background = new SolidColorBrush(Color.FromArgb(255, 50, 58, 71)), // Slightly lighter than parent
                    BorderBrush = new SolidColorBrush(Color.FromArgb(255, 70, 78, 91)),
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
                    Foreground = Brushes.White
                };
                taskHeader.Children.Add(taskTitle);

                if (task.RequiresPEApproval)
                {
                    var approvalBadge = new Border
                    {
                        Background = new SolidColorBrush(theme.ApprovalRequiredColor),
                        CornerRadius = new CornerRadius(10),
                        Padding = new Thickness(8, 2, 8, 2),
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
                Padding = new Thickness(16, 8, 16, 8),
                Margin = new Thickness(0, 0, 8, 0),
                Background = new SolidColorBrush(theme.SuccessColor),
                Foreground = Brushes.White,
                BorderThickness = new Thickness(0)
            };
            
            var rejectButton = new Button
            {
                Content = "Cancel",
                Padding = new Thickness(16, 8, 16, 8),
                Background = new SolidColorBrush(theme.SecondaryButtonBackground),
                Foreground = new SolidColorBrush(theme.SecondaryButtonText),
                BorderThickness = new Thickness(0)
            };
            
            approveButton.Click += (s, e) =>
            {
                // Disable both buttons
                approveButton.IsEnabled = false;
                rejectButton.IsEnabled = false;
                approveButton.Opacity = 0.5;
                rejectButton.Opacity = 0.5;
                
                // Change approve button to show selected state
                approveButton.Background = new SolidColorBrush(Color.FromArgb(128, theme.SuccessColor.R, theme.SuccessColor.G, theme.SuccessColor.B));
                
                content.OnApprove?.Invoke();
            };
            
            rejectButton.Click += (s, e) =>
            {
                // Disable both buttons
                approveButton.IsEnabled = false;
                rejectButton.IsEnabled = false;
                approveButton.Opacity = 0.5;
                rejectButton.Opacity = 0.5;
                
                // Change reject button to show selected state
                rejectButton.Background = new SolidColorBrush(Color.FromArgb(128, theme.SecondaryButtonBackground.R, theme.SecondaryButtonBackground.G, theme.SecondaryButtonBackground.B));
                
                content.OnReject?.Invoke();
            };
            
            buttonPanel.Children.Add(approveButton);
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
                Foreground = Brushes.White
            };
            stackPanel.Children.Add(titleBlock);

            foreach (var calc in content.Calculations)
            {
                var calcBorder = new Border
                {
                    Background = new SolidColorBrush(Color.FromArgb(255, 50, 58, 71)), // Slightly lighter than parent
                    BorderBrush = new SolidColorBrush(Color.FromArgb(255, 70, 78, 91)),
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
                    Foreground = Brushes.White
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
                        Foreground = Brushes.White
                    };
                    stepPanel.Children.Add(stepDesc);

                    // Formula
                    var formulaBorder = new Border
                    {
                        Background = new SolidColorBrush(Color.FromArgb(255, 30, 36, 45)), // Darker for code
                        CornerRadius = new CornerRadius(4),
                        Padding = new Thickness(8, 4, 8, 4),
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
                    Background = new SolidColorBrush(Color.FromArgb(255, 45, 70, 55)), // Green-tinted offset
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
                    Foreground = Brushes.White
                };
                resultsPanel.Children.Add(resultsTitle);

                foreach (var result in calc.Results)
                {
                    var resultText = new TextBlock
                    {
                        Text = $"{result.Key}: {result.Value:F2} {calc.Units}",
                        Foreground = Brushes.White
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
                Foreground = Brushes.White
            };
            stackPanel.Children.Add(titleBlock);

            // Document type badge
            var typeBorder = new Border
            {
                Background = new SolidColorBrush(theme.AccentColor),
                CornerRadius = new CornerRadius(10),
                Padding = new Thickness(8, 2, 8, 2),
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
                Background = new SolidColorBrush(Color.FromArgb(255, 30, 36, 45)), // Darker for code preview
                BorderBrush = new SolidColorBrush(Color.FromArgb(255, 70, 78, 91)),
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
                    Padding = new Thickness(12, 6, 12, 6),
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
                Foreground = new SolidColorBrush(Color.FromArgb(255, 200, 200, 200))
            };
            Grid.SetRow(labelBlock, row);
            Grid.SetColumn(labelBlock, 0);
            grid.Children.Add(labelBlock);

            var valueBlock = new TextBlock
            {
                Text = value,
                FontWeight = FontWeights.SemiBold,
                Foreground = Brushes.White
            };
            Grid.SetRow(valueBlock, row);
            Grid.SetColumn(valueBlock, 1);
            grid.Children.Add(valueBlock);
        }

        private UIElement CreateOrchestrationProgressView(OrchestrationProgressContent content)
        {
            var theme = ThemeManager.CurrentTheme;
            var stackPanel = new StackPanel { Margin = new Thickness(0, 8, 0, 0) };

            // Title
            var titleBlock = new TextBlock
            {
                Text = "🚀 Task Orchestration in Progress",
                FontSize = 16,
                FontWeight = FontWeights.SemiBold,
                Margin = new Thickness(0, 0, 0, 8),
                Foreground = Brushes.White
            };
            stackPanel.Children.Add(titleBlock);

            // Overall progress
            var progressBorder = new Border
            {
                Background = new SolidColorBrush(Color.FromArgb(255, 50, 58, 71)), // Slightly lighter than parent
                CornerRadius = new CornerRadius(8),
                Padding = new Thickness(12),
                Margin = new Thickness(0, 0, 0, 12)
            };

            var progressPanel = new StackPanel();
            progressBorder.Child = progressPanel;

            // Progress bar
            var progressBarBorder = new Border
            {
                Height = 8,
                Background = new SolidColorBrush(Color.FromArgb(255, 30, 36, 45)), // Darker background for contrast
                CornerRadius = new CornerRadius(4),
                Margin = new Thickness(0, 0, 0, 8)
            };

            var progressBar = new Border
            {
                HorizontalAlignment = HorizontalAlignment.Left,
                Width = Math.Max(0, content.Process.OverallProgress / 100.0 * 300), // Fixed width calculation
                Background = new LinearGradientBrush(theme.AccentColor, theme.SuccessColor, 0),
                CornerRadius = new CornerRadius(4),
                Height = 8 // Ensure the progress bar fills the height
            };
            progressBarBorder.Child = progressBar;
            progressPanel.Children.Add(progressBarBorder);

            // Progress text
            var progressText = new TextBlock
            {
                Text = $"Overall Progress: {content.Process.OverallProgress}%",
                FontWeight = FontWeights.SemiBold,
                Foreground = Brushes.White
            };
            progressPanel.Children.Add(progressText);

            stackPanel.Children.Add(progressBorder);

            // Steps
            var stepsBlock = new TextBlock
            {
                Text = "Execution Steps",
                FontWeight = FontWeights.SemiBold,
                Margin = new Thickness(0, 0, 0, 8),
                Foreground = Brushes.White
            };
            stackPanel.Children.Add(stepsBlock);

            foreach (var step in content.Process.Steps)
            {
                var stepBorder = new Border
                {
                    BorderBrush = new SolidColorBrush(theme.BorderColor),
                    BorderThickness = new Thickness(1),
                    CornerRadius = new CornerRadius(6),
                    Margin = new Thickness(0, 0, 0, 8),
                    Padding = new Thickness(12)
                };

                // Set background based on status
                stepBorder.Background = step.Status switch
                {
                    "completed" => new SolidColorBrush(Color.FromArgb(255, 45, 70, 55)), // Green-tinted
                    "running" => new SolidColorBrush(Color.FromArgb(255, 45, 60, 80)), // Blue-tinted
                    _ => new SolidColorBrush(Color.FromArgb(255, 50, 58, 71)) // Default offset
                };

                var stepPanel = new StackPanel();
                stepBorder.Child = stepPanel;

                // Step header
                var stepHeader = new Grid();
                stepHeader.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                stepHeader.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

                var stepNamePanel = new StackPanel { Orientation = Orientation.Horizontal };
                
                // Status icon
                var statusIcon = new TextBlock
                {
                    Text = step.Status switch
                    {
                        "completed" => "✅",
                        "running" => "⚙️",
                        _ => "⏸️"
                    },
                    Margin = new Thickness(0, 0, 8, 0),
                    VerticalAlignment = VerticalAlignment.Center
                };
                stepNamePanel.Children.Add(statusIcon);

                var stepName = new TextBlock
                {
                    Text = step.Name,
                    FontWeight = FontWeights.SemiBold,
                    Foreground = Brushes.White,
                    VerticalAlignment = VerticalAlignment.Center
                };
                stepNamePanel.Children.Add(stepName);

                Grid.SetColumn(stepNamePanel, 0);
                stepHeader.Children.Add(stepNamePanel);

                // Agent badge
                var agentBadge = new Border
                {
                    Background = new SolidColorBrush(theme.AccentColor),
                    CornerRadius = new CornerRadius(10),
                    Padding = new Thickness(8, 2, 8, 2)
                };
                
                var agentText = new TextBlock
                {
                    Text = step.AgentType.Replace("_", " "),
                    FontSize = 11,
                    Foreground = Brushes.White
                };
                agentBadge.Child = agentText;
                Grid.SetColumn(agentBadge, 1);
                stepHeader.Children.Add(agentBadge);

                stepPanel.Children.Add(stepHeader);

                // Step description
                var descBlock = new TextBlock
                {
                    Text = step.Description,
                    TextWrapping = TextWrapping.Wrap,
                    Margin = new Thickness(24, 4, 0, 0),
                    Foreground = new SolidColorBrush(theme.SecondaryText)
                };
                stepPanel.Children.Add(descBlock);

                // Progress for running step
                if (step.Status == "running" && step.Progress > 0)
                {
                    var stepProgressBorder = new Border
                    {
                        Height = 4,
                        Background = new SolidColorBrush(Color.FromArgb(255, 30, 36, 45)), // Darker background for contrast
                        CornerRadius = new CornerRadius(2),
                        Margin = new Thickness(24, 8, 0, 4)
                    };

                    var stepProgressBar = new Border
                    {
                        HorizontalAlignment = HorizontalAlignment.Left,
                        Width = Math.Max(0, step.Progress / 100.0 * 250), // Fixed width calculation
                        Background = new SolidColorBrush(theme.AccentColor),
                        CornerRadius = new CornerRadius(2),
                        Height = 4 // Ensure the progress bar fills the height
                    };
                    stepProgressBorder.Child = stepProgressBar;
                    stepPanel.Children.Add(stepProgressBorder);

                    // Current sub-task
                    if (step.SubTasks.Count > 0)
                    {
                        var subTaskIndex = (int)(step.Progress / 100.0 * step.SubTasks.Count);
                        if (subTaskIndex < step.SubTasks.Count)
                        {
                            var subTaskBlock = new TextBlock
                            {
                                Text = $"▸ {step.SubTasks[subTaskIndex]}",
                                FontSize = 12,
                                FontStyle = FontStyles.Italic,
                                Margin = new Thickness(32, 0, 0, 0),
                                Foreground = new SolidColorBrush(Color.FromRgb(122, 165, 186)) // #7aa5ba
                            };
                            stepPanel.Children.Add(subTaskBlock);
                        }
                    }
                }

                // Execution time for completed steps
                if (step.Status == "completed" && step.StartTime.HasValue && step.EndTime.HasValue)
                {
                    var duration = step.EndTime.Value - step.StartTime.Value;
                    var timeBlock = new TextBlock
                    {
                        Text = $"Completed in {duration.TotalSeconds:F1} seconds",
                        FontSize = 11,
                        Margin = new Thickness(24, 4, 0, 0),
                        Foreground = new SolidColorBrush(theme.SuccessColor),
                        Opacity = 0.8
                    };
                    stepPanel.Children.Add(timeBlock);
                }

                stackPanel.Children.Add(stepBorder);
            }

            return stackPanel;
        }

        private UIElement CreateOrchestrationResultsView(OrchestrationResultsContent content)
        {
            var theme = ThemeManager.CurrentTheme;
            var stackPanel = new StackPanel { Margin = new Thickness(0, 8, 0, 0) };

            // Title
            var titleBlock = new TextBlock
            {
                Text = $"✅ {content.ProcessType} Complete",
                FontSize = 16,
                FontWeight = FontWeights.SemiBold,
                Margin = new Thickness(0, 0, 0, 8),
                Foreground = new SolidColorBrush(theme.SuccessColor)
            };
            stackPanel.Children.Add(titleBlock);

            // Summary
            var summaryBorder = new Border
            {
                Background = new SolidColorBrush(Color.FromArgb(255, 45, 70, 55)), // Green-tinted for success
                CornerRadius = new CornerRadius(8),
                Padding = new Thickness(12),
                Margin = new Thickness(0, 0, 0, 12)
            };

            var summaryText = new TextBlock
            {
                Text = $"Total execution time: {content.ExecutionTime.TotalMinutes:F1} minutes",
                FontWeight = FontWeights.SemiBold,
                Foreground = Brushes.White
            };
            summaryBorder.Child = summaryText;
            stackPanel.Children.Add(summaryBorder);

            // Results by step
            var resultsBlock = new TextBlock
            {
                Text = "Results Summary",
                FontWeight = FontWeights.SemiBold,
                Margin = new Thickness(0, 0, 0, 8),
                Foreground = Brushes.White
            };
            stackPanel.Children.Add(resultsBlock);

            foreach (var step in content.Steps)
            {
                if (string.IsNullOrEmpty(step.Result)) continue;

                var stepResultBorder = new Border
                {
                    Background = new SolidColorBrush(Color.FromArgb(255, 50, 58, 71)), // Slightly lighter than parent
                    BorderBrush = new SolidColorBrush(Color.FromArgb(255, 70, 78, 91)),
                    BorderThickness = new Thickness(1),
                    CornerRadius = new CornerRadius(6),
                    Margin = new Thickness(0, 0, 0, 8),
                    Padding = new Thickness(12)
                };

                var stepResultPanel = new StackPanel();
                stepResultBorder.Child = stepResultPanel;

                // Step name
                var stepNameBlock = new TextBlock
                {
                    Text = step.Name,
                    FontWeight = FontWeights.SemiBold,
                    Margin = new Thickness(0, 0, 0, 4),
                    Foreground = new SolidColorBrush(theme.AccentColor)
                };
                stepResultPanel.Children.Add(stepNameBlock);

                // Step results
                var resultLines = step.Result.Split('\n');
                foreach (var line in resultLines)
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;

                    var resultBlock = new TextBlock
                    {
                        Text = line.Trim(),
                        TextWrapping = TextWrapping.Wrap,
                        Margin = new Thickness(0, 2, 0, 0),
                        Foreground = Brushes.White
                    };

                    // Style based on content
                    if (line.Contains("✓"))
                    {
                        resultBlock.Foreground = new SolidColorBrush(theme.SuccessColor);
                    }
                    else if (line.Contains("⚠") || line.Contains("🟡"))
                    {
                        resultBlock.Foreground = new SolidColorBrush(theme.WarningColor);
                    }
                    else if (line.Contains("🔴"))
                    {
                        resultBlock.Foreground = new SolidColorBrush(theme.ErrorColor);
                    }

                    stepResultPanel.Children.Add(resultBlock);
                }

                stackPanel.Children.Add(stepResultBorder);
            }

            // Action buttons
            var actionPanel = new WrapPanel
            {
                Margin = new Thickness(0, 12, 0, 0)
            };

            var viewDetailsButton = new Button
            {
                Content = "View Detailed Report",
                Padding = new Thickness(16, 8, 16, 8),
                Margin = new Thickness(0, 0, 8, 0),
                Background = new SolidColorBrush(theme.AccentColor),
                Foreground = Brushes.White,
                BorderThickness = new Thickness(0)
            };
            viewDetailsButton.Click += (s, e) =>
            {
                // Create a mock detailed report
                var report = GenerateMockDetailedReport(content);
                System.Windows.MessageBox.Show(report, "Detailed Engineering Report", 
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
            };
            actionPanel.Children.Add(viewDetailsButton);

            var exportButton = new Button
            {
                Content = "Export Results",
                Padding = new Thickness(16, 8, 16, 8),
                Background = new SolidColorBrush(theme.SecondaryButtonBackground),
                Foreground = new SolidColorBrush(theme.SecondaryButtonText),
                BorderThickness = new Thickness(1),
                BorderBrush = new SolidColorBrush(theme.BorderColor)
            };
            exportButton.Click += (s, e) =>
            {
                // Show export options
                ShowExportOptions(content);
            };
            actionPanel.Children.Add(exportButton);

            stackPanel.Children.Add(actionPanel);

            return stackPanel;
        }

        private string GenerateMockDetailedReport(OrchestrationResultsContent content)
        {
            var report = new System.Text.StringBuilder();
            report.AppendLine("DETAILED ENGINEERING REPORT");
            report.AppendLine("=" + new string('=', 40));
            report.AppendLine();
            report.AppendLine($"Process Type: {content.ProcessType}");
            report.AppendLine($"Execution Time: {content.ExecutionTime.TotalMinutes:F1} minutes");
            report.AppendLine($"Completion Date: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            report.AppendLine();
            report.AppendLine("EXECUTION SUMMARY");
            report.AppendLine("-" + new string('-', 40));
            report.AppendLine();
            
            foreach (var step in content.Steps)
            {
                report.AppendLine($"Step: {step.Name}");
                report.AppendLine($"Agent: {step.AgentType}");
                report.AppendLine($"Status: Completed");
                report.AppendLine($"Duration: {(step.EndTime - step.StartTime)?.TotalSeconds:F1} seconds");
                report.AppendLine();
                report.AppendLine("Results:");
                report.AppendLine(step.Result);
                report.AppendLine();
                report.AppendLine("-" + new string('-', 40));
                report.AppendLine();
            }
            
            report.AppendLine("COMPLIANCE SUMMARY");
            report.AppendLine("-" + new string('-', 40));
            report.AppendLine("✓ All calculations verified against NEC 2020");
            report.AppendLine("✓ Equipment sizing meets ASHRAE 90.1 requirements");
            report.AppendLine("✓ Design complies with local building codes");
            report.AppendLine();
            
            report.AppendLine("RECOMMENDATIONS");
            report.AppendLine("-" + new string('-', 40));
            report.AppendLine("1. Review panel schedules for final approval");
            report.AppendLine("2. Coordinate with mechanical team on equipment locations");
            report.AppendLine("3. Update single-line diagram with latest changes");
            report.AppendLine();
            
            report.AppendLine("This report is for review purposes only.");
            report.AppendLine("Professional Engineer seal required for construction documents.");
            
            return report.ToString();
        }

        private void ShowExportOptions(OrchestrationResultsContent content)
        {
            var window = new Window
            {
                Title = "Export Results",
                Width = 400,
                Height = 300,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                Background = new SolidColorBrush(ThemeManager.CurrentTheme.WindowBackground)
            };

            var stackPanel = new StackPanel { Margin = new Thickness(20) };
            
            var titleBlock = new TextBlock
            {
                Text = "Select Export Format",
                FontSize = 16,
                FontWeight = FontWeights.SemiBold,
                Margin = new Thickness(0, 0, 0, 20),
                Foreground = new SolidColorBrush(ThemeManager.CurrentTheme.PrimaryText)
            };
            stackPanel.Children.Add(titleBlock);

            var formats = new[] { "PDF Report", "Excel Workbook", "CSV Data", "Word Document" };
            foreach (var format in formats)
            {
                var button = new Button
                {
                    Content = $"Export as {format}",
                    Padding = new Thickness(12, 8, 12, 8),
                    Margin = new Thickness(0, 0, 0, 10),
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    Background = new SolidColorBrush(ThemeManager.CurrentTheme.SecondaryButtonBackground),
                    Foreground = new SolidColorBrush(ThemeManager.CurrentTheme.SecondaryButtonText),
                    BorderBrush = new SolidColorBrush(ThemeManager.CurrentTheme.BorderColor),
                    BorderThickness = new Thickness(1)
                };
                
                button.Click += (s, e) =>
                {
                    var mockPath = $@"C:\Temp\Engineering_Report_{DateTime.Now:yyyyMMdd_HHmmss}.{format.Split(' ')[0].ToLower()}";
                    System.Windows.MessageBox.Show(
                        $"Mock Export Complete!\n\nFile saved to:\n{mockPath}\n\n(This is a UI test - no actual file was created)",
                        "Export Successful",
                        System.Windows.MessageBoxButton.OK,
                        System.Windows.MessageBoxImage.Information);
                    window.Close();
                };
                
                stackPanel.Children.Add(button);
            }

            var cancelButton = new Button
            {
                Content = "Cancel",
                Padding = new Thickness(12, 8, 12, 8),
                Margin = new Thickness(0, 20, 0, 0),
                HorizontalAlignment = HorizontalAlignment.Right,
                Background = new SolidColorBrush(ThemeManager.CurrentTheme.SecondaryButtonBackground),
                Foreground = new SolidColorBrush(ThemeManager.CurrentTheme.SecondaryButtonText),
                BorderThickness = new Thickness(0)
            };
            cancelButton.Click += (s, e) => window.Close();
            stackPanel.Children.Add(cancelButton);

            window.Content = stackPanel;
            window.ShowDialog();
        }
    }
}
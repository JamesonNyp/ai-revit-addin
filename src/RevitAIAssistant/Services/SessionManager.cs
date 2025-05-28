using System;
using System.Collections.Generic;
using Autodesk.Revit.DB;
using Microsoft.Extensions.Logging;
using RevitAIAssistant.Models;

namespace RevitAIAssistant.Services
{
    /// <summary>
    /// Manages user sessions and context
    /// </summary>
    public class SessionManager
    {
        private readonly ILogger<SessionManager> _logger;
        private Document? _currentDocument;
        private View? _currentView;
        private EngineeringContext? _currentContext;
        private readonly List<string> _sessionHistory = new();
        
        public event EventHandler<ContextChangedEventArgs>? ContextChanged;

        public SessionManager(ILogger<SessionManager> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public Document? CurrentDocument => _currentDocument;
        public View? CurrentView => _currentView;
        public EngineeringContext? CurrentContext => _currentContext;
        public IReadOnlyList<string> SessionHistory => _sessionHistory.AsReadOnly();

        public void UpdateDocumentContext(Document? document)
        {
            _currentDocument = document;
            _logger.LogInformation("Document context updated: {DocumentTitle}", 
                document?.Title ?? "None");

            if (document != null)
            {
                UpdateEngineeringContext();
            }
            else
            {
                _currentContext = null;
            }

            OnContextChanged(new ContextChangedEventArgs
            {
                ChangeType = ContextChangeType.Document,
                NewContext = _currentContext
            });
        }

        public void UpdateViewContext(View? view)
        {
            _currentView = view;
            _logger.LogInformation("View context updated: {ViewName}", 
                view?.Name ?? "None");

            if (_currentContext != null && view != null)
            {
                _currentContext.CurrentView = new ViewContext
                {
                    ViewName = view.Name,
                    ViewType = view.ViewType.ToString(),
                    ViewTemplate = view.ViewTemplateId != ElementId.InvalidElementId ? 
                        _currentDocument?.GetElement(view.ViewTemplateId)?.Name : null,
                    Scale = view.Scale > 0 ? 1.0 / view.Scale : null,
                    DetailLevel = view.DetailLevel.ToString()
                };
            }

            OnContextChanged(new ContextChangedEventArgs
            {
                ChangeType = ContextChangeType.View,
                NewContext = _currentContext
            });
        }

        public void ClearDocumentContext()
        {
            _currentDocument = null;
            _currentView = null;
            _currentContext = null;
            _logger.LogInformation("Document context cleared");

            OnContextChanged(new ContextChangedEventArgs
            {
                ChangeType = ContextChangeType.Document,
                NewContext = null
            });
        }

        public void AddToHistory(string action)
        {
            var historyEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {action}";
            _sessionHistory.Add(historyEntry);
            _logger.LogDebug("Added to session history: {Action}", action);

            // Keep history size manageable
            if (_sessionHistory.Count > 1000)
            {
                _sessionHistory.RemoveRange(0, 100);
            }
        }

        private void UpdateEngineeringContext()
        {
            if (_currentDocument == null) return;

            try
            {
                var projectInfo = _currentDocument.ProjectInformation;
                
                _currentContext = new EngineeringContext
                {
                    ProjectName = projectInfo?.Name ?? "Unnamed Project",
                    ProjectNumber = projectInfo?.Number ?? string.Empty,
                    Discipline = GetProjectDiscipline(),
                    Phase = GetCurrentPhase(),
                    Standards = GetApplicableStandards(),
                    BuildingInfo = ExtractBuildingInfo(),
                    Systems = ExtractSystemInfo(),
                    ExtractedAt = DateTime.UtcNow
                };

                // Add metadata
                _currentContext.Metadata["RevitVersion"] = _currentDocument.Application.VersionName;
                _currentContext.Metadata["FilePath"] = _currentDocument.PathName;
                _currentContext.Metadata["IsWorkshared"] = _currentDocument.IsWorkshared;
                
                _logger.LogInformation("Engineering context updated for project: {ProjectName}", 
                    _currentContext.ProjectName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update engineering context");
                _currentContext = new EngineeringContext
                {
                    ProjectName = "Error extracting context",
                    Metadata = { ["Error"] = ex.Message }
                };
            }
        }

        private string GetProjectDiscipline()
        {
            // Determine discipline based on document categories and elements
            // This is a simplified implementation
            return "MEP"; // Could be enhanced to detect actual discipline
        }

        private string GetCurrentPhase()
        {
            // Get active phase or default
            var phases = new FilteredElementCollector(_currentDocument)
                .OfClass(typeof(Phase))
                .Cast<Phase>()
                .ToList();

            return phases.FirstOrDefault()?.Name ?? "Design Development";
        }

        private List<string> GetApplicableStandards()
        {
            // Extract from project parameters or default standards
            return new List<string>
            {
                "NEC 2020",
                "ASHRAE 90.1-2019",
                "IBC 2021"
            };
        }

        private BuildingInfo? ExtractBuildingInfo()
        {
            try
            {
                var projectInfo = _currentDocument?.ProjectInformation;
                if (projectInfo == null) return null;

                return new BuildingInfo
                {
                    Name = projectInfo.BuildingName ?? string.Empty,
                    Address = projectInfo.Address ?? string.Empty,
                    // Additional properties would be extracted from parameters
                };
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to extract building info");
                return null;
            }
        }

        private List<SystemInfo> ExtractSystemInfo()
        {
            var systems = new List<SystemInfo>();

            try
            {
                // Extract MEP systems
                var mepSystems = new FilteredElementCollector(_currentDocument)
                    .OfClass(typeof(MEPSystem))
                    .Cast<MEPSystem>()
                    .Take(50); // Limit for performance

                foreach (var system in mepSystems)
                {
                    systems.Add(new SystemInfo
                    {
                        SystemType = system.GetType().Name,
                        SystemName = system.Name,
                        Description = GetSystemDescription(system)
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to extract system info");
            }

            return systems;
        }

        private string GetSystemDescription(MEPSystem system)
        {
            // Build description based on system type
            return $"{system.GetType().Name}: {system.Name}";
        }

        protected virtual void OnContextChanged(ContextChangedEventArgs e)
        {
            ContextChanged?.Invoke(this, e);
        }
    }

    public class ContextChangedEventArgs : EventArgs
    {
        public ContextChangeType ChangeType { get; set; }
        public EngineeringContext? NewContext { get; set; }
    }

    public enum ContextChangeType
    {
        Document,
        View,
        Selection,
        Phase
    }
}
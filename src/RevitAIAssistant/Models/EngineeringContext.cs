using System;
using System.Collections.Generic;

namespace RevitAIAssistant.Models
{
    /// <summary>
    /// Represents the current engineering context extracted from Revit
    /// </summary>
    public class EngineeringContext
    {
        public string ProjectName { get; set; } = string.Empty;
        public string ProjectNumber { get; set; } = string.Empty;
        public string Discipline { get; set; } = string.Empty;
        public string Phase { get; set; } = string.Empty;
        public List<string> Standards { get; set; } = new();
        public Dictionary<string, object> Metadata { get; set; } = new();
        public BuildingInfo? BuildingInfo { get; set; }
        public List<SystemInfo> Systems { get; set; } = new();
        public ViewContext? CurrentView { get; set; }
        public DateTime ExtractedAt { get; set; } = DateTime.UtcNow;
    }

    public class BuildingInfo
    {
        public string Name { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public double? GrossArea { get; set; }
        public int? NumberOfFloors { get; set; }
        public string? BuildingType { get; set; }
        public string? OccupancyType { get; set; }
    }

    public class SystemInfo
    {
        public string SystemType { get; set; } = string.Empty;
        public string SystemName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public Dictionary<string, object> Properties { get; set; } = new();
        public List<string> ConnectedSystems { get; set; } = new();
    }

    public class ViewContext
    {
        public string ViewName { get; set; } = string.Empty;
        public string ViewType { get; set; } = string.Empty;
        public string? ViewTemplate { get; set; }
        public double? Scale { get; set; }
        public string? DetailLevel { get; set; }
        public BoundingBox? ViewBounds { get; set; }
    }

    public class BoundingBox
    {
        public Point3D Min { get; set; } = new();
        public Point3D Max { get; set; } = new();
    }

    public class Point3D
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }
    }
}
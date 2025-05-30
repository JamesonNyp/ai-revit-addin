using System;
using System.Windows;
using System.Windows.Media;
using Autodesk.Revit.UI;

namespace RevitAIAssistant.UI.Themes
{
    /// <summary>
    /// Manages theme integration with Revit's UI theme system
    /// Provides company colors and dynamic theme detection
    /// </summary>
    public class ThemeManager
    {
        // Company colors for fallback/accent
        public static readonly Color PrimaryBlue = Color.FromArgb(255, 0, 111, 151);      // #006F97
        public static readonly Color SecondaryBlue = Color.FromArgb(255, 30, 68, 136);     // #1E4488  
        public static readonly Color AccentBlue = Color.FromArgb(255, 122, 165, 186);      // #7AA5BA
        public static readonly Color LightGray = Color.FromArgb(255, 240, 240, 240);      // #F0F0F0
        public static readonly Color DarkGray = Color.FromArgb(255, 45, 45, 45);          // #2D2D2D

        private static ThemeColors? _currentTheme;
        private static readonly object _lock = new object();

        /// <summary>
        /// Gets the current theme colors based on Revit's UI theme
        /// </summary>
        public static ThemeColors CurrentTheme
        {
            get
            {
                lock (_lock)
                {
                    if (_currentTheme == null || ShouldUpdateTheme())
                    {
                        _currentTheme = GetCurrentTheme();
                    }
                    return _currentTheme;
                }
            }
        }

        /// <summary>
        /// Event raised when theme changes
        /// </summary>
        public static event EventHandler<ThemeChangedEventArgs>? ThemeChanged;

        /// <summary>
        /// Initialize theme monitoring
        /// </summary>
        public static void Initialize()
        {
            // Set initial theme
            _currentTheme = GetCurrentTheme();
            
            // Monitor for theme changes (check periodically as Revit doesn't expose theme change events)
            var timer = new System.Windows.Threading.DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            timer.Tick += (s, e) =>
            {
                if (ShouldUpdateTheme())
                {
                    var oldTheme = _currentTheme;
                    _currentTheme = GetCurrentTheme();
                    ThemeChanged?.Invoke(null, new ThemeChangedEventArgs(oldTheme!, _currentTheme));
                }
            };
            timer.Start();
        }

        /// <summary>
        /// Get current theme colors based on Revit theme
        /// </summary>
        private static ThemeColors GetCurrentTheme()
        {
            try
            {
                // Try to detect Revit's current theme
                var currentTheme = UIThemeManager.CurrentCanvasTheme;
                return currentTheme == UITheme.Dark ? GetDarkTheme() : GetLightTheme();
            }
            catch
            {
                // Fallback to light theme if detection fails
                return GetLightTheme();
            }
        }

        /// <summary>
        /// Check if theme should be updated
        /// </summary>
        private static bool ShouldUpdateTheme()
        {
            try
            {
                var currentRevitTheme = UIThemeManager.CurrentCanvasTheme;
                var isDark = currentRevitTheme == UITheme.Dark;
                return _currentTheme?.IsDark != isDark;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Get light theme colors
        /// </summary>
        private static ThemeColors GetLightTheme()
        {
            return new ThemeColors
            {
                IsDark = false,
                
                // Backgrounds
                WindowBackground = Colors.White,
                PanelBackground = Color.FromArgb(255, 250, 250, 250),
                InputBackground = Colors.White,
                ChatBackground = Color.FromArgb(255, 245, 245, 245),
                CardBackground = Color.FromArgb(255, 248, 248, 248),
                
                // Text colors
                PrimaryText = Color.FromArgb(255, 32, 32, 32),
                SecondaryText = Color.FromArgb(255, 96, 96, 96),
                PlaceholderText = Color.FromArgb(255, 160, 160, 160),
                
                // Chat bubbles - Claude.ai style with company blues
                UserBubbleBackground = PrimaryBlue,
                UserBubbleText = Colors.White,
                AiBubbleBackground = Color.FromArgb(255, 240, 240, 240),
                AiBubbleText = Color.FromArgb(255, 32, 32, 32),
                
                // Accent colors
                AccentColor = PrimaryBlue,
                AccentHoverColor = SecondaryBlue,
                SuccessColor = Color.FromArgb(255, 34, 139, 34),
                WarningColor = Color.FromArgb(255, 255, 140, 0),
                ErrorColor = Color.FromArgb(255, 220, 20, 60),
                
                // Borders and dividers
                BorderColor = Color.FromArgb(255, 230, 230, 230),
                DividerColor = Color.FromArgb(255, 240, 240, 240),
                
                // Buttons
                PrimaryButtonBackground = PrimaryBlue,
                PrimaryButtonText = Colors.White,
                SecondaryButtonBackground = Color.FromArgb(255, 240, 240, 240),
                SecondaryButtonText = Color.FromArgb(255, 64, 64, 64),
                
                // Progress indicators
                ProgressBackground = Color.FromArgb(255, 230, 230, 230),
                ProgressForeground = PrimaryBlue,
                
                // Code blocks
                CodeBackground = Color.FromArgb(255, 248, 248, 248),
                CodeText = Color.FromArgb(255, 51, 51, 51),
                
                // Engineering specific
                CalculationBackground = Color.FromArgb(255, 252, 252, 252),
                FormulaHighlight = AccentBlue,
                CriticalValueColor = Color.FromArgb(255, 200, 0, 0),
                ApprovalRequiredColor = Color.FromArgb(255, 255, 165, 0)
            };
        }

        /// <summary>
        /// Get dark theme colors
        /// </summary>
        private static ThemeColors GetDarkTheme()
        {
            return new ThemeColors
            {
                IsDark = true,
                
                // Backgrounds
                WindowBackground = Color.FromArgb(255, 30, 30, 30),
                PanelBackground = Color.FromArgb(255, 37, 37, 37),
                InputBackground = Color.FromArgb(255, 45, 45, 45),
                ChatBackground = Color.FromArgb(255, 25, 25, 25),
                CardBackground = Color.FromArgb(255, 42, 42, 42),
                
                // Text colors
                PrimaryText = Color.FromArgb(255, 240, 240, 240),
                SecondaryText = Color.FromArgb(255, 180, 180, 180),
                PlaceholderText = Color.FromArgb(255, 120, 120, 120),
                
                // Chat bubbles - Claude.ai style adapted for dark
                UserBubbleBackground = AccentBlue,
                UserBubbleText = Colors.White,
                AiBubbleBackground = Color.FromArgb(255, 45, 45, 45),
                AiBubbleText = Color.FromArgb(255, 240, 240, 240),
                
                // Accent colors
                AccentColor = AccentBlue,
                AccentHoverColor = PrimaryBlue,
                SuccessColor = Color.FromArgb(255, 46, 160, 67),
                WarningColor = Color.FromArgb(255, 255, 183, 77),
                ErrorColor = Color.FromArgb(255, 248, 81, 73),
                
                // Borders and dividers
                BorderColor = Color.FromArgb(255, 60, 60, 60),
                DividerColor = Color.FromArgb(255, 50, 50, 50),
                
                // Buttons
                PrimaryButtonBackground = AccentBlue,
                PrimaryButtonText = Colors.White,
                SecondaryButtonBackground = Color.FromArgb(255, 60, 60, 60),
                SecondaryButtonText = Color.FromArgb(255, 240, 240, 240),
                
                // Progress indicators
                ProgressBackground = Color.FromArgb(255, 60, 60, 60),
                ProgressForeground = AccentBlue,
                
                // Code blocks
                CodeBackground = Color.FromArgb(255, 40, 40, 40),
                CodeText = Color.FromArgb(255, 220, 220, 220),
                
                // Engineering specific
                CalculationBackground = Color.FromArgb(255, 35, 35, 35),
                FormulaHighlight = PrimaryBlue,
                CriticalValueColor = Color.FromArgb(255, 255, 100, 100),
                ApprovalRequiredColor = Color.FromArgb(255, 255, 200, 100)
            };
        }
    }

    /// <summary>
    /// Theme color definitions
    /// </summary>
    public class ThemeColors
    {
        public bool IsDark { get; set; }
        
        // Backgrounds
        public Color WindowBackground { get; set; }
        public Color PanelBackground { get; set; }
        public Color InputBackground { get; set; }
        public Color ChatBackground { get; set; }
        public Color CardBackground { get; set; }
        
        // Text colors
        public Color PrimaryText { get; set; }
        public Color SecondaryText { get; set; }
        public Color PlaceholderText { get; set; }
        
        // Chat bubbles
        public Color UserBubbleBackground { get; set; }
        public Color UserBubbleText { get; set; }
        public Color AiBubbleBackground { get; set; }
        public Color AiBubbleText { get; set; }
        
        // Accent colors
        public Color AccentColor { get; set; }
        public Color AccentHoverColor { get; set; }
        public Color SuccessColor { get; set; }
        public Color WarningColor { get; set; }
        public Color ErrorColor { get; set; }
        
        // Borders and dividers
        public Color BorderColor { get; set; }
        public Color DividerColor { get; set; }
        
        // Buttons
        public Color PrimaryButtonBackground { get; set; }
        public Color PrimaryButtonText { get; set; }
        public Color SecondaryButtonBackground { get; set; }
        public Color SecondaryButtonText { get; set; }
        
        // Progress indicators
        public Color ProgressBackground { get; set; }
        public Color ProgressForeground { get; set; }
        
        // Code blocks
        public Color CodeBackground { get; set; }
        public Color CodeText { get; set; }
        
        // Engineering specific
        public Color CalculationBackground { get; set; }
        public Color FormulaHighlight { get; set; }
        public Color CriticalValueColor { get; set; }
        public Color ApprovalRequiredColor { get; set; }
    }

    /// <summary>
    /// Theme changed event arguments
    /// </summary>
    public class ThemeChangedEventArgs : EventArgs
    {
        public ThemeColors OldTheme { get; }
        public ThemeColors NewTheme { get; }

        public ThemeChangedEventArgs(ThemeColors oldTheme, ThemeColors newTheme)
        {
            OldTheme = oldTheme;
            NewTheme = newTheme;
        }
    }
}
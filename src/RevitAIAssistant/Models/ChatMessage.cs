using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace RevitAIAssistant.Models
{
    public enum MessageRole
    {
        User,
        Assistant,
        System
    }

    public enum MessageType
    {
        Info,
        Success,
        Warning,
        Error
    }

    public class ChatMessage : INotifyPropertyChanged
    {
        private object? _richContent;
        private string _content = string.Empty;
        private bool _isTyping;

        public string Id { get; set; } = Guid.NewGuid().ToString();
        public MessageRole Role { get; set; }
        public string Content 
        { 
            get => _content;
            set
            {
                _content = value;
                OnPropertyChanged();
            }
        }
        public object? RichContent 
        { 
            get => _richContent;
            set
            {
                _richContent = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HasRichContent));
                OnPropertyChanged(nameof(RichContentText));
            }
        }
        
        public bool HasRichContent => RichContent != null;
        
        public string RichContentText => RichContent?.ToString() ?? string.Empty;
        
        public DateTime Timestamp { get; set; }
        public bool IsTyping 
        { 
            get => _isTyping;
            set
            {
                _isTyping = value;
                OnPropertyChanged();
            }
        }
        public MessageType MessageType { get; set; } = MessageType.Info;

        public event PropertyChangedEventHandler? PropertyChanged;

        public void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class ActiveTaskInfo : INotifyPropertyChanged
    {
        private string _taskId = "";
        private string _executionId = "";
        private string _title = "";
        private int _progress;
        private string _currentStep = "";
        private string _timeRemaining = "";

        public string TaskId 
        { 
            get => _taskId;
            set
            {
                _taskId = value;
                OnPropertyChanged();
            }
        }
        
        public string ExecutionId 
        { 
            get => _executionId;
            set
            {
                _executionId = value;
                OnPropertyChanged();
            }
        }
        
        public string Title 
        { 
            get => _title;
            set
            {
                _title = value;
                OnPropertyChanged();
            }
        }
        
        public int Progress 
        { 
            get => _progress;
            set
            {
                _progress = value;
                OnPropertyChanged();
            }
        }
        
        public string CurrentStep 
        { 
            get => _currentStep;
            set
            {
                _currentStep = value;
                OnPropertyChanged();
            }
        }
        
        public string TimeRemaining 
        { 
            get => _timeRemaining;
            set
            {
                _timeRemaining = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
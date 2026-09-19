using System;

namespace Features.MvpModule {
    public abstract class WindowsEventData {
        public Type WindowType;
    }
    
    public sealed class OnWindowOpenedEventData : WindowsEventData { }
    public sealed class OnWindowClosedEventData : WindowsEventData { }
    public sealed class OnWindowShowedEventData : WindowsEventData { }
    public sealed class OnWindowHiddenEventData : WindowsEventData { }
}
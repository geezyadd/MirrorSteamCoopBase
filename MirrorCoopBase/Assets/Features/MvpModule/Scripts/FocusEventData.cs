using System;

namespace Features.MvpModule {
    public abstract class FocusEventData {
        public Type FocusableContainerType;
    }
    
    public sealed class OnContainerBecomeFocusableEventData : FocusEventData { }
    public sealed class OnContainerBecomeUnFocusableEventData : FocusEventData { }
}
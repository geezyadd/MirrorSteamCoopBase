using System;

namespace Features.MvpModule {
    public interface IFocusableElement {
        public bool IsFocused { get; }
        public bool IsFocusable { get; }
        
        public event Action OnFocused;
        public event Action OnUnFocused;
        
        public void Focus(Action onInteract);
        public void UnFocus();

        public void MakeFocusable();
        public void MakeUnFocusable();
    }
}
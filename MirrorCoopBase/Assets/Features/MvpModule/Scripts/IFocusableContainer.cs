using System;
using System.Collections.Generic;

namespace Features.MvpModule {
    public interface IFocusableContainer {
        public bool IsContainerFocusable { get; }
        public IEnumerable<IFocusableElement> FocusableElements { get; }
        
        public event Action OnContainerBecomeFocusable;
        public event Action OnContainerBecomeUnFocusable;

        public void MakeFocusable();
        public void MakeUnFocusable();

        public void SetFocusableElements(IEnumerable<IFocusableElement> focusableElements);
        public void RegisterFocusableElement(IFocusableElement focusableElement);
        public void RegisterFocusableElementRange(IEnumerable<IFocusableElement> focusableElements);
        public void UnRegisterFocusableElement(IFocusableElement focusableElement);
        public void UnRegisterFocusableElementRange(IEnumerable<IFocusableElement> focusableElements);
    }
}
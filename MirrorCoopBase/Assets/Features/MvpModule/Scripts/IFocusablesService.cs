using System;

namespace Features.MvpModule {
    public interface IFocusablesService {
        public event Action<Type> OnContainerBecomeFocusable;
        public event Action<Type> OnContainerBecomeUnFocusable;
        
        public void MakeContainerFocusable<TFocusableContainerType>() where TFocusableContainerType : IFocusableContainer;
        public void MakeContainerUnFocusable<TFocusableContainerType>() where TFocusableContainerType : IFocusableContainer;
        public void RegisterFocusableContainer<TFocusableContainerType>(TFocusableContainerType container) where TFocusableContainerType : IFocusableContainer;
        public void UnRegisterFocusableContainer<TFocusableContainerType>() where TFocusableContainerType : IFocusableContainer;

        public void MakeContainerFocusable(Type focusableContainerType);
        public void MakeContainerUnFocusable(Type focusableContainerType);
        public void RegisterFocusableContainer(Type focusableContainerType, IFocusableContainer container);
        public void UnRegisterFocusableContainer(Type focusableContainerType);
    }
}
using System;
using System.Collections.Generic;

namespace Features.MvpModule {
    public class FocusablesService : IFocusablesService {
        private readonly IGenericEventBus<FocusEventData> _focusEventBus;
        private readonly Dictionary<Type, IFocusableContainer> _focusableContainerByTypeDictionary = new();

        public event Action<Type> OnContainerBecomeFocusable;
        public event Action<Type> OnContainerBecomeUnFocusable;

        public FocusablesService(IGenericEventBus<FocusEventData> focusEventBus) =>
            _focusEventBus = focusEventBus ?? throw new ArgumentNullException(nameof(focusEventBus));

        public void MakeContainerFocusable<TFocusableContainerType>() where TFocusableContainerType : IFocusableContainer =>
            MakeContainerFocusable(typeof(TFocusableContainerType));

        public void MakeContainerUnFocusable<TFocusableContainerType>() where TFocusableContainerType : IFocusableContainer =>
            MakeContainerUnFocusable(typeof(TFocusableContainerType));

        public void RegisterFocusableContainer<TFocusableContainerType>(TFocusableContainerType container) where TFocusableContainerType : IFocusableContainer =>
            RegisterFocusableContainer(typeof(TFocusableContainerType), container);

        public void UnRegisterFocusableContainer<TFocusableContainerType>() where TFocusableContainerType : IFocusableContainer =>
            UnRegisterFocusableContainer(typeof(TFocusableContainerType));
        public void MakeContainerFocusable(Type focusableContainerType) {
            if (_focusableContainerByTypeDictionary.TryGetValue(focusableContainerType, out var container) is false)
                throw new FocusableContainerTypeNotFoundException(focusableContainerType, nameof(MakeContainerFocusable));
            
            container.MakeFocusable();
            _focusEventBus.Publish(new OnContainerBecomeFocusableEventData {
                FocusableContainerType = focusableContainerType
            });
            
            OnContainerBecomeFocusable?.Invoke(focusableContainerType);
        }

        public void MakeContainerUnFocusable(Type focusableContainerType) {
            if (_focusableContainerByTypeDictionary.TryGetValue(focusableContainerType, out var container) is false)
                throw new FocusableContainerTypeNotFoundException(focusableContainerType, nameof(MakeContainerFocusable));
            
            container.MakeUnFocusable();
            _focusEventBus.Publish(new OnContainerBecomeUnFocusableEventData {
                FocusableContainerType = focusableContainerType
            });
            OnContainerBecomeUnFocusable?.Invoke(focusableContainerType);
        }

        public void RegisterFocusableContainer(Type focusableContainerType, IFocusableContainer container) =>
            _focusableContainerByTypeDictionary.Add(focusableContainerType, container);

        public void UnRegisterFocusableContainer(Type focusableContainerType) =>
            _focusableContainerByTypeDictionary.Remove(focusableContainerType);
    }
}
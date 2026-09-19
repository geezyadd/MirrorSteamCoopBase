using System;
using System.Collections.Generic;
using System.Linq;

namespace Features.MvpModule {
        public abstract class FocusableWindowBehaviour : WindowBehaviour, IFocusableContainer {
        protected List<IFocusableElement> _focusableElements = new();
        private IFocusablesService _focusablesService;

        public bool IsContainerFocusable { get; protected set; }
        public IEnumerable<IFocusableElement> FocusableElements =>
            _focusableElements;

        public event Action OnContainerBecomeFocusable;
        public event Action OnContainerBecomeUnFocusable;

        protected FocusableWindowBehaviour(IWindowsFactory windowsFactory, IWindowsComponentsFinderService windowsComponentsFinderService, 
            IWindowsService windowsService, IFocusablesService focusablesService) : base(windowsFactory, windowsComponentsFinderService, windowsService) =>
            _focusablesService = focusablesService ?? throw new ArgumentNullException(nameof(focusablesService));

        public override void Initialize() {
            _windowsService.RegisterWindow(GetType(), this);
            _focusablesService.RegisterFocusableContainer(GetType(), this);
        }

        public override void Dispose() {
            if(WindowStatus != WindowStatus.Closed)
                Close();
            
            _windowsService.UnRegisterWindow(GetType());
            _focusablesService.UnRegisterFocusableContainer(GetType());
        }

        public void MakeFocusable() {
            if (IsContainerFocusable)
                throw new FocusableContainerOperationFailureException(GetType(), nameof(MakeFocusable),
                    FocusableContainerOperationFailureCause.OperationIsAlreadyPerformed);

            if (WindowStatus is WindowStatus.Closed or WindowStatus.Hidden)
                throw new FocusableContainerOperationFailureException(GetType(), nameof(MakeFocusable),
                    FocusableContainerOperationFailureCause.ContainerIsInactive);

            MakeAllElementsFocusable();
            IsContainerFocusable = true;
            OnContainerBecomeFocusable?.Invoke();
            OnBecomeFocusable();
        }

        public void MakeUnFocusable() {
            if (IsContainerFocusable is false)
                throw new FocusableContainerOperationFailureException(GetType(), nameof(MakeUnFocusable),
                    FocusableContainerOperationFailureCause.OperationIsAlreadyPerformed);

            MakeAllElementsUnFocusable();
            IsContainerFocusable = false;
            OnContainerBecomeUnFocusable?.Invoke();
            OnBecomeUnFocusable();
        }

        public void SetFocusableElements(IEnumerable<IFocusableElement> focusableElements) {
            _focusableElements = focusableElements.ToList();

            if (IsContainerFocusable)
                MakeAllElementsFocusable();
            else
                MakeAllElementsUnFocusable();
        }

        public void RegisterFocusableElement(IFocusableElement focusableElement) {
            _focusableElements.Add(focusableElement);

            if (IsContainerFocusable)
                focusableElement.MakeFocusable();
            else
                focusableElement.MakeUnFocusable();
        }

        public void RegisterFocusableElementRange(IEnumerable<IFocusableElement> focusableElements) {
            IEnumerable<IFocusableElement> collection = focusableElements.ToList();
            _focusableElements.AddRange(collection);

            if (IsContainerFocusable)
                foreach (IFocusableElement focusableElement in collection)
                    focusableElement.MakeFocusable();
            else
                foreach (IFocusableElement focusableElement in collection)
                    focusableElement.MakeUnFocusable();
        }

        public void UnRegisterFocusableElement(IFocusableElement focusableElement) {
            _focusableElements.Remove(focusableElement);
            focusableElement.MakeUnFocusable();
        }

        public void UnRegisterFocusableElementRange(IEnumerable<IFocusableElement> focusableElements) {
            _focusableElements = _focusableElements.Except(focusableElements)
                .ToList();

            foreach (IFocusableElement focusableElement in focusableElements)
                focusableElement.MakeUnFocusable();
        }

        public override void UpdateWindow() {
            SetFocusableElements(_windowsComponentsFinderService.GetAllFocusablesInWindow(_windowInstance, true));
            _windowsComponentsFinderService.CollectPresentersInWindow(_windowInstance);
        }

        protected override void DisposeWindow() {
            foreach (IFocusableElement focusableElement in _focusableElements)
                focusableElement.MakeUnFocusable();
            
            _focusableElements.Clear();
            foreach (ViewBehaviour viewBehaviour in _windowInstance.GetAllViews()) {
                viewBehaviour.DisableView();
                viewBehaviour.DisposeView();
            }

            _windowsComponentsFinderService.ClearPresentersInWindow(_windowInstance);
            _windowInstance?.Destroy();
            _windowInstance = null;
            IsContainerFocusable = false;
        }

        private void MakeAllElementsFocusable() =>
            _focusableElements.ForEach(element => element.MakeFocusable());

        private void MakeAllElementsUnFocusable() =>
            _focusableElements.ForEach(element => element.MakeUnFocusable());
    }
}
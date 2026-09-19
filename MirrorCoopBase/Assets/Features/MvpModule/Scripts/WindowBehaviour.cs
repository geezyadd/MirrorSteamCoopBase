using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Features.AssetLoaderModule.Scripts;
using UnityEngine;

namespace Features.MvpModule {
        public abstract class WindowBehaviour : IWindow {
        protected IWindowInstance _windowInstance;
        protected readonly IWindowsFactory _windowsFactory;
        protected readonly IWindowsComponentsFinderService _windowsComponentsFinderService;
        protected readonly IWindowsService _windowsService;

        public WindowStatus WindowStatus { get; protected set; }

        public event Action<Type> OnWindowOpened;
        public event Action<Type> OnWindowClosed;
        public event Action<Type> OnWindowShowed;
        public event Action<Type> OnWindowHidden;

        protected WindowBehaviour(IWindowsFactory windowsFactory, IWindowsComponentsFinderService windowsComponentsFinderService, 
                                  IWindowsService windowsService) {
            _windowsService = windowsService ?? throw new ArgumentNullException(nameof(windowsService));
            _windowsFactory = windowsFactory ?? throw new ArgumentNullException(nameof(windowsFactory));
            _windowsComponentsFinderService = windowsComponentsFinderService ?? throw new ArgumentNullException(nameof(windowsComponentsFinderService));
        }

        public virtual void Initialize() =>
            _windowsService.RegisterWindow(GetType(), this);

        public virtual void Dispose() {
            if(WindowStatus != WindowStatus.Closed)
                Close();
            
            _windowsService.UnRegisterWindow(GetType());
        }

        public async Task OpenAsync() {
            if (_windowInstance != null)
                throw new WindowOperationFailureException(GetType(), nameof(OpenAsync), WindowOperationFailureCause.OperationIsAlreadyPerformed);

            await InitializeWindowAsync();

            WindowStatus = WindowStatus.Showed;
            OnWindowOpened?.Invoke(GetType());
            OnOpened();
        }

        public void Open() {
            if (_windowInstance != null)
                throw new WindowOperationFailureException(GetType(), nameof(Open), WindowOperationFailureCause.OperationIsAlreadyPerformed);

            InitializeWindow();

            WindowStatus = WindowStatus.Showed;
            OnWindowOpened?.Invoke(GetType());
            OnOpened();
        }
        
        public void OpenPreloaded() {
            if (_windowInstance != null)
                throw new WindowOperationFailureException(GetType(), nameof(Open), WindowOperationFailureCause.OperationIsAlreadyPerformed);

            InitializePreloadWindow();

            WindowStatus = WindowStatus.Showed;
            OnWindowOpened?.Invoke(GetType());
            OnOpened();
        }

        public void Close() {
            if (WindowStatus == WindowStatus.Closed)
                throw new WindowOperationFailureException(GetType(), nameof(Close), WindowOperationFailureCause.OperationIsAlreadyPerformed);

            DisposeWindow();

            WindowStatus = WindowStatus.Closed;
            OnWindowClosed?.Invoke(GetType());
            OnClosed();
        }

        public void Show() {
            if (WindowStatus == WindowStatus.Showed)
                throw new WindowOperationFailureException(GetType(), nameof(Show), WindowOperationFailureCause.OperationIsAlreadyPerformed);

            if (WindowStatus == WindowStatus.Closed)
                throw new WindowOperationFailureException(GetType(), nameof(Show), WindowOperationFailureCause.WindowIsNotOpened);

            _windowInstance.Show();
            WindowStatus = WindowStatus.Showed;
            OnWindowShowed?.Invoke(GetType());
            OnShowed();
        }

        public void Hide() {
            if (WindowStatus == WindowStatus.Hidden)
                throw new WindowOperationFailureException(GetType(), nameof(Hide), WindowOperationFailureCause.OperationIsAlreadyPerformed);

            if (WindowStatus == WindowStatus.Closed)
                throw new WindowOperationFailureException(GetType(), nameof(Hide), WindowOperationFailureCause.WindowIsNotOpened);

            _windowInstance.Hide();
            WindowStatus = WindowStatus.Hidden;
            OnWindowHidden?.Invoke(GetType());
            OnHidden();
        }

        public void AddView(Transform viewRoot, bool worldPositionStays = true) {
            _windowInstance.AddChild(viewRoot, worldPositionStays);
            UpdateWindow();
        }

        public void RemoveView(Transform viewRoot) {
            _windowInstance.RemoveChild(viewRoot);
            UpdateWindow();
        }

        public TPresenterType GetPresenter<TPresenterType>() where TPresenterType : PresenterBehaviour {
            if (_windowInstance == null)
                throw new WindowIsNotOpenedException(GetType(), nameof(GetPresenter));

            return _windowsComponentsFinderService.GetPresenterInWindow<TPresenterType>(_windowInstance, true);
        }

        public IEnumerable<TPresenterType> GetPresenters<TPresenterType>() where TPresenterType : PresenterBehaviour {
            if (_windowInstance == null)
                throw new WindowIsNotOpenedException(GetType(), nameof(GetPresenters));

            return _windowsComponentsFinderService.GetPresentersInWindow<TPresenterType>(_windowInstance, true);
        }

        public bool TryGetPresenter<TPresenterType>(out TPresenterType presenter) where TPresenterType : PresenterBehaviour {
            presenter = default;
            return _windowInstance != null && _windowsComponentsFinderService.TryGetPresenterInWindow(_windowInstance, out presenter, true);
        }

        public bool TryGetPresenters<TPresenterType>(out IEnumerable<TPresenterType> presenters) where TPresenterType : PresenterBehaviour {
            presenters = default;
            return _windowInstance != null && _windowsComponentsFinderService.TryGetPresentersInWindow(_windowInstance, out presenters, true);
        }

        public TPresenterType GetPresenterForView<TPresenterType>(ViewBehaviour view) where TPresenterType : PresenterBehaviour {
            if (_windowInstance == null)
                throw new WindowIsNotOpenedException(GetType(), nameof(GetPresenter));

            return _windowsComponentsFinderService.GetPresenterInWindowForView<TPresenterType>(_windowInstance, view, true);
        }

        public bool TryGetPresenterForView<TPresenterType>(ViewBehaviour view, out TPresenterType presenter) where TPresenterType : PresenterBehaviour {
            presenter = default;
            return _windowInstance != null && _windowsComponentsFinderService.TryGetPresenterInWindowForView<TPresenterType>(_windowInstance, view, out presenter, true);
        }

        public PresenterBehaviour GetPresenter(Type ofType) {
            if (_windowInstance == null)
                throw new WindowIsNotOpenedException(GetType(), nameof(GetPresenter));

            return _windowsComponentsFinderService.GetPresenterInWindow(ofType, _windowInstance, true);
        }

        public IEnumerable<PresenterBehaviour> GetPresenters(Type presentersType) {
            if (_windowInstance == null)
                throw new WindowIsNotOpenedException(GetType(), nameof(GetPresenters));

            return _windowsComponentsFinderService.GetPresentersInWindow(presentersType, _windowInstance, true);
        }

        public bool TryGetPresenter(Type ofType, out PresenterBehaviour presenter) {
            presenter = null;
            return _windowInstance != null && _windowsComponentsFinderService.TryGetPresenterInWindow(ofType, _windowInstance, out presenter, true);
        }

        public bool TryGetPresenters(Type presentersType, out IEnumerable<PresenterBehaviour> presenterBases) {
            presenterBases = null;
            return _windowInstance != null && _windowsComponentsFinderService.TryGetPresentersInWindow(presentersType, _windowInstance, out presenterBases, true);
        }

        public PresenterBehaviour GetPresenterForView(ViewBehaviour view) {
            if (_windowInstance == null)
                throw new WindowIsNotOpenedException(GetType(), nameof(GetPresenter));

            return _windowsComponentsFinderService.GetPresenterInWindowForView(_windowInstance, view, true);
        }

        public bool TryGetPresenterForView(ViewBehaviour view, out PresenterBehaviour presenter) {
            presenter = default;
            return _windowInstance != null && _windowsComponentsFinderService.TryGetPresenterInWindowForView(_windowInstance,view, out presenter, true);
        }

        public virtual void OnBack() { }
        
        protected virtual void OnOpened() { }

        protected virtual void OnClosed() { }

        protected virtual void OnShowed() { }

        protected virtual void OnHidden() { }

        protected virtual void OnBecomeFocusable() { }

        protected virtual void OnBecomeUnFocusable() { }

        private async Task InitializeWindowAsync() {
            _windowInstance = await _windowsFactory.GetWindowInstanceForWindowTypeAsync(GetType(), AssetLoadSource.Addressables);
            UpdateWindow();
        }

        private void InitializeWindow() {
            _windowInstance = _windowsFactory.GetWindowInstanceForWindowType(GetType(), AssetLoadSource.Addressables);
            UpdateWindow();
        }
        
        private void InitializePreloadWindow() {
            _windowInstance = _windowsFactory.GetPreloadWindowInstanceForWindowType(GetType());
            UpdateWindow();
        }

        public virtual void UpdateWindow() =>
            _windowsComponentsFinderService.CollectPresentersInWindow(_windowInstance);

        protected virtual void DisposeWindow() {
            foreach (ViewBehaviour viewBehaviour in _windowInstance.GetAllViews()) {
                viewBehaviour.DisableView();
                viewBehaviour.DisposeView();
            }

            _windowsComponentsFinderService.ClearPresentersInWindow(_windowInstance);
            _windowInstance?.Destroy();
            _windowInstance = null;
        }
    }
}
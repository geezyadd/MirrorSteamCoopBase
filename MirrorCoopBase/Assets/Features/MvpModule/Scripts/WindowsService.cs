using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Features.MvpModule {
    public sealed class WindowsService : IWindowsService, IDisposable {
        private readonly IGenericEventBus<WindowsEventData> _windowsEventBus;
        private readonly Dictionary<Type, IWindow> _windowByTypeDictionary = new();

        public event Action<Type> OnWindowOpened;
        public event Action<Type> OnWindowClosed;
        public event Action<Type> OnWindowShowed;
        public event Action<Type> OnWindowHidden;

        public WindowsService(IGenericEventBus<WindowsEventData> windowsEventBus) =>
            _windowsEventBus = windowsEventBus ?? throw new ArgumentNullException(nameof(windowsEventBus));

        public void Dispose() {
            foreach (KeyValuePair<Type,IWindow> keyValuePair in _windowByTypeDictionary) {
                keyValuePair.Value.OnWindowOpened -= OnWindowOpenedHandler;
                keyValuePair.Value.OnWindowClosed -= OnWindowClosedHandler;
                keyValuePair.Value.OnWindowShowed -= OnWindowShowedHandler;
                keyValuePair.Value.OnWindowHidden -= OnWindowHiddenHandler;
            }
            
            _windowByTypeDictionary.Clear();
        }

        public void OpenWindow<TWindowType>() where TWindowType : IWindow =>
            OpenWindow(typeof(TWindowType));

        public void OpenPreloadWindow<TWindowType>() where TWindowType : IWindow =>
            OpenPreloadWindow(typeof(TWindowType));

        public async Task OpenWindowAsync<TWindowType>() where TWindowType : IWindow =>
            await OpenWindowAsync(typeof(TWindowType));

        public void CloseWindow<TWindowType>() where TWindowType : IWindow =>
            CloseWindow(typeof(TWindowType));

        public void ShowWindow<TWindowType>() where TWindowType : IWindow =>
            ShowWindow(typeof(TWindowType));

        public void HideWindow<TWindowType>() where TWindowType : IWindow =>
            HideWindow(typeof(TWindowType));

        public void RegisterWindow<TWindowType>(TWindowType window) where TWindowType : IWindow =>
            RegisterWindow(typeof(TWindowType), window);

        public void UnRegisterWindow<TWindowType>() where TWindowType : IWindow =>
            UnRegisterWindow(typeof(TWindowType));

        public void OpenWindow(Type windowType) {
            if (_windowByTypeDictionary.TryGetValue(windowType, out IWindow window) is false)
                throw new WindowTypeNotFoundException(windowType, nameof(OpenWindow));

            window.Open();
        }
        
        public void OpenPreloadWindow(Type windowType) {
            if (_windowByTypeDictionary.TryGetValue(windowType, out IWindow window) is false)
                throw new WindowTypeNotFoundException(windowType, nameof(OpenPreloadWindow));

            window.OpenPreloaded();
        }

        public async Task OpenWindowAsync(Type windowType) {
            if (_windowByTypeDictionary.TryGetValue(windowType, out IWindow window) is false)
                throw new WindowTypeNotFoundException(windowType, nameof(OpenWindow));

            await window.OpenAsync();
        }

        public void CloseWindow(Type windowType) {
            if (_windowByTypeDictionary.TryGetValue(windowType, out IWindow window) is false)
                throw new WindowTypeNotFoundException(windowType, nameof(CloseWindow));

            window.Close();
        }

        public void ShowWindow(Type windowType) {
            if (_windowByTypeDictionary.TryGetValue(windowType, out IWindow window) is false)
                throw new WindowTypeNotFoundException(windowType, nameof(ShowWindow));

            window.Show();
        }

        public void HideWindow(Type windowType) {
            if (_windowByTypeDictionary.TryGetValue(windowType, out IWindow window) is false)
                throw new WindowTypeNotFoundException(windowType, nameof(HideWindow));

            window.Hide();
        }

        public void RegisterWindow(Type windowType, IWindow window) {
            _windowByTypeDictionary.Add(windowType, window);
            window.OnWindowOpened += OnWindowOpenedHandler;
            window.OnWindowClosed += OnWindowClosedHandler;
            window.OnWindowShowed += OnWindowShowedHandler;
            window.OnWindowHidden += OnWindowHiddenHandler;
        }

        public void UnRegisterWindow(Type windowType) {
            if(_windowByTypeDictionary.Any() is false)
                return;
            
            _windowByTypeDictionary[windowType].OnWindowOpened -= OnWindowOpenedHandler;
            _windowByTypeDictionary[windowType].OnWindowClosed -= OnWindowClosedHandler;
            _windowByTypeDictionary[windowType].OnWindowShowed -= OnWindowShowedHandler;
            _windowByTypeDictionary[windowType].OnWindowHidden -= OnWindowHiddenHandler;
            _windowByTypeDictionary.Remove(windowType);
        }

        public IWindow GetWindow(Type type) =>
            _windowByTypeDictionary.Any() is false
                ? default
                : _windowByTypeDictionary.FirstOrDefault(pair => pair.Key == type).Value;

        public IEnumerable<IWindow> GetAllWindowsWithStatus(WindowStatus status) {
            IEnumerable<KeyValuePair<Type, IWindow>> windowWithStatus = _windowByTypeDictionary.Where(pair => pair.Value.WindowStatus == status);
            return windowWithStatus.Any() is false
                ? new List<IWindow>()
                : windowWithStatus.Select(pair => pair.Value);
        }
        
        public IEnumerable<IWindow> GetAllWindows() =>
            _windowByTypeDictionary.Any() is false
                ? new List<IWindow>()
                : _windowByTypeDictionary.Select(pair => pair.Value);

        private void OnWindowOpenedHandler(Type windowType) {
            _windowsEventBus.Publish(new OnWindowOpenedEventData {
                WindowType = windowType
            });
            OnWindowOpened?.Invoke(windowType);
        }

        private void OnWindowClosedHandler(Type windowType) {
            _windowsEventBus.Publish(new OnWindowClosedEventData {
                WindowType = windowType
            });
            OnWindowClosed?.Invoke(windowType);
        }

        private void OnWindowShowedHandler(Type windowType) {
            _windowsEventBus.Publish(new OnWindowShowedEventData {
                WindowType = windowType
            });
            OnWindowShowed?.Invoke(windowType);
        }

        private void OnWindowHiddenHandler(Type windowType) {
            _windowsEventBus.Publish(new OnWindowHiddenEventData {
                WindowType =  windowType
            });
            OnWindowHidden?.Invoke(windowType);
        }
    }
}
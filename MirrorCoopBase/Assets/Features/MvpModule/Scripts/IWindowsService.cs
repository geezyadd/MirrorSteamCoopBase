using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Features.MvpModule {
    public interface IWindowsService {
        public event Action<Type> OnWindowOpened;
        public event Action<Type> OnWindowClosed;
        public event Action<Type> OnWindowShowed;
        public event Action<Type> OnWindowHidden;
        
        public void OpenWindow<TWindowType>() where TWindowType : IWindow;
        public void OpenPreloadWindow<TWindowType>() where TWindowType : IWindow;
        public Task OpenWindowAsync<TWindowType>() where TWindowType : IWindow;
        public void CloseWindow<TWindowType>() where TWindowType : IWindow;
        public void ShowWindow<TWindowType>() where TWindowType : IWindow;
        public void HideWindow<TWindowType>() where TWindowType : IWindow;
        public void RegisterWindow<TWindowType>(TWindowType window) where TWindowType : IWindow;
        public void UnRegisterWindow<TWindowType>() where TWindowType : IWindow;

        public void OpenWindow(Type windowType);
        public void OpenPreloadWindow(Type windowType);
        public Task OpenWindowAsync(Type windowType);
        public void CloseWindow(Type windowType);
        public void ShowWindow(Type windowType);
        public void HideWindow(Type windowType);
        public void RegisterWindow(Type windowType, IWindow window);
        public void UnRegisterWindow(Type windowType);

        public IEnumerable<IWindow> GetAllWindows();
        public IWindow GetWindow(Type type);
        public IEnumerable<IWindow> GetAllWindowsWithStatus(WindowStatus status);
    }
}
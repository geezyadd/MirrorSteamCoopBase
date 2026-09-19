using System;
using System.Collections.Generic;
using Zenject;

namespace Features.MvpModule {
    public class WindowFocusSystem : IInitializable, IDisposable {
        private readonly IFocusablesService _focusablesService;
        private readonly IWindowsService _windowsService;

        private Type _currentFocusedWindow;

        private Stack<Type> _focusWindowQueue = new();
        
        private HashSet<Type> _hiddenWindows = new();

        public WindowFocusSystem(IFocusablesService focusablesService, IWindowsService windowsService) {
            _focusablesService = focusablesService;
            _windowsService = windowsService;
        }
        
        public void Initialize() {
            _windowsService.OnWindowOpened += OnWindowOpened;
            _windowsService.OnWindowClosed += OnWindowClosed;
            _windowsService.OnWindowShowed += OnWindowShowed;
            _windowsService.OnWindowHidden += OnWindowHidden;
        }

        public void Dispose() {
            _windowsService.OnWindowOpened -= OnWindowOpened;
            _windowsService.OnWindowClosed -= OnWindowClosed;
            _windowsService.OnWindowShowed -= OnWindowShowed;
            _windowsService.OnWindowHidden -= OnWindowHidden;
            
            _focusWindowQueue.Clear();
        }

        private void OnWindowOpened(Type openedWindowType) {
            if (!IsWindowFocusable(openedWindowType))
                return;
            
            if (_currentFocusedWindow != null) {
                _focusWindowQueue.Push(_currentFocusedWindow);
                _focusablesService.MakeContainerUnFocusable(_currentFocusedWindow);
            }

            _currentFocusedWindow = openedWindowType;
            _focusablesService.MakeContainerFocusable(_currentFocusedWindow);
        }

        private void OnWindowClosed(Type closedWindowType) {
            if (!IsWindowFocusable(closedWindowType))
                return;
            
            if (_hiddenWindows.Contains(closedWindowType)) {
                _hiddenWindows.Remove(closedWindowType);
                return;
            }
            
            if (_focusWindowQueue.Contains(closedWindowType) is false && _currentFocusedWindow != closedWindowType)
                throw new WindowIsNotAddedToFocusableCollectionException(closedWindowType);

            if (_focusWindowQueue.Contains(closedWindowType))
                RemoveWindowTypeFromStack(closedWindowType);
            else if(_currentFocusedWindow == closedWindowType) {
                if (_focusWindowQueue.TryPop(out Type _poppedWindow)) {
                    _currentFocusedWindow = _poppedWindow;
                    _focusablesService.MakeContainerFocusable(_currentFocusedWindow);
                }
                else
                    _currentFocusedWindow = null;
            }
            else
                throw new WindowIsNotAddedToFocusableCollectionException(closedWindowType);
        }

        private void OnWindowShowed(Type showedWindowType) {
            if (!IsWindowFocusable(showedWindowType))
                return;

            if (_currentFocusedWindow != null) {
                _focusWindowQueue.Push(_currentFocusedWindow);
                _focusablesService.MakeContainerUnFocusable(_currentFocusedWindow);
            }

            _hiddenWindows.Remove(showedWindowType);

            _currentFocusedWindow = showedWindowType;
            _focusablesService.MakeContainerFocusable(_currentFocusedWindow);
        }

        private void OnWindowHidden(Type hiddenWindowType) {
            if (!IsWindowFocusable(hiddenWindowType))
                return;

            if (_focusWindowQueue.Contains(hiddenWindowType) is false && _currentFocusedWindow != hiddenWindowType)
                throw new WindowIsNotAddedToFocusableCollectionException(hiddenWindowType);

            _hiddenWindows.Add(hiddenWindowType);

            if (_focusWindowQueue.Contains(hiddenWindowType))
                RemoveWindowTypeFromStack(hiddenWindowType);
            else if (_currentFocusedWindow == hiddenWindowType) {
                _focusablesService.MakeContainerUnFocusable(_currentFocusedWindow);
                if (_focusWindowQueue.TryPop(out Type _poppedWindow)) {
                    _currentFocusedWindow = _poppedWindow;
                    _focusablesService.MakeContainerFocusable(_currentFocusedWindow);
                }
                else
                    _currentFocusedWindow = null;
            }
            else
                throw new WindowIsNotAddedToFocusableCollectionException(hiddenWindowType);
        }

        private void RemoveWindowTypeFromStack(Type closedWindowType) {
            Stack<Type> tempStack = new();
            while (_focusWindowQueue.Count > 0) {
                Type topElement = _focusWindowQueue.Pop();
                if(topElement == closedWindowType)
                    break;

                tempStack.Push(topElement);
            }
            
            while(tempStack.Count > 0)
                _focusWindowQueue.Push(tempStack.Pop());
        }

        private bool IsWindowFocusable(Type closedWindowType) =>
            typeof(IFocusableContainer).IsAssignableFrom(closedWindowType);
    }
}
using System;
using System.Collections.Generic;

namespace Features.MvpModule {
        public interface IWindowsComponentsFinderService {
        public void CollectPresentersInWindow(IWindowInstance windowInstance);
        public void ClearPresentersInWindow(IWindowInstance windowInstance);

        public TPresenterType GetPresenterInWindow<TPresenterType>(IWindowInstance windowInstance, bool collectIfNotFound = false)
            where TPresenterType : PresenterBehaviour;

        public IEnumerable<TPresenterType> GetPresentersInWindow<TPresenterType>(IWindowInstance windowInstance, bool collectIfNotFound = false)
            where TPresenterType : PresenterBehaviour;

        public bool ContainsPresenterInWindow<TPresenterType>(IWindowInstance windowInstance, bool collectIfNotFound = false)
            where TPresenterType : PresenterBehaviour;

        public bool TryGetPresentersInWindow<TPresenterType>(IWindowInstance windowInstance, out IEnumerable<TPresenterType> presenters,
            bool collectIfNotFound = false) where TPresenterType : PresenterBehaviour;

        public bool TryGetPresenterInWindow<TPresenterType>(IWindowInstance windowInstance, out TPresenterType presenter, bool collectIfNotFound = false)
            where TPresenterType : PresenterBehaviour;
        public TPresenterType GetPresenterInWindowForView<TPresenterType>(IWindowInstance windowInstance, ViewBehaviour view, bool collectIfNotFound = false) where TPresenterType : PresenterBehaviour;
        public bool TryGetPresenterInWindowForView<TPresenterType>(IWindowInstance windowInstance, ViewBehaviour view, out TPresenterType presenter, bool collectIfNotFound = false) where TPresenterType : PresenterBehaviour;

        public PresenterBehaviour GetPresenterInWindow(Type presenterType, IWindowInstance windowInstance, bool collectIfNotFound = false);
        public IEnumerable<PresenterBehaviour> GetPresentersInWindow(Type presenterType, IWindowInstance windowInstance, bool collectIfNotFound = false);
        public bool ContainsPresenterInWindow(Type presentersType, IWindowInstance windowInstance, bool collectIfNotFound = false);

        public bool TryGetPresenterInWindow(Type presenterType, IWindowInstance windowInstance, out PresenterBehaviour presenter, bool collectIfNotFound = false);

        public bool TryGetPresentersInWindow(Type presentersType, IWindowInstance windowInstance, out IEnumerable<PresenterBehaviour> presenterBases,
            bool collectIfNotFound = false);

        public IEnumerable<IFocusableElement> GetAllFocusablesInWindow(IWindowInstance windowInstance, bool includeInactive = false);
        public PresenterBehaviour GetPresenterInWindowForView(IWindowInstance windowInstance, ViewBehaviour view, bool collectIfNotFound = false);
        public bool TryGetPresenterInWindowForView(IWindowInstance windowInstance, ViewBehaviour view, out PresenterBehaviour presenter, bool collectIfNotFound = false);
    }
}
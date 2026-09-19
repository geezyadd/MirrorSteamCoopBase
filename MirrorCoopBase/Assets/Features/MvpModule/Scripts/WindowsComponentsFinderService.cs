using System;
using System.Collections.Generic;
using System.Linq;

namespace Features.MvpModule {
    public class WindowsComponentsFinderService : IWindowsComponentsFinderService {
        private IPresenterFactory _presenterFactory;
        private Dictionary<IWindowInstance, ViewPresenterMapper> _viewPresenterMapsByWindowDictionary = new();

        public WindowsComponentsFinderService(IPresenterFactory presenterFactory) =>
            _presenterFactory = presenterFactory ?? throw new ArgumentNullException(nameof(presenterFactory));

        public void CollectPresentersInWindow(IWindowInstance windowInstance) {
            if (windowInstance == null)
                throw new ArgumentNullException(nameof(windowInstance));

            ViewPresenterMapper viewPresenterMapper = _viewPresenterMapsByWindowDictionary.TryGetValue(windowInstance, out ViewPresenterMapper value)
                ? value
                : new ViewPresenterMapper();
            _viewPresenterMapsByWindowDictionary[windowInstance] = viewPresenterMapper;

            foreach (ViewBehaviour view in windowInstance.GetAllViews()) {
                if(viewPresenterMapper.TryGetPresenterForView(view, out _))
                    continue;
                
                viewPresenterMapper[view] = _presenterFactory.GetPresenter(view);
                viewPresenterMapper.RefreshMaps();
            }
        }

        public void ClearPresentersInWindow(IWindowInstance windowInstance) {
            if (windowInstance == null)
                throw new ArgumentNullException(nameof(windowInstance));
            
            _viewPresenterMapsByWindowDictionary.Remove(windowInstance);
        }

        public TPresenterType GetPresenterInWindow<TPresenterType>(IWindowInstance windowInstance, bool collectIfNotFound = false) where TPresenterType : PresenterBehaviour =>
            GetPresenterInWindow(typeof(TPresenterType), windowInstance, collectIfNotFound) as TPresenterType;

        public IEnumerable<TPresenterType> GetPresentersInWindow<TPresenterType>(IWindowInstance windowInstance, bool collectIfNotFound = false) where TPresenterType : PresenterBehaviour =>
            GetPresentersInWindow(typeof(TPresenterType), windowInstance, collectIfNotFound).Cast<TPresenterType>();

        public bool ContainsPresenterInWindow<TPresenterType>(IWindowInstance windowInstance, bool collectIfNotFound = false) where TPresenterType : PresenterBehaviour =>
            ContainsPresenterInWindow(typeof(TPresenterType), windowInstance, collectIfNotFound);

        public bool TryGetPresentersInWindow<TPresenterType>(IWindowInstance windowInstance, out IEnumerable<TPresenterType> presenters, bool collectIfNotFound = false) where TPresenterType : PresenterBehaviour {
            bool found = TryGetPresentersInWindow(typeof(TPresenterType), windowInstance, out IEnumerable<PresenterBehaviour> presentersBases, collectIfNotFound);
            presenters = presentersBases.Cast<TPresenterType>();
            return found;
        }

        public bool TryGetPresenterInWindow<TPresenterType>(IWindowInstance windowInstance, out TPresenterType presenter, bool collectIfNotFound = false) where TPresenterType : PresenterBehaviour {
            bool found = TryGetPresenterInWindow(typeof(TPresenterType), windowInstance, out var presenterBase, collectIfNotFound);
            presenter = presenterBase as TPresenterType;
            return found;
        }

        public TPresenterType GetPresenterInWindowForView<TPresenterType>(IWindowInstance windowInstance, ViewBehaviour view, bool collectIfNotFound = false) where TPresenterType : PresenterBehaviour =>
            GetPresenterInWindowForView(windowInstance, view, collectIfNotFound) as TPresenterType;

        public bool TryGetPresenterInWindowForView<TPresenterType>(IWindowInstance windowInstance, ViewBehaviour view, out TPresenterType presenter,
            bool collectIfNotFound = false) where TPresenterType : PresenterBehaviour {
            bool found = TryGetPresenterInWindowForView(windowInstance, view,out PresenterBehaviour presenterBehaviour, collectIfNotFound);
            presenter = presenterBehaviour as TPresenterType;
            return found;
        }

        public PresenterBehaviour GetPresenterInWindow(Type presenterType, IWindowInstance windowInstance, bool collectIfNotFound = false) {
            while (true) {
                if (_viewPresenterMapsByWindowDictionary.TryGetValue(windowInstance, out var viewPresenterMaps) is false) {
                    if (!collectIfNotFound)
                        throw new PresentersNotCollectedException(windowInstance.GetType());

                    CollectPresentersInWindow(windowInstance);
                    collectIfNotFound = false;
                    continue;
                }

                if (viewPresenterMaps.TryGetPresenter(presenterType ,out var presenter))
                    return presenter;

                if (!collectIfNotFound)
                    throw new PresenterNotFoundException(windowInstance.GetType(), presenterType);

                CollectPresentersInWindow(windowInstance);
                collectIfNotFound = false;
            }
        }

        public IEnumerable<PresenterBehaviour> GetPresentersInWindow(Type presenterType, IWindowInstance windowInstance, bool collectIfNotFound = false) {
            while (true) {
                if (_viewPresenterMapsByWindowDictionary.TryGetValue(windowInstance, out var viewPresenterMaps) is false) {
                    if(!collectIfNotFound)
                        throw new PresentersNotCollectedException(windowInstance.GetType());
                    
                    CollectPresentersInWindow(windowInstance);
                    collectIfNotFound = false;
                    continue;
                }

                if (viewPresenterMaps.TryGetPresenters(presenterType, out var presenters))
                    return presenters;
                
                if(!collectIfNotFound)
                    throw new PresenterNotFoundException(windowInstance.GetType(), presenterType);
                
                CollectPresentersInWindow(windowInstance);
                collectIfNotFound = false;
            }
        }

        public bool ContainsPresenterInWindow(Type presenterType, IWindowInstance windowInstance, bool collectIfNotFound = false) {
            while (true) {
                if (_viewPresenterMapsByWindowDictionary.TryGetValue(windowInstance, out var viewPresenterMaps) is false) {
                    if (!collectIfNotFound)
                        return false;

                    CollectPresentersInWindow(windowInstance);
                    collectIfNotFound = false;
                    continue;
                }

                if (viewPresenterMaps.TryGetPresenter(presenterType ,out _))
                    return true;

                if (!collectIfNotFound)
                    return false;

                CollectPresentersInWindow(windowInstance);
                collectIfNotFound = false;
            }
        }

        public bool TryGetPresenterInWindow(Type presenterType, IWindowInstance windowInstance, out PresenterBehaviour presenter, bool collectIfNotFound = false) {
            presenter = null;
            while (true) {
                if (_viewPresenterMapsByWindowDictionary.TryGetValue(windowInstance, out var viewPresenterMaps) is false) {
                    if (!collectIfNotFound)
                        return false;

                    CollectPresentersInWindow(windowInstance);
                    collectIfNotFound = false;
                    continue;
                }

                if (viewPresenterMaps.TryGetPresenter(presenterType ,out presenter))
                    return true;

                if (!collectIfNotFound)
                    return false;

                CollectPresentersInWindow(windowInstance);
                collectIfNotFound = false;
            }
        }

        public bool TryGetPresentersInWindow(Type presentersType, IWindowInstance windowInstance, out IEnumerable<PresenterBehaviour> presenterBases, bool collectIfNotFound = false) {
            presenterBases = null;
            while (true) {
                if (_viewPresenterMapsByWindowDictionary.TryGetValue(windowInstance, out var viewPresenterMaps) is false) {
                    if (!collectIfNotFound)
                        return false;

                    CollectPresentersInWindow(windowInstance);
                    collectIfNotFound = false;
                    continue;
                }

                if (viewPresenterMaps.TryGetPresenters(presentersType ,out presenterBases))
                    return true;

                if (!collectIfNotFound)
                    return false;

                CollectPresentersInWindow(windowInstance);
                collectIfNotFound = false;
            }
        }

        public IEnumerable<IFocusableElement> GetAllFocusablesInWindow(IWindowInstance windowInstance, bool includeInactive = false) =>
            windowInstance.GetAllFocusables(includeInactive);

        public PresenterBehaviour GetPresenterInWindowForView(IWindowInstance windowInstance, ViewBehaviour view, bool collectIfNotFound = false) {
            while (true) {
                if (_viewPresenterMapsByWindowDictionary.TryGetValue(windowInstance, out var viewPresenterMaps) is false) {
                    if(!collectIfNotFound)
                        throw new PresentersNotCollectedException(windowInstance.GetType());
                    
                    CollectPresentersInWindow(windowInstance);
                    collectIfNotFound = false;
                    continue;
                }

                if (viewPresenterMaps.TryGetPresenterForView(view, out var presenter))
                    return presenter;
                
                if(!collectIfNotFound)
                    throw new PresenterNotFoundException(windowInstance.GetType());
                
                CollectPresentersInWindow(windowInstance);
                collectIfNotFound = false;
            }
        }

        public bool TryGetPresenterInWindowForView(IWindowInstance windowInstance, ViewBehaviour view, out PresenterBehaviour presenter, bool collectIfNotFound = false) {
            presenter = null;
            while (true) {
                if (_viewPresenterMapsByWindowDictionary.TryGetValue(windowInstance, out var viewPresenterMaps) is false) {
                    if (!collectIfNotFound)
                        return false;

                    CollectPresentersInWindow(windowInstance);
                    collectIfNotFound = false;
                    continue;
                }

                if (viewPresenterMaps.TryGetPresenterForView(view ,out presenter))
                    return true;

                if (!collectIfNotFound)
                    return false;

                CollectPresentersInWindow(windowInstance);
                collectIfNotFound = false;
            }
        }
    }
}
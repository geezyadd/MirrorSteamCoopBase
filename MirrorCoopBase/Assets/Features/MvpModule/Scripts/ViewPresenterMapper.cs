using System;
using System.Collections.Generic;
using System.Linq;

namespace Features.MvpModule {
    public class ViewPresenterMapper {
        private Dictionary<ViewBehaviour, PresenterBehaviour> _presenterByViewDictionary = new ();

        public PresenterBehaviour this[ViewBehaviour viewBehaviour] {
            get =>
                _presenterByViewDictionary[viewBehaviour];
            set =>
                MapViewAndPresenter(viewBehaviour, value);
        }

        public TPresenterType GetPresenterForView<TPresenterType>(ViewBehaviour viewBehaviour) where TPresenterType : PresenterBehaviour =>
            GetPresenterForView(viewBehaviour) as TPresenterType;

        public bool TryGetPresenterForView<TPresenterType>(ViewBehaviour viewBehaviour, out TPresenterType presenter)
            where TPresenterType : PresenterBehaviour {
            bool found = TryGetPresenterForView(viewBehaviour, out PresenterBehaviour presenterBehaviour);
            presenter = presenterBehaviour as TPresenterType;
            return found;
        }

        public PresenterBehaviour GetPresenterForView(ViewBehaviour viewBehaviour) =>
            _presenterByViewDictionary[viewBehaviour];

        public bool TryGetPresenterForView(ViewBehaviour viewBehaviour, out PresenterBehaviour presenterBehaviour) =>
            _presenterByViewDictionary.TryGetValue(viewBehaviour, out presenterBehaviour);

        public TPresenterType GetPresenter<TPresenterType>() where TPresenterType : PresenterBehaviour =>
            GetPresenter(typeof(TPresenterType)) as TPresenterType;

        public IEnumerable<TPresenterType> GetPresenters<TPresenterType>() where TPresenterType : PresenterBehaviour =>
            GetPresenters(typeof(TPresenterType)).Cast<TPresenterType>();

        public bool TryGetPresenter<TPresenterType>(out TPresenterType presenter) where TPresenterType : PresenterBehaviour {
            bool found = TryGetPresenter(typeof(TPresenterType), out var presenterBase);
            presenter = presenterBase as TPresenterType;
            return found;
        }

        public bool TryGetPresenters<TPresenterType>(out IEnumerable<TPresenterType> presenters) where TPresenterType : PresenterBehaviour {
            bool found = TryGetPresenters(typeof(TPresenterType), out var presenterBases);
            presenters = presenterBases.Cast<TPresenterType>();
            return found;
        }

        public PresenterBehaviour GetPresenter(Type typeOf) {
            foreach (KeyValuePair<ViewBehaviour,PresenterBehaviour> map in _presenterByViewDictionary)
                if (map.Value.GetType() == typeOf)
                    return map.Value;

            throw new PresenterNotFoundException(typeOf);
        }

        private IEnumerable<PresenterBehaviour> GetPresenters(Type presentersType) {
            List<PresenterBehaviour> foundPresenters = new();
        
            foreach (KeyValuePair<ViewBehaviour,PresenterBehaviour> map in _presenterByViewDictionary)
                if (map.Value.GetType() == presentersType)
                    foundPresenters.Add(map.Value);

            return foundPresenters;
        }

        public bool TryGetPresenter(Type typeOf, out PresenterBehaviour presenter) {
            presenter = null;
            foreach (KeyValuePair<ViewBehaviour,PresenterBehaviour> map in _presenterByViewDictionary) {
                if (map.Value.GetType() != typeOf)
                    continue;

                presenter = map.Value;
                break;
            }

            return presenter != null;
        }

        public bool TryGetPresenters(Type presentersType, out IEnumerable<PresenterBehaviour> presenters) {
            List<PresenterBehaviour> foundPresenters = new();
            foreach (KeyValuePair<ViewBehaviour,PresenterBehaviour> map in _presenterByViewDictionary) {
                if (map.Value.GetType() != presentersType)
                    continue;

                foundPresenters.Add(map.Value);
            }

            presenters = foundPresenters;
            return presenters.Any();
        }

        public void AddMap(ViewBehaviour viewBehaviour, PresenterBehaviour presenterBehaviour) =>
            _presenterByViewDictionary[viewBehaviour] = presenterBehaviour;

        public void RemoveMap(ViewBehaviour viewBehaviour) =>
            _presenterByViewDictionary.Remove(viewBehaviour);

        public void ClearMaps() =>
            _presenterByViewDictionary.Clear();

        public void RefreshMaps() {
            List<ViewBehaviour> viewsToDelete = new();
            foreach (KeyValuePair<ViewBehaviour,PresenterBehaviour> map in _presenterByViewDictionary)
                if (map.Key == null || map.Key.gameObject == null)
                    viewsToDelete.Add(map.Key);
        
            foreach (ViewBehaviour viewBehaviour in viewsToDelete)
                _presenterByViewDictionary.Remove(viewBehaviour);
        }

        private void MapViewAndPresenter(ViewBehaviour viewBehaviour, PresenterBehaviour presenterBehaviour) {
            _presenterByViewDictionary[viewBehaviour] = presenterBehaviour;
            presenterBehaviour.SetView(viewBehaviour);
        }
    }
}
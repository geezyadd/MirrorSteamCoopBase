using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Features.MvpModule {
    public interface IWindow { 
        public WindowStatus WindowStatus { get; }
        
        public event Action<Type> OnWindowOpened;
        public event Action<Type> OnWindowClosed;
        public event Action<Type> OnWindowShowed;
        public event Action<Type> OnWindowHidden;

        public Task OpenAsync();
        public void Close();
        public void Show();
        public void Hide();
        public void UpdateWindow();

        public void AddView(Transform viewRoot, bool worldPositionStays = true);
        public void RemoveView(Transform viewRoot);

        public TPresenterType GetPresenter<TPresenterType>() where TPresenterType : PresenterBehaviour;
        public IEnumerable<TPresenterType> GetPresenters<TPresenterType>() where TPresenterType : PresenterBehaviour;
        public bool TryGetPresenter<TPresenterType>(out TPresenterType presenter) where TPresenterType : PresenterBehaviour;
        public bool TryGetPresenters<TPresenterType>(out IEnumerable<TPresenterType> presenters) where TPresenterType : PresenterBehaviour;
        public TPresenterType GetPresenterForView<TPresenterType>(ViewBehaviour view) where TPresenterType : PresenterBehaviour;
        public bool TryGetPresenterForView<TPresenterType>(ViewBehaviour view, out TPresenterType presenter) where TPresenterType : PresenterBehaviour;

        public PresenterBehaviour GetPresenter(Type ofType);
        public IEnumerable<PresenterBehaviour> GetPresenters(Type presentersType);
        public bool TryGetPresenter(Type ofType, out PresenterBehaviour presenter);
        public bool TryGetPresenters(Type presentersType, out IEnumerable<PresenterBehaviour> presenterBases);
        public PresenterBehaviour GetPresenterForView(ViewBehaviour view);
        public bool TryGetPresenterForView(ViewBehaviour view, out PresenterBehaviour presenter);

        public void Open();
        public void OpenPreloaded();
        public void OnBack();
    }
}
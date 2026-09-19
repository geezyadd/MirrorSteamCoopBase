using System;
using UnityEngine;

namespace Features.MvpModule {
    public abstract class PresenterBehaviour {
        protected ViewBehaviour ViewBehaviour { get; set; }
        public bool IsViewShown =>
            ViewBehaviour.IsShown;

        public event Action OnViewShowed;
        public event Action OnViewHidden;

        public virtual void ShowView() {
            ViewBehaviour.ShowView();
            OnViewShowed?.Invoke();
            OnShowed();
        }

        public virtual void HideView() {
            ViewBehaviour.HideView();
            OnViewHidden?.Invoke();
            OnHidden();
        }

        public virtual void SetView(ViewBehaviour viewBehaviour) {
            ViewBehaviour = viewBehaviour;
            ViewBehaviour.OnDispose += OnViewDisposed;
            ViewBehaviour.OnEnabled += OnViewEnabled;
            ViewBehaviour.OnDisabled += OnViewDisabled;
            OnViewSet();
            if (ViewBehaviour.IsEnabled)
                OnViewEnabled();
        }

        public virtual Transform GetViewRoot() =>
            ViewBehaviour.GetRoot();

        protected virtual void OnShowed() { }
        protected virtual void OnHidden() { }
        protected virtual void OnDisposed() { }
        protected virtual void OnViewSet() { }
        protected virtual void OnViewEnabled() { }
        protected virtual void OnViewDisabled() { }

        private void OnViewDisposed() {
            ViewBehaviour.OnDispose -= OnViewDisposed;
            ViewBehaviour.OnEnabled -= OnViewEnabled;
            ViewBehaviour.OnDisabled -= OnViewDisabled;
            OnDisposed();
        }
    }
}
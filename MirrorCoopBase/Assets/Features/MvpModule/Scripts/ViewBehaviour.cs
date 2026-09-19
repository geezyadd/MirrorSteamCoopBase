using System;
using UnityEngine;

namespace Features.MvpModule {
    public abstract class ViewBehaviour : MonoBehaviour {
        public bool IsShown { get; protected set; }
        public bool IsEnabled { get; private set; }
        public bool IsViewDisposed { get; private set; }

        public event Action OnDispose;
        public event Action OnDisabled;
        public event Action OnEnabled;

        private void Awake() =>
            IsShown = gameObject.activeSelf;

        private void OnDestroy() =>
            DisposeView();

        public virtual void ShowView() {
            gameObject.SetActive(true);
            IsShown = true;
        }

        protected virtual void OnEnable() {
            IsEnabled = true;
            OnEnabled?.Invoke();
        }

        protected virtual void OnDisable() {
            if (!IsEnabled)
                return;
            IsEnabled = false;
            OnDisabled?.Invoke();
        }

        public virtual void HideView() {
            gameObject.SetActive(false);
            IsShown = false;
        }

        public virtual void DisposeView() {
            if (IsViewDisposed)
                return;
            IsViewDisposed = true;
            OnDispose?.Invoke();
        }

        public void DisableView() =>
            OnDisable();

        public Transform GetRoot() =>
            gameObject.transform;
    }
}
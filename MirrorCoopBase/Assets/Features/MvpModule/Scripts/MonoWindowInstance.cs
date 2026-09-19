using System;
using System.Collections.Generic;
using UnityEngine;

namespace Features.MvpModule {
    public class MonoWindowInstance : MonoBehaviour, IWindowInstance {
        public void Show() {
            EnableInstance();
        }

        public void Hide() {
            DisableInstance();
        }

        public void Destroy() {
            if (IsValid() is false)
                return;
            
            DestroyInstance();
        }

        private void EnableInstance() =>
            gameObject.SetActive(true);

        private void DisableInstance() =>
            gameObject.SetActive(false);

        private void DestroyInstance() {
            if (IsValid())
                Destroy(gameObject);
        }

        private bool IsValid() =>
            this != null;

        public IEnumerable<ViewBehaviour> GetAllViews() {
            if (this == null || gameObject == null)
                return Array.Empty<ViewBehaviour>();
            return GetComponentsInChildren<ViewBehaviour>(true);
        }

        public IEnumerable<IFocusableElement> GetAllFocusables(bool includeInactive) =>
            GetComponentsInChildren<IFocusableElement>(includeInactive);

        public void AddChild(Transform childRoot, bool worldPositionStays) =>
            childRoot.SetParent(gameObject.transform, worldPositionStays);

        public void RemoveChild(Transform childRoot) {
            if(childRoot.parent == gameObject.transform)
                childRoot.SetParent(null);
        }
    }
}
using System.Collections.Generic;
using UnityEngine;

namespace Features.MvpModule {
    public interface IWindowInstance {
        public void Show();
        public void Hide();
        public void Destroy();
        public IEnumerable<ViewBehaviour> GetAllViews();
        public IEnumerable<IFocusableElement> GetAllFocusables(bool includeInactive);
        public void AddChild(Transform childRoot, bool worldPositionStays);
        public void RemoveChild(Transform childRoot);
    }
}
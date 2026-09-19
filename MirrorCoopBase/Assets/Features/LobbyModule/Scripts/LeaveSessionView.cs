using System;
using Features.MvpModule;
using UnityEngine;
using UnityEngine.UI;

namespace Features.LobbyModule.Scripts {
    public abstract class LeaveSessionViewBase : ViewBehaviour {
        public Action OnLeaveClicked;

        public abstract void SetInteractable(bool interactable);
    }

    public sealed class LeaveSessionView : LeaveSessionViewBase {
        [SerializeField] Button _button;

        protected override void OnEnable() {
            base.OnEnable();
            if (_button != null)
                _button.onClick.AddListener(OnClicked);
        }

        protected override void OnDisable() {
            if (_button != null)
                _button.onClick.RemoveListener(OnClicked);
            base.OnDisable();
        }

        public override void SetInteractable(bool interactable) {
            if (_button != null)
                _button.interactable = interactable;
        }

        void OnClicked() =>
            OnLeaveClicked?.Invoke();
    }
}

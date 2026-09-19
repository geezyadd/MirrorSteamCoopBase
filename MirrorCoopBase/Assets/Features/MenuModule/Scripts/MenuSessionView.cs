using System;
using Features.MvpModule;
using UnityEngine;
using UnityEngine.UI;

namespace Features.MenuModule.Scripts {
    public abstract class MenuSessionViewBase : ViewBehaviour {
        public Action OnHostClicked;
        public Action OnJoinClicked;
        public Action OnHostSteamClicked;
        public Action OnJoinSteamClicked;

        public abstract string Address { get; }
        public abstract void SetStatus(string message);
        public abstract void SetInteractable(bool interactable);
    }

    public sealed class MenuSessionView : MenuSessionViewBase {
        [UnityEngine.SerializeField] InputField _addressInput;
        [UnityEngine.SerializeField] Button _hostButton;
        [UnityEngine.SerializeField] Button _joinButton;
        [UnityEngine.SerializeField] Button _hostSteamButton;
        [UnityEngine.SerializeField] Button _joinSteamButton;
        [UnityEngine.SerializeField] UnityEngine.UI.Text _statusText;

        public override string Address =>
            _addressInput != null ? _addressInput.text : "localhost";

        protected override void OnEnable() {
            base.OnEnable();
            AddListener(_hostButton, OnHostClickedHandler);
            AddListener(_joinButton, OnJoinClickedHandler);
            AddListener(_hostSteamButton, OnHostSteamClickedHandler);
            AddListener(_joinSteamButton, OnJoinSteamClickedHandler);
        }

        protected override void OnDisable() {
            RemoveListener(_hostButton, OnHostClickedHandler);
            RemoveListener(_joinButton, OnJoinClickedHandler);
            RemoveListener(_hostSteamButton, OnHostSteamClickedHandler);
            RemoveListener(_joinSteamButton, OnJoinSteamClickedHandler);
            base.OnDisable();
        }

        public override void SetStatus(string message) {
            if (_statusText != null)
                _statusText.text = message ?? string.Empty;
        }

        public override void SetInteractable(bool interactable) {
            SetButton(_hostButton, interactable);
            SetButton(_joinButton, interactable);
            SetButton(_hostSteamButton, interactable);
            SetButton(_joinSteamButton, interactable);
            if (_addressInput != null)
                _addressInput.interactable = interactable;
        }

        void OnHostClickedHandler() =>
            OnHostClicked?.Invoke();

        void OnJoinClickedHandler() =>
            OnJoinClicked?.Invoke();

        void OnHostSteamClickedHandler() =>
            OnHostSteamClicked?.Invoke();

        void OnJoinSteamClickedHandler() =>
            OnJoinSteamClicked?.Invoke();

        static void AddListener(Button button, UnityEngine.Events.UnityAction action) {
            if (button != null)
                button.onClick.AddListener(action);
        }

        static void RemoveListener(Button button, UnityEngine.Events.UnityAction action) {
            if (button != null)
                button.onClick.RemoveListener(action);
        }

        static void SetButton(Button button, bool interactable) {
            if (button != null)
                button.interactable = interactable;
        }
    }
}

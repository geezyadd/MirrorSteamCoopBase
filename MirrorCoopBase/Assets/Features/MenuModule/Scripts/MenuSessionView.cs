using System;
using System.Threading.Tasks;
using Features.GameFlowStateMachineModule.Scripts;
using Features.GameFlowStateMachineModule.Scripts.States;
using Game.Connection;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;
using Zenject;

namespace Features.MenuModule.Scripts {
    public sealed class MenuSessionView : MonoBehaviour {
        [SerializeField] private InputField _addressInput;
        [SerializeField] private Button _hostButton;
        [SerializeField] private Button _joinButton;
        [SerializeField] private Button _hostSteamButton;
        [SerializeField] private Button _joinSteamButton;
        [SerializeField] private Text _statusText;

        private IConnectionSessionService _connectionSession;
        private ISteamLobbyService _steamLobby;
        private IGameFlowStateMachineService _gameFlowStateMachine;
        private bool _isBusy;

        [Inject]
        private void Construct(
            IConnectionSessionService connectionSession,
            ISteamLobbyService steamLobby,
            IGameFlowStateMachineService gameFlowStateMachine) {
            _connectionSession = connectionSession;
            _steamLobby = steamLobby;
            _gameFlowStateMachine = gameFlowStateMachine;
        }

        private void Awake() {
            if (_hostButton == null || _joinButton == null)
                BuildRuntimeUi();

            EnsureSteamButtons();
        }

        private void OnEnable() {
            if (_hostButton != null)
                _hostButton.onClick.AddListener(OnHostClicked);
            if (_joinButton != null)
                _joinButton.onClick.AddListener(OnJoinClicked);
            if (_hostSteamButton != null)
                _hostSteamButton.onClick.AddListener(OnHostSteamClicked);
            if (_joinSteamButton != null)
                _joinSteamButton.onClick.AddListener(OnJoinSteamClicked);
            if (_steamLobby != null)
                _steamLobby.JoinRequested += OnSteamJoinRequested;
            SetStatus(string.Empty);
        }

        private void OnDisable() {
            if (_hostButton != null)
                _hostButton.onClick.RemoveListener(OnHostClicked);
            if (_joinButton != null)
                _joinButton.onClick.RemoveListener(OnJoinClicked);
            if (_hostSteamButton != null)
                _hostSteamButton.onClick.RemoveListener(OnHostSteamClicked);
            if (_joinSteamButton != null)
                _joinSteamButton.onClick.RemoveListener(OnJoinSteamClicked);
            if (_steamLobby != null)
                _steamLobby.JoinRequested -= OnSteamJoinRequested;
        }

        private void OnHostClicked() =>
            _ = RunAsync(HostAsync);

        private void OnJoinClicked() =>
            _ = RunAsync(JoinAsync);

        private void OnHostSteamClicked() =>
            _ = RunAsync(HostSteamAsync);

        private void OnJoinSteamClicked() =>
            _ = RunAsync(JoinSteamAsync);

        private void OnSteamJoinRequested(ulong lobbyId) =>
            _ = RunAsync(() => JoinSteamLobbyAsync(lobbyId));

        private async Task HostAsync() {
            await _gameFlowStateMachine.EnterAsync<SessionGameFlowState>();
            await _connectionSession.HostAsync();
        }

        private async Task JoinAsync() {
            await _gameFlowStateMachine.EnterAsync<SessionGameFlowState>();
            await _connectionSession.JoinAsync(_addressInput != null ? _addressInput.text : "localhost");
        }

        private async Task HostSteamAsync() {
            await _gameFlowStateMachine.EnterAsync<SessionGameFlowState>();
            await _connectionSession.HostSteamAsync();
        }

        private async Task JoinSteamAsync() {
            string text = _addressInput != null ? _addressInput.text : string.Empty;
            if (ulong.TryParse(text.Trim(), out ulong lobbyId) == false)
                throw new InvalidOperationException("Enter a Steam lobby id.");

            await JoinSteamLobbyAsync(lobbyId);
        }

        private async Task JoinSteamLobbyAsync(ulong lobbyId) {
            await _gameFlowStateMachine.EnterAsync<SessionGameFlowState>();
            await _connectionSession.JoinSteamAsync(lobbyId);
        }

        private async Task RunAsync(Func<Task> operation) {
            if (_isBusy)
                return;

            _isBusy = true;
            SetInteractable(false);
            SetStatus("Connecting...");

            try {
                await operation();
                if (_steamLobby != null && _steamLobby.CurrentLobbyId.HasValue)
                    SetStatus($"Steam lobby {_steamLobby.CurrentLobbyId.Value}");
                else
                    SetStatus("Connected.");
            }
            catch (Exception exception) {
                Debug.LogException(exception);
                SetStatus(exception.Message);
            }
            finally {
                _isBusy = false;
                SetInteractable(true);
            }
        }

        private void SetInteractable(bool interactable) {
            if (_hostButton != null)
                _hostButton.interactable = interactable;
            if (_joinButton != null)
                _joinButton.interactable = interactable;
            if (_hostSteamButton != null)
                _hostSteamButton.interactable = interactable;
            if (_joinSteamButton != null)
                _joinSteamButton.interactable = interactable;
            if (_addressInput != null)
                _addressInput.interactable = interactable;
        }

        private void SetStatus(string message) {
            if (_statusText != null)
                _statusText.text = message ?? string.Empty;
        }

        private void EnsureSteamButtons() {
            Transform panel = _hostButton != null ? _hostButton.transform.parent : transform;
            if (_hostSteamButton == null)
                _hostSteamButton = CreateButton(panel, "HostSteamButton", "Host Steam", new Vector2(0f, -160f));
            if (_joinSteamButton == null)
                _joinSteamButton = CreateButton(panel, "JoinSteamButton", "Join Steam", new Vector2(0f, -210f));
        }

        private void BuildRuntimeUi() {
            EnsureEventSystem();

            Canvas canvas = GetComponent<Canvas>();
            if (canvas == null) {
                canvas = gameObject.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                gameObject.AddComponent<CanvasScaler>();
                gameObject.AddComponent<GraphicRaycaster>();
            }

            var panel = new GameObject("Panel", typeof(RectTransform));
            panel.transform.SetParent(transform, false);
            panel.GetComponent<RectTransform>().sizeDelta = new Vector2(720f, 360f);

            _addressInput = CreateInputField(panel.transform, "AddressInput", "localhost or Steam lobby id", new Vector2(0f, 80f));
            _hostButton = CreateButton(panel.transform, "HostButton", "Host", new Vector2(-120f, 20f));
            _joinButton = CreateButton(panel.transform, "JoinButton", "Join", new Vector2(-120f, -40f));
            _hostSteamButton = CreateButton(panel.transform, "HostSteamButton", "Host Steam", new Vector2(120f, 20f));
            _joinSteamButton = CreateButton(panel.transform, "JoinSteamButton", "Join Steam", new Vector2(120f, -40f));
            _statusText = CreateLabel(panel.transform, "StatusText", string.Empty, new Vector2(0f, -100f));
        }

        private static void EnsureEventSystem() {
            if (FindFirstObjectByType<EventSystem>() != null)
                return;

            var eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<EventSystem>();
            eventSystem.AddComponent<InputSystemUIInputModule>();
        }

        private static InputField CreateInputField(Transform parent, string name, string placeholder, Vector2 anchoredPosition) {
            var root = new GameObject(name, typeof(RectTransform));
            root.transform.SetParent(parent, false);
            RectTransform rect = root.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(320f, 40f);
            rect.anchoredPosition = anchoredPosition;
            root.AddComponent<Image>().color = new Color(1f, 1f, 1f, 0.9f);
            InputField inputField = root.AddComponent<InputField>();

            Text text = CreateLabel(root.transform, "Text", string.Empty, Vector2.zero);
            text.color = Color.black;
            text.alignment = TextAnchor.MiddleLeft;
            RectTransform textRect = text.rectTransform;
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(10f, 0f);
            textRect.offsetMax = new Vector2(-10f, 0f);

            Text placeholderText = CreateLabel(root.transform, "Placeholder", placeholder, Vector2.zero);
            placeholderText.fontStyle = FontStyle.Italic;
            placeholderText.color = new Color(0.2f, 0.2f, 0.2f, 0.6f);
            placeholderText.alignment = TextAnchor.MiddleLeft;
            RectTransform placeholderRect = placeholderText.rectTransform;
            placeholderRect.anchorMin = Vector2.zero;
            placeholderRect.anchorMax = Vector2.one;
            placeholderRect.offsetMin = new Vector2(10f, 0f);
            placeholderRect.offsetMax = new Vector2(-10f, 0f);

            inputField.textComponent = text;
            inputField.placeholder = placeholderText;
            return inputField;
        }

        private static Button CreateButton(Transform parent, string name, string label, Vector2 anchoredPosition) {
            var root = new GameObject(name, typeof(RectTransform));
            root.transform.SetParent(parent, false);
            RectTransform rect = root.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(220f, 44f);
            rect.anchoredPosition = anchoredPosition;
            root.AddComponent<Image>().color = new Color(0.2f, 0.45f, 0.8f, 1f);
            Button button = root.AddComponent<Button>();
            Text text = CreateLabel(root.transform, "Text", label, Vector2.zero);
            text.alignment = TextAnchor.MiddleCenter;
            text.color = Color.white;
            RectTransform textRect = text.rectTransform;
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
            return button;
        }

        private static Text CreateLabel(Transform parent, string name, string value, Vector2 anchoredPosition) {
            var root = new GameObject(name, typeof(RectTransform));
            root.transform.SetParent(parent, false);
            RectTransform rect = root.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(360f, 40f);
            rect.anchoredPosition = anchoredPosition;
            Text text = root.AddComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.text = value;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = Color.white;
            return text;
        }
    }
}

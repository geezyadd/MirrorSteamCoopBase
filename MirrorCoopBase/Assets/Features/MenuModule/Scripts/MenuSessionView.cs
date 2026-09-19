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
        [SerializeField] private Text _statusText;

        private IConnectionSessionService _connectionSession;
        private IGameFlowStateMachineService _gameFlowStateMachine;
        private bool _isBusy;

        [Inject]
        private void Construct(
            IConnectionSessionService connectionSession,
            IGameFlowStateMachineService gameFlowStateMachine) {
            _connectionSession = connectionSession;
            _gameFlowStateMachine = gameFlowStateMachine;
        }

        private void Awake() {
            if (_hostButton == null || _joinButton == null)
                BuildRuntimeUi();
        }

        private void OnEnable() {
            if (_hostButton != null)
                _hostButton.onClick.AddListener(OnHostClicked);
            if (_joinButton != null)
                _joinButton.onClick.AddListener(OnJoinClicked);
            SetStatus(string.Empty);
        }

        private void OnDisable() {
            if (_hostButton != null)
                _hostButton.onClick.RemoveListener(OnHostClicked);
            if (_joinButton != null)
                _joinButton.onClick.RemoveListener(OnJoinClicked);
        }

        private void OnHostClicked() =>
            _ = RunAsync(HostAsync);

        private void OnJoinClicked() =>
            _ = RunAsync(JoinAsync);

        private async Task HostAsync() {
            await _gameFlowStateMachine.EnterAsync<SessionGameFlowState>();
            await _connectionSession.HostAsync();
        }

        private async Task JoinAsync() {
            await _gameFlowStateMachine.EnterAsync<SessionGameFlowState>();
            await _connectionSession.JoinAsync(_addressInput != null ? _addressInput.text : "localhost");
        }

        private async Task RunAsync(Func<Task> operation) {
            if (_isBusy)
                return;

            _isBusy = true;
            SetInteractable(false);
            SetStatus("Connecting...");

            try {
                await operation();
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
            if (_addressInput != null)
                _addressInput.interactable = interactable;
        }

        private void SetStatus(string message) {
            if (_statusText != null)
                _statusText.text = message ?? string.Empty;
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
            panel.GetComponent<RectTransform>().sizeDelta = new Vector2(420f, 360f);

            _addressInput = CreateInputField(panel.transform, "AddressInput", "localhost", new Vector2(0f, 80f));
            _hostButton = CreateButton(panel.transform, "HostButton", "Host", new Vector2(0f, 20f));
            _joinButton = CreateButton(panel.transform, "JoinButton", "Join", new Vector2(0f, -40f));
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

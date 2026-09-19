using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace Features.MvpModule {
    public sealed class WindowsRootService {
        private Transform _root;

        public Transform Root {
            get {
                if (_root == null)
                    _root = CreateRoot();

                return _root;
            }
        }

        static Transform CreateRoot() {
            EnsureEventSystem();

            var canvasObject = new GameObject("WindowsRoot");
            Object.DontDestroyOnLoad(canvasObject);

            Canvas canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;

            CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            canvasObject.AddComponent<GraphicRaycaster>();
            return canvasObject.transform;
        }

        static void EnsureEventSystem() {
            EventSystem[] eventSystems = Object.FindObjectsByType<EventSystem>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (EventSystem eventSystem in eventSystems) {
                if (eventSystem == null)
                    continue;

                if (eventSystem.gameObject.scene.name != "DontDestroyOnLoad")
                    Object.DontDestroyOnLoad(eventSystem.gameObject);

                return;
            }

            var eventSystemObject = new GameObject("WindowsEventSystem");
            Object.DontDestroyOnLoad(eventSystemObject);
            eventSystemObject.AddComponent<EventSystem>();
            eventSystemObject.AddComponent<InputSystemUIInputModule>();
        }
    }
}

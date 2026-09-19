#if UNITY_EDITOR
using System.IO;
using Features.AddressablesConstantsGenerator.Generated;
using Features.BootstrapersModule.Scripts;
using Features.GameCoreModule.Scripts.Constants;
using Features.GameCoreModule.Scripts.Installers;
using Features.LobbyModule.Scripts;
using Features.MenuModule.Scripts;
using Game.Connection;
using Mirror;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Zenject;

namespace Features.GameCoreModule.Scripts.Editor {
    public static class GameArchitectureSetup {
        private const string FeaturesRoot = "Assets/Features";
        private const string GameResourcesRoot = FeaturesRoot + "/GameCoreModule/GameResources";
        private const string ScenesRoot = GameResourcesRoot + "/Scenes";
        private const string ResourcesRoot = GameResourcesRoot + "/Resources";
        private const string LobbyPrefabsRoot = FeaturesRoot + "/LobbyModule/GameResources/Prefabs";
        private const string ConfigsRoot = FeaturesRoot + "/Connection/GameResources/Configurations";
        private const string ProjectContextPath = ResourcesRoot + "/ProjectContext.prefab";
        private const string PlayerPrefabPath = LobbyPrefabsRoot + "/LobbyPlayer.prefab";
        private const string ConnectionConfigPath = ConfigsRoot + "/ConnectionConfig_Default.asset";
        private const string BootstrapScenePath = ScenesRoot + "/" + SceneNames.Bootstrap + ".unity";
        private const string ConfigurationsAddressableGroup = "Configurations";
        private const string LocalScenesAddressableGroup = "Scenes";

        [InitializeOnLoadMethod]
        private static void AutoSetupIfMissing() {
            EditorApplication.delayCall += () => {
                if (EditorApplication.isPlayingOrWillChangePlaymode)
                    return;

                bool bootstrapExists = AssetDatabase.LoadAssetAtPath<SceneAsset>(BootstrapScenePath) != null;
                if (bootstrapExists == false) {
                    Debug.Log("GameCore: scenes missing — running architecture setup...");
                    Setup();
                    return;
                }

                EnsureFolders();
                CreateConnectionConfig();
                SetupAddressableScenesAndConfigs();
                UpdateBuildSettings();
                AssetDatabase.SaveAssets();
            };
        }

        [MenuItem("GameCore/Setup Architecture (Scenes, Prefabs, Build Settings)")]
        public static void Setup() {
            EnsureFolders();
            CreateConnectionConfig();
            CreateProjectContext();
            GameObject playerPrefab = CreatePlayerPrefab();

            CreateBootstrapScene();
            CreateGlobalScene(playerPrefab);
            CreateMenuScene();
            CreateLobbyScene();
            SetupAddressableScenesAndConfigs();
            UpdateBuildSettings();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("GameCore architecture setup complete. Open BootstrapScene and press Play.");
        }

        private static void EnsureFolders() {
            EnsureFolder("Assets/Features");
            EnsureFolder(FeaturesRoot + "/GameCoreModule");
            EnsureFolder(GameResourcesRoot);
            EnsureFolder(ScenesRoot);
            EnsureFolder(ResourcesRoot);
            EnsureFolder(FeaturesRoot + "/LobbyModule");
            EnsureFolder(FeaturesRoot + "/LobbyModule/GameResources");
            EnsureFolder(LobbyPrefabsRoot);
            EnsureFolder(FeaturesRoot + "/Connection");
            EnsureFolder(FeaturesRoot + "/Connection/GameResources");
            EnsureFolder(ConfigsRoot);
        }

        private static void EnsureFolder(string path) {
            if (AssetDatabase.IsValidFolder(path))
                return;

            string parent = Path.GetDirectoryName(path)?.Replace("\\", "/");
            string name = Path.GetFileName(path);
            if (string.IsNullOrEmpty(parent) == false && AssetDatabase.IsValidFolder(parent) == false)
                EnsureFolder(parent);

            AssetDatabase.CreateFolder(parent, name);
        }

        private static void CreateProjectContext() {
            if (AssetDatabase.LoadAssetAtPath<GameObject>(ProjectContextPath) != null)
                return;

            var gameObject = new GameObject("ProjectContext");
            try {
                ProjectContext projectContext = gameObject.AddComponent<ProjectContext>();
                ProjectContextInstaller installer = gameObject.AddComponent<ProjectContextInstaller>();
                projectContext.Installers = new MonoInstaller[] { installer };
                PrefabUtility.SaveAsPrefabAsset(gameObject, ProjectContextPath);
            }
            finally {
                Object.DestroyImmediate(gameObject);
            }
        }

        private static GameObject CreatePlayerPrefab() {
            GameObject existing = AssetDatabase.LoadAssetAtPath<GameObject>(PlayerPrefabPath);
            if (existing != null)
                return existing;

            var player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            player.name = "LobbyPlayer";
            Object.DestroyImmediate(player.GetComponent<CapsuleCollider>());

            CharacterController characterController = player.AddComponent<CharacterController>();
            characterController.height = 2f;
            characterController.center = new Vector3(0f, 1f, 0f);
            characterController.radius = 0.4f;

            player.AddComponent<NetworkIdentity>();
            player.AddComponent<NetworkTransformReliable>();
            player.AddComponent<LobbyPlayerMovement>();
            player.AddComponent<LobbyLocalCameraFollow>();

            PrefabUtility.SaveAsPrefabAsset(player, PlayerPrefabPath);
            Object.DestroyImmediate(player);
            return AssetDatabase.LoadAssetAtPath<GameObject>(PlayerPrefabPath);
        }

        private static void CreateBootstrapScene() {
            string path = ScenesRoot + "/" + SceneNames.Bootstrap + ".unity";
            if (File.Exists(ToAbsolute(path)))
                return;

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            GameObject contextObject = CreateSceneContextObject(scene, "BootstrapSceneContext", null, null);
            contextObject.AddComponent<BootstrapSceneBootstrapper>();
            EditorSceneManager.SaveScene(scene, path);
        }

        private static void CreateGlobalScene(GameObject playerPrefab) {
            string path = ScenesRoot + "/" + SceneNames.Global + ".unity";
            if (File.Exists(ToAbsolute(path)))
                return;

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            GameObject contextObject = CreateSceneContextObject(
                scene,
                "GlobalSceneContext",
                new[] { SceneNames.GlobalContract },
                null);
            GlobalSceneInstaller installer = contextObject.AddComponent<GlobalSceneInstaller>();
            contextObject.GetComponent<SceneContext>().Installers = new MonoInstaller[] { installer };
            contextObject.AddComponent<GlobalSceneBootstrapper>();

            CreateConnectionObject(scene, playerPrefab);
            EditorSceneManager.SaveScene(scene, path);
        }

        private static void CreateConnectionObject(Scene scene, GameObject playerPrefab) {
            var connectionObject = new GameObject("ConnectionNetwork");
            SceneManager.MoveGameObjectToScene(connectionObject, scene);

            TelepathyTransport transport = connectionObject.AddComponent<TelepathyTransport>();
            ConnectionAuthenticator authenticator = connectionObject.AddComponent<ConnectionAuthenticator>();
            ConnectionNetworkManager networkManager = connectionObject.AddComponent<ConnectionNetworkManager>();

            SerializedObject networkSerialized = new SerializedObject(networkManager);
            networkSerialized.FindProperty("dontDestroyOnLoad").boolValue = true;
            networkSerialized.FindProperty("maxConnections").intValue = 4;
            networkSerialized.FindProperty("offlineScene").stringValue = string.Empty;
            networkSerialized.FindProperty("onlineScene").stringValue = string.Empty;
            networkSerialized.FindProperty("transport").objectReferenceValue = transport;
            networkSerialized.FindProperty("authenticator").objectReferenceValue = authenticator;
            networkSerialized.FindProperty("playerPrefab").objectReferenceValue = playerPrefab;
            networkSerialized.FindProperty("autoCreatePlayer").boolValue = true;
            networkSerialized.FindProperty("lobbySceneName").stringValue = SceneNames.Lobby;
            networkSerialized.FindProperty("menuSceneName").stringValue = SceneNames.Menu;
            networkSerialized.FindProperty("persistentSceneName").stringValue = SceneNames.Global;
            networkSerialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void CreateMenuScene() {
            string path = ScenesRoot + "/" + SceneNames.Menu + ".unity";
            if (File.Exists(ToAbsolute(path)))
                return;

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            GameObject contextObject = CreateSceneContextObject(
                scene,
                "MenuSceneContext",
                null,
                new[] { SceneNames.GlobalContract });
            MenuSceneInstaller installer = contextObject.AddComponent<MenuSceneInstaller>();
            contextObject.GetComponent<SceneContext>().Installers = new MonoInstaller[] { installer };

            CreateEventSystem(scene);
            GameObject canvasObject = CreateCanvas(scene);
            MenuSessionView view = canvasObject.AddComponent<MenuSessionView>();

            GameObject panel = CreateUiPanel(canvasObject.transform, "Panel");
            InputField addressInput = CreateInputField(panel.transform, "AddressInput", "localhost", new Vector2(0f, 80f));
            Button hostButton = CreateButton(panel.transform, "HostButton", "Host", new Vector2(0f, 20f));
            Button joinButton = CreateButton(panel.transform, "JoinButton", "Join", new Vector2(0f, -40f));
            Text statusText = CreateLabel(panel.transform, "StatusText", string.Empty, new Vector2(0f, -100f));

            SerializedObject viewSerialized = new SerializedObject(view);
            viewSerialized.FindProperty("_addressInput").objectReferenceValue = addressInput;
            viewSerialized.FindProperty("_hostButton").objectReferenceValue = hostButton;
            viewSerialized.FindProperty("_joinButton").objectReferenceValue = joinButton;
            viewSerialized.FindProperty("_statusText").objectReferenceValue = statusText;
            viewSerialized.ApplyModifiedPropertiesWithoutUndo();

            EditorSceneManager.SaveScene(scene, path);
        }

        private static void CreateLobbyScene() {
            string path = ScenesRoot + "/" + SceneNames.Lobby + ".unity";
            if (File.Exists(ToAbsolute(path)))
                return;

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            GameObject contextObject = CreateSceneContextObject(
                scene,
                "LobbySceneContext",
                null,
                new[] { SceneNames.GlobalContract });
            LobbySceneInstaller installer = contextObject.AddComponent<LobbySceneInstaller>();
            contextObject.GetComponent<SceneContext>().Installers = new MonoInstaller[] { installer };

            GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Ground";
            ground.transform.position = Vector3.zero;
            SceneManager.MoveGameObjectToScene(ground, scene);

            GameObject spawnPointObject = new GameObject("SpawnPoint");
            spawnPointObject.transform.position = new Vector3(0f, 1f, 0f);
            SceneManager.MoveGameObjectToScene(spawnPointObject, scene);
            spawnPointObject.AddComponent<ConnectionSpawnPoint>();

            CreateEventSystem(scene);
            GameObject canvasObject = CreateCanvas(scene);
            Button leaveButton = CreateButton(canvasObject.transform, "LeaveButton", "Leave", new Vector2(0f, 200f));
            leaveButton.gameObject.AddComponent<LeaveSessionButton>();

            EditorSceneManager.SaveScene(scene, path);
        }

        private static GameObject CreateSceneContextObject(
            Scene scene,
            string objectName,
            string[] contractNames,
            string[] parentContractNames) {
            var contextObject = new GameObject(objectName);
            SceneManager.MoveGameObjectToScene(contextObject, scene);
            SceneContext sceneContext = contextObject.AddComponent<SceneContext>();
            sceneContext.ContractNames = contractNames ?? new string[0];
            sceneContext.ParentContractNames = parentContractNames ?? new string[0];
            return contextObject;
        }

        private static void UpdateBuildSettings() {
            EditorBuildSettings.scenes = new[] {
                new EditorBuildSettingsScene(ScenesRoot + "/" + SceneNames.Bootstrap + ".unity", true),
            };
        }

        private static ConnectionConfig CreateConnectionConfig() {
            ConnectionConfig existing = AssetDatabase.LoadAssetAtPath<ConnectionConfig>(ConnectionConfigPath);
            if (existing != null) {
                MarkAddressable(
                    ConnectionConfigPath,
                    Address.Configurations.ConnectionConfig_Default,
                    ConfigurationsAddressableGroup);
                return existing;
            }

            ConnectionConfig config = ScriptableObject.CreateInstance<ConnectionConfig>();
            AssetDatabase.CreateAsset(config, ConnectionConfigPath);
            EditorUtility.SetDirty(config);
            MarkAddressable(
                ConnectionConfigPath,
                Address.Configurations.ConnectionConfig_Default,
                ConfigurationsAddressableGroup);
            return config;
        }

        private static void SetupAddressableScenesAndConfigs() {
            CreateConnectionConfig();

            MarkAddressable(
                ScenesRoot + "/" + SceneNames.Global + ".unity",
                SceneNames.Global,
                LocalScenesAddressableGroup);

            MarkAddressable(
                ScenesRoot + "/" + SceneNames.Menu + ".unity",
                SceneNames.Menu,
                LocalScenesAddressableGroup);

            MarkAddressable(
                ScenesRoot + "/" + SceneNames.Lobby + ".unity",
                SceneNames.Lobby,
                LocalScenesAddressableGroup);
        }

        private static void MarkAddressable(string assetPath, string address, string groupName) {
            AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.GetSettings(true);
            if (settings == null) {
                Debug.LogWarning("GameCore: AddressableAssetSettings not found. Skip marking " + assetPath);
                return;
            }

            AddressableAssetGroup group = settings.FindGroup(groupName);
            if (group == null) {
                group = settings.CreateGroup(
                    groupName,
                    false,
                    false,
                    true,
                    null,
                    typeof(UnityEditor.AddressableAssets.Settings.GroupSchemas.BundledAssetGroupSchema),
                    typeof(UnityEditor.AddressableAssets.Settings.GroupSchemas.ContentUpdateGroupSchema));
            }

            string guid = AssetDatabase.AssetPathToGUID(assetPath);
            if (string.IsNullOrEmpty(guid)) {
                Debug.LogWarning("GameCore: Cannot mark addressable, GUID missing for " + assetPath);
                return;
            }

            AddressableAssetEntry entry = settings.CreateOrMoveEntry(guid, group, false, false);
            entry.SetAddress(address);
            EditorUtility.SetDirty(settings);
            EditorUtility.SetDirty(group);
        }

        private static void CreateEventSystem(Scene scene) {
            var eventSystem = new GameObject("EventSystem");
            SceneManager.MoveGameObjectToScene(eventSystem, scene);
            eventSystem.AddComponent<EventSystem>();
            eventSystem.AddComponent<InputSystemUIInputModule>();
        }

        private static GameObject CreateCanvas(Scene scene) {
            var canvasObject = new GameObject("Canvas");
            SceneManager.MoveGameObjectToScene(canvasObject, scene);
            Canvas canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObject.AddComponent<CanvasScaler>();
            canvasObject.AddComponent<GraphicRaycaster>();
            return canvasObject;
        }

        private static GameObject CreateUiPanel(Transform parent, string name) {
            var panel = new GameObject(name, typeof(RectTransform));
            panel.transform.SetParent(parent, false);
            RectTransform rect = panel.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(420f, 360f);
            return panel;
        }

        private static InputField CreateInputField(Transform parent, string name, string placeholder, Vector2 anchoredPosition) {
            var root = new GameObject(name, typeof(RectTransform));
            root.transform.SetParent(parent, false);
            RectTransform rect = root.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(320f, 40f);
            rect.anchoredPosition = anchoredPosition;

            Image image = root.AddComponent<Image>();
            image.color = new Color(1f, 1f, 1f, 0.9f);
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

            Image image = root.AddComponent<Image>();
            image.color = new Color(0.2f, 0.45f, 0.8f, 1f);
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

        private static string ToAbsolute(string assetPath) {
            return Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), assetPath));
        }
    }
}
#endif

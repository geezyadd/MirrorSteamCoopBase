#if UNITY_EDITOR
using System.IO;
using Features.AddressablesConstantsGenerator.Generated;
using Features.BootstrapersModule.Scripts;
using Features.CameraModule.Scripts;
using Features.GameCoreModule.Scripts.Constants;
using Features.GameCoreModule.Scripts.Installers;
using Features.LobbyModule.Scripts;
using Features.MenuModule.Scripts;
using Features.MvpModule;
using Features.CharacterMovableModule.Scripts;
using Features.FloatingControllerModule;
using Features.GrabModule.Scripts;
using Game.Connection;
using Mirror;
using Mirror.FizzySteam;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.SceneManagement;
using UnityEngine;
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
        private const string PlayerPrefabsRoot = FeaturesRoot + "/PlayerModule/GameResources/Prefabs";
        private const string CameraModuleRoot = FeaturesRoot + "/CameraModule";
        private const string CameraPrefabsRoot = CameraModuleRoot + "/GameResources/Prefabs";
        private const string CameraResourcesRoot = CameraModuleRoot + "/GameResources/Resources";
        private const string CameraCatalogPath = CameraResourcesRoot + "/CameraCatalog.asset";
        private const string FpCameraPrefabPath = CameraPrefabsRoot + "/FPCamera.prefab";
        private const string TpCameraPrefabPath = CameraPrefabsRoot + "/TPCamera.prefab";
        private const string GrabModuleRoot = FeaturesRoot + "/GrabModule";
        private const string GrabPrefabsRoot = GrabModuleRoot + "/GameResources/Prefabs";
        private const string DummyGrabbablePrefabPath = GrabPrefabsRoot + "/DummyGrabbable.prefab";
        private const string MenuPrefabsRoot = FeaturesRoot + "/MenuModule/GameResources/Prefabs";
        private const string ConfigsRoot = FeaturesRoot + "/Connection/GameResources/Configurations";
        private const string ProjectContextPath = ResourcesRoot + "/ProjectContext.prefab";
        private const string PlayerPrefabPath = PlayerPrefabsRoot + "/Player.prefab";
        private const string DummyPlayerPrefabPath = PlayerPrefabsRoot + "/DummyPlayer.prefab";
        private const string LegacyLobbyPlayerPrefabPath = LobbyPrefabsRoot + "/LobbyPlayer.prefab";
        private const string ConnectionConfigPath = ConfigsRoot + "/ConnectionConfig_Default.asset";
        private const string MenuWindowPrefabPath = MenuPrefabsRoot + "/MenuWindow.prefab";
        private const string GameHudWindowPrefabPath = LobbyPrefabsRoot + "/GameHudWindow.prefab";
        private const string BootstrapScenePath = ScenesRoot + "/" + SceneNames.Bootstrap + ".unity";
        private const string ConfigurationsAddressableGroup = "Configurations";
        private const string LocalScenesAddressableGroup = "Scenes";
        private const string WindowsAddressableGroup = "Windows";

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
                EnsureCameraModuleAssets();
                GameObject playerPrefab = CreatePlayerPrefab();
                EnsureGrabAssets(playerPrefab);
                CreateBootstrapScene();
                CreateGlobalScene(playerPrefab);
                CreateMenuScene();
                CreateLobbyScene();
                EnsureLobbyGrabbable();
                CreateWindowPrefabs();
                StripLegacySceneUi();
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
            EnsureCameraModuleAssets();
            GameObject playerPrefab = CreatePlayerPrefab();
            EnsureGrabAssets(playerPrefab);

            CreateBootstrapScene();
            CreateGlobalScene(playerPrefab);
            CreateMenuScene();
            CreateLobbyScene();
            EnsureLobbyGrabbable();
            CreateWindowPrefabs();
            StripLegacySceneUi();
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
            EnsureFolder(FeaturesRoot + "/PlayerModule");
            EnsureFolder(FeaturesRoot + "/PlayerModule/GameResources");
            EnsureFolder(PlayerPrefabsRoot);
            EnsureFolder(CameraModuleRoot);
            EnsureFolder(CameraModuleRoot + "/GameResources");
            EnsureFolder(CameraPrefabsRoot);
            EnsureFolder(CameraResourcesRoot);
            EnsureFolder(FeaturesRoot + "/MenuModule");
            EnsureFolder(FeaturesRoot + "/MenuModule/GameResources");
            EnsureFolder(MenuPrefabsRoot);
            EnsureFolder(FeaturesRoot + "/Connection");
            EnsureFolder(FeaturesRoot + "/Connection/GameResources");
            EnsureFolder(ConfigsRoot);
            EnsureFolder(GrabModuleRoot);
            EnsureFolder(GrabModuleRoot + "/GameResources");
            EnsureFolder(GrabPrefabsRoot);
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
            GameObject dummy = AssetDatabase.LoadAssetAtPath<GameObject>(DummyPlayerPrefabPath);
            if (dummy != null) {
                EnsurePlayerCameraRig(dummy);
                return dummy;
            }

            GameObject existing = AssetDatabase.LoadAssetAtPath<GameObject>(PlayerPrefabPath);
            if (existing != null) {
                EnsurePlayerCameraRig(existing);
                return existing;
            }

            if (AssetDatabase.LoadAssetAtPath<GameObject>(LegacyLobbyPlayerPrefabPath) != null) {
                EnsureFolder(PlayerPrefabsRoot);
                AssetDatabase.MoveAsset(LegacyLobbyPlayerPrefabPath, PlayerPrefabPath);
                GameObject moved = AssetDatabase.LoadAssetAtPath<GameObject>(PlayerPrefabPath);
                if (moved != null) {
                    moved.name = "Player";
                    EditorUtility.SetDirty(moved);
                    return moved;
                }
            }

            var player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            player.name = "Player";

            CapsuleCollider capsuleCollider = player.GetComponent<CapsuleCollider>();
            capsuleCollider.height = 2f;
            capsuleCollider.center = new Vector3(0f, 1f, 0f);
            capsuleCollider.radius = 0.4f;

            Rigidbody rigidbody = player.AddComponent<Rigidbody>();
            rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
            rigidbody.collisionDetectionMode = CollisionDetectionMode.Continuous;
            rigidbody.constraints = RigidbodyConstraints.FreezeRotation;

            player.AddComponent<NetworkIdentity>();
            NetworkRigidbodyUnreliable networkRigidbody = player.AddComponent<NetworkRigidbodyUnreliable>();
            networkRigidbody.syncDirection = SyncDirection.ClientToServer;

            CharacterMovable movable = player.AddComponent<CharacterMovable>();
            movable.syncDirection = SyncDirection.ClientToServer;
            CharacterMovableRegistrar registrar = player.AddComponent<CharacterMovableRegistrar>();
            FloatingController floatingController = player.AddComponent<FloatingController>();
            PlayerCameraAnchor cameraAnchor = player.AddComponent<PlayerCameraAnchor>();
            ZenAutoInjecter autoInjecter = player.AddComponent<ZenAutoInjecter>();
            autoInjecter.ContainerSource = ZenAutoInjecter.ContainerSources.SearchHierarchy;

            SerializedObject movableSerialized = new SerializedObject(movable);
            movableSerialized.FindProperty("_rb").objectReferenceValue = rigidbody;
            movableSerialized.FindProperty("_capsuleCollider").objectReferenceValue = capsuleCollider;
            movableSerialized.FindProperty("_floatingController").objectReferenceValue = floatingController;
            Transform rotatablePart = player.transform.Find("RotatablePart");
            if (rotatablePart != null)
                movableSerialized.FindProperty("_rotatablePart").objectReferenceValue = rotatablePart;
            movableSerialized.ApplyModifiedPropertiesWithoutUndo();

            SerializedObject registrarSerialized = new SerializedObject(registrar);
            registrarSerialized.FindProperty("_movable").objectReferenceValue = movable;
            registrarSerialized.ApplyModifiedPropertiesWithoutUndo();

            SerializedObject floatingSerialized = new SerializedObject(floatingController);
            floatingSerialized.FindProperty("_rb").objectReferenceValue = rigidbody;
            floatingSerialized.FindProperty("_capsuleCollider").objectReferenceValue = capsuleCollider;
            floatingSerialized.ApplyModifiedPropertiesWithoutUndo();

            AssignPlayerCameraAnchor(player, cameraAnchor);
            EnsurePlayerGrabController(player);

            EnsureFolder(PlayerPrefabsRoot);
            PrefabUtility.SaveAsPrefabAsset(player, PlayerPrefabPath);
            Object.DestroyImmediate(player);
            return AssetDatabase.LoadAssetAtPath<GameObject>(PlayerPrefabPath);
        }

        private static void EnsureCameraModuleAssets() {
            EnsureFolder(CameraPrefabsRoot);
            EnsureFolder(CameraResourcesRoot);

            GameObject fpPrefab = CreateCinemachineCameraPrefab(FpCameraPrefabPath, CameraIds.FPCamera, GameCameraKind.FirstPerson);
            GameObject tpPrefab = CreateCinemachineCameraPrefab(TpCameraPrefabPath, CameraIds.TPCamera, GameCameraKind.ThirdPerson);

            CameraCatalog catalog = AssetDatabase.LoadAssetAtPath<CameraCatalog>(CameraCatalogPath);
            if (catalog == null) {
                catalog = ScriptableObject.CreateInstance<CameraCatalog>();
                AssetDatabase.CreateAsset(catalog, CameraCatalogPath);
            }

            GameCamera fpCamera = fpPrefab != null ? fpPrefab.GetComponent<GameCamera>() : null;
            GameCamera tpCamera = tpPrefab != null ? tpPrefab.GetComponent<GameCamera>() : null;
            SerializedObject catalogSerialized = new SerializedObject(catalog);
            SerializedProperty startup = catalogSerialized.FindProperty("_startupCameraId");
            SerializedProperty blend = catalogSerialized.FindProperty("_defaultBlendSeconds");
            SerializedProperty fpProperty = catalogSerialized.FindProperty("_fpCameraPrefab");
            SerializedProperty tpProperty = catalogSerialized.FindProperty("_tpCameraPrefab");
            bool catalogDirty =
                startup.stringValue != CameraIds.TPCamera ||
                Mathf.Approximately(blend.floatValue, 0.45f) == false ||
                fpProperty.objectReferenceValue != fpCamera ||
                tpProperty.objectReferenceValue != tpCamera;
            if (catalogDirty == false)
                return;

            startup.stringValue = CameraIds.TPCamera;
            blend.floatValue = 0.45f;
            fpProperty.objectReferenceValue = fpCamera;
            tpProperty.objectReferenceValue = tpCamera;
            catalogSerialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(catalog);
        }

        private static GameObject CreateCinemachineCameraPrefab(string path, string id, GameCameraKind kind) {
            GameObject existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (existing != null) {
                bool needsRebuild = kind == GameCameraKind.ThirdPerson &&
                    existing.GetComponent<Unity.Cinemachine.CinemachineRotationComposer>() != null;
                if (needsRebuild == false)
                    return existing;

                AssetDatabase.DeleteAsset(path);
            }

            GameCamera camera = GameCamera.Create(id, kind, null);
            GameObject created = camera.gameObject;
            PrefabUtility.SaveAsPrefabAsset(created, path);
            Object.DestroyImmediate(created);
            return AssetDatabase.LoadAssetAtPath<GameObject>(path);
        }

        private static void EnsurePlayerCameraRig(GameObject prefabAsset) {
            if (prefabAsset == null)
                return;

            string path = AssetDatabase.GetAssetPath(prefabAsset);
            if (string.IsNullOrEmpty(path))
                return;

            GameObject root = PrefabUtility.LoadPrefabContents(path);
            try {
                bool dirty = false;
                MonoBehaviour[] behaviours = root.GetComponents<MonoBehaviour>();
                for (int i = 0; i < behaviours.Length; i++) {
                    MonoBehaviour behaviour = behaviours[i];
                    if (behaviour == null || behaviour.GetType().Name != "PlayerLocalCameraFollow")
                        continue;

                    Object.DestroyImmediate(behaviour);
                    dirty = true;
                }

                PlayerCameraAnchor anchor = root.GetComponent<PlayerCameraAnchor>();
                if (anchor == null) {
                    anchor = root.AddComponent<PlayerCameraAnchor>();
                    dirty = true;
                }

                if (root.transform.Find("CameraFollow") == null ||
                    root.transform.Find("CameraLookAt") == null ||
                    root.transform.Find("CameraEye") == null)
                    dirty = true;

                if (dirty == false)
                    return;

                AssignPlayerCameraAnchor(root, anchor);
                PrefabUtility.SaveAsPrefabAsset(root, path);
            }
            finally {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        private static void EnsureGrabAssets(GameObject playerPrefab) {
            EnsureFolders();
            EnsureInteractableLayer();
            EnsurePlayerGrab(playerPrefab);
            GameObject itemPrefab = EnsureDummyGrabbablePrefab();
            RegisterSpawnPrefab(itemPrefab);
            EnsureLobbyGrabbable(itemPrefab);
        }

        private static void EnsureInteractableLayer() {
            UnityEngine.Object tagManager = AssetDatabase.LoadMainAssetAtPath("ProjectSettings/TagManager.asset");
            if (tagManager == null)
                return;

            SerializedObject tags = new SerializedObject(tagManager);
            SerializedProperty layers = tags.FindProperty("layers");
            if (layers == null)
                return;

            for (int i = 0; i < layers.arraySize; i++) {
                if (layers.GetArrayElementAtIndex(i).stringValue == InteractableLayers.Name)
                    return;
            }

            for (int i = 8; i < layers.arraySize; i++) {
                SerializedProperty layer = layers.GetArrayElementAtIndex(i);
                if (string.IsNullOrEmpty(layer.stringValue) == false)
                    continue;

                layer.stringValue = InteractableLayers.Name;
                tags.ApplyModifiedPropertiesWithoutUndo();
                return;
            }
        }

        private static void EnsurePlayerGrab(GameObject prefabAsset) {
            if (prefabAsset == null)
                return;

            string path = AssetDatabase.GetAssetPath(prefabAsset);
            if (string.IsNullOrEmpty(path))
                return;

            GameObject root = PrefabUtility.LoadPrefabContents(path);
            try {
                if (EnsurePlayerGrabController(root) == false)
                    return;

                PrefabUtility.SaveAsPrefabAsset(root, path);
            }
            finally {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        private static bool EnsurePlayerGrabController(GameObject player) {
            Transform armPoint = FindNamedChild(player.transform, "ArmPoint");
            if (armPoint == null)
                armPoint = EnsureChild(player.transform, "ArmPoint", new Vector3(0.183f, 1.179f, 0.728f));

            GrabController grab = player.GetComponent<GrabController>();
            bool dirty = false;
            if (grab == null) {
                grab = player.AddComponent<GrabController>();
                dirty = true;
            }

            int interactableBit = LayerMask.GetMask(InteractableLayers.Name);
            SerializedObject serialized = new SerializedObject(grab);
            SerializedProperty armProperty = serialized.FindProperty("_armPoint");
            SerializedProperty maskProperty = serialized.FindProperty("_interactableMask");
            SerializedProperty rangeProperty = serialized.FindProperty("_range");
            if (armProperty.objectReferenceValue != armPoint) {
                armProperty.objectReferenceValue = armPoint;
                dirty = true;
            }

            if (maskProperty.intValue != interactableBit) {
                maskProperty.intValue = interactableBit;
                dirty = true;
            }

            if (Mathf.Approximately(rangeProperty.floatValue, 4f) == false) {
                rangeProperty.floatValue = 4f;
                dirty = true;
            }

            if (dirty)
                serialized.ApplyModifiedPropertiesWithoutUndo();

            return dirty;
        }

        private static Transform FindNamedChild(Transform parent, string childName) {
            if (parent.name == childName)
                return parent;

            for (int i = 0; i < parent.childCount; i++) {
                Transform found = FindNamedChild(parent.GetChild(i), childName);
                if (found != null)
                    return found;
            }

            return null;
        }

        private static GameObject EnsureDummyGrabbablePrefab() {
            EnsureFolder(GrabPrefabsRoot);
            GameObject existing = AssetDatabase.LoadAssetAtPath<GameObject>(DummyGrabbablePrefabPath);
            if (existing != null)
                return existing;

            GameObject item = GameObject.CreatePrimitive(PrimitiveType.Cube);
            item.name = "DummyGrabbable";
            item.transform.localScale = new Vector3(0.35f, 0.35f, 0.35f);
            int interactableLayer = LayerMask.NameToLayer(InteractableLayers.Name);
            if (interactableLayer >= 0)
                item.layer = interactableLayer;

            Rigidbody rigidbody = item.AddComponent<Rigidbody>();
            rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
            rigidbody.collisionDetectionMode = CollisionDetectionMode.Continuous;
            rigidbody.mass = 1f;

            item.AddComponent<NetworkIdentity>();
            NetworkRigidbodyUnreliable networkBody = item.AddComponent<NetworkRigidbodyUnreliable>();
            networkBody.syncDirection = SyncDirection.ServerToClient;
            networkBody.target = item.transform;

            Outline outline = item.AddComponent<Outline>();
            outline.enabled = false;
            outline.OutlineMode = Outline.Mode.OutlineVisible;
            outline.OutlineColor = Color.yellow;
            outline.OutlineWidth = 4f;

            Grabbable grabbable = item.AddComponent<Grabbable>();
            SerializedObject serialized = new SerializedObject(grabbable);
            serialized.FindProperty("_rb").objectReferenceValue = rigidbody;
            serialized.FindProperty("_networkBody").objectReferenceValue = networkBody;
            serialized.FindProperty("_outline").objectReferenceValue = outline;
            serialized.ApplyModifiedPropertiesWithoutUndo();

            PrefabUtility.SaveAsPrefabAsset(item, DummyGrabbablePrefabPath);
            Object.DestroyImmediate(item);
            return AssetDatabase.LoadAssetAtPath<GameObject>(DummyGrabbablePrefabPath);
        }

        private static void RegisterSpawnPrefab(GameObject prefab) {
            if (prefab == null)
                return;

            string scenePath = ScenesRoot + "/" + SceneNames.Global + ".unity";
            if (File.Exists(ToAbsolute(scenePath)) == false)
                return;

            Scene scene = OpenSceneIfNeeded(scenePath, out bool openedAdditive);
            try {
                ConnectionNetworkManager networkManager = FindInScene<ConnectionNetworkManager>(scene);
                if (networkManager == null)
                    return;

                SerializedObject serialized = new SerializedObject(networkManager);
                SerializedProperty list = serialized.FindProperty("spawnPrefabs");
                for (int i = 0; i < list.arraySize; i++) {
                    if (list.GetArrayElementAtIndex(i).objectReferenceValue == prefab)
                        return;
                }

                list.arraySize += 1;
                list.GetArrayElementAtIndex(list.arraySize - 1).objectReferenceValue = prefab;
                serialized.ApplyModifiedPropertiesWithoutUndo();
                EditorSceneManager.SaveScene(scene);
            }
            finally {
                if (openedAdditive)
                    EditorSceneManager.CloseScene(scene, true);
            }
        }

        private static void EnsureLobbyGrabbable() {
            EnsureLobbyGrabbable(AssetDatabase.LoadAssetAtPath<GameObject>(DummyGrabbablePrefabPath));
        }

        private static void EnsureLobbyGrabbable(GameObject prefab) {
            if (prefab == null)
                return;

            string scenePath = ScenesRoot + "/" + SceneNames.Lobby + ".unity";
            if (File.Exists(ToAbsolute(scenePath)) == false)
                return;

            Scene scene = OpenSceneIfNeeded(scenePath, out bool openedAdditive);
            try {
                Grabbable existing = FindInScene<Grabbable>(scene);
                GameObject instance = existing != null ? existing.gameObject : null;
                bool created = false;
                if (instance == null) {
                    instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, scene);
                    instance.name = "DummyGrabbable";
                    instance.transform.position = new Vector3(1.2f, 1.2f, 1.5f);
                    created = true;
                }

                bool assignedId = EnsureSceneIdentity(instance);
                if (created == false && assignedId == false)
                    return;

                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
            }
            finally {
                if (openedAdditive)
                    EditorSceneManager.CloseScene(scene, true);
            }
        }

        private static Scene OpenSceneIfNeeded(string scenePath, out bool openedAdditive) {
            for (int i = 0; i < SceneManager.sceneCount; i++) {
                Scene loaded = SceneManager.GetSceneAt(i);
                if (loaded.path != scenePath)
                    continue;

                openedAdditive = false;
                return loaded;
            }

            openedAdditive = true;
            return EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);
        }

        private static bool EnsureSceneIdentity(GameObject instance) {
            NetworkIdentity identity = instance.GetComponent<NetworkIdentity>();
            if (identity == null)
                return false;

            SerializedObject serialized = new SerializedObject(identity);
            SerializedProperty sceneId = serialized.FindProperty("sceneId");
            if (sceneId == null || sceneId.longValue != 0)
                return false;

            sceneId.longValue = (long)(uint)UnityEngine.Random.Range(1, int.MaxValue);
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(identity);
            return true;
        }

        private static T FindInScene<T>(Scene scene) where T : Object {
            foreach (GameObject root in scene.GetRootGameObjects()) {
                T found = root.GetComponentInChildren<T>(true);
                if (found != null)
                    return found;
            }

            return null;
        }

        private static void AssignPlayerCameraAnchor(GameObject player, PlayerCameraAnchor anchor) {
            Transform follow = EnsureChild(player.transform, "CameraFollow", new Vector3(0f, 1.55f, 0f));
            Transform lookAt = EnsureChild(player.transform, "CameraLookAt", new Vector3(0f, 1.4f, 0f));
            Transform eye = EnsureChild(player.transform, "CameraEye", new Vector3(0f, 1.65f, 0f));

            SerializedObject anchorSerialized = new SerializedObject(anchor);
            anchorSerialized.FindProperty("_follow").objectReferenceValue = follow;
            anchorSerialized.FindProperty("_lookAt").objectReferenceValue = lookAt;
            anchorSerialized.FindProperty("_eye").objectReferenceValue = eye;
            anchorSerialized.FindProperty("_startupCameraId").stringValue = CameraIds.TPCamera;
            anchorSerialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static Transform EnsureChild(Transform parent, string childName, Vector3 localPosition) {
            Transform existing = parent.Find(childName);
            if (existing != null) {
                existing.localPosition = localPosition;
                return existing;
            }

            var child = new GameObject(childName);
            child.transform.SetParent(parent, false);
            child.transform.localPosition = localPosition;
            child.transform.localRotation = Quaternion.identity;
            return child.transform;
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
            CreateSteamManager(scene);
            EditorSceneManager.SaveScene(scene, path);
        }

        private static void CreateConnectionObject(Scene scene, GameObject playerPrefab) {
            var connectionObject = new GameObject("ConnectionNetwork");
            SceneManager.MoveGameObjectToScene(connectionObject, scene);

            TelepathyTransport transport = connectionObject.AddComponent<TelepathyTransport>();
            FizzySteamworks fizzyTransport = connectionObject.AddComponent<FizzySteamworks>();
            fizzyTransport.enabled = false;
            ConnectionAuthenticator authenticator = connectionObject.AddComponent<ConnectionAuthenticator>();
            ConnectionNetworkManager networkManager = connectionObject.AddComponent<ConnectionNetworkManager>();

            SerializedObject networkSerialized = new SerializedObject(networkManager);
            networkSerialized.FindProperty("dontDestroyOnLoad").boolValue = true;
            networkSerialized.FindProperty("maxConnections").intValue = 4;
            networkSerialized.FindProperty("offlineScene").stringValue = string.Empty;
            networkSerialized.FindProperty("onlineScene").stringValue = string.Empty;
            networkSerialized.FindProperty("transport").objectReferenceValue = transport;
            networkSerialized.FindProperty("telepathyTransport").objectReferenceValue = transport;
            networkSerialized.FindProperty("fizzyTransport").objectReferenceValue = fizzyTransport;
            networkSerialized.FindProperty("authenticator").objectReferenceValue = authenticator;
            networkSerialized.FindProperty("playerPrefab").objectReferenceValue = playerPrefab;
            networkSerialized.FindProperty("autoCreatePlayer").boolValue = true;
            networkSerialized.FindProperty("lobbySceneName").stringValue = SceneNames.Lobby;
            networkSerialized.FindProperty("menuSceneName").stringValue = SceneNames.Menu;
            networkSerialized.FindProperty("persistentSceneName").stringValue = SceneNames.Global;
            networkSerialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void CreateSteamManager(Scene scene) {
            if (UnityEngine.Object.FindFirstObjectByType<SteamManager>(FindObjectsInactive.Include) != null)
                return;

            var steamObject = new GameObject("SteamManager");
            SceneManager.MoveGameObjectToScene(steamObject, scene);
            steamObject.AddComponent<SteamManager>();
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

            CreateWindowPrefabs();
        }

        private static void StripLegacySceneUi() {
            StripSceneRootObjects(ScenesRoot + "/" + SceneNames.Menu + ".unity", "Canvas");
            StripSceneRootObjects(ScenesRoot + "/" + SceneNames.Lobby + ".unity", "Canvas", "EventSystem");
        }

        private static void StripSceneRootObjects(string scenePath, params string[] rootNames) {
            if (File.Exists(ToAbsolute(scenePath)) == false)
                return;

            Scene scene = default;
            bool openedAdditive = false;
            for (int i = 0; i < SceneManager.sceneCount; i++) {
                Scene loaded = SceneManager.GetSceneAt(i);
                if (loaded.path != scenePath)
                    continue;

                scene = loaded;
                break;
            }

            if (scene.IsValid() == false) {
                scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);
                openedAdditive = true;
            }

            bool dirty = false;
            foreach (GameObject root in scene.GetRootGameObjects()) {
                bool isCanvas = root.GetComponent<Canvas>() != null;
                bool nameMatches = false;
                foreach (string rootName in rootNames) {
                    if (root.name != rootName)
                        continue;

                    nameMatches = true;
                    break;
                }

                if (isCanvas == false && nameMatches == false)
                    continue;

                Object.DestroyImmediate(root);
                dirty = true;
            }

            if (dirty)
                EditorSceneManager.SaveScene(scene);

            if (openedAdditive)
                EditorSceneManager.CloseScene(scene, true);
        }

        private static void CreateWindowPrefabs() {
            CreateMenuWindowPrefab();
            CreateGameHudWindowPrefab();
        }

        private static void CreateMenuWindowPrefab() {
            if (AssetDatabase.LoadAssetAtPath<GameObject>(MenuWindowPrefabPath) != null) {
                MarkAddressable(MenuWindowPrefabPath, nameof(MenuWindow), WindowsAddressableGroup);
                return;
            }

            var root = new GameObject(nameof(MenuWindow), typeof(RectTransform));
            StretchFullScreen(root.GetComponent<RectTransform>());
            root.AddComponent<MonoWindowInstance>();

            GameObject panel = CreateUiPanel(root.transform, "Panel");
            MenuSessionView view = panel.AddComponent<MenuSessionView>();
            InputField addressInput = CreateInputField(panel.transform, "AddressInput", "localhost or Steam lobby id", new Vector2(0f, 80f));
            Button hostButton = CreateButton(panel.transform, "HostButton", "Host", new Vector2(-120f, 20f));
            Button joinButton = CreateButton(panel.transform, "JoinButton", "Join", new Vector2(-120f, -40f));
            Button hostSteamButton = CreateButton(panel.transform, "HostSteamButton", "Host Steam", new Vector2(120f, 20f));
            Button joinSteamButton = CreateButton(panel.transform, "JoinSteamButton", "Join Steam", new Vector2(120f, -40f));
            Text statusText = CreateLabel(panel.transform, "StatusText", string.Empty, new Vector2(0f, -100f));

            SerializedObject viewSerialized = new SerializedObject(view);
            viewSerialized.FindProperty("_addressInput").objectReferenceValue = addressInput;
            viewSerialized.FindProperty("_hostButton").objectReferenceValue = hostButton;
            viewSerialized.FindProperty("_joinButton").objectReferenceValue = joinButton;
            viewSerialized.FindProperty("_hostSteamButton").objectReferenceValue = hostSteamButton;
            viewSerialized.FindProperty("_joinSteamButton").objectReferenceValue = joinSteamButton;
            viewSerialized.FindProperty("_statusText").objectReferenceValue = statusText;
            viewSerialized.ApplyModifiedPropertiesWithoutUndo();

            EnsureFolder(MenuPrefabsRoot);
            PrefabUtility.SaveAsPrefabAsset(root, MenuWindowPrefabPath);
            Object.DestroyImmediate(root);
            MarkAddressable(MenuWindowPrefabPath, nameof(MenuWindow), WindowsAddressableGroup);
        }

        private static void CreateGameHudWindowPrefab() {
            if (AssetDatabase.LoadAssetAtPath<GameObject>(GameHudWindowPrefabPath) != null) {
                MarkAddressable(GameHudWindowPrefabPath, nameof(GameHudWindow), WindowsAddressableGroup);
                return;
            }

            var root = new GameObject(nameof(GameHudWindow), typeof(RectTransform));
            StretchFullScreen(root.GetComponent<RectTransform>());
            root.AddComponent<MonoWindowInstance>();

            Button leaveButton = CreateButton(root.transform, "LeaveButton", "Leave", new Vector2(0f, 200f));
            LeaveSessionView leaveView = leaveButton.gameObject.AddComponent<LeaveSessionView>();
            SerializedObject leaveSerialized = new SerializedObject(leaveView);
            leaveSerialized.FindProperty("_button").objectReferenceValue = leaveButton;
            leaveSerialized.ApplyModifiedPropertiesWithoutUndo();

            EnsureFolder(LobbyPrefabsRoot);
            PrefabUtility.SaveAsPrefabAsset(root, GameHudWindowPrefabPath);
            Object.DestroyImmediate(root);
            MarkAddressable(GameHudWindowPrefabPath, nameof(GameHudWindow), WindowsAddressableGroup);
        }

        private static void StretchFullScreen(RectTransform rect) {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
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

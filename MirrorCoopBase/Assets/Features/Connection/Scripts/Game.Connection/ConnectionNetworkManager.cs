using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Features.SceneLoaderModule.Scripts;
using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;
using Zenject;

namespace Game.Connection
{
    [DisallowMultipleComponent]
    [AddComponentMenu("Network/Connection Network Manager")]
    public class ConnectionNetworkManager : NetworkManager
    {
        public static ConnectionNetworkManager Singleton => singleton as ConnectionNetworkManager;

        public static Vector3 SpawnPosition { get; private set; }
        public static Quaternion SpawnRotation { get; private set; } = Quaternion.identity;
        public static bool HasSpawn { get; private set; }

        public static event Action ServerStarted;
        public static event Action ClientStarted;
        public static event Action ServerStopped;
        public static event Action ClientStopped;
        public static event Action MapLoadStarted;
        public static event Action MapReady;
        public static event Action<string> MapUnloading;

        [Header("Connection Scenes")]
        [SerializeField] string lobbySceneName = "LobbyScene";
        [SerializeField] string menuSceneName = "MenuScene";
        [SerializeField] string persistentSceneName = "GlobalScene";

        [Header("Map Change")]
        [SerializeField] float everyoneReadyDelay = 0.5f;

        [Header("Transports")]
        [SerializeField] Transport telepathyTransport;
        [SerializeField] Transport fizzyTransport;

        ISceneLoaderService sceneLoader;
        ConnectionSessionModel sessionModel;

        HashSet<int> joiningConnections;
        Dictionary<NetworkConnectionToClient, bool> mapLoadedByConnection;
        string sceneToUnload;
        bool lookForReadyPlayers;
        bool returningToMenu;

        public string LobbySceneName => lobbySceneName;
        public string MenuSceneName => menuSceneName;
        public bool IsMapLoaded { get; private set; }
        public bool IsChangingMap { get; private set; }
        public bool UsesSteamTransport { get; private set; }

        public bool IsJoinable =>
            string.IsNullOrEmpty(networkSceneName) || networkSceneName == lobbySceneName;

        public static void SetSpawn(Vector3 position, Quaternion rotation)
        {
            SpawnPosition = position;
            SpawnRotation = rotation;
            HasSpawn = true;
        }

        [Inject]
        void Construct(
            ISceneLoaderService sceneLoaderService,
            ConnectionConfig connectionConfig,
            ConnectionSessionModel connectionSessionModel)
        {
            sceneLoader = sceneLoaderService;
            sessionModel = connectionSessionModel;
            RegisterInModel();

            if (connectionConfig == null)
                return;

            maxConnections = connectionConfig.MaxConnections;
            everyoneReadyDelay = connectionConfig.EveryoneReadyDelay;
        }

        public override void Awake()
        {
            ResolveTransports();
            if (transport == null)
                transport = telepathyTransport;

            offlineScene = string.Empty;
            onlineScene = string.Empty;
            base.Awake();
        }

        void ResolveTransports()
        {
            foreach (Transport candidate in GetComponents<Transport>())
            {
                if (candidate is TelepathyTransport telepathy)
                    telepathyTransport = telepathy;
                else
                    fizzyTransport = candidate;
            }

            if (telepathyTransport != null)
                telepathyTransport.enabled = UsesSteamTransport == false;
            if (fizzyTransport != null)
                fizzyTransport.enabled = UsesSteamTransport;
        }

        void ResetTransportIfIdle()
        {
            if (UsesSteamTransport == false)
                return;
            if (NetworkServer.active || NetworkClient.active)
                return;

            try
            {
                SetUseSteamTransport(false);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
            }
        }

        void OnEnable()
        {
            RegisterInModel();
        }

        void OnDisable()
        {
            if (sessionModel == null || sessionModel.NetworkManager != this)
                return;

            sessionModel.NetworkManager = null;
        }

        void Reset()
        {
            maxConnections = 4;
            dontDestroyOnLoad = true;
            autoCreatePlayer = true;
            offlineScene = string.Empty;
            onlineScene = string.Empty;
        }

        public override void OnStartServer()
        {
            base.OnStartServer();
            sceneToUnload = SceneManager.GetActiveScene().name;
            joiningConnections = new HashSet<int>();
            mapLoadedByConnection = new Dictionary<NetworkConnectionToClient, bool>();
            NetworkServer.RegisterHandler<MapLoadedMessage>(OnClientMapLoaded);
            if (SceneManager.GetActiveScene().name == lobbySceneName)
                IsMapLoaded = true;

            ServerStarted?.Invoke();
        }

        public override void OnStopServer()
        {
            NetworkServer.UnregisterHandler<MapLoadedMessage>();
            IsMapLoaded = false;
            IsChangingMap = false;
            lookForReadyPlayers = false;
            joiningConnections = null;
            mapLoadedByConnection = null;
            HasSpawn = false;
            ServerStopped?.Invoke();
            base.OnStopServer();
            ResetTransportIfIdle();
        }

        public override void OnStartClient()
        {
            base.OnStartClient();
            NetworkClient.ReplaceHandler<SceneMessage>(OnMirrorSceneMessage);
            NetworkClient.RegisterHandler<EveryoneIsReadyMessage>(OnEveryoneIsReady);
            NetworkClient.RegisterHandler<SceneChangeMessage>(OnSceneChangeMessage);
            NetworkClient.RegisterHandler<KickMessage>(_ => StopSessionAndReturnToMenu());
            NetworkClient.RegisterHandler<ReturnToLobbyMessage>(_ =>
            {
                if (NetworkServer.active)
                    ChangeMap(lobbySceneName);
            });
            NetworkClient.RegisterHandler<TeleportMessage>(OnTeleportMessage);
            if (SceneManager.GetActiveScene().name == lobbySceneName)
                IsMapLoaded = true;

            ClientStarted?.Invoke();
        }

        public override void OnStopClient()
        {
            NetworkClient.UnregisterHandler<SceneMessage>();
            NetworkClient.UnregisterHandler<EveryoneIsReadyMessage>();
            NetworkClient.UnregisterHandler<SceneChangeMessage>();
            NetworkClient.UnregisterHandler<KickMessage>();
            NetworkClient.UnregisterHandler<ReturnToLobbyMessage>();
            NetworkClient.UnregisterHandler<TeleportMessage>();
            lookForReadyPlayers = false;
            loadingSceneAsync = null;
            networkSceneName = string.Empty;
            sceneToUnload = string.Empty;
            IsMapLoaded = false;
            IsChangingMap = false;
            ClientStopped?.Invoke();
            base.OnStopClient();
            ResetTransportIfIdle();
        }

        public override void OnServerConnect(NetworkConnectionToClient conn)
        {
            base.OnServerConnect(conn);
            joiningConnections?.Add(conn.connectionId);
        }

        public override void OnServerReady(NetworkConnectionToClient conn)
        {
            base.OnServerReady(conn);
            joiningConnections?.Remove(conn.connectionId);
        }

        public override void OnServerDisconnect(NetworkConnectionToClient conn)
        {
            joiningConnections?.Remove(conn.connectionId);
            mapLoadedByConnection?.Remove(conn);
            base.OnServerDisconnect(conn);
            if (lookForReadyPlayers)
                CheckIfEverybodyIsReady();
        }

        public override void OnServerAddPlayer(NetworkConnectionToClient conn)
        {
            Transform start = HasSpawn ? null : GetStartPosition();
            Vector3 position = HasSpawn ? SpawnPosition : (start != null ? start.position : Vector3.zero);
            Quaternion rotation = HasSpawn ? SpawnRotation : (start != null ? start.rotation : Quaternion.identity);

            GameObject player = Instantiate(playerPrefab, position, rotation);
            player.name = $"{playerPrefab.name} [connId={conn.connectionId}]";
            NetworkServer.AddPlayerForConnection(conn, player);
            if (mapLoadedByConnection != null)
                mapLoadedByConnection[conn] = false;
        }

        public override void OnClientDisconnect()
        {
            base.OnClientDisconnect();
            if (!returningToMenu)
                StopSessionAndReturnToMenu();
        }

        public async Task PrepareLobbySceneAsync()
        {
            if (string.IsNullOrEmpty(lobbySceneName))
                return;

            await LoadAddressableSceneAsync(lobbySceneName);

            Scene lobby = SceneManager.GetSceneByName(lobbySceneName);
            if (lobby.IsValid())
                SceneManager.SetActiveScene(lobby);
        }

        public async Task UnloadSceneIfLoadedAsync(string sceneName)
        {
            if (string.IsNullOrEmpty(sceneName) || sceneLoader == null)
                return;

            await sceneLoader.UnloadSceneAsync(sceneName);
        }

        public IEnumerator PrepareLobbyScene()
        {
            yield return AwaitTask(PrepareLobbySceneAsync());
        }

        public IEnumerator UnloadSceneIfLoaded(string sceneName)
        {
            yield return AwaitTask(UnloadSceneIfLoadedAsync(sceneName));
        }

        public bool CanStartMap()
        {
            if (IsChangingMap)
                return false;

            ConnectionAuthenticator authenticator = this.authenticator as ConnectionAuthenticator;
            if (authenticator != null && authenticator.JoiningIds.Count > 0)
                return false;

            if (joiningConnections != null && joiningConnections.Count > 0)
                return false;

            if (mapLoadedByConnection == null)
                return false;

            foreach (NetworkConnectionToClient conn in NetworkServer.connections.Values)
            {
                if (!mapLoadedByConnection.ContainsKey(conn))
                    return false;
            }

            return true;
        }

        public void ChangeMap(string newSceneName)
        {
            if (!NetworkServer.active)
            {
                Debug.LogError("ChangeMap can only be called on the server.");
                return;
            }

            if (string.IsNullOrWhiteSpace(newSceneName))
            {
                Debug.LogError("ChangeMap empty scene name.");
                return;
            }

            if (NetworkServer.isLoadingScene && newSceneName == networkSceneName)
            {
                Debug.LogError($"Scene change is already in progress for {newSceneName}");
                return;
            }

            NetworkServer.isLoadingScene = true;
            networkSceneName = newSceneName;
            lookForReadyPlayers = true;
            IsChangingMap = true;
            MapLoadStarted?.Invoke();
            NetworkServer.SendToAll(new SceneChangeMessage
            {
                sceneName = newSceneName,
                operation = SceneOperation.LoadAdditive
            });
        }

        public bool ReturnToLobby()
        {
            if (!NetworkServer.active)
                return false;

            if (SceneManager.GetActiveScene().name == lobbySceneName)
                return false;

            NetworkServer.SendToAll(new ReturnToLobbyMessage());
            return true;
        }

        public void Kick(NetworkConnectionToClient conn)
        {
            if (conn == null)
                return;

            conn.Send(new KickMessage());
            StartCoroutine(DisconnectAfterDelay(conn, 0.5f));
        }

        public void SetUseSteamTransport(bool useSteam)
        {
            Transport selected = useSteam ? fizzyTransport : telepathyTransport;
            if (selected == null)
            {
                throw new InvalidOperationException(useSteam
                    ? "FizzySteamworks transport is missing on ConnectionNetwork."
                    : "Telepathy transport is missing on ConnectionNetwork.");
            }

            UsesSteamTransport = useSteam;
            if (telepathyTransport != null)
                telepathyTransport.enabled = useSteam == false;
            if (fizzyTransport != null)
                fizzyTransport.enabled = useSteam;

            transport = selected;
            Transport.active = selected;
        }

        public void StopSession()
        {
            if (returningToMenu)
                return;

            returningToMenu = true;
            StopHost();
            IsMapLoaded = false;
            returningToMenu = false;
        }

        public void StopSessionAndReturnToMenu()
        {
            if (returningToMenu)
                return;

            returningToMenu = true;
            StopHost();
            IsMapLoaded = false;
            StartCoroutine(ReturnToMenuRoutine());
        }

        void OnMirrorSceneMessage(SceneMessage msg)
        {
            StartCoroutine(HandleMirrorSceneMessage(msg));
        }

        IEnumerator HandleMirrorSceneMessage(SceneMessage msg)
        {
            if (NetworkServer.active)
                yield break;

            if (msg.sceneOperation == SceneOperation.UnloadAdditive)
            {
                yield return UnloadSceneIfLoaded(msg.sceneName);
                NetworkClient.isLoadingScene = false;
                yield break;
            }

            Scene existing = SceneManager.GetSceneByName(msg.sceneName);
            if (existing.IsValid() && existing.isLoaded)
            {
                SceneManager.SetActiveScene(existing);
                NetworkClient.isLoadingScene = false;
                OnClientSceneChanged();
                yield break;
            }

            NetworkClient.isLoadingScene = true;
            yield return LoadAddressableScene(msg.sceneName);

            Scene loaded = SceneManager.GetSceneByName(msg.sceneName);
            if (loaded.IsValid())
                SceneManager.SetActiveScene(loaded);

            NetworkClient.isLoadingScene = false;
            OnClientSceneChanged();
        }

        void OnSceneChangeMessage(SceneChangeMessage msg)
        {
            StartCoroutine(HandleSceneChange(msg));
        }

        IEnumerator HandleSceneChange(SceneChangeMessage msg)
        {
            if (msg.operation == SceneOperation.LoadAdditive)
            {
                IsMapLoaded = false;
                HasSpawn = false;
                yield return new WaitForSeconds(0.1f);
                MapUnloading?.Invoke(SceneManager.GetActiveScene().name);
            }

            switch (msg.operation)
            {
                case SceneOperation.LoadAdditive:
                    if (!SceneManager.GetSceneByName(msg.sceneName).IsValid())
                    {
                        NetworkClient.isLoadingScene = true;
                        yield return LoadAddressableScene(msg.sceneName);

                        Scene loaded = SceneManager.GetSceneByName(msg.sceneName);
                        if (!loaded.IsValid())
                        {
                            NetworkClient.isLoadingScene = false;
                            StopSessionAndReturnToMenu();
                            yield break;
                        }

                        SceneManager.SetActiveScene(loaded);
                        NetworkClient.isLoadingScene = false;
                    }

                    if (NetworkServer.active)
                        NetworkServer.SpawnObjects();

                    NetworkClient.Send(new MapLoadedMessage());
                    break;

                case SceneOperation.UnloadAdditive:
                    yield return UnloadSceneIfLoaded(msg.sceneName);
                    Scene leftover = SceneManager.GetSceneByName(msg.sceneName);
                    if (leftover.IsValid() && leftover.isLoaded)
                        yield return SceneManager.UnloadSceneAsync(leftover);
                    break;
            }
        }

        void OnClientMapLoaded(NetworkConnectionToClient conn, MapLoadedMessage _)
        {
            if (mapLoadedByConnection == null)
                return;

            mapLoadedByConnection[conn] = true;
            if (lookForReadyPlayers)
                CheckIfEverybodyIsReady();
        }

        void CheckIfEverybodyIsReady()
        {
            foreach (NetworkConnectionToClient conn in NetworkServer.connections.Values)
            {
                if (!mapLoadedByConnection.TryGetValue(conn, out bool loaded) || !loaded)
                    return;
            }

            foreach (NetworkConnectionToClient conn in NetworkServer.connections.Values.ToArray())
                mapLoadedByConnection[conn] = false;

            TeleportEverybodyToSpawn();
            StartCoroutine(SendEveryoneReady());
        }

        IEnumerator SendEveryoneReady()
        {
            yield return new WaitForSeconds(everyoneReadyDelay);
            NetworkServer.SendToAll(new EveryoneIsReadyMessage { sceneToUnload = sceneToUnload });
            sceneToUnload = networkSceneName;
        }

        void OnEveryoneIsReady(EveryoneIsReadyMessage ready)
        {
            if (NetworkServer.active && !string.IsNullOrEmpty(ready.sceneToUnload))
            {
                NetworkServer.SendToAll(new SceneChangeMessage
                {
                    sceneName = ready.sceneToUnload,
                    operation = SceneOperation.UnloadAdditive
                });
            }

            StartCoroutine(FinishMapReady());
        }

        IEnumerator FinishMapReady()
        {
            yield return new WaitForSeconds(everyoneReadyDelay);
            IsMapLoaded = true;
            IsChangingMap = false;
            lookForReadyPlayers = false;
            if (NetworkServer.active)
                NetworkServer.isLoadingScene = false;
            MapReady?.Invoke();
        }

        void TeleportEverybodyToSpawn()
        {
            Transform start = HasSpawn ? null : GetStartPosition();
            Vector3 position = HasSpawn ? SpawnPosition : (start != null ? start.position : Vector3.zero);
            Quaternion rotation = HasSpawn ? SpawnRotation : (start != null ? start.rotation : Quaternion.identity);

            foreach (NetworkConnectionToClient conn in NetworkServer.connections.Values)
            {
                if (conn.identity == null)
                    continue;

                NetworkServer.SendToReady(new TeleportMessage
                {
                    netId = conn.identity.netId,
                    position = position,
                    rotation = rotation
                });
            }
        }

        static void OnTeleportMessage(TeleportMessage msg)
        {
            if (!NetworkClient.spawned.TryGetValue(msg.netId, out NetworkIdentity identity) || identity == null)
                return;

            ApplyTeleport(identity.transform, msg.position, msg.rotation);
        }

        static void ApplyTeleport(Transform target, Vector3 position, Quaternion rotation)
        {
            CharacterController controller = target.GetComponent<CharacterController>();
            if (controller != null)
                controller.enabled = false;

            Rigidbody rigidbody = target.GetComponent<Rigidbody>();
            if (rigidbody != null)
            {
                rigidbody.position = position;
                rigidbody.rotation = rotation;
                rigidbody.linearVelocity = Vector3.zero;
                rigidbody.angularVelocity = Vector3.zero;
            }

            target.SetPositionAndRotation(position, rotation);

            if (controller != null)
                controller.enabled = true;
        }

        static IEnumerator DisconnectAfterDelay(NetworkConnectionToClient conn, float delay)
        {
            yield return new WaitForSeconds(delay);
            conn?.Disconnect();
        }

        IEnumerator ReturnToMenuRoutine()
        {
            yield return null;

            if (!string.IsNullOrEmpty(menuSceneName))
            {
                yield return LoadAddressableScene(menuSceneName);
                Scene menu = SceneManager.GetSceneByName(menuSceneName);
                if (menu.IsValid())
                    SceneManager.SetActiveScene(menu);
            }

            if (!string.IsNullOrEmpty(lobbySceneName))
                yield return UnloadSceneIfLoaded(lobbySceneName);

            for (int i = SceneManager.sceneCount - 1; i >= 0; i--)
            {
                Scene scene = SceneManager.GetSceneAt(i);
                if (scene.IsValid() == false)
                    continue;
                if (scene.name == menuSceneName || scene.name == persistentSceneName || scene.name == "DontDestroyOnLoad")
                    continue;

                yield return UnloadSceneIfLoaded(scene.name);
            }

            returningToMenu = false;
        }

        IEnumerator LoadAddressableScene(string sceneName)
        {
            yield return AwaitTask(LoadAddressableSceneAsync(sceneName));
        }

        async Task LoadAddressableSceneAsync(string sceneName)
        {
            if (string.IsNullOrEmpty(sceneName))
                return;

            if (sceneLoader == null)
            {
                Debug.LogError("ConnectionNetworkManager: ISceneLoaderService is missing.");
                return;
            }

            await sceneLoader.LoadSceneAsync(sceneName, false);
        }

        void RegisterInModel()
        {
            if (sessionModel == null)
                return;

            sessionModel.NetworkManager = this;
        }

        static IEnumerator AwaitTask(Task task)
        {
            while (task != null && task.IsCompleted == false)
                yield return null;

            if (task is { IsFaulted: true })
                Debug.LogException(task.Exception?.InnerException ?? task.Exception);
        }
    }
}

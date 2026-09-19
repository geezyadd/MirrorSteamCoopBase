using System.Collections;
using System.Collections.Generic;
using Mirror;
using Steamworks;
using UnityEngine;

namespace Game.Connection
{
    [AddComponentMenu("Network/ Authenticators/Connection Authenticator")]
    public class ConnectionAuthenticator : NetworkAuthenticator
    {
        public struct AuthRequestMessage : NetworkMessage
        {
            public ulong steamId;
            public string playerName;
            public string version;
        }

        public struct AuthResponseMessage : NetworkMessage
        {
            public byte rejection;
        }

        [Header("Client")]
        [SerializeField] string playerName = "Player";

        public readonly HashSet<int> JoiningIds = new HashSet<int>();
        readonly HashSet<NetworkConnection> pendingDisconnects = new HashSet<NetworkConnection>();

        public string PlayerName
        {
            get => playerName;
            set => playerName = value;
        }

        public override void OnStartServer()
        {
            JoiningIds.Clear();
            NetworkServer.RegisterHandler<AuthRequestMessage>(OnAuthRequestMessage, requireAuthentication: false);
        }

        public override void OnStopServer()
        {
            NetworkServer.UnregisterHandler<AuthRequestMessage>();
            JoiningIds.Clear();
        }

        public override void OnServerAuthenticate(NetworkConnectionToClient conn)
        {
            JoiningIds.Add(conn.connectionId);
        }

        void OnAuthRequestMessage(NetworkConnectionToClient conn, AuthRequestMessage msg)
        {
            if (pendingDisconnects.Contains(conn))
                return;

            if (Application.version != msg.version)
            {
                Reject(conn, ConnectionRejectReason.VersionMismatch);
                return;
            }

            ConnectionNetworkManager networkManager = NetworkManager.singleton as ConnectionNetworkManager;
            if (networkManager != null && networkManager.UsesSteamTransport && ((CSteamID)msg.steamId).IsValid() == false)
            {
                Reject(conn, ConnectionRejectReason.InvalidSteamId);
                return;
            }

            if (networkManager != null && !networkManager.IsJoinable)
            {
                Reject(conn, ConnectionRejectReason.MapAlreadyStarted);
                return;
            }

            conn.authenticationData = msg;
            conn.Send(new AuthResponseMessage { rejection = (byte)ConnectionRejectReason.Accepted });
            ServerAccept(conn);
            JoiningIds.Remove(conn.connectionId);
        }

        void Reject(NetworkConnectionToClient conn, ConnectionRejectReason reason)
        {
            pendingDisconnects.Add(conn);
            conn.Send(new AuthResponseMessage { rejection = (byte)reason });
            conn.isAuthenticated = false;
            StartCoroutine(DelayedDisconnect(conn));
        }

        IEnumerator DelayedDisconnect(NetworkConnectionToClient conn)
        {
            yield return new WaitForSeconds(1f);
            ServerReject(conn);
            yield return null;
            JoiningIds.Remove(conn.connectionId);
            pendingDisconnects.Remove(conn);
        }

        public override void OnStartClient()
        {
            NetworkClient.RegisterHandler<AuthResponseMessage>(OnAuthResponseMessage, requireAuthentication: false);
        }

        public override void OnStopClient()
        {
            NetworkClient.UnregisterHandler<AuthResponseMessage>();
        }

        public override void OnClientAuthenticate()
        {
            ulong steamId = 0;
            string name = string.IsNullOrWhiteSpace(playerName) ? "Player" : playerName;
            try
            {
                CSteamID id = SteamUser.GetSteamID();
                if (id.IsValid())
                {
                    steamId = id.m_SteamID;
                    string personaName = SteamFriends.GetPersonaName();
                    if (string.IsNullOrWhiteSpace(personaName) == false)
                        name = personaName;
                }
            }
            catch
            {
                // Direct IP still authenticates without Steam.
            }

            NetworkClient.Send(new AuthRequestMessage
            {
                steamId = steamId,
                playerName = name,
                version = Application.version
            });
        }

        void OnAuthResponseMessage(AuthResponseMessage msg)
        {
            ConnectionRejectReason reason = (ConnectionRejectReason)msg.rejection;
            if (reason == ConnectionRejectReason.Accepted)
            {
                ClientAccept();
                return;
            }

            Debug.LogWarning($"Connection rejected: {reason}");
            ConnectionNetworkManager.Singleton?.StopSessionAndReturnToMenu();
            if (NetworkClient.connection != null)
                ClientReject();
        }
    }
}

using UnityEngine;

namespace Game.Connection
{
    [CreateAssetMenu(fileName = "ConnectionConfig_Default", menuName = "Game/Connection Config")]
    public sealed class ConnectionConfig : ScriptableObject
    {
        [SerializeField] int maxConnections = 4;
        [SerializeField] float everyoneReadyDelay = 0.5f;

        public int MaxConnections => maxConnections;
        public float EveryoneReadyDelay => everyoneReadyDelay;
    }
}

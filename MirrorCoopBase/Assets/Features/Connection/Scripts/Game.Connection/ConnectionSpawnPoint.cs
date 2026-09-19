using UnityEngine;

namespace Game.Connection
{
    public class ConnectionSpawnPoint : MonoBehaviour
    {
        void Awake()
        {
            Apply();
        }

        public void Apply()
        {
            ConnectionNetworkManager.SetSpawn(transform.position, transform.rotation);
        }
    }
}

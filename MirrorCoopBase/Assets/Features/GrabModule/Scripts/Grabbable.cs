using Mirror;
using UnityEngine;

namespace Features.GrabModule.Scripts {
    public sealed class Grabbable : NetworkBehaviour {
        [SerializeField] private Rigidbody _rb;
        [SerializeField] private NetworkRigidbodyUnreliable _networkBody;
        [SerializeField] private Outline _outline;

        [SyncVar(hook = nameof(OnHolderChanged))]
        private uint _holderNetId;

        private Transform _cachedArmPoint;

        public bool CanBeGrabbed => _holderNetId == 0;

        public override void OnStartClient() {
            ApplyHold(_holderNetId != 0);
            CacheArmPoint(_holderNetId);
        }

        public override void OnStartServer() {
            ApplyHold(_holderNetId != 0);
            CacheArmPoint(_holderNetId);
        }

        internal void SetHovered(bool hovered) {
            if (_outline == null)
                return;

            _outline.enabled = hovered && CanBeGrabbed;
        }

        internal void ServerBind(uint holderNetId) {
            if (isServer == false || _holderNetId != 0 || holderNetId == 0)
                return;

            _holderNetId = holderNetId;
        }

        internal void ServerUnbind() {
            if (isServer == false || _holderNetId == 0)
                return;

            _holderNetId = 0;
        }

        private void OnHolderChanged(uint previous, uint current) {
            ApplyHold(current != 0);
            CacheArmPoint(current);
        }

        private void LateUpdate() {
            if (_cachedArmPoint == null)
                return;

            transform.SetPositionAndRotation(_cachedArmPoint.position, _cachedArmPoint.rotation);
            if (_rb != null) {
                _rb.position = _cachedArmPoint.position;
                _rb.rotation = _cachedArmPoint.rotation;
            }
        }

        private void ApplyHold(bool held) {
            if (_rb != null) {
                _rb.linearVelocity = Vector3.zero;
                _rb.angularVelocity = Vector3.zero;
                _rb.isKinematic = held;
                _rb.detectCollisions = held == false;
            }

            if (_networkBody != null)
                _networkBody.enabled = held == false;

            if (held)
                SetHovered(false);
        }

        private void CacheArmPoint(uint holderNetId) {
            if (holderNetId == 0) {
                _cachedArmPoint = null;
                return;
            }

            if (TryResolveArmPoint(holderNetId, out Transform armPoint) == false) {
                _cachedArmPoint = null;
                return;
            }

            _cachedArmPoint = armPoint;
        }

        private static bool TryResolveArmPoint(uint holderNetId, out Transform armPoint) {
            armPoint = null;
            NetworkIdentity identity = null;
            if (NetworkServer.active && NetworkServer.spawned.TryGetValue(holderNetId, out identity) == false)
                identity = null;
            if (identity == null && NetworkClient.active && NetworkClient.spawned.TryGetValue(holderNetId, out identity) == false)
                return false;
            if (identity == null)
                return false;

            GrabController controller = identity.GetComponent<GrabController>();
            if (controller == null)
                return false;

            armPoint = controller.ArmPoint;
            return armPoint != null;
        }
    }
}

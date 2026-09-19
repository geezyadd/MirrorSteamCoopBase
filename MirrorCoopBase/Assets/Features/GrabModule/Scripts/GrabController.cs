using Features.InputModule.Realization.Scripts.Generated;
using Mirror;
using UnityEngine;
using Zenject;

namespace Features.GrabModule.Scripts {
    public sealed class GrabController : NetworkBehaviour {
        [SerializeField] private Transform _armPoint;
        [SerializeField] private LayerMask _interactableMask;
        [SerializeField] private float _range = 4f;

        [Inject]
        private IInputService _input;

        private Grabbable _held;
        private Grabbable _hovered;

        public Transform ArmPoint => _armPoint != null ? _armPoint : transform;

        public override void OnStartLocalPlayer() {
            _input.Grab.Performed += OnGrab;
            _input.Release.Performed += OnRelease;
        }

        public override void OnStopLocalPlayer() {
            if (_input != null) {
                _input.Grab.Performed -= OnGrab;
                _input.Release.Performed -= OnRelease;
            }

            ClearHover();
            if (isOwned)
                CmdRelease();
        }

        public override void OnStopServer() {
            ReleaseHeld();
        }

        [Command]
        private void CmdTryGrab(uint itemNetId) {
            if (_held != null || itemNetId == 0)
                return;

            if (NetworkServer.spawned.TryGetValue(itemNetId, out NetworkIdentity identity) == false)
                return;

            Grabbable item = identity.GetComponent<Grabbable>();
            if (item == null || item.CanBeGrabbed == false)
                return;

            Vector3 from = transform.position + Vector3.up;
            float reach = _range + 1.5f;
            if ((item.transform.position - from).sqrMagnitude > reach * reach)
                return;

            item.ServerBind(netId);
            _held = item;
        }

        [Command]
        private void CmdRelease() {
            ReleaseHeld();
        }

        private void Update() {
            if (isLocalPlayer == false)
                return;

            RefreshHover();
        }

        private void RefreshHover() {
            Grabbable next = null;
            if (_held == null && TryRaycast(out Grabbable item))
                next = item;

            if (_hovered == next)
                return;

            if (_hovered != null)
                _hovered.SetHovered(false);

            _hovered = next;
            if (_hovered != null)
                _hovered.SetHovered(true);
        }

        private void ClearHover() {
            if (_hovered == null)
                return;

            _hovered.SetHovered(false);
            _hovered = null;
        }

        private void OnGrab() {
            if (TryRaycast(out Grabbable item) == false)
                return;

            CmdTryGrab(item.netId);
        }

        private void OnRelease() {
            CmdRelease();
        }

        private void ReleaseHeld() {
            if (_held == null)
                return;

            _held.ServerUnbind();
            _held = null;
        }

        private bool TryRaycast(out Grabbable item) {
            item = null;
            Camera camera = Camera.main;
            if (camera == null)
                return false;

            Ray ray = new Ray(camera.transform.position, camera.transform.forward);
            if (Physics.Raycast(ray, out RaycastHit hit, _range, _interactableMask, QueryTriggerInteraction.Ignore) == false)
                return false;

            item = hit.collider.GetComponentInParent<Grabbable>();
            return item != null && item.CanBeGrabbed;
        }
    }
}

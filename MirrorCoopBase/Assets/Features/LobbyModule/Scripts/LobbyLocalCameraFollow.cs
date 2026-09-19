using Mirror;
using UnityEngine;

namespace Features.LobbyModule.Scripts {
    public sealed class LobbyLocalCameraFollow : NetworkBehaviour {
        [SerializeField] private Vector3 offset = new Vector3(0f, 8f, -8f);
        [SerializeField] private float followSpeed = 10f;

        private Transform cameraTransform;

        public override void OnStartLocalPlayer() {
            Camera mainCamera = Camera.main;
            if (mainCamera == null)
                return;

            cameraTransform = mainCamera.transform;
        }

        private void LateUpdate() {
            if (isLocalPlayer == false || cameraTransform == null)
                return;

            Vector3 targetPosition = transform.position + offset;
            cameraTransform.position = Vector3.Lerp(
                cameraTransform.position,
                targetPosition,
                Time.deltaTime * followSpeed);
            cameraTransform.LookAt(transform.position + Vector3.up);
        }
    }
}

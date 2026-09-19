using Features.CameraModule.Scripts.Services;
using Mirror;
using UnityEngine;
using Zenject;

namespace Features.CameraModule.Scripts {
    public sealed class PlayerCameraAnchor : NetworkBehaviour {
        [SerializeField] private Transform _follow;
        [SerializeField] private Transform _lookAt;
        [SerializeField] private Transform _eye;
        [SerializeField] private string _startupCameraId = CameraIds.TPCamera;

        [Inject]
        private IGameCameraService _cameras;

        public override void OnStartLocalPlayer() {
            Transform follow = _follow != null ? _follow : transform;
            Transform lookAt = _lookAt != null ? _lookAt : follow;
            Transform eye = _eye != null ? _eye : follow;
            _cameras.BindToLocalPlayer(follow, lookAt, eye);
            _cameras.BlendTo(string.IsNullOrEmpty(_startupCameraId) ? CameraIds.TPCamera : _startupCameraId, 0f);
        }

        public override void OnStopLocalPlayer() {
            _cameras.ClearLocalPlayer();
        }
    }
}

using UnityEngine;

namespace Features.CameraModule.Scripts {
    [CreateAssetMenu(menuName = "Game/Camera Catalog", fileName = "CameraCatalog")]
    public sealed class CameraCatalog : ScriptableObject {
        public const string ResourceName = "CameraCatalog";

        [SerializeField] private string _startupCameraId = CameraIds.TPCamera;
        [SerializeField] private float _defaultBlendSeconds = 0.45f;
        [SerializeField] private float _lookSensitivity = 0.18f;
        [SerializeField] private float _minPitch = -70f;
        [SerializeField] private float _maxPitch = 70f;
        [SerializeField] private bool _invertY;
        [SerializeField] private GameCamera _fpCameraPrefab;
        [SerializeField] private GameCamera _tpCameraPrefab;

        public string StartupCameraId =>
            string.IsNullOrEmpty(_startupCameraId) ? CameraIds.TPCamera : _startupCameraId;

        public float DefaultBlendSeconds => _defaultBlendSeconds;
        public float LookSensitivity => _lookSensitivity;
        public float MinPitch => _minPitch;
        public float MaxPitch => _maxPitch;
        public bool InvertY => _invertY;
        public GameCamera FpCameraPrefab => _fpCameraPrefab;
        public GameCamera TpCameraPrefab => _tpCameraPrefab;
    }
}

using System;
using Features.CameraModule.Scripts.Models;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.SceneManagement;
using Zenject;

namespace Features.CameraModule.Scripts.Services {
    public sealed class GameCameraService : IGameCameraService, IInitializable, IDisposable {
        private const int LivePriority = 100;
        private const int StandbyPriority = 10;

        private readonly GameCameraModel _model;
        private readonly CameraCatalog _catalog;

        private Transform _root;
        private CinemachineBrain _brain;

        public GameCameraService(GameCameraModel model, CameraCatalog catalog) {
            _model = model;
            _catalog = catalog;
        }

        public string ActiveId => _model.ActiveId;

        public void Initialize() {
            SceneManager.sceneLoaded += OnSceneLoaded;
            EnsureRoot();
            EnsureBrain();
            SpawnCameras();
        }

        public void Dispose() {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            if (_root != null)
                UnityEngine.Object.Destroy(_root.gameObject);

            _root = null;
            _brain = null;
            _model.Clear();
        }

        public void Register(GameCamera camera) {
            _model.Register(camera);
            ApplyBinding(camera);
        }

        public void Unregister(GameCamera camera) {
            _model.Unregister(camera);
        }

        public bool TryGet(string id, out GameCamera camera) {
            return _model.TryGet(id, out camera);
        }

        public void BindToLocalPlayer(Transform follow, Transform lookAt, Transform eye) {
            _model.Follow = follow;
            _model.LookAt = lookAt;
            _model.Eye = eye;

            foreach (GameCamera camera in _model.Cameras.Values)
                ApplyBinding(camera);

            if (string.IsNullOrEmpty(_model.ActiveId))
                BlendTo(_catalog != null ? _catalog.StartupCameraId : CameraIds.TPCamera, 0f);
        }

        public void ClearLocalPlayer() {
            _model.Follow = null;
            _model.LookAt = null;
            _model.Eye = null;

            foreach (GameCamera camera in _model.Cameras.Values)
                ApplyBinding(camera);
        }

        public void BlendTo(string id, float duration = -1f) {
            if (_model.TryGet(id, out GameCamera next) == false || next == null)
                return;

            EnsureBrain();
            float blendTime = duration >= 0f
                ? duration
                : _catalog != null ? _catalog.DefaultBlendSeconds : 0.45f;
            if (_brain != null) {
                _brain.DefaultBlend = blendTime <= 0.001f
                    ? new CinemachineBlendDefinition(CinemachineBlendDefinition.Styles.Cut, 0f)
                    : new CinemachineBlendDefinition(CinemachineBlendDefinition.Styles.EaseInOut, blendTime);
            }

            foreach (GameCamera camera in _model.Cameras.Values)
                camera.SetPriority(camera == next ? LivePriority : StandbyPriority);

            _model.ActiveId = id;
        }

        public void ToggleFpAndTp() {
            string next = ActiveId == CameraIds.FPCamera ? CameraIds.TPCamera : CameraIds.FPCamera;
            BlendTo(next);
        }

        private void SpawnCameras() {
            GameCamera fpPrefab = _catalog != null ? _catalog.FpCameraPrefab : null;
            GameCamera tpPrefab = _catalog != null ? _catalog.TpCameraPrefab : null;
            Spawn(CameraIds.FPCamera, GameCameraKind.FirstPerson, fpPrefab);
            Spawn(CameraIds.TPCamera, GameCameraKind.ThirdPerson, tpPrefab);
        }

        private void Spawn(string id, GameCameraKind kind, GameCamera prefab) {
            if (_model.TryGet(id, out GameCamera existing) && existing != null)
                return;

            GameCamera camera = prefab != null
                ? UnityEngine.Object.Instantiate(prefab, _root)
                : GameCamera.Create(id, kind, _root);

            camera.name = id;
            Register(camera);
        }

        private void ApplyBinding(GameCamera camera) {
            if (camera == null)
                return;

            Transform target = _model.LookPivot != null ? _model.LookPivot : _model.Follow;
            camera.Bind(target, null);
        }

        private void EnsureRoot() {
            if (_root != null)
                return;

            var rootObject = new GameObject("GameCameras");
            UnityEngine.Object.DontDestroyOnLoad(rootObject);
            _root = rootObject.transform;
            EnsureLookPivot();
        }

        private void EnsureLookPivot() {
            if (_model.LookPivot != null)
                return;

            var pivotObject = new GameObject("CameraLook");
            pivotObject.transform.SetParent(_root, false);
            pivotObject.AddComponent<CameraLookRig>();
            _model.LookPivot = pivotObject.transform;
        }

        private void EnsureBrain() {
            Camera output = Camera.main;
            if (output == null)
                return;

            if (_brain != null && _brain.gameObject == output.gameObject)
                return;

            _brain = output.GetComponent<CinemachineBrain>();
            if (_brain == null)
                _brain = output.gameObject.AddComponent<CinemachineBrain>();

            _brain.UpdateMethod = CinemachineBrain.UpdateMethods.LateUpdate;
            _brain.BlendUpdateMethod = CinemachineBrain.BrainUpdateMethods.LateUpdate;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
            EnsureBrain();
        }
    }
}

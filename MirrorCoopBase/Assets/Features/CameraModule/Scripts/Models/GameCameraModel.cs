using System.Collections.Generic;
using UnityEngine;

namespace Features.CameraModule.Scripts.Models {
    public sealed class GameCameraModel {
        private readonly Dictionary<string, GameCamera> _cameras = new();

        public string ActiveId { get; internal set; }
        public Transform Follow { get; internal set; }
        public Transform LookAt { get; internal set; }
        public Transform Eye { get; internal set; }
        public Transform LookPivot { get; internal set; }

        public IReadOnlyDictionary<string, GameCamera> Cameras => _cameras;

        public bool TryGet(string id, out GameCamera camera) {
            if (string.IsNullOrEmpty(id)) {
                camera = null;
                return false;
            }

            return _cameras.TryGetValue(id, out camera);
        }

        internal void Register(GameCamera camera) {
            if (camera == null || string.IsNullOrEmpty(camera.Id))
                return;

            _cameras[camera.Id] = camera;
        }

        internal void Unregister(GameCamera camera) {
            if (camera == null || string.IsNullOrEmpty(camera.Id))
                return;

            if (_cameras.TryGetValue(camera.Id, out GameCamera current) == false || current != camera)
                return;

            _cameras.Remove(camera.Id);
            if (ActiveId == camera.Id)
                ActiveId = null;
        }

        internal void Clear() {
            _cameras.Clear();
            ActiveId = null;
            Follow = null;
            LookAt = null;
            Eye = null;
            LookPivot = null;
        }
    }
}

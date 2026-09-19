using System;
using Features.CameraModule.Scripts.Services;
using Features.InputModule.Realization.Scripts.Generated;
using Zenject;

namespace Features.CameraModule.Scripts {
    public sealed class CameraSwitchBinder : IInitializable, IDisposable {
        private readonly IInputService _input;
        private readonly IGameCameraService _cameras;

        public CameraSwitchBinder(IInputService input, IGameCameraService cameras) {
            _input = input;
            _cameras = cameras;
        }

        public void Initialize() {
            _input.SwitchCamera.Performed += OnSwitchCamera;
        }

        public void Dispose() {
            _input.SwitchCamera.Performed -= OnSwitchCamera;
        }

        private void OnSwitchCamera() {
            _cameras.ToggleFpAndTp();
        }
    }
}

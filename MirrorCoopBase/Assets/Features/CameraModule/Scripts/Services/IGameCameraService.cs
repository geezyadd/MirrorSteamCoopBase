using UnityEngine;

namespace Features.CameraModule.Scripts.Services {
    public interface IGameCameraService {
        string ActiveId { get; }

        void Register(GameCamera camera);
        void Unregister(GameCamera camera);
        void BindToLocalPlayer(Transform follow, Transform lookAt, Transform eye);
        void ClearLocalPlayer();
        void BlendTo(string id, float duration = -1f);
        void ToggleFpAndTp();
        bool TryGet(string id, out GameCamera camera);
    }
}

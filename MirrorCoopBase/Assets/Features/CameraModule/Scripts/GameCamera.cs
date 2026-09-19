using Unity.Cinemachine;
using Unity.Cinemachine.TargetTracking;
using UnityEngine;

namespace Features.CameraModule.Scripts {
    public enum GameCameraKind {
        ThirdPerson = 0,
        FirstPerson = 1
    }

    public sealed class GameCamera : MonoBehaviour {
        [SerializeField] private string _id;
        [SerializeField] private GameCameraKind _kind;
        [SerializeField] private CinemachineCamera _virtualCamera;

        public string Id => _id;
        public GameCameraKind Kind => _kind;

        public CinemachineCamera VirtualCamera => _virtualCamera;

        public static GameCamera Create(string id, GameCameraKind kind, Transform parent) {
            var gameObject = new GameObject(id);
            if (parent != null)
                gameObject.transform.SetParent(parent, false);

            CinemachineCamera virtualCamera = gameObject.AddComponent<CinemachineCamera>();
            ApplyPipeline(gameObject, kind);

            GameCamera camera = gameObject.AddComponent<GameCamera>();
            camera._id = id;
            camera._kind = kind;
            camera._virtualCamera = virtualCamera;
            camera.SetPriority(0);
            return camera;
        }

        public void Bind(Transform follow, Transform lookAt) {
            CinemachineCamera camera = VirtualCamera;
            if (camera == null)
                return;

            CameraTarget target = camera.Target;
            target.TrackingTarget = follow;
            if (lookAt != null && lookAt != follow) {
                target.LookAtTarget = lookAt;
                target.CustomLookAtTarget = true;
            }
            else {
                target.LookAtTarget = null;
                target.CustomLookAtTarget = false;
            }

            camera.Target = target;
        }

        public void SetPriority(int value) {
            CinemachineCamera camera = VirtualCamera;
            if (camera == null)
                return;

            camera.Priority = value;
        }

        private static void ApplyPipeline(GameObject gameObject, GameCameraKind kind) {
            if (kind == GameCameraKind.FirstPerson) {
                gameObject.AddComponent<CinemachineHardLockToTarget>();
                gameObject.AddComponent<CinemachineRotateWithFollowTarget>();
                return;
            }

            CinemachineFollow follow = gameObject.AddComponent<CinemachineFollow>();
            TrackerSettings tracker = follow.TrackerSettings;
            tracker.BindingMode = BindingMode.LockToTarget;
            tracker.PositionDamping = new Vector3(0.12f, 0.18f, 0.12f);
            tracker.RotationDamping = Vector3.zero;
            tracker.QuaternionDamping = 0f;
            follow.TrackerSettings = tracker;
            follow.FollowOffset = new Vector3(0f, 0.35f, -4.4f);

            CinemachineRotateWithFollowTarget rotate = gameObject.AddComponent<CinemachineRotateWithFollowTarget>();
            rotate.Damping = 0f;
        }
    }
}

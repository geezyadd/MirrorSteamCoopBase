using Features.FloatingControllerModule;
using Mirror;
using UnityEngine;
using Zenject;

namespace Features.CharacterMovableModule.Scripts {
    public abstract class CharacterMovableBase : NetworkBehaviour {
        [System.Serializable]
        public class LocomotionTuning {
            public float walk = 5f;
            public float sprint = 8f;
            public float startRate = 40f;
            public float haltRate = 80f;
            public float haltWindow = 0.12f;
            public float airborneScale = 0.075f;
            public float maxDrive = 250f;
            public float maxBrake = 800f;
            public bool cameraRelative = true;
        }

        [System.Serializable]
        public class JumpTuning {
            public float height = 4f;
            public float cooldown = 0.1f;
            public float edgeGrace = 0.2f;
            public float pressGrace = 0.2f;
            public float extraRiseGravity = 1.2f;
            public float extraFallGravity = 3f;
            public float shortHopGravity = 2f;
            public int extraAirJumps;
        }

        [Header("Body")]
        [SerializeField] private Rigidbody _rb;
        [SerializeField] private CapsuleCollider _capsuleCollider;
        [SerializeField] private FloatingController _floatingController;
        [SerializeField] private Transform _rotatablePart;
        [SerializeField] private float _lookTurnRate = 12f;

        [Header("Locomotion")]
        [SerializeField] private LocomotionTuning _locomotion = new();
        [SerializeField] private JumpTuning _jump = new();

        [Header("Proxy Shove")]
        [SerializeField] private float _proxyReach = 0.2f;
        [SerializeField] private float _proxyPadding = 0.1f;

        [SyncVar] private float _pushStrength;
        [SyncVar] private Vector3 _pushHeading;
        [SyncVar] private bool _locomoting;
        [SyncVar] private float _facingYaw;

        private static readonly Collider[] OverlapScratch = new Collider[32];

        [Inject]
        private CharacterInputBuffer _input;
        private float _moveSpeed;
        private bool _jumpHeld;
        private bool _jumpQueued;
        private float _jumpReadyIn;
        private float _jumpPressedAgo = 999f;
        private float _airTime = 999f;
        private bool _jumpedThisAir;
        private int _airJumpsLeft;
        private Vector3 _weight;

        public Rigidbody Body => _rb;
        public float PushStrength => _pushStrength;
        public Vector3 PushHeading => _pushHeading;
        public bool Locomoting => _locomoting;

        private bool ControlsSelf => isOwned;

        protected virtual void Awake() {
            CacheWeight();
        }

        public override void OnStartLocalPlayer() {
            _moveSpeed = _locomotion.walk;
            _airJumpsLeft = _jump.extraAirJumps;
            CacheWeight();
        }

        private void Update() {
            if (ControlsSelf == false)
                return;

            _jumpPressedAgo += Time.deltaTime;
            bool supported = HasSupport();
            _airTime = supported ? 0f : _airTime + Time.deltaTime;

            bool holdingJump = _input != null && _input.JumpHeld;
            if (holdingJump && _jumpHeld == false) {
                _jumpQueued = true;
                _jumpPressedAgo = 0f;
            }

            _jumpHeld = holdingJump;
        }

        private void LateUpdate() {
            if (ControlsSelf)
                CaptureFacingYaw();

            ApplyRotatableLook();
        }

        private void FixedUpdate() {
            if (_rb == null)
                return;

            if (ControlsSelf)
                SimulateOwner();
            else if (isServer)
                ShoveNearbyBodies();
        }

        private void SimulateOwner() {
            if (_input == null)
                return;

            Vector3 planar = Flatten(_rb.linearVelocity);
            _locomoting = planar.magnitude > 0.1f;
            _moveSpeed = _input.SprintHeld ? _locomotion.sprint : _locomotion.walk;

            Vector3 wish = ReadWishDirection();
            DrivePlanar(wish, planar);
            ApplyJumpAndAirGravity();
        }

        private Vector3 ReadWishDirection() {
            Vector2 stick = _input.MoveStick;
            Vector3 wish = _locomotion.cameraRelative
                ? CameraPlanar(stick)
                : new Vector3(stick.x, 0f, stick.y);

            if (HasSupport())
                wish = Vector3.ProjectOnPlane(wish, _floatingController.SupportNormal);

            if (wish.sqrMagnitude > 1f)
                wish.Normalize();

            return wish;
        }

        private void DrivePlanar(Vector3 wish, Vector3 planar) {
            bool steering = wish.sqrMagnitude > 0.0001f;
            Vector3 desired;
            float rate;
            float forceCap;

            if (steering) {
                desired = wish * _moveSpeed;
                rate = _locomotion.startRate;
                forceCap = _locomotion.maxDrive;
            }
            else {
                float speed = planar.magnitude;
                float window = Mathf.Max(_locomotion.haltWindow, 0.01f);
                float distanceBrake = speed > 0.01f ? (speed * speed) / (2f * window) : _locomotion.haltRate;
                rate = Mathf.Max(_locomotion.haltRate, distanceBrake);
                forceCap = Mathf.Max(_locomotion.maxBrake, rate);
                desired = Vector3.zero;
            }

            if (HasSupport() == false)
                rate *= _locomotion.airborneScale;

            Vector3 next = Vector3.MoveTowards(planar, desired, rate * Time.fixedDeltaTime);
            Vector3 accel = (next - planar) / Time.fixedDeltaTime;
            accel.y = 0f;
            Vector3 force = Vector3.ClampMagnitude(accel, forceCap) * _rb.mass;
            _rb.AddForce(force);

            _pushStrength = force.magnitude;
            _pushHeading = wish;
        }

        private void ApplyJumpAndAirGravity() {
            if (_floatingController == null)
                return;

            _jumpReadyIn -= Time.fixedDeltaTime;
            bool supported = HasSupport();
            bool fromLedge = _airTime <= _jump.edgeGrace && _jumpedThisAir == false;
            bool buffered = _jumpPressedAgo <= _jump.pressGrace;
            bool canGroundJump = (supported || fromLedge) && _jumpReadyIn <= 0f;
            bool canAirJump = _airJumpsLeft > 0 && _jumpReadyIn <= 0f;

            if (_jumpQueued && buffered && (canGroundJump || canAirJump)) {
                bool groundedTakeoff = supported || fromLedge;
                if (groundedTakeoff == false)
                    _airJumpsLeft--;

                _jumpedThisAir = true;
                _jumpQueued = false;
                _jumpPressedAgo = _jump.pressGrace;
                _jumpReadyIn = _jump.cooldown;
                _floatingController.PauseSupport();

                Vector3 velocity = _rb.linearVelocity;
                if (groundedTakeoff == false)
                    velocity.y = 0f;

                float takeoff = Mathf.Sqrt(2f * _jump.height * -Physics.gravity.y);
                _rb.AddForce(new Vector3(0f, takeoff - velocity.y, 0f) * _rb.mass, ForceMode.Impulse);
            }

            if (supported && _jumpedThisAir && _rb.linearVelocity.y <= 0.05f) {
                _jumpedThisAir = false;
                _airJumpsLeft = _jump.extraAirJumps;
            }

            if (supported)
                return;

            float vy = _rb.linearVelocity.y;
            float extra = 0f;
            if (vy > 0.15f)
                extra = _jumpHeld ? _jump.extraRiseGravity : _jump.shortHopGravity;
            else if (vy < -0.15f)
                extra = _jump.extraFallGravity;

            if (extra > 0f)
                _rb.AddForce(_weight * extra);
        }

        private void ShoveNearbyBodies() {
            if (_locomoting == false || _pushStrength < 0.01f || _capsuleCollider == null)
                return;

            Vector3 shove = _pushHeading * Mathf.Max(_pushStrength, 0.1f);
            Vector3 center = _capsuleCollider.transform.TransformPoint(_capsuleCollider.center);
            float radius = _capsuleCollider.radius;
            float height = _capsuleCollider.height;
            Vector3 up = _capsuleCollider.transform.up;
            Vector3 lead = Vector3.ClampMagnitude(shove, 1f) * _proxyReach;
            if (connectionToClient != null)
                lead += lead.normalized * (float)connectionToClient.rtt * 0.5f;

            Vector3 origin = center + lead;
            Vector3 top = origin + up * (height * 0.5f - radius);
            Vector3 bottom = origin - up * (height * 0.5f - radius);
            LayerMask mask = _floatingController != null ? _floatingController.ProbeMask : Physics.DefaultRaycastLayers;
            int count = Physics.OverlapCapsuleNonAlloc(
                top,
                bottom,
                radius + _proxyPadding,
                OverlapScratch,
                mask,
                QueryTriggerInteraction.Ignore);

            for (int i = 0; i < count; i++) {
                Rigidbody other = OverlapScratch[i].attachedRigidbody;
                if (other == null || other.isKinematic || other == _rb)
                    continue;

                Vector3 contact = OverlapScratch[i].ClosestPoint(center);
                Vector3 away = (contact - center).normalized;
                if (Vector3.Dot(away, shove.normalized) <= 0.1f)
                    continue;

                Vector3 impulse = Vector3.Project(shove, away);
                if (other.linearVelocity.magnitude < impulse.magnitude)
                    other.AddForceAtPosition(impulse, contact);
            }
        }

        private void CaptureFacingYaw() {
            Transform cameraTransform = Camera.main != null ? Camera.main.transform : null;
            if (cameraTransform == null)
                return;

            Vector3 flat = cameraTransform.forward;
            flat.y = 0f;
            if (flat.sqrMagnitude < 0.0001f)
                return;

            float yaw = Quaternion.LookRotation(flat.normalized, Vector3.up).eulerAngles.y;
            if (Mathf.Abs(Mathf.DeltaAngle(_facingYaw, yaw)) < 0.25f)
                return;

            _facingYaw = yaw;
        }

        private void ApplyRotatableLook() {
            if (_rotatablePart == null)
                return;

            Quaternion target = Quaternion.Euler(0f, _facingYaw, 0f);
            float t = 1f - Mathf.Exp(-_lookTurnRate * Time.deltaTime);
            _rotatablePart.rotation = Quaternion.Slerp(_rotatablePart.rotation, target, t);
        }

        private static Vector3 CameraPlanar(Vector2 stick) {
            Transform cameraTransform = Camera.main != null ? Camera.main.transform : null;
            if (cameraTransform == null || stick.sqrMagnitude < 0.0001f)
                return Vector3.zero;

            Vector3 forward = cameraTransform.forward;
            Vector3 right = cameraTransform.right;
            forward.y = 0f;
            right.y = 0f;
            if (forward.sqrMagnitude < 0.0001f || right.sqrMagnitude < 0.0001f)
                return new Vector3(stick.x, 0f, stick.y);

            Vector3 world = forward.normalized * stick.y + right.normalized * stick.x;
            if (world.sqrMagnitude > 1f)
                world.Normalize();

            return world;
        }

        private static Vector3 Flatten(Vector3 velocity) {
            return new Vector3(velocity.x, 0f, velocity.z);
        }

        private bool HasSupport() {
            return _floatingController != null && _floatingController.HasSupport;
        }

        private void CacheWeight() {
            if (_rb != null)
                _weight = Physics.gravity * _rb.mass;
        }
    }
}

using Mirror;
using UnityEngine;

namespace Features.FloatingControllerModule {
    [DefaultExecutionOrder(-100)]
    public sealed class FloatingController : NetworkBehaviour {
        [Header("Probe")]
        [SerializeField] private Transform _probeOrigin;
        [SerializeField] private CapsuleCollider _capsuleCollider;
        [SerializeField] private Vector3 _probeLocal = Vector3.down;
        [SerializeField] private LayerMask _supportMask = ~0;
        [SerializeField] private float _probeLength = 2f;
        [SerializeField] private float _probeRadius = 0.25f;
        [SerializeField] private float _maxSlope = 45f;

        [Header("Hover")]
        [SerializeField] private float _hoverClearance = 1f;
        [SerializeField] private float _stiffness = 250f;
        [SerializeField] private float _damping = 25f;
        [SerializeField] private float _maxHoverForce = 50f;
        [SerializeField] private float _skipHoverIfFallingFasterThan = -5f;
        [SerializeField] private float _supportPause = 0.1f;

        [Header("Body")]
        [SerializeField] private Rigidbody _rb;
        [SerializeField] private bool _pushSupportBody = true;

        private RaycastHit _supportHit;
        private Vector3 _supportNormal = Vector3.up;
        private bool _hasSupport;
        private float _lockoutLeft;

        public bool HasSupport => _hasSupport && _lockoutLeft <= 0f;
        public Vector3 SupportNormal => _supportNormal;
        public LayerMask ProbeMask => _supportMask;
        public bool HoverEnabled { get; set; } = true;

        private void FixedUpdate() {
            if (isOwned == false)
                return;

            if (_lockoutLeft > 0f) {
                _lockoutLeft -= Time.fixedDeltaTime;
                _hasSupport = false;
                _supportNormal = Vector3.up;
            }
            else {
                SampleSupport();
            }

            if (HoverEnabled)
                HoldClearance();
        }

        public void PauseSupport() {
            _lockoutLeft = _supportPause;
            _hasSupport = false;
            _supportNormal = Vector3.up;
        }

        private void SampleSupport() {
            Vector3 origin = ProbeStart();
            Vector3 direction = transform.TransformDirection(_probeLocal.normalized);
            float radius = Mathf.Max(_probeRadius, 0.05f);
            float castDistance = Mathf.Max(_probeLength - radius, 0.01f);

            int hits = Physics.SphereCastNonAlloc(
                origin,
                radius,
                direction,
                _hits,
                castDistance,
                _supportMask,
                QueryTriggerInteraction.Ignore);

            float nearest = float.MaxValue;
            bool found = false;
            for (int i = 0; i < hits; i++) {
                RaycastHit candidate = _hits[i];
                if (BelongsToSelf(candidate) || candidate.distance >= nearest)
                    continue;

                nearest = candidate.distance;
                _supportHit = candidate;
                found = true;
            }

            if (found)
                _supportHit.distance += radius;

            if (found == false
                || Vector3.Angle(Vector3.up, _supportHit.normal) > _maxSlope
                || _supportHit.distance > _hoverClearance * 1.3f) {
                _hasSupport = false;
                _supportNormal = Vector3.up;
                return;
            }

            _hasSupport = true;
            _supportNormal = _supportHit.normal;
        }

        private void HoldClearance() {
            if (HasSupport == false || _rb == null)
                return;

            Vector3 velocity = _rb.linearVelocity;
            if (velocity.y < _skipHoverIfFallingFasterThan)
                return;

            Vector3 supportVelocity = _supportHit.rigidbody != null
                ? _supportHit.rigidbody.linearVelocity
                : Vector3.zero;
            float heightError = _hoverClearance - _supportHit.distance;
            float closingSpeed = Vector3.Dot(Vector3.up, velocity - supportVelocity);
            float magnitude = heightError * _stiffness - closingSpeed * _damping;
            magnitude = Mathf.Clamp(magnitude, -_maxHoverForce, _maxHoverForce);

            Vector3 force = (_rb.mass * -Physics.gravity) + (Vector3.up * (magnitude * _rb.mass));
            _rb.AddForce(force);

            Rigidbody supportBody = _supportHit.rigidbody;
            if (_pushSupportBody && supportBody != null && supportBody.isKinematic == false)
                supportBody.AddForceAtPosition(-force, _supportHit.point);
        }

        private Vector3 ProbeStart() {
            if (_probeOrigin != null)
                return _probeOrigin.position;

            if (_capsuleCollider != null)
                return _capsuleCollider.transform.TransformPoint(_capsuleCollider.center);

            return _rb != null ? _rb.position : transform.position;
        }

        private bool BelongsToSelf(RaycastHit hit) {
            if (hit.collider == null)
                return true;

            if (_rb != null && hit.rigidbody == _rb)
                return true;

            Transform hitTransform = hit.collider.transform;
            return hitTransform == transform || hitTransform.IsChildOf(transform);
        }

        private void OnDrawGizmosSelected() {
            Vector3 origin = ProbeStart();
            Vector3 direction = transform.TransformDirection(_probeLocal.normalized);
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(origin, origin + direction * _probeLength);
            Gizmos.color = Color.white;
            Gizmos.DrawWireSphere(origin + direction * _hoverClearance, 0.05f);
        }

        private readonly RaycastHit[] _hits = new RaycastHit[8];
    }
}

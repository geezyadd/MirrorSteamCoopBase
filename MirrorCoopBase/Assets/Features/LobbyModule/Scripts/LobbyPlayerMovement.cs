using Mirror;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Features.LobbyModule.Scripts {
    public sealed class LobbyPlayerMovement : NetworkBehaviour {
        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private CharacterController characterController;

        public override void OnStartLocalPlayer() {
            if (characterController == null)
                characterController = GetComponent<CharacterController>();
        }

        private void Update() {
            if (isLocalPlayer == false)
                return;

            if (characterController == null)
                characterController = GetComponent<CharacterController>();

            Vector3 direction = ReadMoveDirection();
            Vector3 motion = direction * moveSpeed * Time.deltaTime;
            motion.y = -9.81f * Time.deltaTime;

            if (characterController != null)
                characterController.Move(motion);
            else
                transform.position += motion;
        }

        private static Vector3 ReadMoveDirection() {
            Vector3 direction = Vector3.zero;
            Keyboard keyboard = Keyboard.current;
            if (keyboard == null)
                return direction;

            if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed)
                direction.z += 1f;
            if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed)
                direction.z -= 1f;
            if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed)
                direction.x += 1f;
            if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed)
                direction.x -= 1f;

            if (direction.sqrMagnitude > 1f)
                direction.Normalize();

            return direction;
        }
    }
}

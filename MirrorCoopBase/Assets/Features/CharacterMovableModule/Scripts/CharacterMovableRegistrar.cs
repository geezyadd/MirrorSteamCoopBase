using Features.CharacterMovableModule.Scripts.Models;
using Mirror;
using UnityEngine;
using Zenject;

namespace Features.CharacterMovableModule.Scripts {
    public sealed class CharacterMovableRegistrar : NetworkBehaviour {
        [SerializeField] private CharacterMovableBase _movable;

        private CharacterMovableModel _model;
        private uint _registeredPlayerId;

        [Inject]
        private void Construct(CharacterMovableModel model) {
            _model = model;
        }

        public override void OnStartClient() {
            Register();
        }

        public override void OnStopClient() {
            Unregister();
        }

        private void Register() {
            if (_model == null || _movable == null || netId == 0)
                return;

            _registeredPlayerId = netId;
            _model.Register(_registeredPlayerId, _movable);

            if (isLocalPlayer)
                _model.LocalMovable = _movable;
        }

        private void Unregister() {
            if (_model == null || _movable == null || _registeredPlayerId == 0)
                return;

            _model.Unregister(_registeredPlayerId, _movable);
            _registeredPlayerId = 0;
        }
    }
}

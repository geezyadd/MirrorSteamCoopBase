using System.Collections.Generic;

namespace Features.CharacterMovableModule.Scripts.Models {
    public sealed class CharacterMovableModel {
        private readonly Dictionary<uint, CharacterMovableBase> _movables = new();

        public CharacterMovableBase LocalMovable { get; internal set; }

        public IReadOnlyDictionary<uint, CharacterMovableBase> Movables => _movables;

        public bool TryGet(uint playerId, out CharacterMovableBase movable) =>
            _movables.TryGetValue(playerId, out movable);

        internal void Register(uint playerId, CharacterMovableBase movable) {
            if (playerId == 0 || movable == null)
                return;

            _movables[playerId] = movable;
        }

        internal void Unregister(uint playerId, CharacterMovableBase movable) {
            if (playerId == 0 || movable == null)
                return;

            if (_movables.TryGetValue(playerId, out CharacterMovableBase current) == false || current != movable)
                return;

            _movables.Remove(playerId);

            if (LocalMovable == movable)
                LocalMovable = null;
        }
    }
}

using System;
using System.Collections.Generic;
using Features.GameCoreModule.Scripts.Constants;
using Features.GameFlowStateMachineModule.Scripts.States;

namespace Features.GameFlowStateMachineModule.Scripts {
    public sealed class GameFlowStateSceneMapper {
        private readonly IReadOnlyDictionary<Type, string> _stateToScene =
            new Dictionary<Type, string> {
                { typeof(MenuGameFlowState), SceneNames.Menu },
                { typeof(SessionGameFlowState), SceneNames.Lobby },
            };

        public bool TryGetScene(Type stateType, out string sceneName) {
            if (stateType == null) {
                sceneName = null;
                return false;
            }

            return _stateToScene.TryGetValue(stateType, out sceneName);
        }
    }
}

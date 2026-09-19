using System;
using System.Collections.Generic;
using Features.GameCoreModule.Scripts.Constants;
using Features.GameFlowStateMachineModule.Scripts.States;

namespace Features.GameFlowStateMachineModule.Scripts {
    internal static class GameFlowStateSceneMapper {
        private static readonly IReadOnlyDictionary<Type, string> StateToScene =
            new Dictionary<Type, string> {
                { typeof(MenuGameFlowState), SceneNames.Menu },
                { typeof(SessionGameFlowState), SceneNames.Lobby },
            };

        public static bool TryGetScene(Type stateType, out string sceneName) {
            if (stateType == null) {
                sceneName = null;
                return false;
            }

            return StateToScene.TryGetValue(stateType, out sceneName);
        }
    }
}

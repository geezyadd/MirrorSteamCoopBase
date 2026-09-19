using System;

namespace Features.GameFlowStateMachineModule.Scripts {
    public sealed class GameFlowStateLifecycleEventClass {
        public event Action<Type> OnStateEntered;
        public event Action<Type> OnStateExited;

        public void InvokeOnStateEntered(Type stateType) =>
            OnStateEntered?.Invoke(stateType);

        public void InvokeOnStateExited(Type stateType) =>
            OnStateExited?.Invoke(stateType);
    }
}

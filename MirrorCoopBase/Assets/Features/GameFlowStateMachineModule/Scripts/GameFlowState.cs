using System;
using System.Threading.Tasks;

namespace Features.GameFlowStateMachineModule.Scripts {
    public abstract class GameFlowState : IGameFlowState {
        public event Action Activated;
        public event Action Deactivated;

        public async Task EnterAsync() {
            await OnEnterAsync();
            Activated?.Invoke();
        }

        public async Task ExitAsync() {
            Deactivated?.Invoke();
            await OnExitAsync();
        }

        protected virtual Task OnEnterAsync() => Task.CompletedTask;
        protected virtual Task OnExitAsync() => Task.CompletedTask;
    }
}

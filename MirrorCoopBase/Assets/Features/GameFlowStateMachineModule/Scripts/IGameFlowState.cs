using System;
using System.Threading.Tasks;

namespace Features.GameFlowStateMachineModule.Scripts {
    public interface IGameFlowState {
        event Action Activated;
        event Action Deactivated;

        Task EnterAsync();
        Task ExitAsync();
    }
}

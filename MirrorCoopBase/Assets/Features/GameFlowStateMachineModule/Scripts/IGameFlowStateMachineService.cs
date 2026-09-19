using System;
using System.Threading.Tasks;

namespace Features.GameFlowStateMachineModule.Scripts {
    public interface IGameFlowStateMachineService {
        Type CurrentStateType { get; }
        bool IsTransitioning { get; }

        Task EnterAsync<TState>() where TState : class, IGameFlowState;
    }
}

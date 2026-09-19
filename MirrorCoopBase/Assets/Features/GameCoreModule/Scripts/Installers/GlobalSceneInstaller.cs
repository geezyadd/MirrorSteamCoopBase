using Game.Connection;
using Features.GameFlowStateMachineModule.Scripts.Installers;
using Zenject;

namespace Features.GameCoreModule.Scripts.Installers {
    public sealed class GlobalSceneInstaller : MonoInstaller<GlobalSceneInstaller> {
        public override void InstallBindings() {
            GameFlowStateMachineModuleInstaller.Install(Container);
            ConnectionModuleInstaller.Install(Container);
            WindowsModuleInstaller.Install(Container);
        }
    }
}

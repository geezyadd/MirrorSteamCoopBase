using Features.CameraModule.Scripts;
using Features.CharacterMovableModule.Scripts;
using Features.GameFlowStateMachineModule.Scripts.Installers;
using Features.GrabModule.Scripts;
using Features.InputModule.Realization.Scripts;
using Game.Connection;
using Zenject;

namespace Features.GameCoreModule.Scripts.Installers {
    public sealed class GlobalSceneInstaller : MonoInstaller<GlobalSceneInstaller> {
        public override void InstallBindings() {
            GameFlowStateMachineModuleInstaller.Install(Container);
            InputModuleInstaller.Install(Container);
            CharacterMovableModuleInstaller.Install(Container);
            CameraModuleInstaller.Install(Container);
            GrabModuleInstaller.Install(Container);
            ConnectionModuleInstaller.Install(Container);
            WindowsModuleInstaller.Install(Container);
        }
    }
}

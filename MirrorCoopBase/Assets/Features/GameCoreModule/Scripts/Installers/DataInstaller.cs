using Features.GameFlowStateMachineModule.Scripts;
using Features.MvpModule;
using Game.Connection;
using Zenject;

namespace Features.GameCoreModule.Scripts.Installers {
    public sealed class DataInstaller : Installer<DataInstaller> {
        public override void InstallBindings() {
            Container.Bind<GameFlowStateLifecycleEventClass>().AsSingle();
            Container.Bind<ConnectionSessionModel>().AsSingle();
            Container.Bind<SteamLobbyModel>().AsSingle();
            Container.Bind<PreloadedWindowsModel>().AsSingle();
        }
    }
}

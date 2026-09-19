using System;
using Features.GameCoreModule.Scripts;
using Features.LobbyModule.Scripts;
using Features.MenuModule.Scripts;
using Features.MvpModule;
using Zenject;

namespace Features.GameCoreModule.Scripts.Installers {
    public sealed class WindowsModuleInstaller : Installer<WindowsModuleInstaller> {
        public override void InstallBindings() {
            ViewSystemInstaller.Install(Container);
            UIByContextInstaller.Install(Container);

            Container.Bind<MenuWindow>().AsSingle();
            Container.Bind(typeof(IInitializable), typeof(IDisposable)).To<MenuWindow>().FromResolve().NonLazy();
            Container.Bind<GameHudWindow>().AsSingle();
            Container.Bind(typeof(IInitializable), typeof(IDisposable)).To<GameHudWindow>().FromResolve().NonLazy();
            Container.BindInterfacesTo<GameFlowWindowsSystem>().AsSingle().NonLazy();
        }
    }
}

using Zenject;

namespace Features.MvpModule {
    public class UIByContextInstaller : Installer<UIByContextInstaller> {
        public override void InstallBindings() {
            Container.Bind<IPresenterFactory>()
                .To<DIPresenterFactory>()
                .AsSingle();

            Container.Bind<IWindowsComponentsFinderService>()
                .To<WindowsComponentsFinderService>()
                .AsSingle();
            
            Container.Bind<IWindowsFactory>()
                .To<MonoWindowsFactory>()
                .AsSingle();

            Container.Bind<WindowsRootService>()
                .AsSingle();
        }
    }
}
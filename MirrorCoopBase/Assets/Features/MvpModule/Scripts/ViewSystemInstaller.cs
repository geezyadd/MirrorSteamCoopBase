using Zenject;

namespace Features.MvpModule {
    public class ViewSystemInstaller : Installer<ViewSystemInstaller> {
        public override void InstallBindings() {
            Container.BindInterfacesTo<WindowsService>()
                .AsSingle();

            Container.BindInterfacesTo<FocusablesService>()
                .AsSingle();

            Container.BindInterfacesTo<GenericEventBus<WindowsEventData>>()
                .AsSingle();
            
            Container.BindInterfacesTo<GenericEventBus<FocusEventData>>()
                .AsSingle();
            
            Container.BindInterfacesTo<WindowFocusSystem>()
                .AsSingle()
                .NonLazy();
        }
    }
}
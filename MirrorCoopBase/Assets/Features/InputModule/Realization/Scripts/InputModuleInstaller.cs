using Features.InputModule.Realization.Scripts.Generated;
using Zenject;

namespace Features.InputModule.Realization.Scripts {
    public class InputModuleInstaller : Installer<InputModuleInstaller> {
        public override void InstallBindings() {
            Container.Bind<InputActions>()
                .AsSingle();

            Container.BindInterfacesAndSelfTo<InputService>()
                .AsSingle();
        }
    }
}

using Zenject;

namespace Features.CharacterMovableModule.Scripts {
    public sealed class CharacterMovableModuleInstaller : Installer<CharacterMovableModuleInstaller> {
        public override void InstallBindings() {
            Container.BindInterfacesAndSelfTo<CharacterInputBuffer>().AsSingle();
        }
    }
}

using Features.CameraModule.Scripts.Services;
using UnityEngine;
using Zenject;

namespace Features.CameraModule.Scripts {
    public sealed class CameraModuleInstaller : Installer<CameraModuleInstaller> {
        public override void InstallBindings() {
            Container.Bind<CameraCatalog>().FromMethod(LoadCatalog).AsSingle();
            Container.BindInterfacesTo<GameCameraService>().AsSingle().NonLazy();
            Container.BindInterfacesTo<CameraLookDriver>().AsSingle();
            Container.BindInterfacesTo<CameraSwitchBinder>().AsSingle();
        }

        private static CameraCatalog LoadCatalog() {
            CameraCatalog catalog = Resources.Load<CameraCatalog>(CameraCatalog.ResourceName);
            return catalog != null ? catalog : ScriptableObject.CreateInstance<CameraCatalog>();
        }
    }
}

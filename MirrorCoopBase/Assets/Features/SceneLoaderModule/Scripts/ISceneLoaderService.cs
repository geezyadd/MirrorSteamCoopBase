using System.Collections.Generic;
using System.Threading.Tasks;

namespace Features.SceneLoaderModule.Scripts {
    public interface ISceneLoaderService {
        Task LoadSceneAsync(string sceneToLoad, bool unloadRedundant);
        Task LoadScenesAsync(List<string> scenesToLoad, string activeScene, bool unloadRedundant);
        Task UnloadSceneAsync(string sceneToUnload);
        Task UnloadScenesAsync(List<string> scenesToUnload);
    }
}

using Cysharp.Threading.Tasks;

namespace Services
{
    public interface ISceneLoader
    {
        UniTask LoadGameplayScene();
        UniTask LoadScene(string sceneName);
        UniTask UnloadCurrentScenes();
    }
}
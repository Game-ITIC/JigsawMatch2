using Cysharp.Threading.Tasks;

namespace Loaders
{
    public interface ILevelAssetLoader
    {
        LevelDefinition LoadLevel(int levelId);
        UniTask<LevelDefinition> LoadLevelAsync(int levelId);
    }
}
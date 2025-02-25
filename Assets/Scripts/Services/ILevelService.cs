using Cysharp.Threading.Tasks;

namespace Services
{
    public interface ILevelService
    {
        LevelDefinition LoadAndInitializeLevel(int levelId);
        UniTask<LevelDefinition> LoadAndInitializeLevelAsync(int levelId);
    }
}
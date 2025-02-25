using Cysharp.Threading.Tasks;
using Loaders;
using UnityEngine;

namespace Services
{
    public class LevelService : ILevelService
    {
        private readonly ILevelAssetLoader _levelAssetLoader;

        public LevelService(ILevelAssetLoader levelAssetLoader)
        {
            _levelAssetLoader = levelAssetLoader;
        }

        public LevelDefinition LoadAndInitializeLevel(int levelId)
        {
            var def = _levelAssetLoader.LoadLevel(levelId);
            if (def == null)
            {
                UnityEngine.Debug.LogError($"Level {levelId} not found!");
                return null;
            }

            // Шаг 2: здесь мог быть код, который создаёт логический Grid,
            // пробегается по def.cells и назначает TileFactory, Goals, Events и т.д.
            // Для примера просто вернём def.

            return def;
        }

        public async UniTask<LevelDefinition> LoadAndInitializeLevelAsync(int levelId)
        {
            var def = await _levelAssetLoader.LoadLevelAsync(levelId);
            Debug.Log(def.name);
            if (def == null)
            {
                UnityEngine.Debug.LogError($"Level {levelId} not found!");
                return null;
            }

            // Шаг 2: здесь мог быть код, который создаёт логический Grid,
            // пробегается по def.cells и назначает TileFactory, Goals, Events и т.д.
            // Для примера просто вернём def.

            return def;
        }
    }
}
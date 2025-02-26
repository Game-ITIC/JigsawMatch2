using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Loaders
{
    public class ResourcesLevelAssetLoader : ILevelAssetLoader
    {
        private readonly string _assetPath;

        public ResourcesLevelAssetLoader(string assetPath)
        {
            _assetPath = assetPath;
        }

        public LevelDefinition LoadLevel(int levelId)
        {
            var fullPath = $"{_assetPath}/{nameof(LevelDefinition)}_{levelId}";

            var def = Resources.Load<LevelDefinition>(fullPath);
            if (def == null)
            {
                Debug.LogWarning($"LevelDefinition not found at Resources/{fullPath}");
                throw new NullReferenceException($"LevelDefinition not found at Resources/{fullPath}");
            }

            return def;
        }

        public async UniTask<LevelDefinition> LoadLevelAsync(int levelId)
        {
            var fullPath = $"{_assetPath}/{nameof(LevelDefinition)}_{levelId}";

            var request = Resources.LoadAsync<LevelDefinition>(fullPath);

            await request.ToUniTask();

            if (request.asset is LevelDefinition levelDefinition)
            {
                return levelDefinition;
            }

            Debug.LogWarning($"LevelDefinition not found at Resources/{fullPath}");
            throw new NullReferenceException($"LevelDefinition not found at Resources/{fullPath}");
        }
    }
}
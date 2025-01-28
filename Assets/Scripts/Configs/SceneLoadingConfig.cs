using System.Collections.Generic;
using UnityEngine;

namespace Configs
{
    [CreateAssetMenu(menuName = "Configs/Scene Loading Config")]
    public class SceneLoadingConfig : ScriptableObject
    {
        [field: SerializeField] public string GameplaySceneName { get; private set; } = "Gameplay";
        [field: SerializeField] public List<string> PersistentScenes { get; private set; } = new()
        {
            "Bootstrapper",
            "ProjectScope"
        };
    }
}
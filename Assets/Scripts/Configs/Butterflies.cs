using System.Collections.Generic;
using UnityEngine;

namespace Configs
{
    [CreateAssetMenu(fileName = nameof(GameResources), menuName = nameof(Configs) + "/" + nameof(GameResources), order = 0)]
    public class GameResources : ScriptableObject
    {
        [SerializeField] public List<Butterfly> Butterflies { get; private set; }
    }

    [CreateAssetMenu(fileName = nameof(Butterfly), menuName = nameof(Configs) + "/" + nameof(Butterfly), order = 0)]
    public class Butterfly : ScriptableObject
    {
        public int Id;
        public Sprite sprite;
    }
}
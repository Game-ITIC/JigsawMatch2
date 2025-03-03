using Sirenix.OdinInspector;
using UnityEngine;

namespace Core.Grid.Configs
{
    [CreateAssetMenu(fileName = nameof(TileViewSpawnConfig), menuName = "Game/" + nameof(TileViewSpawnConfig))]
    public class TileViewSpawnConfig : SerializedScriptableObject
    {
        [Header("Cell Layout")] public float cellWidth = 1f;
        public float cellHeight = 1f;

        [Tooltip("Margin by X")] public float offsetX = 0f;
        [Tooltip("Margin by Y")] public float offsetY = 0f;

        [Header("Parent Object")] [Tooltip("If there's not a parentObject, new will be Instantiated with this name")]
        public string parentObjectName = "GridParent";
    }
}
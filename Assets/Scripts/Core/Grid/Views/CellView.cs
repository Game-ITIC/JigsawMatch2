using Core.Grid.Interfaces;
using Cysharp.Threading.Tasks;
using Interfaces;
using UnityEngine;

namespace Core.Grid.Views
{
    public class CellView : MonoBehaviour, IPreload
    {
        [Header("Sprite Renderers")] [SerializeField]
        private SpriteRenderer backgroundSr;

        [SerializeField] private SpriteRenderer layerSr;
        [SerializeField] private SpriteRenderer tileSr;

        public ICell LogicalCell;

        [Header("Sprites - Fallbacks")] [SerializeField]
        private Sprite normalCellSprite;

        [SerializeField] private Sprite emptyTileSprite;

        [Header("Layer Sprites")] [SerializeField]
        private Sprite pinkSprite;

        [SerializeField] private Sprite pinkDoubleSprite;
        [SerializeField] private Sprite honeySprite;
        [SerializeField] private Sprite flowerSprite;

        [Header("Tile Sprites")] [SerializeField]
        private Sprite redTile, blueTile, greenTile;


        public async UniTask WarmUp()
        {
            UpdateVisual();
            await UniTask.Yield();
        }

        public void UpdateVisual()
        {
            if (LogicalCell == null || !LogicalCell.IsEnabled)
            {
                backgroundSr.sprite = null;
                layerSr.sprite = null;
                tileSr.sprite = null;
                return;
            }

            backgroundSr.sprite = normalCellSprite;

            switch (LogicalCell.State)
            {
                case CellState.Pink:
                    layerSr.sprite = pinkSprite;
                    break;
                case CellState.DoublePink:
                    layerSr.sprite = pinkDoubleSprite;
                    break;
                case CellState.Honey:
                    layerSr.sprite = honeySprite;
                    break;
                case CellState.Blocked:
                    layerSr.sprite = flowerSprite;
                    break;
                default:
                    layerSr.sprite = null; // Normal
                    break;
            }

            // 3) Тайл
            if (LogicalCell.Tile == null)
            {
                tileSr.sprite = emptyTileSprite;
            }
            else
            {
                // Смотрим TileId
                switch (LogicalCell.Tile.TileId)
                {
                    case "Red":
                        tileSr.sprite = redTile;
                        break;
                    case "Blue":
                        tileSr.sprite = blueTile;
                        break;
                    case "Green":
                        tileSr.sprite = greenTile;
                        break;
                    // ...
                    default:
                        tileSr.sprite = emptyTileSprite;
                        break;
                }
            }
        }
    }
}
using System;
using UnityEngine;

namespace JuiceFresh
{
    [DisallowMultipleComponent]
    public sealed class GridOutlineGenerator : MonoBehaviour
    {
        [Header("Border Sprites")]
        [Tooltip("Straight side border")]
        [SerializeField] private Sprite outline1;

        [Tooltip("Top-left corner")]
        [SerializeField] private Sprite outline2;

        [Tooltip("Bottom-right corner")]
        [SerializeField] private Sprite outline3;

        [Tooltip("Top-right corner")]
        [SerializeField] private Sprite outline4;

        [Tooltip("Bottom-left corner")]
        [SerializeField] private Sprite outline5;

#if UNITY_EDITOR
        private const string Outline1Path = "Assets/JuiceFresh/Textures/Blocks/border_01.png";
        private const string Outline2Path = "Assets/JuiceFresh/Textures/Blocks/border_02.png";
        private const string Outline3Path = "Assets/JuiceFresh/Textures/Blocks/border_03.png";
        private const string Outline4Path = "Assets/JuiceFresh/Textures/Blocks/border_04.png";
        private const string Outline5Path = "Assets/JuiceFresh/Textures/Blocks/border_05.png";

        private void OnValidate()
        {
            AssignDefaultOutlineSprites();
        }

        [ContextMenu("Assign Default Outline Sprites")]
        private void AssignDefaultOutlineSprites()
        {
            bool changed = false;

            changed |= AssignSprite(ref outline1, Outline1Path);
            changed |= AssignSprite(ref outline2, Outline2Path);
            changed |= AssignSprite(ref outline3, Outline3Path);
            changed |= AssignSprite(ref outline4, Outline4Path);
            changed |= AssignSprite(ref outline5, Outline5Path);

            if(changed)
                UnityEditor.EditorUtility.SetDirty(this);
        }

        private static bool AssignSprite(ref Sprite target, string path)
        {
            Sprite sprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(path);

            if(sprite == null || target == sprite)
                return false;

            target = sprite;
            return true;
        }
#endif

        public void Generate(int maxCols, int maxRows, Func<int, int, bool, Square> getSquare)
        {
            if(getSquare == null)
                return;

            int row = 0;
            int col = 0;

            for(row = 0; row < maxRows; row++)
                SetOutline(maxCols, maxRows, getSquare, col, row, 0);

            row = maxRows - 1;

            for(col = 1; col < maxCols; col++)
                SetOutline(maxCols, maxRows, getSquare, col, row, 90);

            col = maxCols - 1;

            for(row = maxRows - 2; row >= 0; row--)
                SetOutline(maxCols, maxRows, getSquare, col, row, 180);

            row = 0;

            for(col = maxCols - 2; col > 0; col--)
                SetOutline(maxCols, maxRows, getSquare, col, row, 270);

            col = 0;

            for(row = 1; row < maxRows - 1; row++)
            {
                for(col = 1; col < maxCols - 1; col++)
                    SetOutline(maxCols, maxRows, getSquare, col, row, 0);
            }
        }

        private void SetOutline(
            int maxCols,
            int maxRows,
            Func<int, int, bool, Square> getSquare,
            int col,
            int row,
            float zRot)
        {
            Square square = getSquare(col, row, true);

            if(square.type != SquareTypes.NONE)
            {
                if(row == 0 || col == 0 || col == maxCols - 1 || row == maxRows - 1)
                {
                    GameObject outline = CreateOutline(square);
                    SpriteRenderer spr = outline.GetComponent<SpriteRenderer>();
                    outline.transform.localRotation = Quaternion.Euler(0, 0, zRot);
                    if(zRot == 0)
                        outline.transform.localPosition = Vector3.zero + Vector3.left * 0.83f;
                    if(zRot == 90)
                        outline.transform.localPosition = Vector3.zero + Vector3.down * 0.83f;
                    if(zRot == 180)
                        outline.transform.localPosition = Vector3.zero + Vector3.right * 0.83f;
                    if(zRot == 270)
                        outline.transform.localPosition = Vector3.zero + Vector3.up * 0.83f;

                    if(row == 0 && col == 0)
                    {
                        SetCornerOutline(outline, spr, outline2);
                        outline.transform.localPosition = Vector3.zero + Vector3.left * 0.01f + Vector3.up * 0.01f;
                    }

                    if(row == 0 && col == maxCols - 1)
                    {
                        SetCornerOutline(outline, spr, outline4);
                        outline.transform.localPosition = Vector3.zero + Vector3.right * 0.01f + Vector3.up * 0.01f;
                    }

                    if(row == maxRows - 1 && col == 0)
                    {
                        SetCornerOutline(outline, spr, outline5);
                        outline.transform.localPosition = Vector3.zero + Vector3.left * 0.01f + Vector3.down * 0.01f;
                    }

                    if(row == maxRows - 1 && col == maxCols - 1)
                    {
                        SetCornerOutline(outline, spr, outline3);
                        outline.transform.localPosition = Vector3.zero + Vector3.right * 0.01f + Vector3.down * 0.01f;
                    }
                }
                else
                {
                    if(getSquare(col - 1, row - 1, true).type == SquareTypes.NONE &&
                       getSquare(col, row - 1, true).type == SquareTypes.NONE &&
                       getSquare(col - 1, row, true).type == SquareTypes.NONE)
                    {
                        GameObject outline = CreateOutline(square);
                        SpriteRenderer spr = outline.GetComponent<SpriteRenderer>();
                        SetCornerOutline(outline, spr, outline2);
                        outline.transform.localPosition = Vector3.zero + Vector3.left * 0.015f + Vector3.up * 0.015f;
                    }

                    if(getSquare(col + 1, row - 1, true).type == SquareTypes.NONE &&
                       getSquare(col, row - 1, true).type == SquareTypes.NONE &&
                       getSquare(col + 1, row, true).type == SquareTypes.NONE)
                    {
                        GameObject outline = CreateOutline(square);
                        SpriteRenderer spr = outline.GetComponent<SpriteRenderer>();
                        SetCornerOutline(outline, spr, outline4);
                        outline.transform.localPosition = Vector3.zero + Vector3.right * 0.015f + Vector3.up * 0.015f;
                    }

                    if(getSquare(col - 1, row + 1, true).type == SquareTypes.NONE &&
                       getSquare(col, row + 1, true).type == SquareTypes.NONE &&
                       getSquare(col - 1, row, true).type == SquareTypes.NONE)
                    {
                        GameObject outline = CreateOutline(square);
                        SpriteRenderer spr = outline.GetComponent<SpriteRenderer>();
                        SetCornerOutline(outline, spr, outline5);
                        outline.transform.localPosition = Vector3.zero + Vector3.left * 0.015f + Vector3.down * 0.015f;
                    }

                    if(getSquare(col + 1, row + 1, true).type == SquareTypes.NONE &&
                       getSquare(col, row + 1, true).type == SquareTypes.NONE &&
                       getSquare(col + 1, row, true).type == SquareTypes.NONE)
                    {
                        GameObject outline = CreateOutline(square);
                        SpriteRenderer spr = outline.GetComponent<SpriteRenderer>();
                        SetCornerOutline(outline, spr, outline3);
                        outline.transform.localPosition = Vector3.zero + Vector3.right * 0.015f + Vector3.down * 0.015f;
                    }
                }
            }
            else
            {
                bool corner = false;

                if(getSquare(col - 1, row, true).type != SquareTypes.NONE &&
                   getSquare(col, row - 1, true).type != SquareTypes.NONE)
                {
                    GameObject outline = CreateOutline(square);
                    SpriteRenderer spr = outline.GetComponent<SpriteRenderer>();
                    SetCornerOutline(outline, spr, outline2);
                    outline.transform.localPosition = Vector3.zero;
                    corner = true;
                }

                if(getSquare(col + 1, row, true).type != SquareTypes.NONE &&
                   getSquare(col, row + 1, true).type != SquareTypes.NONE)
                {
                    GameObject outline = CreateOutline(square);
                    SpriteRenderer spr = outline.GetComponent<SpriteRenderer>();
                    SetCornerOutline(outline, spr, outline3);
                    outline.transform.localPosition = Vector3.zero;
                    corner = true;
                }

                if(getSquare(col + 1, row, true).type != SquareTypes.NONE &&
                   getSquare(col, row - 1, true).type != SquareTypes.NONE)
                {
                    GameObject outline = CreateOutline(square);
                    SpriteRenderer spr = outline.GetComponent<SpriteRenderer>();
                    SetCornerOutline(outline, spr, outline4);
                    outline.transform.localPosition = Vector3.zero;
                    corner = true;
                }

                if(getSquare(col - 1, row, true).type != SquareTypes.NONE &&
                   getSquare(col, row + 1, true).type != SquareTypes.NONE)
                {
                    GameObject outline = CreateOutline(square);
                    SpriteRenderer spr = outline.GetComponent<SpriteRenderer>();
                    SetCornerOutline(outline, spr, outline5);
                    outline.transform.localPosition = Vector3.zero;
                    corner = true;
                }

                if(!corner)
                {
                    if(getSquare(col, row - 1, true).type != SquareTypes.NONE)
                    {
                        GameObject outline = CreateOutline(square);
                        outline.transform.localPosition = Vector3.zero + Vector3.up * 0.79f;
                        outline.transform.localRotation = Quaternion.Euler(0, 0, 90);
                    }

                    if(getSquare(col, row + 1, true).type != SquareTypes.NONE)
                    {
                        GameObject outline = CreateOutline(square);
                        outline.transform.localPosition = Vector3.zero + Vector3.down * 0.79f;
                        outline.transform.localRotation = Quaternion.Euler(0, 0, 90);
                    }

                    if(getSquare(col - 1, row, true).type != SquareTypes.NONE)
                    {
                        GameObject outline = CreateOutline(square);
                        outline.transform.localPosition = Vector3.zero + Vector3.left * 0.79f;
                        outline.transform.localRotation = Quaternion.Euler(0, 0, 0);
                    }

                    if(getSquare(col + 1, row, true).type != SquareTypes.NONE)
                    {
                        GameObject outline = CreateOutline(square);
                        outline.transform.localPosition = Vector3.zero + Vector3.right * 0.79f;
                        outline.transform.localRotation = Quaternion.Euler(0, 0, 0);
                    }
                }
            }
        }

        private void SetCornerOutline(GameObject outline, SpriteRenderer spr, Sprite sprite)
        {
            spr.sprite = sprite != null ? sprite : outline3;
            outline.transform.localRotation = Quaternion.identity;
        }

        private GameObject CreateOutline(Square square)
        {
            GameObject outline = new GameObject();
            outline.name = "outline";
            outline.transform.SetParent(square.transform);
            outline.transform.localPosition = Vector3.zero;
            outline.transform.localScale = Vector3.one * 2;
            SpriteRenderer spr = outline.AddComponent<SpriteRenderer>();
            spr.sprite = outline1;
            spr.sortingOrder = 1;
            return outline;
        }
    }
}

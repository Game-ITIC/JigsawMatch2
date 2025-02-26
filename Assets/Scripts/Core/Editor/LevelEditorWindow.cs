using System;
using Core.Grid.Entities;
using Core.Grid.Interfaces;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Core.Editor
{
    public class LevelEditorWindow : EditorWindow
    {
        [MenuItem("Tools/Level Editor Window")]
        private static void ShowWindow()
        {
            var window = GetWindow<LevelEditorWindow>("Level Editor");
            window.Show();
        }

        [InlineEditor(InlineEditorObjectFieldModes.Hidden)] [LabelText("Active Level Definition")]
        public LevelDefinition levelDefinition;

        [InlineEditor(InlineEditorObjectFieldModes.Hidden)] [LabelText("CellButtonSize")]
        public float CellButtonSize = 70f;


        private Vector2 _scrollPos;
        private PropertyTree _propertyTree;


        private void OnGUI()
        {
            EditorGUILayout.BeginHorizontal();
            {
                levelDefinition = (LevelDefinition)EditorGUILayout.ObjectField("Level Definition:", levelDefinition,
                    typeof(LevelDefinition), false);

                if (GUILayout.Button("Save to Asset", GUILayout.Width(110)))
                {
                    if (levelDefinition != null)
                    {
                        EditorUtility.SetDirty(levelDefinition);
                        AssetDatabase.SaveAssets();
                        Debug.LogWarning($"LevelDefinition {levelDefinition.name} saved.");
                    }
                }
            }
            EditorGUILayout.EndHorizontal();

            if (!levelDefinition)
            {
                EditorGUILayout.HelpBox("Please assign a LevelDefinition asset.", MessageType.Info);
                return;
            }

            // Создаём (или пересоздаём) PropertyTree, если оно ещё не существует 
            // или поменялся тип (на всякий случай).
            if (_propertyTree == null || _propertyTree.TargetType != typeof(LevelDefinition))
            {
                _propertyTree = PropertyTree.Create(levelDefinition);
            }

            EditorGUILayout.BeginHorizontal();
            {
                // Левая панель (Odin-инспектор)
                EditorGUILayout.BeginVertical(GUILayout.Width(position.width * 0.4f));
                {
                    _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos, GUILayout.ExpandWidth(true),
                        GUILayout.ExpandHeight(true));

                    SirenixEditorGUI.Title("Level Definition Inspector", null, TextAlignment.Left, true);

                    // Рекомендуется делать Update/Draw/Apply
                    _propertyTree.UpdateTree();
                    _propertyTree.Draw();
                    _propertyTree.ApplyChanges();

                    EditorGUILayout.EndScrollView();
                }
                EditorGUILayout.EndVertical();

                // Правая панель (плиточная сетка)
                EditorGUILayout.BeginVertical("box", GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
                {
                    SirenixEditorGUI.Title("Grid Editor (Tile-Based)", null, TextAlignment.Left, true);
                    DrawTileGrid();
                }
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndHorizontal(); // <-- не забываем закрывать горизонталь
        }

        private void DrawTileGrid()
        {
            if (levelDefinition.cells == null ||
                levelDefinition.cells.Count != levelDefinition.rows * levelDefinition.columns)
            {
                EditorGUILayout.HelpBox("Cells count mismatch. Click 'Regenerate Cells' on the left panel.",
                    MessageType.Warning);
                return;
            }

            // Скролл для сетки
            _scrollPos =
                EditorGUILayout.BeginScrollView(_scrollPos, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));

            for (int r = 0; r < levelDefinition.rows; r++)
            {
                EditorGUILayout.BeginHorizontal();
                for (int c = 0; c < levelDefinition.columns; c++)
                {
                    int index = r * levelDefinition.columns + c;
                    var cellData = levelDefinition.cells[index];

                    Color oldBg = GUI.backgroundColor;

                    if (!cellData.isEnabled)
                    {
                        GUI.backgroundColor = new Color32(100, 100, 100, 175);
                    }
                    else
                    {
                        switch (cellData.state)
                        {
                            case CellState.Blocked:
                                GUI.backgroundColor = new Color32(0, 170, 255, 255);
                                break;
                            case CellState.DoubleBlocked:
                                GUI.backgroundColor = new Color32(0, 70, 255, 255);
                                break;
                            case CellState.Pink:
                                GUI.backgroundColor = new Color32(255, 0, 255, 255);
                                break;
                            case CellState.DoublePink:
                                GUI.backgroundColor = new Color32(180, 0, 255, 255);
                                break;
                            case CellState.Honey:
                                GUI.backgroundColor = new Color32(255, 120, 0, 255);
                                break;
                            case CellState.Infected:
                                GUI.backgroundColor = new Color32(60, 255, 0, 255);
                                break;
                            default:
                                GUI.backgroundColor = Color.white;
                                break;
                        }
                    }

                    if (GUILayout.Button(new GUIContent(GetCellLabel(cellData)), GUILayout.Width(CellButtonSize),
                            GUILayout.Height(CellButtonSize)))
                    {
                        OnCellClick(cellData);
                    }

                    GUI.backgroundColor = oldBg;
                }

                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndScrollView();
        }

        private string GetCellLabel(CellData cell)
        {
            if (!cell.isEnabled) return "OFF";
            return cell.state.ToString();
        }

        private void OnCellClick(CellData cellData)
        {
            GenericMenu menu = new GenericMenu();

            menu.AddItem(new GUIContent("Toggle Enabled"), false, () =>
            {
                cellData.isEnabled = !cellData.isEnabled;
                MarkLevelDirty();
            });

            menu.AddSeparator("");

            menu.AddItem(new GUIContent("State/Normal"), false, () =>
            {
                cellData.state = CellState.Normal;
                MarkLevelDirty();
            });

            menu.AddItem(new GUIContent("State/Blocked"), false, () =>
            {
                cellData.state = CellState.Blocked;
                MarkLevelDirty();
            });

            menu.AddItem(new GUIContent("State/DoubleBlocked"), false, () =>
            {
                cellData.state = CellState.DoubleBlocked;
                MarkLevelDirty();
            });

            menu.AddItem(new GUIContent("State/Pink"), false, () =>
            {
                cellData.state = CellState.Pink;
                MarkLevelDirty();
            });

            menu.AddItem(new GUIContent("State/DoublePink"), false, () =>
            {
                cellData.state = CellState.DoublePink;
                MarkLevelDirty();
            });

            menu.AddItem(new GUIContent("State/Honey"), false, () =>
            {
                cellData.state = CellState.Honey;
                MarkLevelDirty();
            });

            menu.AddItem(new GUIContent("State/Infected"), false, () =>
            {
                cellData.state = CellState.Infected;
                MarkLevelDirty();
            });

            menu.ShowAsContext();
        }

        private void MarkLevelDirty()
        {
            if (levelDefinition != null)
            {
                EditorUtility.SetDirty(levelDefinition);
            }
        }
    }
}
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = nameof(LevelDefinition), menuName = "Game/" + nameof(LevelDefinition))]
public class LevelDefinition : SerializedScriptableObject
{
    [Title("Grid Settings")] [LabelText("Rows")]
    public int rows = 6;

    [LabelText("Columns")] public int columns = 8;

    [Space] [FoldoutGroup("Common Settings")] [LabelText("Move Limit")]
    public int moveLimit = 20;

    [FoldoutGroup("Common Settings")] [LabelText("Level ID")]
    public int levelId = 1;

    [FoldoutGroup("Common Settings")] [LabelText("Theme Name")]
    public string themeName;

    [FoldoutGroup("Common Settings")]
    [Button("Regenerate Cells")]
    private void RegenerateCells()
    {
        GenerateCellsInternal();
    }

    private void AutoRegenerateCells()
    {
        GenerateCellsInternal();
    }

    [FoldoutGroup("Cells Data")]
    [TableList(AlwaysExpanded = true,
        DrawScrollView = false,
        NumberOfItemsPerPage = 50,
        ShowIndexLabels = true)]
    public List<CellData> cells;

    [FoldoutGroup("Goals")] [ListDrawerSettings(ShowFoldout = true)]
    public List<GoalData> goals;

    [FoldoutGroup("Events")] [ListDrawerSettings(ShowFoldout = true)]
    public List<LevelEvent> events;

    private void GenerateCellsInternal()
    {
        if (rows <= 0 || columns <= 0)
        {
            Debug.LogWarning("Rows or Columns must be greater than zero");
            return;
        }

        var neededCount = rows * columns;
        if (cells == null)
        {
            cells = new List<CellData>(neededCount);
        }

        while (cells.Count > neededCount)
        {
            cells.RemoveAt(cells.Count - 1);
        }

        while (cells.Count < neededCount)
        {
            cells.Add(new CellData());
        }

        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < columns; c++)
            {
                int index = r * columns + c;
                cells[index].row = r;
                cells[index].column = c;
            }
        }
        Debug.LogWarning($"RegenerateCells done. Total cells: {cells.Count}");
    }
}
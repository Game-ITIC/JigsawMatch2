using Core.Grid.Interfaces;
using Sirenix.OdinInspector;
using UnityEngine;

[System.Serializable]
public class CellData
{
    [HideInInspector] public int row;
    [HideInInspector] public int column;

    [LabelWidth(70)] [LabelText("Enabled")]
    public bool isEnabled = true;

    [LabelWidth(70)] [EnumToggleButtons, LabelText("State")]
    public CellState state = CellState.Normal;

    [LabelWidth(70)] [LabelText("Tile ID")]
    public string tileId = "";

    [LabelWidth(70)] [LabelText("Block hits")]
    public int blockHits;

}
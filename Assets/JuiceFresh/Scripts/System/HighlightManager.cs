using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using JuiceFresh;

/// <summary>
/// Manages the highlighting of squares and items when players make matches
/// </summary>
public class HighlightManager 
{
    #region Private Fields
    private static List<Item> crossedExtraItems = new List<Item>();
    private static List<Item> extraItems = new List<Item>();
    private static bool enableHighlights;
    #endregion

    #region Public Methods
    /// <summary>
    /// Highlights an item when it's selected by the player
    /// </summary>
    /// <param name="item">The item to highlight</param>
    public static void SelectItem(Item item) 
    {
        ClearHighlight();
        
        if (item.currentType == ItemsTypes.HORIZONTAL_STRIPPED || item.currentType == ItemsTypes.VERTICAL_STRIPPED)
            extraItems.Add(item);
            
        LightItem(item);
    }

    /// <summary>
    /// Unhighlights an item when it's deselected by the player
    /// </summary>
    /// <param name="deleteItem">The item being removed from selection</param>
    /// <param name="item">The item that remains selected after deselection</param>
    public static void DeselectItem(Item deleteItem, Item item) 
    {
        ClearHighlight();
        
        if (deleteItem.currentType == ItemsTypes.HORIZONTAL_STRIPPED || deleteItem.currentType == ItemsTypes.VERTICAL_STRIPPED)
            extraItems.Remove(deleteItem);
            
        LightItem(item);
    }

    /// <summary>
    /// Clears all highlighting and resets the highlight state
    /// </summary>
    public static void StopAndClearAll() 
    {
        extraItems.Clear();
        ClearHighlight();
    }
    #endregion

    #region Private Methods
    /// <summary>
    /// Highlights an item and any affected rows or columns based on its type
    /// </summary>
    private static void LightItem(Item item) 
    {
        if (extraItems.Count == 1) 
        {
            if (extraItems[0].currentType == ItemsTypes.HORIZONTAL_STRIPPED)
                LightRow(item.square);
            else if (extraItems[0].currentType == ItemsTypes.VERTICAL_STRIPPED)
                LightColumn(item.square);
        }
        else if (extraItems.Count >= 2) 
        {
            LightCross(item.square);
        }
    }

    /// <summary>
    /// Highlights rows or columns affected by a special item
    /// </summary>
    private static void LightExtraItem(Item item) 
    {
        if (item.currentType == ItemsTypes.HORIZONTAL_STRIPPED)
            LightRow(item.square);
        else if (item.currentType == ItemsTypes.VERTICAL_STRIPPED)
            LightColumn(item.square);
    }

    /// <summary>
    /// Highlights both row and column of a square (cross pattern)
    /// </summary>
    private static void LightCross(Square square) 
    {
        LightRow(square);
        LightColumn(square);
    }

    /// <summary>
    /// Highlights a full row from a square
    /// </summary>
    private static void LightRow(Square square) 
    {
        List<Square> squareList = LevelManager.THIS.GetRowSquare(square.row);
        
        foreach (Square squareSelected in squareList) 
        {
            squareSelected.HighLight(true);
            
            if (CheckCrossedItem(squareSelected))
                LightExtraItem(squareSelected.item);
        }
    }

    /// <summary>
    /// Highlights a full column from a square
    /// </summary>
    private static void LightColumn(Square square) 
    {
        List<Square> squareList = LevelManager.THIS.GetColumnSquare(square.col);
        
        foreach (Square squareSelected in squareList) 
        {
            squareSelected.HighLight(true);
            
            if (CheckCrossedItem(squareSelected))
                LightExtraItem(squareSelected.item);
        }
    }

    /// <summary>
    /// Checks if a square contains a crossed extra item (for chain reactions)
    /// </summary>
    /// <returns>True if the square contains a previously unchecked extra item</returns>
    private static bool CheckCrossedItem(Square square) 
    {
        if (square.IsExtraItem()) 
        {
            if (crossedExtraItems.IndexOf(square.item) < 0 && extraItems.IndexOf(square.item) < 0) 
            {
                crossedExtraItems.Add(square.item);
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Highlights an individual square
    /// </summary>
    private static void LightSquare(Square square) 
    {
        square.HighLight(true);
    }

    /// <summary>
    /// Clears all highlighting effects
    /// </summary>
    private static void ClearHighlight(bool boost = false) 
    {
        crossedExtraItems.Clear();
        List<Square> itemsList = LevelManager.THIS.GetSquares();
        
        foreach (Square square in itemsList) 
        {
            square.HighLight(false);
        }
    }
    #endregion
}
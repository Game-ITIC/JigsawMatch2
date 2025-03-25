using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using JuiceFresh;

/// <summary>
/// Provides gameplay hints when the player is stuck by finding and highlighting possible matches
/// </summary>
public class TipsManager : MonoBehaviour
{
    #region Singleton
    public static TipsManager THIS;
    #endregion

    #region Public Fields
    public bool gotTip;
    public bool allowShowTip;
    public int corCount;
    #endregion

    #region Private Fields
    private int tipID;
    private List<Item> nextMoveItems = new List<Item>();
    private bool checkCombinesStarted;
    
    // Define different pattern types for readability
    private enum PatternType { 
        L_SHAPE, 
        T_SHAPE, 
        HORIZONTAL_LINE, 
        VERTICAL_LINE,
        DIAGONAL
    }
    
    // Store pattern definitions for reuse
    private class MatchPattern {
        public PatternType Type;
        public Vector2Int[] ItemPositions;
        public Vector2Int MovePosition;
        
        public MatchPattern(PatternType type, Vector2Int[] itemPositions, Vector2Int movePosition) {
            Type = type;
            ItemPositions = itemPositions;
            MovePosition = movePosition;
        }
    }
    
    // Collection of all match patterns to check
    private List<MatchPattern> matchPatterns;
    #endregion

    #region Unity Lifecycle
    void Awake() {
        InitializePatterns();
    }
    
    void Start()
    {
        THIS = this;
    }
    #endregion

    #region Pattern Initialization
    /// <summary>
    /// Initializes all the patterns to check for matches
    /// </summary>
    private void InitializePatterns() {
        matchPatterns = new List<MatchPattern>();
        
        // L-Shapes (8 possible orientations)
        // Format: First position is where to move, others are item positions to check
        
        // L-shape: o-o-x (move to x, check o's)
        //              o
        matchPatterns.Add(new MatchPattern(
            PatternType.L_SHAPE, 
            new Vector2Int[] { new Vector2Int(0, -1), new Vector2Int(0, -2), new Vector2Int(1, 0) },
            new Vector2Int(0, 0)
        ));
        
        //    o
        // o-o-x
        matchPatterns.Add(new MatchPattern(
            PatternType.L_SHAPE, 
            new Vector2Int[] { new Vector2Int(0, -1), new Vector2Int(0, -2), new Vector2Int(-1, 0) },
            new Vector2Int(0, 0)
        ));
        
        // x-o-o
        // o
        matchPatterns.Add(new MatchPattern(
            PatternType.L_SHAPE, 
            new Vector2Int[] { new Vector2Int(0, 1), new Vector2Int(0, 2), new Vector2Int(1, 0) },
            new Vector2Int(0, 0)
        ));
        
        // o
        // x-o-o
        matchPatterns.Add(new MatchPattern(
            PatternType.L_SHAPE, 
            new Vector2Int[] { new Vector2Int(0, 1), new Vector2Int(0, 2), new Vector2Int(-1, 0) },
            new Vector2Int(0, 0)
        ));
        
        // o
        // o
        // x-o
        matchPatterns.Add(new MatchPattern(
            PatternType.L_SHAPE, 
            new Vector2Int[] { new Vector2Int(-1, 0), new Vector2Int(-2, 0), new Vector2Int(0, 1) },
            new Vector2Int(0, 0)
        ));
        
        // x-o
        // o
        // o
        matchPatterns.Add(new MatchPattern(
            PatternType.L_SHAPE, 
            new Vector2Int[] { new Vector2Int(1, 0), new Vector2Int(2, 0), new Vector2Int(0, 1) },
            new Vector2Int(0, 0)
        ));
        
        //   o
        //   o
        // o-x
        matchPatterns.Add(new MatchPattern(
            PatternType.L_SHAPE, 
            new Vector2Int[] { new Vector2Int(-1, 0), new Vector2Int(-2, 0), new Vector2Int(0, -1) },
            new Vector2Int(0, 0)
        ));
        
        // o-x
        // o
        // o
        matchPatterns.Add(new MatchPattern(
            PatternType.L_SHAPE, 
            new Vector2Int[] { new Vector2Int(1, 0), new Vector2Int(2, 0), new Vector2Int(0, -1) },
            new Vector2Int(0, 0)
        ));
        
        // Straight lines
        // o-o-o (horizontal)
        matchPatterns.Add(new MatchPattern(
            PatternType.HORIZONTAL_LINE, 
            new Vector2Int[] { new Vector2Int(0, 0), new Vector2Int(0, 1), new Vector2Int(0, 2) },
            new Vector2Int(0, 0)
        ));
        
        matchPatterns.Add(new MatchPattern(
            PatternType.HORIZONTAL_LINE, 
            new Vector2Int[] { new Vector2Int(0, 0), new Vector2Int(0, -1), new Vector2Int(0, -2) },
            new Vector2Int(0, 0)
        ));
        
        // o (vertical)
        // o
        // o
        matchPatterns.Add(new MatchPattern(
            PatternType.VERTICAL_LINE, 
            new Vector2Int[] { new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(2, 0) },
            new Vector2Int(0, 0)
        ));
        
        matchPatterns.Add(new MatchPattern(
            PatternType.VERTICAL_LINE, 
            new Vector2Int[] { new Vector2Int(0, 0), new Vector2Int(-1, 0), new Vector2Int(-2, 0) },
            new Vector2Int(0, 0)
        ));
        
        // T-shapes
        // o o
        //  o  (T shape)
        matchPatterns.Add(new MatchPattern(
            PatternType.T_SHAPE, 
            new Vector2Int[] { new Vector2Int(-1, -1), new Vector2Int(-1, 1), new Vector2Int(0, 0) },
            new Vector2Int(-1, 0)
        ));
        
        //  o
        // o o (upside-down T)
        matchPatterns.Add(new MatchPattern(
            PatternType.T_SHAPE, 
            new Vector2Int[] { new Vector2Int(1, -1), new Vector2Int(1, 1), new Vector2Int(0, 0) },
            new Vector2Int(1, 0)
        ));
        
        //  o
        // o o
        //  o  (plus shape)
        matchPatterns.Add(new MatchPattern(
            PatternType.T_SHAPE, 
            new Vector2Int[] { new Vector2Int(-1, 0), new Vector2Int(1, 0), new Vector2Int(0, 1) },
            new Vector2Int(0, -1)
        ));
        
        matchPatterns.Add(new MatchPattern(
            PatternType.T_SHAPE, 
            new Vector2Int[] { new Vector2Int(-1, 0), new Vector2Int(1, 0), new Vector2Int(0, -1) },
            new Vector2Int(0, 1)
        ));
    }
    #endregion

    #region Public Methods
    /// <summary>
    /// Checks for possible matches on the board and shows a hint if found
    /// </summary>
    public IEnumerator CheckPossibleCombines()
    {
        // Prevent multiple checks running simultaneously
        if (checkCombinesStarted) yield break;
        checkCombinesStarted = true;

        yield return new WaitForSeconds(1);

        allowShowTip = true;
        
        // Wait for game to be in playing state
        while (LevelManager.THIS == null)
        {
            yield return new WaitForEndOfFrame();
        }
        
        while (LevelManager.THIS.gameStatus != GameState.Playing)
        {
            yield return new WaitForEndOfFrame();
        }

        if (!LevelManager.THIS.DragBlocked && LevelManager.THIS.gameStatus == GameState.Playing)
        {
            // Try to find possible matches
            // bool foundMatch = yield return StartCoroutine(SearchForMatches());
            
            // If no matches found, regenerate the level
            // if (!foundMatch && LevelManager.THIS.gameStatus == GameState.Playing)
            // {
                // Debug.Log("No possible matches found");
                // checkCombinesStarted = false;
                // LevelManager.THIS.NoMatches();
                // StopCoroutine(CheckPossibleCombines());
            // }
        }
        
        yield return new WaitForEndOfFrame();
        
        if (!LevelManager.THIS.DragBlocked)
            StartCoroutine(CheckPossibleCombines());
    }

    /// <summary>
    /// Returns the currently identified possible combination
    /// </summary>
    public List<Item> GetCombine()
    {
        return nextMoveItems;
    }
    #endregion

    #region Match Finding Methods
    /// <summary>
    /// Searches the entire board for possible matches
    /// </summary>
    /// <returns>True if a match is found</returns>
    private IEnumerator SearchForMatches()
    {
        nextMoveItems.Clear();
        gotTip = false;
        int itemsChecked = 0;
        int matchingColorsFound = 0;

        // Get a reference to any item to access color data
        Item anyItem = FindAnyItem();
        if (anyItem == null) yield break;
        
        int colorCount = anyItem.items.Length;
        int maxRow = LevelManager.THIS.maxRows;
        int maxCol = LevelManager.THIS.maxCols;

        // Check each color
        for (int color = 0; color < colorCount; color++)
        {
            // Check each board position
            for (int row = 0; row < maxRow; row++)
            {
                for (int col = 0; col < maxCol; col++)
                {
                    // Update counter
                    itemsChecked++;
                    
                    // Count matching colors around this position for statistics
                    matchingColorsFound = CountMatchingColorsAround(col, row, color, matchingColorsFound);
                    
                    // Get the square at this position
                    Square square = GetSquare(row, col);
                    
                    // Skip if this is a wireblock or has no item
                    if (square == null || square.type == SquareTypes.WIREBLOCK || square.item == null)
                        continue;
                    
                    // Check all patterns at this position
                    // if (yield return StartCoroutine(CheckAllPatternsAtPosition(square, row, col, color)))
                    // {
                        // yield return true; // Match found
                    // }
                }
            }
        }
        
        // No matches found
        if (!gotTip && itemsChecked > 0 && matchingColorsFound < 3)
        {
            yield return false;
        }
        
        yield return gotTip;
    }
    
    /// <summary>
    /// Finds any item on the board
    /// </summary>
    private Item FindAnyItem()
    {
        GameObject itemObject = GameObject.FindGameObjectWithTag("Item");
        return itemObject?.GetComponent<Item>();
    }
    
    /// <summary>
    /// Checks all possible match patterns at a specific position
    /// </summary>
    private IEnumerator CheckAllPatternsAtPosition(Square square, int row, int col, int color)
    {
        foreach (MatchPattern pattern in matchPatterns)
        {
            nextMoveItems.Clear();
            
            // Check if this pattern fits at the current position
            if (CheckPattern(row, col, color, pattern))
            {
                showTip(nextMoveItems);
                yield return true;
            }
        }
        
        yield return false;
    }
    
    /// <summary>
    /// Checks a specific pattern at a given position
    /// </summary>
    private bool CheckPattern(int baseRow, int baseCol, int color, MatchPattern pattern)
    {
        int maxRow = LevelManager.THIS.maxRows;
        int maxCol = LevelManager.THIS.maxCols;
        
        // Calculate actual position to move to
        int moveRow = baseRow + pattern.MovePosition.x;
        int moveCol = baseCol + pattern.MovePosition.y;
        
        // Ensure move position is on the board
        if (moveRow < 0 || moveRow >= maxRow || moveCol < 0 || moveCol >= maxCol)
            return false;
        
        Square moveSquare = GetSquare(moveRow, moveCol);
        
        // Ensure the move square exists and can be moved into
        if (moveSquare == null || !moveSquare.CanGoInto())
            return false;
            
        // Check item at move position
        CheckSquare(moveSquare, color, true);
        
        // Check all items in the pattern
        foreach (Vector2Int pos in pattern.ItemPositions)
        {
            int itemRow = baseRow + pos.x;
            int itemCol = baseCol + pos.y;
            
            // Ensure position is on the board
            if (itemRow < 0 || itemRow >= maxRow || itemCol < 0 || itemCol >= maxCol)
                return false;
                
            Square itemSquare = GetSquare(itemRow, itemCol);
            
            // Check the item at this position
            CheckSquare(itemSquare, color, false);
        }
        
        // If we found 3 matching items, we have a match
        return nextMoveItems.Count == 3;
    }
    
    /// <summary>
    /// Counts the number of matching color items around a position
    /// </summary>
    private int CountMatchingColorsAround(int col, int row, int color, int countColors)
    {
        for (int i = -1; i <= 1; i++)
        {
            for (int j = -1; j <= 1; j++)
            {
                Square sq = GetSquare(row + j, col + i);
                if (sq != null && sq.item != null && sq.item.color == color && sq.type != SquareTypes.WIREBLOCK)
                {
                    countColors++;
                }
            }
        }

        return countColors;
    }

    /// <summary>
    /// Gets a square at a specific position
    /// </summary>
    private Square GetSquare(int row, int col)
    {
        return LevelManager.THIS.GetSquare(col, row);
    }

    /// <summary>
    /// Checks if a square has an item of the specified color and adds it to the match list
    /// </summary>
    private void CheckSquare(Square square, int color, bool moveThis = false)
    {
        if (square == null || square.item == null)
            return;
            
        if (square.item.color == color)
        {
            // Only add the item if it's not in a wire block (when it's the move item)
            if ((moveThis && square.type != SquareTypes.WIREBLOCK) || !moveThis)
            {
                nextMoveItems.Add(square.item);
            }
        }
    }
    #endregion

    #region Tip Handling
    /// <summary>
    /// Shows a hint to the player
    /// </summary>
    private void showTip(List<Item> itemsToHighlight)
    {
        StopCoroutine(showTipCor(itemsToHighlight));
        StartCoroutine(showTipCor(itemsToHighlight));
    }

    /// <summary>
    /// Coroutine that handles showing the hint animation
    /// </summary>
    private IEnumerator showTipCor(List<Item> itemsToHighlight)
    {
        yield return new WaitForSeconds(1);

        gotTip = true;
        corCount++;
        
        // Don't show duplicate tips
        if (corCount > 1)
        {
            corCount--;
            yield break;
        }
        
        // Don't show tips when drag is blocked or not allowed
        if (LevelManager.THIS.DragBlocked && !allowShowTip)
        {
            corCount--;
            yield break;
        }
        
        tipID = LevelManager.THIS.moveID;
        
        yield return new WaitForSeconds(1);
        
        // Check if the game state changed while waiting
        if (LevelManager.THIS.DragBlocked && !allowShowTip && tipID != LevelManager.THIS.moveID)
        {
            corCount--;
            yield break;
        }
        
        // Check if all items still exist
        if (!AreAllItemsValid(itemsToHighlight))
        {
            corCount--;
            yield break;
        }
        
        // Show the tip animation on each item
        foreach (Item item in itemsToHighlight)
        {
            if (item != null)
                item.anim.SetTrigger("tip");
        }
        
        // Start checking for the next set of matches
        StartCoroutine(CheckPossibleCombines());
        corCount--;
        checkCombinesStarted = false;
    }
    
    /// <summary>
    /// Checks if all items in the list are still valid
    /// </summary>
    private bool AreAllItemsValid(List<Item> items)
    {
        foreach (Item item in items)
        {
            if (item == null)
                return false;
        }
        return true;
    }
    #endregion
}
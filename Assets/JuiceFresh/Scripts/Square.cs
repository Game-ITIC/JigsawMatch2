using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace JuiceFresh
{
    public enum FindSeparating
    {
        NONE = 0,
        HORIZONTAL,
        VERTICAL
    }

    public enum SquareTypes
    {
        NONE = 0,
        EMPTY,
        BLOCK,
        WIREBLOCK,
        SOLIDBLOCK,
        DOUBLESOLIDBLOCK,
        DOUBLEBLOCK,
        UNDESTROYABLE,
        THRIVING
    }

    /// <summary>
    /// Represents a square on the game grid
    /// Handles square behavior, matching logic, and obstacle interactions
    /// </summary>
    public class Square : MonoBehaviour
    {
        #region Properties and Fields
        [Header("References")]
        public Square square;
        public Item item;
        
        [Header("Position")]
        public int row;
        public int col;
        
        [Header("State")]
        public SquareTypes type;
        public int cageHP;
        public int cageHPPreview;
        
        [Header("Prefabs")]
        public GameObject boomPrefab;
        public GameObject icePrefab;
        
        [Header("Blocks")]
        public List<GameObject> block = new List<GameObject>();
        
        private bool cageActive;
        private int OldcageHP;
        private bool hightlighted;
        private bool justHighlighted;
        private bool canDamage;
        #endregion

        #region Unity Lifecycle
        void Start()
        {
            square = this;
        }

        void Update()
        {
            canDamage = cageActive;
        }

        void LateUpdate()
        {
            cageHPPreview = cageHP;
        }
        #endregion

        #region Item Generation and Management
        /// <summary>
        /// Generates a new item on this square
        /// </summary>
        /// <param name="falling">Should the item be created with falling animation</param>
        /// <returns>The created item</returns>
        public Item GenItem(bool falling = true)
        {
            if (IsNone() && !CanGoInto())
                return null;
                
            GameObject itemObj = Instantiate(LevelManager.THIS.itemPrefab) as GameObject;
            itemObj.transform.localScale = Vector2.one * 0.9f;
            itemObj.GetComponent<Item>().square = this;
            
            itemObj.transform.SetParent(transform.parent);
            
            if (falling)
            {
                itemObj.transform.position = transform.position + Vector3.back * 0.2f + Vector3.up * 3f;
                itemObj.GetComponent<Item>().justCreatedItem = true;
            }
            else
            {
                itemObj.transform.position = transform.position + Vector3.back * 0.2f;
            }

            this.item = itemObj.GetComponent<Item>();
            return this.item;
        }
        #endregion

        #region Neighbor Access
        public Square GetNeighborLeft(bool safe = false)
        {
            if (col == 0 && !safe)
                return null;
            return LevelManager.THIS.GetSquare(col - 1, row, safe);
        }

        public Square GetNeighborRight(bool safe = false)
        {
            if (col >= LevelManager.THIS.maxCols && !safe)
                return null;
            return LevelManager.THIS.GetSquare(col + 1, row, safe);
        }

        public Square GetNeighborTop(bool safe = false)
        {
            if (row == 0 && !safe)
                return null;
            return LevelManager.THIS.GetSquare(col, row - 1, safe);
        }

        public Square GetNeighborBottom(bool safe = false)
        {
            if (row >= LevelManager.THIS.maxRows && !safe)
                return null;
            return LevelManager.THIS.GetSquare(col, row + 1, safe);
        }

        public List<Square> GetAllNeghbors()
        {
            List<Square> sqList = new List<Square>();
            
            Square nextSquare = GetNeighborBottom();
            if (nextSquare != null)
                sqList.Add(nextSquare);
                
            nextSquare = GetNeighborTop();
            if (nextSquare != null)
                sqList.Add(nextSquare);
                
            nextSquare = GetNeighborLeft();
            if (nextSquare != null)
                sqList.Add(nextSquare);
                
            nextSquare = GetNeighborRight();
            if (nextSquare != null)
                sqList.Add(nextSquare);
                
            return sqList;
        }
        #endregion

        #region Match Finding
        Hashtable FindMoreMatches(int colorToMatch, Hashtable countedSquares, FindSeparating separating,
            Hashtable countedSquaresGlobal = null)
        {
            bool globalCounter = countedSquaresGlobal != null;
            
            if (!globalCounter)
            {
                countedSquaresGlobal = new Hashtable();
            }

            if (this.item == null || this.item.destroying)
                return countedSquares;
                
            if (this.item.color == colorToMatch && 
                !countedSquares.ContainsValue(this.item) &&
                this.item.currentType != ItemsTypes.INGREDIENT)
            {
                if (LevelManager.THIS.onlyFalling && this.item.justCreatedItem)
                    countedSquares.Add(countedSquares.Count - 1, this.item);
                else if (!LevelManager.THIS.onlyFalling)
                    countedSquares.Add(countedSquares.Count - 1, this.item);
                else
                    return countedSquares;

                CheckDirectionalMatches(colorToMatch, countedSquares, separating);
            }

            return countedSquares;
        }

        private void CheckDirectionalMatches(int colorToMatch, Hashtable countedSquares, FindSeparating separating)
        {
            if (separating == FindSeparating.HORIZONTAL)
            {
                Square leftNeighbor = GetNeighborLeft();
                if (leftNeighbor != null)
                    countedSquares = leftNeighbor.FindMoreMatches(colorToMatch, countedSquares, FindSeparating.HORIZONTAL);
                    
                Square rightNeighbor = GetNeighborRight();
                if (rightNeighbor != null)
                    countedSquares = rightNeighbor.FindMoreMatches(colorToMatch, countedSquares, FindSeparating.HORIZONTAL);
            }
            else if (separating == FindSeparating.VERTICAL)
            {
                Square topNeighbor = GetNeighborTop();
                if (topNeighbor != null)
                    countedSquares = topNeighbor.FindMoreMatches(colorToMatch, countedSquares, FindSeparating.VERTICAL);
                    
                Square bottomNeighbor = GetNeighborBottom();
                if (bottomNeighbor != null)
                    countedSquares = bottomNeighbor.FindMoreMatches(colorToMatch, countedSquares, FindSeparating.VERTICAL);
            }
        }

        /// <summary>
        /// Finds all matching items around this square
        /// </summary>
        public List<Item> FindMatchesAround(FindSeparating separating = FindSeparating.NONE, int minimumMatches = 3,
            Hashtable countedSquaresGlobal = null)
        {
            bool globalCounter = countedSquaresGlobal != null;
            List<Item> newList = new List<Item>();
            
            if (!globalCounter)
            {
                countedSquaresGlobal = new Hashtable();
            }

            Hashtable countedSquares = new Hashtable();
            countedSquares.Clear();
            
            if (this.item == null)
                return newList;
                
            // Check horizontal matches
            if (separating != FindSeparating.VERTICAL)
            {
                countedSquares = this.FindMoreMatches(this.item.color, countedSquares, FindSeparating.HORIZONTAL,
                    countedSquaresGlobal);
            }

            foreach (DictionaryEntry de in countedSquares)
            {
                LevelManager.THIS.countedSquares.Add(LevelManager.THIS.countedSquares.Count - 1, de.Value);
            }

            if (countedSquares.Count < minimumMatches)
                countedSquares.Clear();

            // Check vertical matches
            if (separating != FindSeparating.HORIZONTAL)
            {
                countedSquares = this.FindMoreMatches(this.item.color, countedSquares, FindSeparating.VERTICAL,
                    countedSquaresGlobal);
            }

            foreach (DictionaryEntry de in countedSquares)
            {
                LevelManager.THIS.countedSquares.Add(LevelManager.THIS.countedSquares.Count - 1, de.Value);
            }

            if (countedSquares.Count < minimumMatches)
                countedSquares.Clear();
                
            // Convert to list
            foreach (DictionaryEntry de in countedSquares)
            {
                newList.Add((Item)de.Value);
            }

            return newList;
        }
        #endregion

        #region Visual Effects
        public void HighLight(bool on, float col = 0.5f)
        {
            if (on)
            {
                GetComponent<SpriteRenderer>().color = new Color(col, 0.65f, 0);
                if (!hightlighted)
                    StartCoroutine(AnimateHighlight(col));
            }
            else
            {
                StopCoroutine(AnimateHighlight(col));
                GetComponent<SpriteRenderer>().color = new Color(1, 1, 1);
            }

            hightlighted = on;
            SetActiveCage(on);
        }

        IEnumerator AnimateHighlight(float col = 0.8f)
        {
            GetComponent<SpriteRenderer>().color = new Color(col, 0.65f, 0);
            while (hightlighted)
            {
                yield return new WaitForSeconds(0.3f);
                GetComponent<SpriteRenderer>().color = new Color(col, 0.65f, 0);
                yield return new WaitForSeconds(0.3f);
                GetComponent<SpriteRenderer>().color = new Color(col - 0.1f, col - 0.1f, col - 0.1f);
            }
        }
        #endregion

        #region Item Movement
        public void FallInto()
        {
            // Reserved for future functionality
        }

        /// <summary>
        /// Checks if this square's item can fall to a square below
        /// </summary>
        public void FallOut()
        {
            if (item != null && type != SquareTypes.WIREBLOCK)
            {
                Square nextSquare = GetNeighborBottom();
                if (nextSquare != null)
                {
                    if (nextSquare.IsNone())
                    {
                        nextSquare = FindBottomMostValidSquare();
                    }

                    if (nextSquare.CanFallInto() && nextSquare.item == null)
                    {
                        item.CheckNeedToFall(nextSquare);
                    }
                }
            }
        }

        private Square FindBottomMostValidSquare()
        {
            Square nextSquare = null;
            
            for (int i = row + 1; i < LevelManager.THIS.maxRows; i++)
            {
                if (LevelManager.THIS.GetSquare(col, i) != null)
                {
                    if (!LevelManager.THIS.GetSquare(col, i).IsNone())
                    {
                        nextSquare = LevelManager.THIS.GetSquare(col, i);
                        break;
                    }
                }
            }
            
            return nextSquare ?? GetNeighborBottom();
        }
        #endregion

        #region Square Type Checking
        public bool IsNone()
        {
            return type == SquareTypes.NONE;
        }

        public bool IsHaveDestroybleObstacle()
        {
            return type == SquareTypes.SOLIDBLOCK || type == SquareTypes.THRIVING;
        }

        public bool CanGoOut()
        {
            return type != SquareTypes.WIREBLOCK;
        }

        public bool CanGoInto()
        {
            return type != SquareTypes.SOLIDBLOCK && 
                  type != SquareTypes.UNDESTROYABLE && 
                  type != SquareTypes.NONE &&
                  type != SquareTypes.THRIVING;
        }

        public bool CanFallInto()
        {
            return type != SquareTypes.WIREBLOCK && 
                  type != SquareTypes.SOLIDBLOCK &&
                  type != SquareTypes.UNDESTROYABLE && 
                  type != SquareTypes.NONE && 
                  type != SquareTypes.THRIVING;
        }

        public bool IsHaveSolidAbove()
        {
            for (int i = row; i >= 0; i--)
            {
                SquareTypes squareType = LevelManager.THIS.GetSquare(col, i).type;
                if (squareType == SquareTypes.WIREBLOCK ||
                    squareType == SquareTypes.SOLIDBLOCK ||
                    squareType == SquareTypes.UNDESTROYABLE ||
                    squareType == SquareTypes.THRIVING)
                {
                    return true;
                }
            }

            return false;
        }

        public bool IsExtraItem()
        {
            if (item != null)
            {
                return item.IsExtraItem();
            }

            return false;
        }
        #endregion

        #region Block Destruction
        /// <summary>
        /// Destroys the block on this square
        /// </summary>
        public void DestroyBlock()
        {
            if (type == SquareTypes.UNDESTROYABLE)
                return;
                
            // Handle adjacent solid blocks
            if (type != SquareTypes.SOLIDBLOCK && type != SquareTypes.THRIVING)
            {
                DestroyAdjacentSolidBlocks();
            }

            if (block.Count <= 0)
                return;

            // Handle specific block types
            HandleBlockDestruction();
        }

        private void DestroyAdjacentSolidBlocks()
        {
            List<Square> sqList = GetAllNeghbors();
            foreach (Square sq in sqList)
            {
                if (sq.type == SquareTypes.SOLIDBLOCK || sq.type == SquareTypes.THRIVING)
                    sq.DestroyBlock();
            }
        }

        private void HandleBlockDestruction()
        {
            if (type == SquareTypes.BLOCK)
            {
                DestroyNormalBlock();
            }
            else if (type == SquareTypes.WIREBLOCK)
            {
                DestroyWireBlock();
            }
            else if (type == SquareTypes.SOLIDBLOCK)
            {
                DestroySolidBlock();
            }
            else if (type == SquareTypes.THRIVING)
            {
                DestroyThrivingBlock();
            }
            else
            {
                DestroyGenericBlock();
            }
            
            UpdateBlockState();
        }

        private void DestroyNormalBlock()
        {
            LevelManager.THIS.CheckCollectedTarget(block[block.Count - 1].gameObject);
            LevelManager.THIS.PopupScore(LevelManager.THIS.scoreForBlock, transform.position, 0);
            LevelManager.THIS.TargetBlocks--;
            block[block.Count - 1].GetComponent<SpriteRenderer>().enabled = false;
        }

        private void DestroyWireBlock()
        {
            if (cageHP > 0)
                return;
                
            LevelManager.THIS.TargetCages--;
            LevelManager.THIS.PopupScore(LevelManager.THIS.scoreForWireBlock, transform.position, 0);
        }

        private void DestroySolidBlock()
        {
            LevelManager.THIS.PopupScore(LevelManager.THIS.scoreForSolidBlock, transform.position, 0);
            PlayBlockDestructionAnimation();
        }

        private void DestroyThrivingBlock()
        {
            LevelManager.THIS.PopupScore(LevelManager.THIS.scoreForThrivingBlock, transform.position, 0);
            
            block[block.Count - 1].SetActive(false);
            GameObject boom = Instantiate(boomPrefab, transform.position, Quaternion.identity) as GameObject;
            Destroy(boom, 1);
        }

        private void DestroyGenericBlock()
        {
            PlayBlockDestructionAnimation();
        }

        private void PlayBlockDestructionAnimation()
        {
            SoundBase.Instance.PlaySound(SoundBase.Instance.block_destroy);
            
            if (type != SquareTypes.THRIVING)
            {
                block[block.Count - 1].GetComponent<Animation>().Play("BrickRotate");
                block[block.Count - 1].GetComponent<SpriteRenderer>().sortingOrder = 4;
                block[block.Count - 1].AddComponent<Rigidbody2D>();
                block[block.Count - 1].GetComponent<Rigidbody2D>().AddRelativeForce(
                    new Vector2(UnityEngine.Random.insideUnitCircle.x * UnityEngine.Random.Range(30, 200),
                        UnityEngine.Random.Range(100, 150)), ForceMode2D.Force);
            }
        }

        private void UpdateBlockState()
        {
            GameObject.Destroy(block[block.Count - 1], 1.5f);
            block.Remove(block[block.Count - 1]);
            
            if (block.Count > 0)
                type = block[block.Count - 1].GetComponent<Square>().type;
            else
                type = SquareTypes.EMPTY;
        }
        #endregion

        #region Cage Management
        public void SetCage(int cageHP_)
        {
            cageHP = cageHP_;
            OldcageHP = cageHP;
        }

        /// <summary>
        /// Activates or deactivates the cage on this square
        /// </summary>
        public void SetActiveCage(bool active, bool _justHighlighted = false)
        {
            if (type == SquareTypes.WIREBLOCK)
            {
                justHighlighted = _justHighlighted;
                cageActive = active;
                
                if (active)
                {
                    OldcageHP = cageHP;
                }
            }
        }

        /// <summary>
        /// Checks if the cage on this square will be destroyed with additional damage
        /// </summary>
        public bool IsCageGoingToBroke(int extraDamage = 0)
        {
            if (type == SquareTypes.WIREBLOCK && cageHP >= 0)
            {
                int damage = LevelManager.THIS.destroyAnyway.Count + extraDamage;
                int estimateCageHP = Mathf.Clamp(cageHP - damage, 0, cageHP);
                OldcageHP = cageHP;
                AddDagame(damage);
                cageActive = false;

                return estimateCageHP == 0;
            }

            return true;
        }

        /// <summary>
        /// Checks if the cage will be destroyed after taking damage
        /// </summary>
        public bool CheckDamage(int damage)
        {
            if (type == SquareTypes.WIREBLOCK && cageHP >= 0)
            {
                int estimateCageHP = Mathf.Clamp(cageHP - damage, 0, cageHP);
                canDamage = true;
                OldcageHP = cageHP;
                AddDagame(damage);
                
                return estimateCageHP == 0;
            }

            return true;
        }

        public void AddDagame(int damage)
        {
            BurstIceParticles();
            canDamage = false;
            cageHP = OldcageHP - damage;
            
            if (cageHP < 0)
                cageHP = 0;
        }

        private void BurstIceParticles()
        {
            iTween.ShakePosition(block[block.Count - 1].gameObject, Vector3.one * 0.3f, 0.2f);
            GameObject ice = Instantiate(icePrefab, transform.position, Quaternion.identity) as GameObject;
            SoundBase.Instance.PlaySound(SoundBase.Instance.iceCrack);

            Destroy(ice, 1);
        }
        #endregion

        #region Special Blocks
        public void GenThriveBlock(Square newSquare)
        {
            // Reserved for future functionality
        }
        #endregion
    }
}
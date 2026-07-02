using UnityEngine;
using UI;

namespace JuiceFresh.States
{
    public class WaitForPopupState : GameStateBase
    {
        private ILevelManagerActions _levelManagerActions;
        public WaitForPopupState(LevelManager levelManager) : base(levelManager)
        {
            _levelManagerActions = levelManager;
        }

        public override void EnterState()
        {
            InitLevel();

            LevelManager.TriggerOnLevelLoaded();

            float defaultScale = levelManager.GetComponent<Camera>().orthographicSize;
            ScaleCamera(defaultScale);
        }

        public override void UpdateState()
        {
            // No specific update logic for this state
        }

        public override void ExitState()
        {
            // Nothing specific to clean up
        }

        private void InitLevel()
        {
            _levelManagerActions.GenerateLevel();
            _levelManagerActions.GenerateNewItems(false);
            levelManager.nextExtraItems = 0;
            levelManager.bombTimers.Clear();

            RestartTimer();

            InitTargets();

            levelManager.GameField.gameObject.SetActive(true);
            PlayGameFieldIntro();
        }

        private void PlayGameFieldIntro()
        {
            GameFieldTweenAnimation animation = levelManager.GameField.GetComponent<GameFieldTweenAnimation>();
            if(animation == null)
            {
                levelManager.GameField.localPosition = levelManager.GameFieldTargetLocalPosition;
                _levelManagerActions.GenerateOutline();
                levelManager.gameStatus = GameState.PrepareBoosts;
                return;
            }

            animation.Play(levelManager.GameFieldTargetLocalPosition,
                () =>
                {
                    _levelManagerActions.GenerateOutline();
                    levelManager.gameStatus = GameState.PrepareBoosts;
                });
        }

        private void RestartTimer()
        {
            if (levelManager.limitType == LIMIT.TIME)
            {
                levelManager.StopCoroutine("TimeTick");
                levelManager.StartCoroutine("TimeTick");
            }
        }

        private void InitTargets()
        {
            SetActiveIfAssigned(levelManager.blocksObject, false);
            SetActiveIfAssigned(levelManager.ingrObject, false);
            SetActiveIfAssigned(levelManager.scoreTargetObject, false);
            SetActiveIfAssigned(levelManager.cageTargetObject, false);
            SetActiveIfAssigned(levelManager.bombTargetObject, false);

            foreach (GameObject item in levelManager.listIngredientsGUIObjects)
            {
                Object.Destroy(item);
            }

            levelManager.listIngredientsGUIObjects.Clear();

            SetupTargetUI();
        }

        private void SetupTargetUI()
        {
            if (levelManager.target != Target.COLLECT && levelManager.target != Target.ITEMS)
            {
                SetActiveIfAssigned(levelManager.ingrObject, false);
            }
            else if (levelManager.target == Target.COLLECT)
            {
                SetActiveIfAssigned(levelManager.blocksObject, false);
                CreateCollectableTargetIfAssigned();
            }
            else if (levelManager.target == Target.ITEMS)
            {
                SetActiveIfAssigned(levelManager.blocksObject, false);
                CreateCollectableTargetIfAssigned();
            }

            if (levelManager.targetBlocks > 0 && levelManager.target == Target.BLOCKS)
            {
                SetActiveIfAssigned(levelManager.blocksObject, true);
                SetTargetCount(levelManager.blocksObject, levelManager.targetBlocks);
            }
            else if (levelManager.target == Target.CAGES)
            {
                SetActiveIfAssigned(levelManager.cageTargetObject, true);
                SetTargetCount(levelManager.cageTargetObject, levelManager.TargetCages);
            }
            else if (levelManager.target == Target.BOMBS)
            {
                levelManager.StartCoroutine(_levelManagerActions.InitBombs());
                SetActiveIfAssigned(levelManager.bombTargetObject, true);
                SetTargetCount(levelManager.bombTargetObject, levelManager.bombsCollect);
            }
            else if (levelManager.target == Target.SCORE)
            {
                SetActiveIfAssigned(levelManager.ingrObject, false);
                SetActiveIfAssigned(levelManager.blocksObject, false);
                SetActiveIfAssigned(levelManager.scoreTargetObject, true);
                SetActiveIfAssigned(levelManager.cageTargetObject, false);
                SetActiveIfAssigned(levelManager.bombTargetObject, false);
            }
        }

        private void CreateCollectableTargetIfAssigned()
        {
            if (levelManager.ingrObject != null)
            {
                _levelManagerActions.CreateCollectableTarget(levelManager.ingrObject, levelManager.target, false);
            }
        }

        private static void SetTargetCount(GameObject target, int count)
        {
            if (target == null)
            {
                return;
            }

            TargetGUI targetGui = target.GetComponent<TargetGUI>();
            if (targetGui == null || targetGui.text == null)
            {
                return;
            }

            Counter_ counter = targetGui.text.GetComponent<Counter_>();
            if (counter != null)
            {
                counter.totalCount = count;
            }
        }

        private static void SetActiveIfAssigned(GameObject target, bool active)
        {
            if (target != null)
            {
                target.SetActive(active);
            }
        }

        private void ScaleCamera(float defaultScale)
        {
            // Get the width of the playfield
            Square leftSquare = _levelManagerActions.GetSquare(0, 0);
            Square rightSquare = _levelManagerActions.GetSquare(levelManager.maxCols - 1, 0);
        
            if (leftSquare == null || rightSquare == null)
                return;
            
            float leftX = leftSquare.transform.position.x;
            float rightX = rightSquare.transform.position.x;

            // Include half-cell padding so edge cells are not clipped.
            float cellWidth = Mathf.Max(0.001f, levelManager.squareWidth);
            float gridWorldWidth = Mathf.Abs(rightX - leftX) + cellWidth;

            // Use safe area dimensions so phones with gesture/notch areas keep comfortable margins.
            Rect safe = Screen.safeArea;
            float safeWidth = Mathf.Max(1f, safe.width);
            float safeHeight = Mathf.Max(1f, safe.height);
            float safeAspect = safeHeight / safeWidth;

            // Extra breathing room for swipes (in world units).
            float swipePadding = Mathf.Max(1.25f, cellWidth * 0.85f);

            // Required orthographic size to fit the board width inside the safe area.
            float requiredOrtho = (gridWorldWidth * safeAspect) * 0.5f + swipePadding;

            // Set the camera size, clamped between the default and calculated values
            levelManager.GetComponent<Camera>().orthographicSize = Mathf.Max(defaultScale, requiredOrtho);
        }
    }
}

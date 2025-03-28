using UnityEngine;

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
            _levelManagerActions.GenerateOutline();
            _levelManagerActions.GenerateNewItems(false);
            levelManager.nextExtraItems = 0;
            levelManager.bombTimers.Clear();

            RestartTimer();

            InitTargets();

            levelManager.GameField.gameObject.SetActive(true);
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
            levelManager.blocksObject.SetActive(false);
            levelManager.ingrObject.SetActive(false);
            levelManager.scoreTargetObject.SetActive(false);
            levelManager.cageTargetObject.SetActive(false);
            levelManager.bombTargetObject.SetActive(false);

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
                levelManager.ingrObject.SetActive(false);
            }
            else if (levelManager.target == Target.COLLECT)
            {
                levelManager.blocksObject.SetActive(false);
                _levelManagerActions.CreateCollectableTarget(levelManager.ingrObject, levelManager.target, false);
            }
            else if (levelManager.target == Target.ITEMS)
            {
                levelManager.blocksObject.SetActive(false);
                _levelManagerActions.CreateCollectableTarget(levelManager.ingrObject, levelManager.target, false);
            }

            if (levelManager.targetBlocks > 0 && levelManager.target == Target.BLOCKS)
            {
                levelManager.blocksObject.SetActive(true);
                levelManager.blocksObject.GetComponent<TargetGUI>().text.GetComponent<Counter_>().totalCount =
                    levelManager.targetBlocks;
            }
            else if (levelManager.target == Target.CAGES)
            {
                levelManager.cageTargetObject.SetActive(true);
                levelManager.cageTargetObject.GetComponent<TargetGUI>().text.GetComponent<Counter_>().totalCount =
                    levelManager.TargetCages;
            }
            else if (levelManager.target == Target.BOMBS)
            {
                levelManager.StartCoroutine(_levelManagerActions.InitBombs());
                levelManager.bombTargetObject.SetActive(true);
                levelManager.bombTargetObject.GetComponent<TargetGUI>().text.GetComponent<Counter_>().totalCount =
                    levelManager.bombsCollect;
            }
            else if (levelManager.target == Target.SCORE)
            {
                levelManager.ingrObject.SetActive(false);
                levelManager.blocksObject.SetActive(false);
                levelManager.scoreTargetObject.SetActive(true);
                levelManager.cageTargetObject.SetActive(false);
                levelManager.bombTargetObject.SetActive(false);
            }
        }

        private void ScaleCamera(float defaultScale)
        {
            // Get the width of the playfield
            Square leftSquare = _levelManagerActions.GetSquare(0, 0);
            Square rightSquare = _levelManagerActions.GetSquare(levelManager.maxCols - 1, 0);
        
            if (leftSquare == null || rightSquare == null)
                return;
            
            float width = rightSquare.transform.position.x - leftSquare.transform.position.x;
        
            // Calculate the required orthographic size based on the aspect ratio
            float h = width * Screen.height / Screen.width / 2 + 1.5f;
        
            // Set the camera size, clamped between the default and calculated values
            levelManager.GetComponent<Camera>().orthographicSize = Mathf.Clamp(h, defaultScale, h);
        }
    }
}
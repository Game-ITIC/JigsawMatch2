using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Threading;
using Configs;
using Cysharp.Threading.Tasks;
using JuiceFresh;
using JuiceFresh.Scripts;
using JuiceFresh.States;
using Models;
using Monobehaviours;
using Providers;
using Services;
using Systems;
using UI;
using UnityEngine.UI;
using VContainer;
using Views;
using ZLinq;

// Data structure for square blocks in the level
public class SquareBlocks
{
    public SquareTypes block;
    public SquareTypes obstacle;
}

// Game state enum
public enum GameState
{
    Map,
    PrepareGame,
    PrepareBoosts,
    Playing,
    Highscore,
    GameOver,
    Pause,
    PreWinAnimations,
    Win,
    WaitForPopup,
    WaitAfterClose,
    BlockedGame,
    Tutorial,
    PreTutorial,
    WaitForPotion,
    PreFailed,
    PreFailedBomb,
    RegenLevel,
    ToMap
}

public class LevelManager : MonoBehaviour, ILevelManagerActions
{
    #region Static References

    // Instance of LevelManager for direct references
    public static LevelManager THIS;

    // Instance of LevelManager for direct references
    public static LevelManager Instance;

    // Global Score amount on current level
    public static int Score;

    #endregion

    #region Prefabs and Objects

    public List<CollectedIngredients> collectedIngredients = new List<CollectedIngredients>();

    // Prefab of item
    public GameObject itemPrefab;

    // Prefab of square
    public GameObject squarePrefab;

    // Prefab of block, for more info check enum SquareTypes
    public GameObject blockPrefab;

    // Prefab of wire, for more info check enum SquareTypes
    public GameObject wireBlockPrefab;

    // Prefab of solid block, for more info check enum SquareTypes
    public GameObject solidBlockPrefab;

    // Prefab of undestroyable block, for more info check enum SquareTypes
    public GameObject undesroyableBlockPrefab;

    // Prefab of growing block, for more info check enum SquareTypes
    public GameObject thrivingBlockPrefab;

    // Life shop scene object
    public LifeShop lifeShop;

    // Gamefield scene object
    public Transform GameField;

    public Vector3 GameFieldTargetLocalPosition { get; private set; }

    // Scene object
    public GameObject LevelsMap;

    // UI object
    public GameObject Level;

    // UI objects for targets
    public GameObject ingrObject;
    public GameObject blocksObject;
    public GameObject scoreTargetObject;
    public GameObject cageTargetObject;
    public GameObject bombTargetObject;

    // Camera
    public Camera gameCamera;

    // Line renderer
    public Line line;

    #endregion

    #region Sprites

    // Sprite of square
    public Sprite squareSprite;

    // Second sprite of square
    public Sprite squareSprite1;

    // Outline side border for squares
    public Sprite outline1;

    // Top-left outline corner for squares
    public Sprite outline2;

    // Bottom-right outline corner for squares
    public Sprite outline3;

    // Top-right outline corner for squares
    public Sprite outline4;

    // Bottom-left outline corner for squares
    public Sprite outline5;

    // Sprites of collectable items
    public Sprite[] ingrediendSprites;

    // Double block sprite
    public Sprite doubleBlock;

    // Double solid block sprite
    public Sprite doubleSolidBlock;

    #endregion

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

    #region Level Configuration

    // Enabling iapps flag
    public bool enableInApps;

    // Type of game limit (moves or time)
    public LIMIT limitType;

    // Value of rest limit (moves or time)
    public int Limit = 30;

    // Current level number
    public int currentLevel = 1;

    // Cost of continue playing after fail
    public int FailedCost;

    // Extra moves that you get to continue game after fail
    public int ExtraFailedMoves = 5;

    // Extra seconds that you get to continue game after fail
    public int ExtraFailedSecs = 30;

    // Max rows of gamefield
    public int maxRows = 8;

    // Max cols of gamefield
    public int maxCols = 7;

    // Right square size for level generation
    public float squareWidth = 1.2f;

    // Right square size for level generation
    public float squareHeight = 1.2f;

    // Position of the first square on the game field
    public Vector2 firstSquarePosition;

    // Editor variable for limitation of colors
    public int colorLimit;

    // Editor values of description tasks
    public string[] targetDiscriptions;

    // Facebook enable
    public bool FacebookEnable;

    // PlayFab
    public bool PlayFab;

    // Extra item every
    public float extraItemEvery = 6;

    // Bomb timer
    public int bombTimer;

    // Number of ingredients
    public int NumIngredients = 4;

    #endregion

    #region Game State and Targets

    public Target target;

    // Inner using
    private GameState GameStatus;

    // Amount of scores for item
    public int scoreForItem = 10;

    // Amount of scores for block
    public int scoreForBlock = 100;

    // Amount of scores for wire block
    public int scoreForWireBlock = 100;

    // Amount of scores for solid block
    public int scoreForSolidBlock = 100;

    // Amount of scores for growing block
    public int scoreForThrivingBlock = 100;

    // Array of colors for popup scores
    public Color[] scoresColors;

    // Stars amount on current level
    public int stars;

    // Amount of scores is necessary for reaching first star
    public int star1;

    // Amount of scores is necessary for reaching second star
    public int star2;

    // Amount of scores is necessary for reaching third star
    public int star3;

    // Get stars
    public CollectStars starsTargetCount;

    // Amount of blocks for collecting
    public int targetBlocks;

    // Target cages
    public int TargetCages;

    // Target bombs
    public int TargetBombs;

    #endregion

    #region Items and Squares

    // Array for holding squares data of the game field
    public Square[] squaresArray;

    // Array of combined items
    public List<List<Item>> combinedItems = new List<List<Item>>();

    // Latest touched item
    public Item lastDraggedItem;

    // Array for items prepeared to destory
    public List<Item> destroyAnyway = new List<Item>();

    // Level data from the file
    SquareBlocks[] levelSquaresFile = new SquareBlocks[81];

    // Array of highlighted items
    public List<Item> highlightedItems;

    // Necessary amount of collectable items
    public int[] ingrCountTarget = new int[4];

    // Necessary collectable items
    public List<CollectedIngredients> ingrTarget = new List<CollectedIngredients>();

    // Necessary collectable items
    public CollectItems[] collectItems = new CollectItems[6];

    // List of GameObject for UI ingredients
    public List<GameObject> listIngredientsGUIObjects = new List<GameObject>();

    // List of types
    public List<ItemsTypes> gatheredTypes = new List<ItemsTypes>();

    // List of vector3 for flowers
    public List<Vector3> startPosFlowers = new List<Vector3>();

    #endregion

    #region Boosts and Effects

    // Amount of boosts
    public int BoostColorfullBomb;

    // Amount of boosts
    public int BoostPackage;

    // Amount of boosts
    public int BoostStriped;

    // Inner using variable
    public BoostIcon emptyBoostIcon;

    // Currently active boost
    public BoostIcon activatedBoost;

    // Array of in-game boosts
    public BoostIcon[] InGameBoosts;

    // Waiting boost
    public BoostIcon waitingBoost;

    #endregion

    #region Game Data

    // Array of iapps products
    public List<GemProduct> gemsProducts = new List<GemProduct>();

    // List of bomb timers
    public List<int> bombTimers = new List<int>();

    // Level pass counter
    public int passLevelCounter;

    // Move ID
    public int moveID;

    // Last random color
    public int lastRandColor;

    // Selected color
    public int selectedColor;

    // Inner using
    public int nextExtraItems;

    // Cage HP
    private int cageHP;

    // Deprecated
    private int linePoint;

    // Items hidden flag
    public bool itemsHided;

    // Only falling flag
    public bool onlyFalling;

    // Level loaded flag
    public bool levelLoaded;

    // Counted squares
    public Hashtable countedSquares;

    CollectedTargetFlyController _collectedTargetFlyController;
    ScorePopupTweenSpawner _scorePopupTweenSpawner;
    LevelEffectsController _levelEffectsController;

    public ScorePopupTweenSpawner ScorePopupTweenSpawner
    {
        get
        {
            if(_scorePopupTweenSpawner == null)
            {
                _scorePopupTweenSpawner = GetComponent<ScorePopupTweenSpawner>();
            }

            return _scorePopupTweenSpawner;
        }
    }

    public LevelEffectsController LevelEffectsController
    {
        get
        {
            if(_levelEffectsController == null)
            {
                _levelEffectsController = GetComponent<LevelEffectsController>();
            }

            return _levelEffectsController;
        }
    }

    // Kept as a property for the board state checks that wait for target flights to finish.
    public bool ingredientFly => _collectedTargetFlyController != null &&
                                 _collectedTargetFlyController.IsFlying;

    // Test by play flag
    public bool testByPlay;

    // Extra cage add item
    public int extraCageAddItem;

    // Bombs collect
    public int bombsCollect;

    // Stop sliding flag
    private bool stopSliding;

    // Offset
    private float offset;

    #endregion

    #region Board Mechanics

    private BoardMechanicsService _boardMechanicsService;

    public BoardMechanicsService BoardMechanics
    {
        get { return _boardMechanicsService; }
    }

    private Dictionary<GameState, GameStateBase> _states;
    private GameStateBase _currentState;

    #endregion

    #region Properties

    // Is any growing blocks destroyed in that turn
    public bool thrivingBlockDestroyed;

    // Is touch blocks
    public bool dragBlocked;

    public bool DragBlocked
    {
        get { return dragBlocked; }
        set
        {
            if(value)
            {
                List<Item> items = GetItems();

                foreach (Item item in items)
                {
                    // if (item != null)
                    //    item.anim.SetBool("stop", true);
                }
            }
            else
            {
                // StartCoroutine(StartIdleCor());
            }

            dragBlocked = value;
        }
    }

    public int TargetBlocks
    {
        get { return targetBlocks; }
        set
        {
            if(targetBlocks < 0)
                targetBlocks = 0;
            targetBlocks = value;
        }
    }

    public GameState gameStatus
    {
        get { return GameStatus; }
        set
        {
            Debug.Log("Changing game state from " + GameStatus + " to " + value);

            if(_currentState != null)
            {
                Debug.Log("Exiting current state: " + _currentState.GetType().Name);
                _currentState.ExitState();
            }

            if(gameObject.activeInHierarchy)
            {
                StopAllCoroutines();
            }

            GameStatus = value;

            if(_states != null && _states.ContainsKey(value))
            {
                _currentState = _states[value];

                Debug.Log("Entering new state: " + _currentState.GetType().Name);
                _currentState.EnterState();
            }
            else
            {
                Debug.Log("Handling legacy state: " + value);
                HandleLegacyState(value);
            }
        }
    }

    [Inject] public BoostersProvider BoostersProvider;
    [Inject] public CoinModel CoinModel;
    [Inject] public Models.StarModel StarModel;
    [Inject] public GameEventDispatcher GameEventDispatcher;
    [Inject] public AdRewardService AdRewardService;
    [Inject] public Itic.Scopes.SceneLoader SceneLoader;
    [Inject] public IronSourceManager IronSourceManager;
    [Inject] public GameProvider GameProvider;
    [Inject] public GameCompleteView GameCompleteView;
    [Inject] public GameConfig GameConfig;

    [Inject] public HealthSystem HealthSystem;

    // Field of getting and setting currently activated boost
    public BoostIcon ActivatedBoost
    {
        get
        {
            if(activatedBoost == null)
            {
                return emptyBoostIcon;
            }
            else
                return activatedBoost;
        }
        set
        {
            if(value == null)
            {
                // if (activatedBoost != null && gameStatus == GameState.Playing)
                //     InitScript.Instance.SpendBoost(activatedBoost.type);
                UnLockBoosts();
            }

            activatedBoost = value;

            if(value != null)
            {
                LockBoosts();
            }

            if(activatedBoost != null)
            {
                if(activatedBoost.type == BoostType.ExtraMoves)
                {
                    //here is checking what kind of limitation in level - Moves or Time
                    if(LevelManager.Instance.limitType == LIMIT.MOVES)
                    {
                        var model = BoostersProvider.BoostersModels.AsValueEnumerable()
                            .First(t => t.Type == BoostType.ExtraMoves);

                        if(model.HasBooster())
                        {
                            LevelManager.THIS.Limit += 5;
                            model.Use();
                        }
                    }
                    // else
                    // LevelManager.THIS.Limit += 30;

                    ActivatedBoost = null;
                }
            }
        }
    }

    #endregion

    #region Events

    public delegate void GameStateEvents();

    public static event GameStateEvents OnMapState;
    public static event GameStateEvents OnEnterGame;
    public static event GameStateEvents OnLevelLoaded;
    public static event GameStateEvents OnMenuPlay;
    public static event GameStateEvents OnMenuComplete;
    public static event GameStateEvents OnTouchDetected;
    public static event GameStateEvents OnWin;
    public static event GameStateEvents OnLose;

    // Methods to trigger each event
    public static void TriggerOnMapState()
    {
        OnMapState?.Invoke();
    }

    public static void TriggerOnEnterGame()
    {
        OnEnterGame?.Invoke();
    }

    public static void TriggerOnLevelLoaded()
    {
        OnLevelLoaded?.Invoke();
    }

    public static void TriggerOnMenuPlay()
    {
        OnMenuPlay?.Invoke();
    }

    public static void TriggerOnMenuComplete()
    {
        OnMenuComplete?.Invoke();
    }

    public static void TriggerOnTouchDetected()
    {
        OnTouchDetected?.Invoke();
    }

    public static void TriggerOnWin()
    {
        OnWin?.Invoke();
    }

    public static void TriggerOnLose()
    {
        OnLose?.Invoke();
    }

    #endregion

    #region Unity Lifecycle Methods

    public void InvokeStart()
    {
        ResolveTargetUIReferences();
        InitializeCollectedTargetFlyController();
        _scorePopupTweenSpawner = GetComponent<ScorePopupTweenSpawner>();
        _levelEffectsController = GetComponent<LevelEffectsController>();
        _levelEffectsController?.Initialize(this);

        _boardMechanicsService = new BoardMechanicsService(this);

        ingrCountTarget = new int[NumIngredients]; // Necessary amount of collectable items

        if(Level.gameObject.activeSelf)
            Level.gameObject.SetActive(false);

        CoroutineManager.Instance.GetType();

        _states = new Dictionary<GameState, GameStateBase>
        {
            {
                GameState.Map, new MapState(this)
            },
            {
                GameState.PrepareGame, new PrepareGameState(this)
            },
            {
                GameState.WaitForPopup, new WaitForPopupState(this)
            },
            {
                GameState.PrepareBoosts, new PrepareBoostsState(this)
            },
            {
                GameState.Playing, new PlayingState(this)
            },
            {
                GameState.GameOver, new GameOverState(this, AdRewardService, SceneLoader)
            },
            {
                GameState.PreWinAnimations, new PreWinAnimationsState(this)
            },
            {
                GameState.Win, new WinState(this)
            },
            {
                GameState.RegenLevel, new RegenLevelState(this)
            }
        };

        THIS = this;
        Instance = this;
        // gameStatus = GameState.Map;

        passLevelCounter = 0;

        gameStatus = GameState.PrepareGame;
        LoadLevel();
    }

    void InitializeCollectedTargetFlyController()
    {
        if (_collectedTargetFlyController == null)
        {
            _collectedTargetFlyController = GetComponent<CollectedTargetFlyController>();
        }

        if (_collectedTargetFlyController == null)
        {
            _collectedTargetFlyController = gameObject.AddComponent<CollectedTargetFlyController>();
        }

        _collectedTargetFlyController.Initialize(this);
    }

    private void ResolveTargetUIReferences()
    {
        if (Level == null)
        {
            return;
        }

        Transform topBarTransform = Level.transform.Find("Canvas/Panel/TopBarPanel");
        if (topBarTransform != null)
        {
            AssignIfMissing(ref ingrObject, topBarTransform, "Mission/TargetIngr", "TargetIngr");
            AssignIfMissing(ref blocksObject, topBarTransform, "Mission/TargetBlocks", "TargetBlocks");
            AssignIfMissing(ref scoreTargetObject, topBarTransform, "Mission/TargetScore", "TargetScore");
            AssignIfMissing(ref cageTargetObject, topBarTransform, "Mission/TargetCages", "TargetCages");
            AssignIfMissing(ref bombTargetObject, topBarTransform, "Mission/TargetBombs", "TargetBombs");
        }

        Transform prePlayTransform = Level.transform.Find("Canvas/PrePlay");
        if (prePlayTransform != null && prePlayTransform.TryGetComponent(out PrePlay prePlay))
        {
            if (ingrObject == null)
                ingrObject = prePlay.ingrObject;
            if (blocksObject == null)
                blocksObject = prePlay.blocksObject;
            if (scoreTargetObject == null)
                scoreTargetObject = prePlay.scoreTargetObject;
            if (cageTargetObject == null)
                cageTargetObject = prePlay.cage;
            if (bombTargetObject == null)
                bombTargetObject = prePlay.bomb;
        }
    }

    private static void AssignIfMissing(ref GameObject target, Transform root, params string[] paths)
    {
        if (target != null)
        {
            return;
        }

        foreach (string path in paths)
        {
            Transform child = root.Find(path);
            if (child == null)
            {
                continue;
            }

            target = child.gameObject;
            return;
        }
    }

    public void InvokeUpdate()
    {
        if(_currentState != null)
        {
            _currentState.UpdateState();
        }

        // Now draw the lines based on the updated destroyAnyway list
        if(destroyAnyway.Count > 0)
        {
            Debug.Log("Drawing lines for " + destroyAnyway.Count + " items");
            int i = 0;
            line.SetVertexCount(destroyAnyway.Count);

            foreach (Item item in destroyAnyway)
            {
                if(item != null)
                {
                    line.AddPoint(item.transform.position, i);
                    i++;
                }
            }
        }
        else
        {
            line.SetVertexCount(0);
        }
    }

    private void OnDestroy()
    {
        CoroutineManager coroutineManager = CoroutineManager.ExistingInstance;
        if(coroutineManager != null)
        {
            coroutineManager.StopAllManagedCoroutines();
        }

        THIS = null;
        Instance = null;

        OnMapState = null;
        OnEnterGame = null;
        OnLevelLoaded = null;
        OnMenuPlay = null;
        OnMenuComplete = null;
        OnTouchDetected = null;
        OnWin = null;
        OnLose = null;
    }

    #endregion

    #region State Management

    public void ResumeGame()
    {
        gameStatus = GameState.Playing;
        Time.timeScale = 1;
    }

    private void HandleLegacyState(GameState state)
    {
        if(state == GameState.PreFailedBomb) { }
        else if(state == GameState.PreFailed)
        {
            GameObject.Find("CanvasGlobal").transform.Find("PreFailed").gameObject.SetActive(true);
        }
        else if(state == GameState.Pause)
        {
            Time.timeScale = 0;
        }
        else if(state == GameState.ToMap)
        {
            MusicBase.Instance.GetComponent<AudioSource>().Stop();
            SoundBase.Instance.PlaySound(SoundBase.Instance.gameOver[0]);
            GameObject.Find("CanvasGlobal").transform.Find("MenuFailed").gameObject.SetActive(true);
        }
    }

    public void EnableMap(bool enable)
    {
        MapState mapState = _states[GameState.Map] as MapState;
        mapState.EnableMap(enable);
    }

    public void LockBoosts()
    {
        foreach (BoostIcon item in InGameBoosts)
        {
            if(item != ActivatedBoost)
                item.LockBoost();
        }
    }

    public void UnLockBoosts()
    {
        foreach (BoostIcon item in InGameBoosts)
        {
            item.UnLockBoost();
        }
    }

    public void RestartTimer()
    {
        if(_currentState is PlayingState playingState)
        {
            playingState.RestartTimer();
        }
    }

    #endregion

    #region Level Management

    public void LoadLevel()
    {
        currentLevel = PlayerPrefs.GetInt("OpenLevel"); // TargetHolder.level;
        if(currentLevel == 0)
            currentLevel = 1;

        currentLevel = CalculateLevel(currentLevel);
        LoadDataFromLocal(currentLevel);
        NumIngredients = ingrTarget.Count;
    }

    public void LoadDataFromLocal(int currentLevel)
    {
        levelLoaded = false;
        // Read data from text file
        TextAsset mapText = Resources.Load("Levels/" + currentLevel) as TextAsset;

        if(mapText == null)
        {
            mapText = Resources.Load("Levels/" + currentLevel) as TextAsset;
        }

        ProcessGameDataFromString(mapText.text);
    }

    int CalculateLevel(int playerLevel)
    {
        // Если уровень меньше или равен 45, просто возвращаем его
        if(playerLevel <= 45)
            return playerLevel;

        // Вычисляем, сколько раз игрок "прошел полный круг" после 45
        int beyond45 = playerLevel - 45;

        // Используем модуло для циклического расчета
        // Диапазон уровней в цикле: от 10 до 45 (всего 36 уровней)
        int cyclePosition = beyond45 % 36;

        // Прибавляем 10 (начальный уровень цикла) и учитываем особый случай
        int result = cyclePosition + 10;

        // Если результат получился 46 или больше, это означает,
        // что мы вышли за пределы диапазона 10-45, поэтому начинаем с 10 снова
        if(result > 45)
            result = (result - 45) + 9;

        return result;
    }

    void ProcessGameDataFromString(string mapText)
    {
        string[] lines = mapText.Split(new string[]
                                       {
                                           "\n"
                                       },
                                       StringSplitOptions.RemoveEmptyEntries);
        ingrTarget = new List<CollectedIngredients>();
        bombsCollect = 0; // 1.4.11  
        int mapLine = 0;

        foreach (string line in lines)
        {
            // Check if line is game mode line
            if(line.StartsWith("MODE"))
            {
                // Replace GM to get mode number, 
                string modeString = line.Replace("MODE", string.Empty).Trim();
                // Then parse it to interger
                target = (Target)int.Parse(modeString);
                // Assign game mode
            }
            else if(line.StartsWith("SIZE "))
            {
                string blocksString = line.Replace("SIZE", string.Empty).Trim();
                string[] sizes = blocksString.Split(new string[]
                                                    {
                                                        "/"
                                                    },
                                                    StringSplitOptions.RemoveEmptyEntries);
                maxCols = int.Parse(sizes[0]);
                maxRows = int.Parse(sizes[1]);
                squaresArray = new Square[maxCols * maxRows];
                levelSquaresFile = new SquareBlocks[maxRows * maxCols];

                for (int i = 0; i < levelSquaresFile.Length; i++)
                {
                    SquareBlocks sqBlocks = new SquareBlocks();
                    sqBlocks.block = SquareTypes.EMPTY;
                    sqBlocks.obstacle = SquareTypes.NONE;

                    levelSquaresFile[i] = sqBlocks;
                }
            }
            else if(line.StartsWith("LIMIT"))
            {
                string blocksString = line.Replace("LIMIT", string.Empty).Trim();
                string[] sizes = blocksString.Split(new string[]
                                                    {
                                                        "/"
                                                    },
                                                    StringSplitOptions.RemoveEmptyEntries);
                limitType = (LIMIT)int.Parse(sizes[0]);
                Limit = int.Parse(sizes[1]);
            }
            else if(line.StartsWith("COLOR LIMIT "))
            {
                string blocksString = line.Replace("COLOR LIMIT", string.Empty).Trim();
                colorLimit = int.Parse(blocksString);
            }
            // Check third line to get missions
            else if(line.StartsWith("STARS"))
            {
                string blocksString = line.Replace("STARS", string.Empty).Trim();
                string[] blocksNumbers =
                    blocksString.Split(new string[]
                                       {
                                           "/"
                                       },
                                       StringSplitOptions.RemoveEmptyEntries);
                star1 = int.Parse(blocksNumbers[0]);
                star2 = int.Parse(blocksNumbers[1]);
                star3 = int.Parse(blocksNumbers[2]);
            }
            else if(line.StartsWith("COLLECT COUNT "))
            {
                string blocksString = line.Replace("COLLECT COUNT", string.Empty).Trim();
                string[] blocksNumbers =
                    blocksString.Split(new string[]
                                       {
                                           "/"
                                       },
                                       StringSplitOptions.RemoveEmptyEntries);

                for (int i = 0; i < blocksNumbers.Length; i++)
                {
                    if(collectedIngredients.Count <= i && target == Target.COLLECT)
                        break;
                    if(target == Target.COLLECT)
                        ingrTarget.Add(collectedIngredients[i]);
                    else
                        ingrTarget.Add(new CollectedIngredients());
                    ingrTarget[ingrTarget.Count - 1].count = int.Parse(blocksNumbers[i]);
                }
            }
            else if(line.StartsWith("COLLECT ITEMS "))
            {
                string blocksString = line.Replace("COLLECT ITEMS", string.Empty).Trim();
                string[] blocksNumbers =
                    blocksString.Split(new string[]
                                       {
                                           "/"
                                       },
                                       StringSplitOptions.RemoveEmptyEntries);

                for (int i = 0; i < blocksNumbers.Length; i++)
                {
                    if(target == Target.COLLECT)
                    {
                        if(ingrTarget.Count > i)
                        {
                            CollectedIngredients ingFromList =
                                collectedIngredients[int.Parse(blocksNumbers[i])];
                            ingrTarget[i].check = true;
                            ingrTarget[i].name = ingFromList.name;
                            ingrTarget[i].sprite = ingFromList.sprite;
                        }
                    }
                    else if(target == Target.ITEMS)
                    {
                        collectItems[i] = (CollectItems)int.Parse(blocksNumbers[i]) + 1;
                    }
                }
            }
            else if(line.StartsWith("CAGE "))
            {
                string blocksString = line.Replace("CAGE ", string.Empty).Trim();
                cageHP = int.Parse(blocksString);
            }
            else if(line.StartsWith("BOMBS "))
            {
                Debug.Log("load bomb");
                string blocksString = line.Replace("BOMBS ", string.Empty).Trim();
                string[] blocksNumbers =
                    blocksString.Split(new string[]
                                       {
                                           "/"
                                       },
                                       StringSplitOptions.RemoveEmptyEntries);
                bombsCollect = int.Parse(blocksNumbers[0]);
                bombTimer = int.Parse(blocksNumbers[1]);
            }
            else if(line.StartsWith("GETSTARS "))
            {
                string blocksString = line.Replace("GETSTARS ", string.Empty).Trim();
                starsTargetCount = (CollectStars)int.Parse(blocksString);
            }
            else
            {
                // Maps
                // Split lines again to get map numbers
                string[] st = line.Split(new string[]
                                         {
                                             " "
                                         },
                                         StringSplitOptions.RemoveEmptyEntries);

                for (int i = 0; i < st.Length; i++)
                {
                    levelSquaresFile[mapLine * maxCols + i].block = (SquareTypes)int.Parse(st[i][0].ToString());
                    levelSquaresFile[mapLine * maxCols + i].obstacle = (SquareTypes)int.Parse(st[i][1].ToString());
                }

                mapLine++;
            }
        }

        TargetBlocks = 0;

        for (int row = 0; row < maxRows; row++)
        {
            for (int col = 0; col < maxCols; col++)
            {
                if(levelSquaresFile[row * maxCols + col].block == SquareTypes.BLOCK)
                    TargetBlocks++;
                else if(levelSquaresFile[row * maxCols + col].block == SquareTypes.DOUBLEBLOCK)
                    TargetBlocks += 2;
            }
        }

        TargetCages = 0;

        for (int row = 0; row < maxRows; row++)
        {
            for (int col = 0; col < maxCols; col++)
            {
                if(levelSquaresFile[row * maxCols + col].obstacle == SquareTypes.WIREBLOCK)
                    TargetCages++;
            }
        }

        levelLoaded = true;
    }

    public void ReGenLevel()
    {
        itemsHided = false;
        DragBlocked = true;
        if(gameStatus != GameState.Playing && gameStatus != GameState.RegenLevel)
            DestroyItems();
        else if(gameStatus == GameState.RegenLevel)
            DestroyItems(true);
        GenerateNewItems(false);

        StartCoroutine(InitBombs());

        DragBlocked = false;
        gameStatus = GameState.Playing;
        OnLevelLoaded();
    }

    public void GenerateLevel()
    {
        bool chessColor = false;
        float sqWidth = 1.6f;
        float halfSquare = sqWidth / 2;
        Vector3 fieldPos = new Vector3(-maxCols * sqWidth / 2 + halfSquare, maxRows / 1.4f, -10);

        for (int row = 0; row < maxRows; row++)
        {
            if(maxCols % 2 == 0)
                chessColor = !chessColor;

            for (int col = 0; col < maxCols; col++)
            {
                CreateSquare(col, row, chessColor);
                chessColor = !chessColor;
            }
        }

        float yOffset = 0;
        if(target == Target.COLLECT)
            yOffset = 0.3f;
        GameFieldTargetLocalPosition = new Vector3(fieldPos.x, fieldPos.y + yOffset, GameField.localPosition.z);
    }

    void CreateSquare(int col, int row, bool chessColor = false)
    {
        GameObject square = null;
        square = Instantiate(squarePrefab,
                             firstSquarePosition + new Vector2(col * squareWidth, -row * squareHeight),
                             Quaternion.identity) as GameObject;

        if(chessColor)
        {
            square.GetComponent<SpriteRenderer>().sprite = squareSprite1;
        }

        square.transform.SetParent(GameField);
        square.transform.localPosition = firstSquarePosition + new Vector2(col * squareWidth, -row * squareHeight);
        squaresArray[row * maxCols + col] = square.GetComponent<Square>();
        square.GetComponent<Square>().row = row;
        square.GetComponent<Square>().col = col;
        square.GetComponent<Square>().type = SquareTypes.EMPTY;

        if(levelSquaresFile[row * maxCols + col].block == SquareTypes.EMPTY)
        {
            CreateObstacles(col, row, square, SquareTypes.NONE);
        }
        else if(levelSquaresFile[row * maxCols + col].block == SquareTypes.NONE)
        {
            square.GetComponent<SpriteRenderer>().enabled = false;
            square.GetComponent<Square>().type = SquareTypes.NONE;
        }
        else if(levelSquaresFile[row * maxCols + col].block == SquareTypes.BLOCK)
        {
            GameObject block = Instantiate(blockPrefab,
                                           firstSquarePosition + new Vector2(col * squareWidth, -row * squareHeight),
                                           Quaternion.identity) as GameObject;
            block.transform.SetParent(square.transform);
            block.transform.localPosition = new Vector3(0, 0, -0.01f);
            square.GetComponent<Square>().block.Add(block);
            square.GetComponent<Square>().type = SquareTypes.BLOCK;
            block.GetComponent<Square>().type = SquareTypes.BLOCK;

            CreateObstacles(col, row, square, SquareTypes.NONE);
        }
        else if(levelSquaresFile[row * maxCols + col].block == SquareTypes.DOUBLEBLOCK)
        {
            GameObject block = Instantiate(blockPrefab,
                                           firstSquarePosition + new Vector2(col * squareWidth, -row * squareHeight),
                                           Quaternion.identity) as GameObject;
            block.transform.SetParent(square.transform);
            block.transform.localPosition = new Vector3(0, 0, -0.01f);
            square.GetComponent<Square>().block.Add(block);
            square.GetComponent<Square>().type = SquareTypes.BLOCK;
            block.GetComponent<Square>().type = SquareTypes.BLOCK;

            block = Instantiate(blockPrefab,
                                firstSquarePosition + new Vector2(col * squareWidth, -row * squareHeight),
                                Quaternion.identity) as GameObject;
            block.transform.SetParent(square.transform);
            block.transform.localPosition = new Vector3(0, 0, -0.01f);
            square.GetComponent<Square>().block.Add(block);
            square.GetComponent<Square>().type = SquareTypes.BLOCK;
            block.GetComponent<Square>().type = SquareTypes.BLOCK;

            block.GetComponent<SpriteRenderer>().sprite = doubleBlock;
            block.GetComponent<SpriteRenderer>().sortingOrder = 1;

            CreateObstacles(col, row, square, SquareTypes.NONE);
        }
    }

    public void CreateObstacles(int col, int row, GameObject square, SquareTypes type)
    {
        if((levelSquaresFile[row * maxCols + col].obstacle == SquareTypes.WIREBLOCK && type == SquareTypes.NONE) ||
           type == SquareTypes.WIREBLOCK)
        {
            GameObject block = Instantiate(wireBlockPrefab,
                                           firstSquarePosition + new Vector2(col * squareWidth, -row * squareHeight),
                                           Quaternion.identity) as GameObject;
            block.transform.SetParent(square.transform);
            block.transform.localPosition = new Vector3(0, 0, -0.5f);
            square.GetComponent<Square>().block.Add(block);
            square.GetComponent<Square>().type = SquareTypes.WIREBLOCK;
            block.GetComponent<SpriteRenderer>().sortingOrder = 3;
            block.GetComponent<Square>().type = SquareTypes.WIREBLOCK;
            square.GetComponent<Square>().SetCage(cageHP);
        }
        else if((levelSquaresFile[row * maxCols + col].obstacle == SquareTypes.SOLIDBLOCK &&
                 type == SquareTypes.NONE) ||
                type == SquareTypes.SOLIDBLOCK)
        {
            GameObject block = Instantiate(solidBlockPrefab,
                                           firstSquarePosition + new Vector2(col * squareWidth, -row * squareHeight),
                                           Quaternion.identity) as GameObject;
            block.transform.SetParent(square.transform);
            block.transform.localPosition = new Vector3(0, 0, -0.5f);
            square.GetComponent<Square>().block.Add(block);
            block.GetComponent<SpriteRenderer>().sortingOrder = 3;
            square.GetComponent<Square>().type = SquareTypes.SOLIDBLOCK;
            block.GetComponent<Square>().type = SquareTypes.SOLIDBLOCK;
        }
        else if((levelSquaresFile[row * maxCols + col].obstacle == SquareTypes.DOUBLESOLIDBLOCK &&
                 type == SquareTypes.NONE) ||
                type == SquareTypes.DOUBLESOLIDBLOCK)
        {
            GameObject block = Instantiate(solidBlockPrefab,
                                           firstSquarePosition + new Vector2(col * squareWidth, -row * squareHeight),
                                           Quaternion.identity) as GameObject;
            block.transform.SetParent(square.transform);
            block.transform.localPosition = new Vector3(0, 0, -0.5f);
            square.GetComponent<Square>().block.Add(block);
            block.GetComponent<SpriteRenderer>().sortingOrder = 3;
            square.GetComponent<Square>().type = SquareTypes.SOLIDBLOCK;
            block.GetComponent<Square>().type = SquareTypes.SOLIDBLOCK;

            block = Instantiate(solidBlockPrefab,
                                firstSquarePosition + new Vector2(col * squareWidth, -row * squareHeight),
                                Quaternion.identity) as GameObject;
            block.transform.SetParent(square.transform);
            block.transform.localPosition = new Vector3(0, 0, -0.5f);
            square.GetComponent<Square>().block.Add(block);
            block.GetComponent<SpriteRenderer>().sprite = doubleSolidBlock;
            block.GetComponent<SpriteRenderer>().sortingOrder = 4;
            square.GetComponent<Square>().type = SquareTypes.SOLIDBLOCK;
            block.GetComponent<Square>().type = SquareTypes.SOLIDBLOCK;
        }
        else if((levelSquaresFile[row * maxCols + col].obstacle == SquareTypes.UNDESTROYABLE &&
                 type == SquareTypes.NONE) ||
                type == SquareTypes.UNDESTROYABLE)
        {
            GameObject block = Instantiate(undesroyableBlockPrefab,
                                           firstSquarePosition + new Vector2(col * squareWidth, -row * squareHeight),
                                           Quaternion.identity) as GameObject;
            block.transform.SetParent(square.transform);
            block.transform.localPosition = new Vector3(0, 0, -0.5f);
            square.GetComponent<Square>().block.Add(block);
            square.GetComponent<Square>().type = SquareTypes.UNDESTROYABLE;
            block.GetComponent<Square>().type = SquareTypes.UNDESTROYABLE;
        }
        else if((levelSquaresFile[row * maxCols + col].obstacle == SquareTypes.THRIVING && type == SquareTypes.NONE) ||
                type == SquareTypes.THRIVING)
        {
            GameObject block = Instantiate(thrivingBlockPrefab,
                                           firstSquarePosition + new Vector2(col * squareWidth, -row * squareHeight),
                                           Quaternion.identity) as GameObject;
            block.transform.SetParent(square.transform);
            block.transform.localPosition = new Vector3(0, 0, -0.5f);
            block.GetComponent<SpriteRenderer>().sortingOrder = 3;
            if(square.GetComponent<Square>().item != null)
                Destroy(square.GetComponent<Square>().item.gameObject);
            square.GetComponent<Square>().block.Add(block);
            square.GetComponent<Square>().type = SquareTypes.THRIVING;
            block.GetComponent<Square>().type = SquareTypes.THRIVING;
        }
    }

    public void GenerateOutline()
    {
        int row = 0;
        int col = 0;

        for (row = 0; row < maxRows; row++)
        {
            // Down
            SetOutline(col, row, 0);
        }

        row = maxRows - 1;

        for (col = 1; col < maxCols; col++)
        {
            // Right
            SetOutline(col, row, 90);
        }

        col = maxCols - 1;

        for (row = maxRows - 2; row >= 0; row--)
        {
            // Up
            SetOutline(col, row, 180);
        }

        row = 0;

        for (col = maxCols - 2; col > 0; col--)
        {
            // Left
            SetOutline(col, row, 270);
        }

        col = 0;

        for (row = 1; row < maxRows - 1; row++)
        {
            for (col = 1; col < maxCols - 1; col++)
            {
                SetOutline(col, row, 0);
            }
        }
    }

    void SetOutline(int col, int row, float zRot)
    {
        Square square = GetSquare(col, row, true);

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
                    // Top left
                    SetCornerOutline(outline, spr, outline2);
                    outline.transform.localPosition = Vector3.zero + Vector3.left * 0.01f + Vector3.up * 0.01f;
                }

                if(row == 0 && col == maxCols - 1)
                {
                    // Top right
                    SetCornerOutline(outline, spr, outline4);
                    outline.transform.localPosition = Vector3.zero + Vector3.right * 0.01f + Vector3.up * 0.01f;
                }

                if(row == maxRows - 1 && col == 0)
                {
                    // Bottom left
                    SetCornerOutline(outline, spr, outline5);
                    outline.transform.localPosition = Vector3.zero + Vector3.left * 0.01f + Vector3.down * 0.01f;
                }

                if(row == maxRows - 1 && col == maxCols - 1)
                {
                    // Bottom right
                    SetCornerOutline(outline, spr, outline3);
                    outline.transform.localPosition = Vector3.zero + Vector3.right * 0.01f + Vector3.down * 0.01f;
                }
            }
            else
            {
                // Top left
                if(GetSquare(col - 1, row - 1, true).type == SquareTypes.NONE &&
                   GetSquare(col, row - 1, true).type == SquareTypes.NONE &&
                   GetSquare(col - 1, row, true).type == SquareTypes.NONE)
                {
                    GameObject outline = CreateOutline(square);
                    SpriteRenderer spr = outline.GetComponent<SpriteRenderer>();
                    SetCornerOutline(outline, spr, outline2);
                    outline.transform.localPosition = Vector3.zero + Vector3.left * 0.015f + Vector3.up * 0.015f;
                }

                // Top right
                if(GetSquare(col + 1, row - 1, true).type == SquareTypes.NONE &&
                   GetSquare(col, row - 1, true).type == SquareTypes.NONE &&
                   GetSquare(col + 1, row, true).type == SquareTypes.NONE)
                {
                    GameObject outline = CreateOutline(square);
                    SpriteRenderer spr = outline.GetComponent<SpriteRenderer>();
                    SetCornerOutline(outline, spr, outline4);
                    outline.transform.localPosition = Vector3.zero + Vector3.right * 0.015f + Vector3.up * 0.015f;
                }

                // Bottom left
                if(GetSquare(col - 1, row + 1, true).type == SquareTypes.NONE &&
                   GetSquare(col, row + 1, true).type == SquareTypes.NONE &&
                   GetSquare(col - 1, row, true).type == SquareTypes.NONE)
                {
                    GameObject outline = CreateOutline(square);
                    SpriteRenderer spr = outline.GetComponent<SpriteRenderer>();
                    SetCornerOutline(outline, spr, outline5);
                    outline.transform.localPosition = Vector3.zero + Vector3.left * 0.015f + Vector3.down * 0.015f;
                }

                // Bottom right
                if(GetSquare(col + 1, row + 1, true).type == SquareTypes.NONE &&
                   GetSquare(col, row + 1, true).type == SquareTypes.NONE &&
                   GetSquare(col + 1, row, true).type == SquareTypes.NONE)
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

            if(GetSquare(col - 1, row, true).type != SquareTypes.NONE &&
               GetSquare(col, row - 1, true).type != SquareTypes.NONE)
            {
                GameObject outline = CreateOutline(square);
                SpriteRenderer spr = outline.GetComponent<SpriteRenderer>();
                SetCornerOutline(outline, spr, outline2);
                outline.transform.localPosition = Vector3.zero;
                corner = true;
            }

            if(GetSquare(col + 1, row, true).type != SquareTypes.NONE &&
               GetSquare(col, row + 1, true).type != SquareTypes.NONE)
            {
                GameObject outline = CreateOutline(square);
                SpriteRenderer spr = outline.GetComponent<SpriteRenderer>();
                SetCornerOutline(outline, spr, outline3);
                outline.transform.localPosition = Vector3.zero;
                corner = true;
            }

            if(GetSquare(col + 1, row, true).type != SquareTypes.NONE &&
               GetSquare(col, row - 1, true).type != SquareTypes.NONE)
            {
                GameObject outline = CreateOutline(square);
                SpriteRenderer spr = outline.GetComponent<SpriteRenderer>();
                SetCornerOutline(outline, spr, outline4);
                outline.transform.localPosition = Vector3.zero;
                corner = true;
            }

            if(GetSquare(col - 1, row, true).type != SquareTypes.NONE &&
               GetSquare(col, row + 1, true).type != SquareTypes.NONE)
            {
                GameObject outline = CreateOutline(square);
                SpriteRenderer spr = outline.GetComponent<SpriteRenderer>();
                SetCornerOutline(outline, spr, outline5);
                outline.transform.localPosition = Vector3.zero;
                corner = true;
            }

            if(!corner)
            {
                if(GetSquare(col, row - 1, true).type != SquareTypes.NONE)
                {
                    GameObject outline = CreateOutline(square);
                    SpriteRenderer spr = outline.GetComponent<SpriteRenderer>();
                    outline.transform.localPosition = Vector3.zero + Vector3.up * 0.79f;
                    outline.transform.localRotation = Quaternion.Euler(0, 0, 90);
                }

                if(GetSquare(col, row + 1, true).type != SquareTypes.NONE)
                {
                    GameObject outline = CreateOutline(square);
                    SpriteRenderer spr = outline.GetComponent<SpriteRenderer>();
                    outline.transform.localPosition = Vector3.zero + Vector3.down * 0.79f;
                    outline.transform.localRotation = Quaternion.Euler(0, 0, 90);
                }

                if(GetSquare(col - 1, row, true).type != SquareTypes.NONE)
                {
                    GameObject outline = CreateOutline(square);
                    SpriteRenderer spr = outline.GetComponent<SpriteRenderer>();
                    outline.transform.localPosition = Vector3.zero + Vector3.left * 0.79f;
                    outline.transform.localRotation = Quaternion.Euler(0, 0, 0);
                }

                if(GetSquare(col + 1, row, true).type != SquareTypes.NONE)
                {
                    GameObject outline = CreateOutline(square);
                    SpriteRenderer spr = outline.GetComponent<SpriteRenderer>();
                    outline.transform.localPosition = Vector3.zero + Vector3.right * 0.79f;
                    outline.transform.localRotation = Quaternion.Euler(0, 0, 0);
                }
            }
        }
    }

    void SetCornerOutline(GameObject outline, SpriteRenderer spr, Sprite sprite)
    {
        spr.sprite = sprite != null ? sprite : outline3;
        outline.transform.localRotation = Quaternion.identity;
    }

    GameObject CreateOutline(Square square)
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

    #endregion

    #region Item Management

    public void GenerateNewItems(bool falling = true)
    {
        for (int col = 0; col < maxCols; col++)
        {
            for (int row = maxRows - 1; row >= 0; row--)
            {
                if(GetSquare(col, row) != null)
                {
                    if(!GetSquare(col, row).IsNone() &&
                       GetSquare(col, row).CanGoInto() &&
                       GetSquare(col, row).item == null)
                    {
                        if((GetSquare(col, row).item == null && !GetSquare(col, row).IsHaveSolidAbove()) || !falling)
                        {
                            GetSquare(col, row).GenItem(falling);
                        }
                    }
                }
            }
        }
    }

    public void DestroyItems(bool withoutEffects = false)
    {
        GameObject[] items = GameObject.FindGameObjectsWithTag("Item");

        foreach (GameObject item in items)
        {
            if(item != null)
            {
                if(item.GetComponent<Item>().currentType != ItemsTypes.INGREDIENT)
                {
                    if(!withoutEffects)
                        item.GetComponent<Item>().DestroyItem();
                    else
                        item.GetComponent<Item>().SmoothDestroy();
                }
            }
        }
    }

    public async UniTask FindMatches()
    {
        if(_currentState is PlayingState playingState)
        {
            await playingState.FindMatches();
        }
    }

    public IEnumerator FindMatchDelay()
    {
        yield return new WaitForSeconds(0.2f);
        LevelManager.THIS.FindMatches();
    }

    public async UniTask ProcessMatchesAndFalling()
    {
        // var routine = StartCoroutine(_boardMechanicsService.ProcessBoardAfterMatches());
        await _boardMechanicsService.ProcessBoardAfterMatches(CancellationToken.None);
    }

    void DestroyGatheredExtraItems(Item item)
    {
        if(gatheredTypes.Count > 1)
        {
            item.DestroyHorizontal();
            item.DestroyVertical();
        }

        foreach (ItemsTypes itemType in gatheredTypes)
        {
            if(itemType == ItemsTypes.HORIZONTAL_STRIPPED)
                item.DestroyHorizontal();
            else
                item.DestroyVertical();
        }
    }

    public void ClearHighlight(bool boost = false)
    {
        if(!boost)
            return;
        highlightedItems.Clear();

        for (int col = 0; col < maxCols; col++)
        {
            for (int row = 0; row < maxRows; row++)
            {
                if(GetSquare(col, row) != null)
                {
                    GetSquare(col, row).SetActiveCage(false);
                    GetSquare(col, row).HighLight(false);
                }
            }
        }
    }

    public void SetTypeByColor(int p, ItemsTypes nextType)
    {
        GameObject[] items = GameObject.FindGameObjectsWithTag("Item");

        foreach (GameObject item in items)
        {
            if(item.GetComponent<Item>().color == p)
            {
                if(nextType == ItemsTypes.HORIZONTAL_STRIPPED || nextType == ItemsTypes.VERTICAL_STRIPPED)
                    item.GetComponent<Item>().nextType = (ItemsTypes)UnityEngine.Random.Range(1, 3);
                else
                    item.GetComponent<Item>().nextType = nextType;

                item.GetComponent<Item>().ChangeType();
                if(nextType == ItemsTypes.NONE)
                    destroyAnyway.Add(item.GetComponent<Item>());
            }
        }
    }

    public void SetColorToRandomItems()
    {
        LevelEffectsController?.PlayRandomColorReveal();
    }

    #endregion

    #region Square and Item Queries

    public Square GetSquare(int col, int row, bool safe = false)
    {
        if(!safe)
        {
            if(row >= maxRows || col >= maxCols || row < 0 || col < 0) // 1.4.7
                return null;
            return squaresArray[row * maxCols + col];
        }
        else
        {
            row = Mathf.Clamp(row, 0, maxRows - 1);
            col = Mathf.Clamp(col, 0, maxCols - 1);
            return squaresArray[row * maxCols + col];
        }
    }

    public List<Item> GetRow(int row)
    {
        List<Item> itemsList = new List<Item>();

        for (int col = 0; col < maxCols; col++)
        {
            itemsList.Add(GetSquare(col, row, true).item);
        }

        return itemsList;
    }

    public List<Square> GetRowSquare(int row)
    {
        List<Square> itemsList = new List<Square>();

        for (int col = 0; col < maxCols; col++)
        {
            itemsList.Add(GetSquare(col, row, true));
        }

        return itemsList;
    }

    public List<Item> GetColumn(int col)
    {
        List<Item> itemsList = new List<Item>();

        for (int row = 0; row < maxRows; row++)
        {
            itemsList.Add(GetSquare(col, row, true).item);
        }

        return itemsList;
    }

    public List<Square> GetColumnSquare(int col)
    {
        List<Square> itemsList = new List<Square>();

        for (int row = 0; row < maxRows; row++)
        {
            itemsList.Add(GetSquare(col, row, true));
        }

        return itemsList;
    }

    public List<Item> GetRandomItems(int count)
    {
        List<Item> list = new List<Item>();
        List<Item> list2 = new List<Item>();
        if(count <= 0)
            return list2;
        GameObject[] items = GameObject.FindGameObjectsWithTag("Item");
        if(items.Length < count)
            count = items.Length;

        foreach (GameObject item in items)
        {
            if(!item.GetComponent<Item>().destroying &&
               item.GetComponent<Item>().currentType == ItemsTypes.NONE &&
               item.GetComponent<Item>().nextType == ItemsTypes.NONE &&
               item.GetComponent<Item>().square.type != SquareTypes.WIREBLOCK)
            {
                list.Add(item.GetComponent<Item>());
            }
        }

        while (list2.Count < count)
        {
            Item newItem = list[UnityEngine.Random.Range(0, list.Count)];

            if(list2.IndexOf(newItem) < 0)
            {
                list2.Add(newItem);
            }
        }

        return list2;
    }

    public List<Item> GetAllExtraItems()
    {
        List<Item> list = new List<Item>();
        GameObject[] items = GameObject.FindGameObjectsWithTag("Item");

        foreach (GameObject item in items)
        {
            if(item.GetComponent<Item>().currentType != ItemsTypes.NONE)
            {
                list.Add(item.GetComponent<Item>());
            }
        }

        return list;
    }

    public List<Item> GetItemsAround(Square square)
    {
        int col = square.col;
        int row = square.row;
        List<Item> itemsList = new List<Item>();

        for (int r = row - 1; r <= row + 1; r++)
        {
            for (int c = col - 1; c <= col + 1; c++)
            {
                itemsList.Add(GetSquare(c, r, true).item);
            }
        }

        return itemsList;
    }

    public List<Square> GetSquaresAround(Square square)
    {
        int col = square.col;
        int row = square.row;
        List<Square> itemsList = new List<Square>();

        for (int r = row - 1; r <= row + 1; r++)
        {
            for (int c = col - 1; c <= col + 1; c++)
            {
                itemsList.Add(GetSquare(c, r, true));
            }
        }

        return itemsList;
    }

    public List<Item> GetItems()
    {
        List<Item> itemsList = new List<Item>();

        for (int row = 0; row < maxRows; row++)
        {
            for (int col = 0; col < maxCols; col++)
            {
                if(GetSquare(col, row) != null)
                {
                    if(GetSquare(col, row).item != null)
                    {
                        itemsList.Add(GetSquare(col, row, true).item);
                    }
                }
            }
        }

        return itemsList;
    }

    public List<Square> GetSquares()
    {
        List<Square> itemsList = new List<Square>();

        for (int row = 0; row < maxRows; row++)
        {
            for (int col = 0; col < maxCols; col++)
            {
                if(GetSquare(col, row) != null)
                {
                    itemsList.Add(GetSquare(col, row));
                }
            }
        }

        return itemsList;
    }

    public List<Square> GetBottomRow()
    {
        List<Square> itemsList = new List<Square>();
        int listCounter = 0;

        for (int col = 0; col < maxCols; col++)
        {
            for (int row = maxRows - 1; row >= 0; row--)
            {
                Square square = GetSquare(col, row, true);

                if(square.type != SquareTypes.NONE)
                {
                    itemsList.Add(square);
                    listCounter++;
                    break;
                }
            }
        }

        return itemsList;
    }

    public List<Item> GetIngredients(int i = -1)
    {
        List<Item> list = new List<Item>();
        GameObject[] items = GameObject.FindGameObjectsWithTag("Item");

        foreach (GameObject item in items)
        {
            if(i > -1)
            {
                if(item.GetComponent<Item>().currentType == ItemsTypes.INGREDIENT &&
                   item.GetComponent<Item>().color == 1000 + i)
                {
                    list.Add(item.GetComponent<Item>());
                }
            }
            else
            {
                if(item.GetComponent<Item>().currentType == ItemsTypes.INGREDIENT)
                {
                    list.Add(item.GetComponent<Item>());
                }
            }
        }

        return list;
    }

    #endregion

    #region Special Effects

    public void DestroyDoubleBomb(int col)
    {
        LevelEffectsController?.PlayDoubleBombWave(col);
    }

    public void StrippedShow(GameObject obj, bool horrizontal)
    {
        LevelEffectsController?.PlayStripedEffect(obj, horrizontal);
    }

    public GameObject GetExplFromPool()
    {
        return LevelEffectsController != null ? LevelEffectsController.GetExplosion() : null;
    }

    public GameObject GetFlowerFromPool()
    {
        return LevelEffectsController != null ? LevelEffectsController.GetFlower() : null;
    }

    public bool CheckFlowerStillFly()
    {
        return LevelEffectsController != null && LevelEffectsController.HasFlyingFlowers();
    }

    #endregion

    #region Target Management

    public IEnumerator TimeTick()
    {
        while (true)
        {
            if(gameStatus == GameState.Playing)
            {
                if(LevelManager.Instance.limitType == LIMIT.TIME)
                {
                    LevelManager.THIS.Limit--;
                    CheckWinLose();
                }
            }

            if(gameStatus == GameState.Map || LevelManager.THIS.Limit <= 0 || gameStatus == GameState.GameOver)
                yield break;

            yield return new WaitForSeconds(1);
        }
    }

    public void CreateCollectableTarget(GameObject parentTransform, Target tar, bool ForDialog = true)
    {
        tar = target;
        GameObject ingrPrefab = Resources.Load("Prefabs/CollectGUIObj") as GameObject;

        parentTransform.SetActive(true);
        RectTransform containerRect = parentTransform.GetComponent<RectTransform>();
        int Sprites_Length = (Resources.Load("Prefabs/Item") as GameObject).GetComponent<Item>().items.Length;
        Sprite[] spr = new Sprite[Sprites_Length];

        for (int i = 0; i < Sprites_Length; i++)
        {
            spr[i] = (Resources.Load("Prefabs/Item") as GameObject).GetComponent<Item>().items[i];
        }

        int num = NumIngredients;
        List<object> collectionItems = new List<object>();

        if(tar == Target.ITEMS)
        {
            for (int i = 0; i < num; i++)
            {
                collectionItems.Add(collectItems[i]);
            }

            Sprite[] sprOld = spr;
            int ii = 0;

            for (int i = 0; i < collectItems.Length; i++)
            {
                if(collectItems[i] != CollectItems.None)
                {
                    spr[ii] = sprOld[(int)collectItems[i] - 1];
                    ii++;
                }
            }
        }
        else if(tar == Target.COLLECT)
        {
            spr = ingrediendSprites;
            for (int i = 0; i < num; i++)
                collectionItems.Add(ingrTarget[i]);
        }
        else if(tar == Target.BLOCKS)
        {
            num = 1;
            spr = new Sprite[]
            {
                blockPrefab.GetComponent<SpriteRenderer>().sprite
            };
            for (int i = 0; i < num; i++)
                collectionItems.Add(Ingredients.Ingredient1);
            ingrTarget.Add(new CollectedIngredients());

            ingrTarget[0].count = TargetBlocks;
        }
        else if(tar == Target.CAGES)
        {
            num = 1;
            spr = new Sprite[]
            {
                wireBlockPrefab.GetComponent<SpriteRenderer>().sprite
            };
            for (int i = 0; i < num; i++)
                collectionItems.Add(Ingredients.Ingredient1);
            ingrTarget.Add(new CollectedIngredients());

            ingrTarget[0].count = TargetCages;
        }
        else if(tar == Target.BOMBS)
        {
            num = 1;
            spr = new Sprite[]
            {
                ingrPrefab.GetComponent<TargetGUI>().bomb
            };
            for (int i = 0; i < num; i++)
                collectionItems.Add(Ingredients.Ingredient1);
            ingrTarget.Add(new CollectedIngredients());
            ingrTarget[0].count = 1;
        }
        else if(tar == Target.SCORE)
        {
            num = 1;
            spr = new Sprite[]
            {
                ingrPrefab.GetComponent<TargetGUI>().star
            };
            for (int i = 0; i < num; i++)
                collectionItems.Add(Ingredients.Ingredient1);
            ingrTarget.Add(new CollectedIngredients());

            ingrTarget[0].count = 1;
        }

        int f = 0;

        for (int i = 0; i < num; i++)
        {
            if(collectionItems[i] != (object)0 && ingrTarget[i].count > 0)
            {
                f++;
            }
        }

        float offset = 100;
        if(ForDialog)
            offset = 200;

        containerRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal,
                                                (f - 1) * offset +
                                                ingrPrefab.transform.GetComponent<RectTransform>().rect.width / 2 * f -
                                                ingrPrefab.transform.GetComponent<RectTransform>().rect.width / 2 * (f - 2));

        int j = 0;

        for (int i = 0; i < num; i++)
        {
            if(collectionItems[i] != (object)0 && ingrTarget[i].count > 0)
            {
                GameObject ingr = Instantiate(ingrPrefab) as GameObject;
                ingr.name = "Ingr" + i;
                ingr.GetComponent<TargetGUI>().SetBack(ForDialog);
                listIngredientsGUIObjects.Add(ingr);
                if(tar != Target.COLLECT)
                    ingr.transform.Find("Image").GetComponent<Image>().sprite = spr[j];
                ingr.transform.Find("CountIngr").GetComponent<Counter_>().ingrTrackNumber = i;
                ingr.transform.Find("CountIngr").GetComponent<Counter_>().totalCount = ingrTarget[i].count;
                ingr.transform.Find("CountIngrForMenu").GetComponent<Counter_>().totalCount = ingrTarget[i].count;
                if(tar == Target.SCORE)
                    ingr.transform.Find("CountIngrForMenu").GetComponent<Counter_>().totalCount =
                        (int)LevelManager.THIS.starsTargetCount;
                else if(tar == Target.BLOCKS)
                    ingr.transform.Find("CountIngr").name = "TargetBlocks";
                else if(tar == Target.CAGES)
                    ingr.transform.Find("CountIngr").name = "TargetCages";
                else if(tar == Target.BOMBS)
                    ingr.transform.Find("CountIngr").name = "TargetBombs";

                if(tar == Target.COLLECT)
                {
                    ingr.GetComponent<TargetGUI>().SetSprite(ingrTarget[i].sprite);
                }

                ingr.transform.SetParent(parentTransform.transform);
                ingr.transform.localScale = Vector3.one;
                int heightPos = 0;

                ingr.transform.GetComponent<RectTransform>().anchoredPosition = new Vector3(
                    j * offset -
                    containerRect.rect.width / 2 +
                    ingr.transform.GetComponent<RectTransform>().rect.width / 2,
                    heightPos,
                    0);
                j++;
            }
        }
    }

    public void CheckCollectedTarget(GameObject _item)
    {
        if (_collectedTargetFlyController == null)
        {
            InitializeCollectedTargetFlyController();
        }

        _collectedTargetFlyController.TryFly(_item);
    }

    public int GetRestIngredients()
    {
        int count = 0;

        for (int i = 0; i < ingrTarget.Count; i++)
        {
            count += LevelManager.THIS.ingrTarget[i].count;
        }

        return count;
    }

    public void CheckWinLose()
    {
        if(Limit <= 0)
        {
            bool lose = false;
            Limit = 0;

            if(LevelManager.THIS.target == Target.BLOCKS && LevelManager.THIS.TargetBlocks > 0)
            {
                lose = true;
            }
            else if(LevelManager.THIS.target == Target.CAGES && LevelManager.THIS.TargetCages > 0)
            {
                lose = true;
            }
            else if(LevelManager.THIS.target == Target.COLLECT || LevelManager.THIS.target == Target.ITEMS)
            {
                if(GetRestIngredients() > 0)
                {
                    lose = true;
                }
            }
            else if(LevelManager.THIS.target == Target.SCORE && LevelManager.Score < GetScoresOfTargetStars())
            {
                lose = true;
            }

            if(LevelManager.Score < LevelManager.THIS.star1 && LevelManager.THIS.target != Target.SCORE)
            {
                lose = true;
            }

            if(lose)
                gameStatus = GameState.GameOver;
            else if(LevelManager.Score >= LevelManager.THIS.star1 &&
                    (LevelManager.THIS.target == Target.BOMBS) &&
                    LevelManager.THIS.TargetBombs >= bombsCollect)
            {
                gameStatus = GameState.PreWinAnimations;
            }
            else if(LevelManager.Score >= LevelManager.THIS.star1 &&
                    LevelManager.THIS.target == Target.BLOCKS &&
                    LevelManager.THIS.TargetBlocks <= 0)
            {
                gameStatus = GameState.PreWinAnimations;
            }
            else if(LevelManager.Score >= LevelManager.THIS.star1 &&
                    LevelManager.THIS.target == Target.CAGES &&
                    LevelManager.THIS.TargetCages <= 0)
            {
                gameStatus = GameState.PreWinAnimations;
            }
            else if(LevelManager.Score >= LevelManager.THIS.star1 &&
                    (LevelManager.THIS.target == Target.COLLECT || LevelManager.THIS.target == Target.ITEMS) &&
                    GetRestIngredients() <= 0)
            {
                gameStatus = GameState.PreWinAnimations;
            }
            else if(LevelManager.THIS.target == Target.SCORE && LevelManager.Score >= GetScoresOfTargetStars())
            {
                gameStatus = GameState.PreWinAnimations;
            }
        }
        else
        {
            bool win = false;

            if(LevelManager.THIS.target == Target.BLOCKS && LevelManager.THIS.TargetBlocks <= 0)
            {
                win = true;
            }

            if(LevelManager.THIS.target == Target.CAGES && LevelManager.THIS.TargetCages <= 0)
            {
                win = true;
            }

            if(LevelManager.THIS.target == Target.BOMBS && LevelManager.THIS.TargetBombs >= bombsCollect)
            {
                win = true;
            }
            else if(LevelManager.THIS.target == Target.COLLECT || LevelManager.THIS.target == Target.ITEMS)
            {
                win = true;

                if(GetRestIngredients() > 0)
                {
                    win = false;
                }
            }

            if(LevelManager.THIS.target == Target.SCORE && LevelManager.Score >= GetScoresOfTargetStars())
            {
                win = true;
            }

            if(LevelManager.Score < LevelManager.THIS.star1 && LevelManager.THIS.target != Target.SCORE)
            {
                win = false;
            }

            if(win)
                gameStatus = GameState.PreWinAnimations;
        }
    }

    public int GetScoresOfTargetStars()
    {
        return (int)this.GetType()
            .GetField("star" + (int)starsTargetCount)
            .GetValue(this); // Get value of appropriate field (star1, star2 or star3)
    }

    #endregion

    #region Score and Progression

    public void PopupScore(int value, Vector3 pos, int color)
    {
        Score += value;
        UpdateBar();
        CheckStars();

        ScorePopupTweenSpawner?.Show(value, pos, color, scoresColors);
    }

    void UpdateBar()
    {
        if (ProgressBarScript.Instance == null)
        {
            return;
        }

        ProgressBarScript.Instance.SetProgress(Score, star3);
    }

    void CheckStars()
    {
        int earnedStars = Score >= star3 ? 3 : Score >= star2 ? 2 : Score >= star1 ? 1 : 0;
        stars = Mathf.Max(stars, earnedStars);
    }

    #endregion

    #region Bomb Management

    public IEnumerator InitBombs()
    {
        yield return new WaitUntil(() => !TipsManager.THIS.gotTip); // 1.3
        yield return new WaitForSeconds(1);
        int bombsOnField = 0;
        List<Item> items = GetItems();

        foreach (Item item in items)
        {
            if(item.currentType == ItemsTypes.BOMB)
                bombsOnField++;
        }

        List<Item> itemsRand = GetRandomItems(bombsCollect - bombsOnField - LevelManager.THIS.TargetBombs); // 1.3
        int i = 0;

        foreach (Item item in itemsRand)
        {
            item.nextType = ItemsTypes.BOMB;

            if(bombTimers.Count > 0) // 1.3
                item.bombTimer = bombTimers[i];
            i++;
            item.ChangeType();
        }
    }

    public void RechargeBombs()
    {
        // 1.3
        StartCoroutine(InitBombs());
    }

    #endregion
}

[System.Serializable]
public class GemProduct
{
    public int count;
    public float price;
}

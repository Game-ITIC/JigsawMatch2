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

[RequireComponent(typeof(GridOutlineGenerator))]
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

    [Header("Grid Perimeter Line")]
    [Tooltip("Prefab with LineRenderer (use Assets/Line.prefab).")]
    [SerializeField] private GameObject gridPerimeterLinePrefab;

    [Tooltip("Z offset for the perimeter line (relative to GameField).")]
    [SerializeField] private float gridPerimeterLineZOffset = -0.2f;

    [Tooltip("Time used to draw each grid contour after the cells finish appearing.")]
    [SerializeField, Min(0.05f)] private float gridOutlineDrawDuration = 0.72f;

    [Tooltip("Small delay between separate contour loops.")]
    [SerializeField, Min(0f)] private float gridOutlineLoopDelay = 0.08f;

    private readonly List<GameObject> _gridPerimeterLineInstances = new List<GameObject>();
    private readonly List<Material> _gridPerimeterLineMaterials = new List<Material>();

    private readonly struct GridBoundaryEdge
    {
        public readonly Vector2Int Start;
        public readonly Vector2Int End;

        public GridBoundaryEdge(Vector2Int start, Vector2Int end)
        {
            Start = start;
            End = end;
        }
    }

    [SerializeField] private GridOutlineGenerator gridOutlineGenerator;

    #endregion

    #region Sprites

    // Sprite of square
    public Sprite squareSprite;

    // Second sprite of square
    public Sprite squareSprite1;

    // Sprites of collectable items
    public Sprite[] ingrediendSprites;

    // Double block sprite
    public Sprite doubleBlock;

    // Double solid block sprite
    public Sprite doubleSolidBlock;

    #endregion

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
    private ScoreTrackerService _scoreTrackerService;
    private BoardQueryService _boardQueryService;

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

            if(ShouldHideGridOutline(value))
                ClearGridPerimeterLines();
            else if(value == GameState.Pause)
                SetGridPerimeterLinesVisible(false);
            else if(value == GameState.Playing)
                SetGridPerimeterLinesVisible(true);

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
        new TargetUIResolverService(this).Resolve();
        InitializeCollectedTargetFlyController();
        _scorePopupTweenSpawner = GetComponent<ScorePopupTweenSpawner>();
        _levelEffectsController = GetComponent<LevelEffectsController>();
        _levelEffectsController?.Initialize(this);

        if(GameFeelManager.Instance != null)
        {
            GameFeelManager.Instance.BindCamera(gameCamera);
            GameFeelManager.Instance.BindBoard(GameField);
        }
        else
        {
            GameFeelManager.EnsureInitialized(gameCamera, GameField);
        }

        _boardMechanicsService = new BoardMechanicsService(this);
        _scoreTrackerService = new ScoreTrackerService(this);
        _boardQueryService = new BoardQueryService(this);

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

    public void InvokeUpdate()
    {
        if(_currentState != null)
        {
            _currentState.UpdateState();
        }

        // Now draw the lines based on the updated destroyAnyway list
        if(destroyAnyway.Count > 0)
        {
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
        ClearGridPerimeterLines();

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

        currentLevel = LevelProgressionHelper.CalculateLevel(currentLevel);
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

    private void DrawGridPerimeterLine()
    {
        if(GameField == null)
            return;

        if(gridPerimeterLinePrefab == null)
        {
            Debug.LogWarning("Grid perimeter line prefab is not assigned on LevelManager. Assign Assets/Line.prefab to 'Grid Perimeter Line Prefab'.", this);
            return;
        }

        ClearGridPerimeterLines();

        List<GridBoundaryEdge> edges = BuildGridBoundaryEdges();
        List<List<Vector2Int>> contours = TraceGridContours(edges);

        for(int i = 0; i < contours.Count; i++)
            CreateGridContourLine(contours[i], i);
    }

    private List<GridBoundaryEdge> BuildGridBoundaryEdges()
    {
        List<GridBoundaryEdge> edges = new List<GridBoundaryEdge>();

        for(int row = 0; row < maxRows; row++)
        {
            for(int col = 0; col < maxCols; col++)
            {
                if(!IsGridCellActive(col, row))
                    continue;

                Vector2Int topLeft = new Vector2Int(col, row);
                Vector2Int topRight = new Vector2Int(col + 1, row);
                Vector2Int bottomRight = new Vector2Int(col + 1, row + 1);
                Vector2Int bottomLeft = new Vector2Int(col, row + 1);

                // Clockwise winding keeps the playable area on the right side.
                if(!IsGridCellActive(col, row - 1))
                    edges.Add(new GridBoundaryEdge(topLeft, topRight));
                if(!IsGridCellActive(col + 1, row))
                    edges.Add(new GridBoundaryEdge(topRight, bottomRight));
                if(!IsGridCellActive(col, row + 1))
                    edges.Add(new GridBoundaryEdge(bottomRight, bottomLeft));
                if(!IsGridCellActive(col - 1, row))
                    edges.Add(new GridBoundaryEdge(bottomLeft, topLeft));
            }
        }

        return edges;
    }

    private bool IsGridCellActive(int col, int row)
    {
        if(col < 0 || col >= maxCols || row < 0 || row >= maxRows)
            return false;

        Square square = GetSquare(col, row, true);
        return square != null && square.type != SquareTypes.NONE;
    }

    private static List<List<Vector2Int>> TraceGridContours(List<GridBoundaryEdge> edges)
    {
        List<List<Vector2Int>> contours = new List<List<Vector2Int>>();

        while(edges.Count > 0)
        {
            GridBoundaryEdge edge = edges[0];
            edges.RemoveAt(0);

            Vector2Int start = edge.Start;
            Vector2Int current = edge.End;
            Vector2Int direction = edge.End - edge.Start;
            List<Vector2Int> contour = new List<Vector2Int> { start };
            int safety = 0;

            while(current != start && safety++ < 10000)
            {
                contour.Add(current);
                int nextIndex = FindNextBoundaryEdge(edges, current, direction);
                if(nextIndex < 0)
                    break;

                GridBoundaryEdge next = edges[nextIndex];
                edges.RemoveAt(nextIndex);
                direction = next.End - next.Start;
                current = next.End;
            }

            if(current == start && contour.Count >= 4)
                contours.Add(SimplifyGridContour(contour));
        }

        return contours;
    }

    private static int FindNextBoundaryEdge(
        List<GridBoundaryEdge> edges,
        Vector2Int start,
        Vector2Int previousDirection)
    {
        int previousDirectionIndex = GetGridDirectionIndex(previousDirection);
        int bestIndex = -1;
        int bestRank = int.MaxValue;

        for(int i = 0; i < edges.Count; i++)
        {
            if(edges[i].Start != start)
                continue;

            int directionIndex = GetGridDirectionIndex(edges[i].End - edges[i].Start);
            int turn = (directionIndex - previousDirectionIndex + 4) % 4;
            int rank = turn == 1 ? 0 : turn == 0 ? 1 : turn == 3 ? 2 : 3;
            if(rank >= bestRank)
                continue;

            bestRank = rank;
            bestIndex = i;
        }

        return bestIndex;
    }

    private static int GetGridDirectionIndex(Vector2Int direction)
    {
        if(direction.x > 0)
            return 0;
        if(direction.y > 0)
            return 1;
        if(direction.x < 0)
            return 2;
        return 3;
    }

    private static List<Vector2Int> SimplifyGridContour(List<Vector2Int> contour)
    {
        List<Vector2Int> simplified = new List<Vector2Int>();
        int count = contour.Count;

        for(int i = 0; i < count; i++)
        {
            Vector2Int previous = contour[(i - 1 + count) % count];
            Vector2Int current = contour[i];
            Vector2Int next = contour[(i + 1) % count];

            if(current - previous == next - current)
                continue;

            simplified.Add(current);
        }

        return simplified;
    }

    private void CreateGridContourLine(List<Vector2Int> contour, int contourIndex)
    {
        if(contour.Count < 4)
            return;

        GameObject instance = Instantiate(gridPerimeterLinePrefab, GameField, false);
        instance.name = $"Grid Perimeter Line {contourIndex + 1}";
        instance.transform.localPosition = Vector3.zero;
        instance.transform.localRotation = Quaternion.identity;
        instance.transform.localScale = Vector3.one;

        LineRenderer lineRenderer = instance.GetComponentInChildren<LineRenderer>();
        if(lineRenderer == null)
        {
            Debug.LogWarning("Grid perimeter line prefab has no LineRenderer component.", this);
            Destroy(instance);
            return;
        }

        lineRenderer.useWorldSpace = false;
        lineRenderer.loop = true;
        lineRenderer.positionCount = contour.Count;
        lineRenderer.numCornerVertices = Mathf.Max(lineRenderer.numCornerVertices, 6);
        lineRenderer.numCapVertices = Mathf.Max(lineRenderer.numCapVertices, 4);
        lineRenderer.alignment = LineAlignment.View;
        lineRenderer.textureMode = LineTextureMode.Stretch;
        lineRenderer.sortingLayerName = "Default";
        lineRenderer.sortingOrder = 50;
        lineRenderer.widthMultiplier = Mathf.Max(lineRenderer.widthMultiplier, 0.24f);

        // Tent 2 is an opaque URP material. Its normal ZWrite setting can make
        // the line cover Screen Space Camera canvases. Use a private copy that
        // keeps Tent 2's appearance but renders before UI without writing depth.
        Material sharedMaterial = lineRenderer.sharedMaterial;
        if(sharedMaterial != null)
        {
            Material runtimeMaterial = new Material(sharedMaterial)
            {
                name = sharedMaterial.name + " (Grid Outline Instance)",
                renderQueue = 3000
            };

            runtimeMaterial.SetOverrideTag("RenderType", "Transparent");
            runtimeMaterial.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            runtimeMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");

            if(runtimeMaterial.HasProperty("_Surface"))
                runtimeMaterial.SetFloat("_Surface", 1f);
            if(runtimeMaterial.HasProperty("_Blend"))
                runtimeMaterial.SetFloat("_Blend", 0f);
            if(runtimeMaterial.HasProperty("_SrcBlend"))
                runtimeMaterial.SetFloat("_SrcBlend", 5f); // SrcAlpha
            if(runtimeMaterial.HasProperty("_DstBlend"))
                runtimeMaterial.SetFloat("_DstBlend", 10f); // OneMinusSrcAlpha
            if(runtimeMaterial.HasProperty("_ZWrite"))
                runtimeMaterial.SetFloat("_ZWrite", 0f);

            lineRenderer.sharedMaterial = runtimeMaterial;
            _gridPerimeterLineMaterials.Add(runtimeMaterial);
        }

        List<Vector3> positions = new List<Vector3>(contour.Count);
        for(int i = 0; i < contour.Count; i++)
            positions.Add(GridCornerToLocalPosition(contour[i]));

        lineRenderer.SetPositions(positions.ToArray());

        GridOutlineRevealAnimation reveal = instance.AddComponent<GridOutlineRevealAnimation>();
        reveal.Play(
            lineRenderer,
            positions,
            gridOutlineDrawDuration,
            contourIndex * gridOutlineLoopDelay);

        _gridPerimeterLineInstances.Add(instance);
    }

    private Vector3 GridCornerToLocalPosition(Vector2Int corner)
    {
        float x = firstSquarePosition.x + (corner.x - 0.5f) * squareWidth;
        float y = firstSquarePosition.y - (corner.y - 0.5f) * squareHeight;
        return new Vector3(x, y, gridPerimeterLineZOffset);
    }

    private void ClearGridPerimeterLines()
    {
        for(int i = 0; i < _gridPerimeterLineInstances.Count; i++)
        {
            if(_gridPerimeterLineInstances[i] != null)
            {
                _gridPerimeterLineInstances[i].SetActive(false);
                Destroy(_gridPerimeterLineInstances[i]);
            }
        }

        _gridPerimeterLineInstances.Clear();

        for(int i = 0; i < _gridPerimeterLineMaterials.Count; i++)
        {
            if(_gridPerimeterLineMaterials[i] != null)
                Destroy(_gridPerimeterLineMaterials[i]);
        }

        _gridPerimeterLineMaterials.Clear();
    }

    private void SetGridPerimeterLinesVisible(bool visible)
    {
        for(int i = 0; i < _gridPerimeterLineInstances.Count; i++)
        {
            if(_gridPerimeterLineInstances[i] != null)
                _gridPerimeterLineInstances[i].SetActive(visible);
        }
    }

    private static bool ShouldHideGridOutline(GameState state)
    {
        // Keep the outline visible while the board plays its final animation.
        // Win/GameOver are assigned before their result popups are shown, so
        // clearing here prevents the line from remaining behind the UI.
        return state == GameState.Win ||
               state == GameState.GameOver ||
               state == GameState.ToMap ||
               state == GameState.Map;
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
        // Disable sprite-based outlines and use the Line prefab instead.
        DisableAndClearSpriteOutlines();
        DrawGridPerimeterLine();
    }

    public void ClearOutline()
    {
        ClearGridPerimeterLines();
    }

    private GridOutlineGenerator ResolveGridOutlineGenerator()
    {
        if(gridOutlineGenerator == null)
            gridOutlineGenerator = GetComponent<GridOutlineGenerator>();

        return gridOutlineGenerator;
    }

    private void DisableAndClearSpriteOutlines()
    {
        GridOutlineGenerator generator = ResolveGridOutlineGenerator();
        if(generator != null)
            generator.enabled = false;

        // GridOutlineGenerator creates child objects named "outline" under squares.
        // Clear them so we don't keep the old visual border after switching to Line.prefab.
        if(GameField == null)
            return;

        for(int i = 0; i < GameField.childCount; i++)
        {
            Transform square = GameField.GetChild(i);
            if(square == null)
                continue;

            // Iterate backwards because we may destroy children.
            for(int j = square.childCount - 1; j >= 0; j--)
            {
                Transform child = square.GetChild(j);
                if(child != null && child.name == "outline")
                    Destroy(child.gameObject);
            }
        }
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

    public Square GetSquare(int col, int row, bool safe = false) =>
        _boardQueryService.GetSquare(col, row, safe);

    public List<Item> GetRow(int row) => _boardQueryService.GetRow(row);

    public List<Square> GetRowSquare(int row) => _boardQueryService.GetRowSquare(row);

    public List<Item> GetColumn(int col) => _boardQueryService.GetColumn(col);

    public List<Square> GetColumnSquare(int col) => _boardQueryService.GetColumnSquare(col);

    public List<Item> GetRandomItems(int count) => _boardQueryService.GetRandomItems(count);

    public List<Item> GetAllExtraItems() => _boardQueryService.GetAllExtraItems();

    public List<Item> GetItemsAround(Square square) => _boardQueryService.GetItemsAround(square);

    public List<Square> GetSquaresAround(Square square) => _boardQueryService.GetSquaresAround(square);

    public List<Item> GetItems() => _boardQueryService.GetItems();

    public List<Square> GetSquares() => _boardQueryService.GetSquares();

    public List<Square> GetBottomRow() => _boardQueryService.GetBottomRow();

    public List<Item> GetIngredients(int i = -1) => _boardQueryService.GetIngredients(i);

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
        _scoreTrackerService.PopupScore(value, pos, color);
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

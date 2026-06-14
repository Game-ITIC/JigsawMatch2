using Cysharp.Threading.Tasks;
using Interfaces;
using Models;
using Monobehaviours.Buildings;
using R3;
using Systems;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using VContainer;
using Views;

public class MenuView : MonoBehaviour, IPreload
{
    [SerializeField] private Button taskButton;
    [SerializeField] private Button shopButton;
    [SerializeField] private Button mainButton;
    [SerializeField] private Button starsCountButton;
    [SerializeField] private Button diamondCountButton;
    [SerializeField] private GameObject dailyAndTasksPanel;
    [SerializeField] private GameObject shopPanel;

    [SerializeField] private Button startGame;
    [SerializeField] private TextView startGameText;
    [SerializeField] private Button buildButton;
    [SerializeField] private BuildingShopManager buildingShopManager;
    [SerializeField] private Button dailyButton;
    [SerializeField] private Button inAppButton;
    [SerializeField] private Button mapButton;

    private readonly CompositeDisposable _disposable = new();
    private StarModel _starModel;
    private GemModel _gemModel;
    private HealthSystem _healthSystem;
    private TMP_Text _starsCountText;
    private TMP_Text _diamondCountText;
    private TMP_Text _lifeCountText;
    private bool _hudBound;

    public Button StartGame => startGame;
    public Button DailyButton => dailyButton;
    public Button InAppButton => inAppButton;
    public TextView StartGameText => startGameText;
    public Button MapButton => mapButton;
    public Button BuildButton => buildButton;

    [Inject]
    private void Inject(StarModel starModel, GemModel gemModel, HealthSystem healthSystem)
    {
        _starModel = starModel;
        _gemModel = gemModel;
        _healthSystem = healthSystem;
    }

    public async UniTask Warmup()
    {
        ResolveReferences();
        WirePanelButtons();
        BindHud();

        await UniTask.Yield();
    }

    private void ResolveReferences()
    {
        taskButton = taskButton != null ? taskButton : FindButton("Task Button 01");
        taskButton = taskButton != null ? taskButton : dailyButton;

        shopButton = shopButton != null ? shopButton : FindButton("ShopButton", "Shop Button");
        shopButton = shopButton != null ? shopButton : inAppButton;

        mainButton = mainButton != null ? mainButton : FindButton("MainMenuButton", "Main Button", "MainMenu Button");
        mainButton = mainButton != null ? mainButton : mapButton;

        starsCountButton = starsCountButton != null ? starsCountButton : FindButton("Stars Count Button", "StarsCountButton");
        diamondCountButton = diamondCountButton != null
            ? diamondCountButton
            : FindButton("Diamond Count Button", "Diamond Count Button Variant", "Dimond Count Button Variant");

        dailyAndTasksPanel = dailyAndTasksPanel != null
            ? dailyAndTasksPanel
            : FindSceneObject("Daily And Tasks Panel");
        shopPanel = shopPanel != null ? shopPanel : FindSceneObject("Shop Panel");

        _starsCountText = _starsCountText != null ? _starsCountText : FindTextInsideButton(starsCountButton);
        _diamondCountText = _diamondCountText != null ? _diamondCountText : FindTextInsideButton(diamondCountButton);
        _lifeCountText = _lifeCountText != null ? _lifeCountText : FindText("CountHelth", "CountHealth", "Life Count", "Lives Count");
    }

    private void WirePanelButtons()
    {
        if(dailyAndTasksPanel != null)
        {
            WireButton(taskButton, ToggleDailyAndTasksPanel);
            WireCloseButtons(dailyAndTasksPanel, CloseDailyAndTasksPanel);
            dailyAndTasksPanel.SetActive(false);
        }

        if(shopPanel != null)
        {
            WireButton(shopButton, OpenShopPanel);
            WireButton(starsCountButton, OpenShopPanel);
            WireButton(diamondCountButton, OpenShopPanel);
            WireCloseButtons(shopPanel, CloseShopPanel);
            shopPanel.SetActive(false);
        }

        if(dailyAndTasksPanel != null || shopPanel != null)
        {
            WireButton(mainButton, CloseAllPanels);
        }
    }

    private void BindHud()
    {
        if(_hudBound)
        {
            UpdateHudValues();
            return;
        }

        _hudBound = true;

        if(_starModel != null && _starsCountText != null)
        {
            _starModel.Stars.Subscribe(value => _starsCountText.SetText(value.ToString())).AddTo(_disposable);
        }

        if(_gemModel != null && _diamondCountText != null)
        {
            _gemModel.Gems.Subscribe(value => _diamondCountText.SetText(value.ToString())).AddTo(_disposable);
        }

        if(_healthSystem != null && _lifeCountText != null)
        {
            _healthSystem.CurrentLives.Subscribe(value => _lifeCountText.SetText(value.ToString())).AddTo(_disposable);
            Observable.EveryUpdate(UnityFrameProvider.EarlyUpdate)
                .Subscribe(_ => _healthSystem.UpdateRegeneration())
                .AddTo(_disposable);
        }

        UpdateHudValues();
    }

    private void UpdateHudValues()
    {
        if(_starModel != null && _starsCountText != null)
        {
            _starsCountText.SetText(_starModel.Stars.Value.ToString());
        }

        if(_gemModel != null && _diamondCountText != null)
        {
            _diamondCountText.SetText(_gemModel.Gems.Value.ToString());
        }

        if(_healthSystem != null && _lifeCountText != null)
        {
            _lifeCountText.SetText(_healthSystem.CurrentLives.Value.ToString());
        }
    }

    private void ToggleDailyAndTasksPanel()
    {
        if(dailyAndTasksPanel == null)
        {
            return;
        }

        var isVisible = !dailyAndTasksPanel.activeSelf;
        CloseAllPanels();
        dailyAndTasksPanel.SetActive(isVisible);
    }

    private void OpenShopPanel()
    {
        if(shopPanel == null)
        {
            return;
        }

        CloseAllPanels();
        shopPanel.SetActive(true);
    }

    private void CloseDailyAndTasksPanel()
    {
        if(dailyAndTasksPanel != null)
        {
            dailyAndTasksPanel.SetActive(false);
        }
    }

    private void CloseShopPanel()
    {
        if(shopPanel != null)
        {
            shopPanel.SetActive(false);
        }
    }

    private void CloseAllPanels()
    {
        CloseDailyAndTasksPanel();
        CloseShopPanel();
    }

    private static void WireButton(Button button, UnityEngine.Events.UnityAction action)
    {
        if(button == null)
        {
            return;
        }

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(action);
    }

    private static void WireCloseButtons(GameObject panel, UnityEngine.Events.UnityAction action)
    {
        var buttons = panel.GetComponentsInChildren<Button>(true);

        foreach (var button in buttons)
        {
            var buttonName = button.name.ToLowerInvariant();

            if(!buttonName.Contains("close") && !buttonName.Contains("back"))
            {
                continue;
            }

            WireButton(button, action);
        }
    }

    private Button FindButton(params string[] names)
    {
        var buttonObject = FindSceneObject(names);
        return buttonObject != null ? buttonObject.GetComponent<Button>() : null;
    }

    private TMP_Text FindText(params string[] names)
    {
        var textObject = FindSceneObject(names);
        return textObject != null ? textObject.GetComponent<TMP_Text>() : null;
    }

    private static TMP_Text FindTextInsideButton(Button button)
    {
        if(button == null)
        {
            return null;
        }

        return button.GetComponentInChildren<TMP_Text>(true);
    }

    private static GameObject FindSceneObject(params string[] names)
    {
        GameObject inactiveMatch = null;

        for (var sceneIndex = 0; sceneIndex < SceneManager.sceneCount; sceneIndex++)
        {
            var scene = SceneManager.GetSceneAt(sceneIndex);

            if(!scene.isLoaded)
            {
                continue;
            }

            var roots = scene.GetRootGameObjects();

            foreach (var root in roots)
            {
                var match = FindInChildren(root.transform, names, ref inactiveMatch);

                if(match != null)
                {
                    return match;
                }
            }
        }

        return inactiveMatch;
    }

    private static GameObject FindInChildren(Transform parent, string[] names, ref GameObject inactiveMatch)
    {
        var current = parent.gameObject;

        if(MatchesName(current.name, names))
        {
            if(current.activeInHierarchy)
            {
                return current;
            }

            inactiveMatch ??= current;
        }

        for (var i = 0; i < parent.childCount; i++)
        {
            var match = FindInChildren(parent.GetChild(i), names, ref inactiveMatch);

            if(match != null)
            {
                return match;
            }
        }

        return null;
    }

    private static bool MatchesName(string objectName, string[] names)
    {
        foreach (var name in names)
        {
            if(string.Equals(objectName, name, System.StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    private void OnDestroy()
    {
        _disposable.Dispose();
    }
}

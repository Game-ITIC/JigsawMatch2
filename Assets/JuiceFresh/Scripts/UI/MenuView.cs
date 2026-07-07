using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Interfaces;
using Models;
using Monobehaviours.Buildings;
using R3;
using Systems;
using TMPro;
using UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Events;
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
    [SerializeField] private GameObject dailyContentPanel;
    [SerializeField] private GameObject tasksContentPanel;
    [SerializeField] private GameObject dailyRewardsPanel;
    [SerializeField] private GameObject shopPanel;
    [SerializeField] private bool animatePanelTransitions = true;
    [SerializeField, Min(0f)] private float panelOpenDuration = 0.28f;
    [SerializeField, Min(0f)] private float panelCloseDuration = 0.18f;
    [SerializeField] private AnimationCurve panelOpenCurve = CurvedUIPanelAnimator.CreateDefaultOpenCurve();
    [SerializeField] private AnimationCurve panelCloseCurve = CurvedUIPanelAnimator.CreateDefaultCloseCurve();
    [SerializeField] private AnimationCurve panelFadeCurve = CurvedUIPanelAnimator.CreateDefaultFadeCurve();

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
    private readonly List<TMP_Text> _starsCountTexts = new();
    private readonly List<TMP_Text> _diamondCountTexts = new();
    private readonly List<TMP_Text> _lifeCountTexts = new();
    private readonly List<TMP_Text> _lifeStatusTexts = new();
    private bool _hudBound;
    private string _lastLifeStatusText;

    private enum DailyAndTasksTab
    {
        Daily,
        Tasks
    }

    public Button StartGame => startGame;
    public Button DailyButton => dailyButton;
    public Button DailyRewardsButton => dailyButton != null && dailyButton != taskButton ? dailyButton : null;
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
        taskButton = taskButton != null ? taskButton : FindButton("Task Button 01", "TaskButton");

        dailyButton = dailyButton != null && dailyButton != taskButton
            ? dailyButton
            : FindButton("Daily Button", "Task Button 02", "DailyButton");

        shopButton = shopButton != null ? shopButton : FindButton("ShopButton", "Shop Button");
        shopButton = shopButton != null ? shopButton : inAppButton;

        mainButton = mainButton != null ? mainButton : FindButton("MainMenuButton", "Main Button", "MainMenu Button");
        mainButton = mainButton != null ? mainButton : mapButton;

        starsCountButton = starsCountButton != null ? starsCountButton : FindButton("Stars Count Button", "StarsCountButton");
        diamondCountButton = diamondCountButton != null
            ? diamondCountButton
            : FindButton("Diamond Count Button Variant", "Dimond Count Button Variant", "Diamond Count Button");

        dailyAndTasksPanel = dailyAndTasksPanel != null
            ? dailyAndTasksPanel
            : FindSceneObject("Daily And Tasks Panel");
        dailyContentPanel = dailyContentPanel != null
            ? dailyContentPanel
            : FindSceneObject("Daily Content Panel");
        tasksContentPanel = tasksContentPanel != null
            ? tasksContentPanel
            : FindSceneObject("Tasks Content Panel");
        dailyRewardsPanel = dailyRewardsPanel != null
            ? dailyRewardsPanel
            : FindSceneObject("DailyRewardsPanel", "Daily Rewards Panel");
        shopPanel = shopPanel != null ? shopPanel : FindSceneObject("Shop Panel", "Shop Content Panel");

        RefreshCounterTextCaches();
    }

    private void WirePanelButtons()
    {
        var dailyPanel = GetDailyPanel();

        if(dailyPanel != null)
        {
            WireButtons(FindButtons(dailyButton, "Daily Button", "Task Button 02", "DailyButton"), OpenDailyContentPanel);
            WireButtons(FindButtons(taskButton, "Task Button 01", "TaskButton"), OpenTasksContentPanel);
            WireButtons(FindButtons("Daily Tab Button"), () => OpenDailyAndTasksPanel(DailyAndTasksTab.Daily));
            WireButtons(FindButtons("Tasks Tab Button"), () => OpenDailyAndTasksPanel(DailyAndTasksTab.Tasks));
            WireCloseButtons(dailyPanel, CloseDailyAndTasksPanel);
            HidePanelImmediate(dailyPanel);
            ShowDailyAndTasksTab(DailyAndTasksTab.Daily);
        }

        if(shopPanel != null)
        {
            WireButtons(
                FindButtons(
                    shopButton,
                    inAppButton,
                    starsCountButton,
                    diamondCountButton,
                    "ShopButton",
                    "Shop Button",
                    "Stars Count Button",
                    "StarsCountButton",
                    "Diamond Count Button Variant",
                    "Dimond Count Button Variant",
                    "Diamond Count Button"),
                OpenShopPanel);

            WireCloseButtons(shopPanel, CloseShopPanel);
            HidePanelImmediate(shopPanel);
        }

        if(dailyPanel != null || shopPanel != null)
        {
            WireButtons(
                FindButtons(mainButton, mapButton, "MainMenuButton", "Main Button", "MainMenu Button", "IslandButton"),
                CloseAllPanels);
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

        if(_starModel != null && _starsCountTexts.Count > 0)
        {
            _starModel.Stars.Subscribe(value => SetTexts(_starsCountTexts, value)).AddTo(_disposable);
        }

        if(_gemModel != null && _diamondCountTexts.Count > 0)
        {
            _gemModel.Gems.Subscribe(value => SetTexts(_diamondCountTexts, value)).AddTo(_disposable);
        }

        if(_healthSystem != null && (_lifeCountTexts.Count > 0 || _lifeStatusTexts.Count > 0))
        {
            _healthSystem.CurrentLives.Subscribe(_ => UpdateLifeTexts()).AddTo(_disposable);

            // "TimeLives" is based on remaining time until next life.
            // It changes every second, even when CurrentLives doesn't change.
            Observable.Interval(TimeSpan.FromSeconds(1))
                .Subscribe(_ =>
                           {
                               _healthSystem.UpdateRegeneration();
                               SetLifeStatusTexts();
                           })
                .AddTo(_disposable);
        }

        UpdateHudValues();
    }

    private void UpdateHudValues()
    {
        if(_starModel != null)
        {
            SetTexts(_starsCountTexts, _starModel.Stars.Value);
        }

        if(_gemModel != null)
        {
            SetTexts(_diamondCountTexts, _gemModel.Gems.Value);
        }

        if(_healthSystem != null)
        {
            UpdateLifeTexts();
        }
    }

    public void ToggleDailyAndTasksPanel()
    {
        var dailyPanel = GetDailyPanel();

        if(dailyPanel == null)
        {
            return;
        }

        if(dailyPanel.activeSelf)
        {
            HidePanel(dailyPanel);
            return;
        }

        OpenDailyAndTasksPanel(DailyAndTasksTab.Daily);
    }

    public void OpenDailyContentPanel()
    {
        OpenDailyAndTasksPanel(DailyAndTasksTab.Daily);
    }

    public void OpenTasksContentPanel()
    {
        OpenDailyAndTasksPanel(DailyAndTasksTab.Tasks);
    }

    private void OpenDailyAndTasksPanel(DailyAndTasksTab tab)
    {
        var dailyPanel = GetDailyPanel();

        if(dailyPanel == null)
        {
            return;
        }

        CloseShopPanel();
        ShowDailyAndTasksTab(tab);

        var canvasGroup = dailyPanel.GetComponent<CanvasGroup>();
        if(dailyPanel.activeInHierarchy && canvasGroup != null && canvasGroup.alpha > 0.99f)
        {
            return;
        }

        ShowPanel(dailyPanel);
    }

    private void ShowDailyAndTasksTab(DailyAndTasksTab tab)
    {
        var dailyPanel = dailyContentPanel != null ? dailyContentPanel : dailyRewardsPanel;

        if(dailyPanel != null)
        {
            dailyPanel.SetActive(tab == DailyAndTasksTab.Daily);
        }

        if(tasksContentPanel != null)
        {
            tasksContentPanel.SetActive(tab == DailyAndTasksTab.Tasks);
        }
    }

    public void OpenShopPanel()
    {
        if(shopPanel == null)
        {
            return;
        }

        CloseDailyAndTasksPanel();
        ShowPanel(shopPanel);
    }

    public void CloseDailyAndTasksPanel()
    {
        if(dailyAndTasksPanel != null)
        {
            HidePanel(dailyAndTasksPanel);
        }

        // if(dailyRewardsPanel != null)
        // {
        //     HidePanel(dailyRewardsPanel);
        // }
    }

    public void CloseShopPanel()
    {
        if(shopPanel != null)
        {
            HidePanel(shopPanel);
        }
    }

    public void CloseAllPanels()
    {
        CloseDailyAndTasksPanel();
        CloseShopPanel();
    }

    private GameObject GetDailyPanel()
    {
        return dailyAndTasksPanel != null ? dailyAndTasksPanel : dailyRewardsPanel;
    }

    private void ShowPanel(GameObject panel)
    {
        ActivateParents(panel);
        BringToFront(panel);

        CurvedUIPanelAnimator.Show(
            panel,
            animatePanelTransitions ? panelOpenDuration : 0f,
            panelOpenCurve,
            panelFadeCurve);
    }

    private void HidePanel(GameObject panel)
    {
        if(animatePanelTransitions)
        {
            CurvedUIPanelAnimator.Hide(panel, panelCloseDuration, panelCloseCurve, panelFadeCurve);
            return;
        }

        CurvedUIPanelAnimator.HideImmediate(panel);
    }

    private static void HidePanelImmediate(GameObject panel)
    {
        CurvedUIPanelAnimator.HideImmediate(panel);
    }

    private static void ActivateParents(GameObject panel)
    {
        if(panel == null)
        {
            return;
        }

        var parent = panel.transform.parent;
        if(parent == null)
        {
            return;
        }

        ActivateParents(parent.gameObject);

        if(!parent.gameObject.activeSelf)
        {
            parent.gameObject.SetActive(true);
        }
    }

    private static void BringToFront(GameObject panel)
    {
        if(panel == null)
        {
            return;
        }

        var parent = panel.transform.parent;
        if(parent != null)
        {
            parent.SetAsLastSibling();
            KeepSiblingOnTop(parent, "HUD Side Bar");
        }

        panel.transform.SetAsLastSibling();
    }

    private static void KeepSiblingOnTop(Transform transform, string siblingName)
    {
        var root = transform.parent;
        if(root == null)
        {
            return;
        }

        for(var i = 0; i < root.childCount; i++)
        {
            var sibling = root.GetChild(i);
            if(string.Equals(sibling.name.Trim(), siblingName, System.StringComparison.OrdinalIgnoreCase))
            {
                sibling.SetAsLastSibling();
                return;
            }
        }
    }

    private static void WireButtons(List<Button> buttons, UnityAction action)
    {
        foreach (var button in buttons)
        {
            WireButton(button, action);
        }
    }

    private static void WireButton(Button button, UnityAction action)
    {
        if(button == null)
        {
            return;
        }

        button.onClick.RemoveListener(action);
        button.onClick.AddListener(action);
    }

    private static void WireCloseButtons(GameObject panel, UnityAction action)
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

    private void RefreshCounterTextCaches()
    {
        _starsCountTexts.Clear();
        _diamondCountTexts.Clear();
        _lifeCountTexts.Clear();
        _lifeStatusTexts.Clear();

        AddCounterTexts(_starsCountTexts, starsCountButton);
        AddCounterTexts(_diamondCountTexts, diamondCountButton);

        AddCounterTextsByName(_starsCountTexts, "Stars Count Button", "StarsCountButton");
        AddCounterTextsByName(_diamondCountTexts, "Diamond Count Button Variant", "Dimond Count Button Variant", "Diamond Count Button");
        AddCounterTextsByName(_lifeCountTexts, "CountHelth", "CountHealth", "Life Count", "Lives Count");
        AddLifeStatusTexts();
    }

    private Button FindButton(params string[] names)
    {
        var buttonObject = FindSceneObject(names);
        return buttonObject != null ? buttonObject.GetComponent<Button>() : null;
    }

    private static List<Button> FindButtons(params object[] referencesAndNames)
    {
        var buttons = new List<Button>();

        foreach (var referenceOrName in referencesAndNames)
        {
            if(referenceOrName is Button button)
            {
                AddUnique(buttons, button);
                continue;
            }

            var name = referenceOrName as string;

            if(name == null)
            {
                continue;
            }

            foreach (var sceneObject in FindSceneObjects(name))
            {
                var sceneButton = sceneObject.GetComponent<Button>();
                AddUnique(buttons, sceneButton);
            }
        }

        return buttons;
    }

    private static void AddCounterTexts(List<TMP_Text> texts, Button button)
    {
        if(button == null)
        {
            return;
        }

        AddCounterTexts(texts, button.gameObject);
    }

    private static void AddCounterTextsByName(List<TMP_Text> texts, params string[] names)
    {
        foreach (var sceneObject in FindSceneObjects(names))
        {
            AddCounterTexts(texts, sceneObject);
        }
    }

    private static void AddCounterTexts(List<TMP_Text> texts, GameObject root)
    {
        if(root == null)
        {
            return;
        }

        var rootText = root.GetComponent<TMP_Text>();
        AddUnique(texts, rootText);

        foreach (var text in root.GetComponentsInChildren<TMP_Text>(true))
        {
            AddUnique(texts, text);
        }
    }

    private void AddLifeStatusTexts()
    {
        AddCounterTextsByName(_lifeStatusTexts, "TimeLives");

        foreach (var healthBar in FindSceneObjects("HealthBar"))
        {
            foreach (var text in healthBar.GetComponentsInChildren<TMP_Text>(true))
            {
                if(IsLifeCountText(text.gameObject.name))
                {
                    continue;
                }

                if(MatchesName(text.gameObject.name, new[] { "TimeLives" })
                   || text.gameObject.name.StartsWith("Text (TMP", System.StringComparison.OrdinalIgnoreCase))
                {
                    AddUnique(_lifeStatusTexts, text);
                }
            }
        }
    }

    private static GameObject FindSceneObject(params string[] names)
    {
        var matches = FindSceneObjects(names);

        foreach (var match in matches)
        {
            if(match.activeInHierarchy)
            {
                return match;
            }
        }

        return matches.Count > 0 ? matches[0] : null;
    }

    private static List<GameObject> FindSceneObjects(params string[] names)
    {
        var matches = new List<GameObject>();

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
                FindInChildren(root.transform, names, matches);
            }
        }

        return matches;
    }

    private static void FindInChildren(Transform parent, string[] names, List<GameObject> matches)
    {
        var current = parent.gameObject;

        if(MatchesName(current.name, names))
        {
            AddUnique(matches, current);
        }

        for (var i = 0; i < parent.childCount; i++)
        {
            FindInChildren(parent.GetChild(i), names, matches);
        }
    }

    private static bool MatchesName(string objectName, string[] names)
    {
        var normalizedObjectName = objectName.Trim();

        foreach (var name in names)
        {
            if(string.Equals(normalizedObjectName, name.Trim(), System.StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    private static bool IsLifeCountText(string objectName)
    {
        var lifeCountNames = new[]
        {
            "CountHelth",
            "CountHealth",
            "Life Count",
            "Lives Count"
        };

        return MatchesName(objectName, lifeCountNames);
    }

    private static void SetTexts(List<TMP_Text> texts, int value)
    {
        var valueText = value.ToString();

        foreach (var text in texts)
        {
            if(text != null)
            {
                text.SetText(valueText);
            }
        }
    }

    private void UpdateLifeTexts()
    {
        SetTexts(_lifeCountTexts, _healthSystem.CurrentLives.Value);
        SetLifeStatusTexts();
    }

    private void SetLifeStatusTexts()
    {
        if(_lifeStatusTexts.Count <= 0)
        {
            return;
        }

        var statusText = _healthSystem.GetLifeStatusText();

        if(_lastLifeStatusText == statusText)
        {
            return;
        }

        _lastLifeStatusText = statusText;

        foreach (var text in _lifeStatusTexts)
        {
            if(text != null)
            {
                text.SetText(statusText);
            }
        }
    }

    private static void AddUnique<T>(List<T> items, T item)
        where T : UnityEngine.Object
    {
        if(item != null && !items.Contains(item))
        {
            items.Add(item);
        }
    }

    private void OnDestroy()
    {
        _disposable.Dispose();
    }
}

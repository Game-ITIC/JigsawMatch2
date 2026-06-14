using System;
using System.Collections.Generic;
using UI;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuPanelController : MonoBehaviour
{
    [SerializeField] private string[] taskButtonNames = { "Task Button 01", "TaskButton", "Tasks Button", "Daily Button", "DailyButton" };
    [SerializeField] private string[] shopButtonNames = { "ShopButton", "Shop Button" };
    [SerializeField] private string[] starsCountButtonNames = { "Stars Count Button", "StarsCountButton" };
    [SerializeField] private string[] diamondCountButtonNames =
    {
        "Diamond Count Button",
        "Diamond Count Button Variant",
        "Dimond Count Button Variant"
    };

    [SerializeField] private string[] dailyAndTasksPanelNames = { "Daily And Tasks Panel", "DailyAndTasksPanel", "Daily Tasks Panel", "Tasks Panel", "DailyRewardsPanel", "Daily Rewards Panel" };
    [SerializeField] private string[] shopPanelNames = { "Shop Panel", "Shop Content Panel", "ShopContentPanel" };
    [SerializeField] private string[] closeButtonNames = { "Close Button", "CloseButton", "Close", "Back Button", "BackButton" };
    [SerializeField] private bool hidePanelsOnStart = true;
    [SerializeField] private bool closeOtherPanelsOnOpen = true;
    [SerializeField] private bool animatePanelTransitions = true;
    [SerializeField, Min(0f)] private float panelOpenDuration = 0.28f;
    [SerializeField, Min(0f)] private float panelCloseDuration = 0.18f;
    [SerializeField] private AnimationCurve panelOpenCurve = CurvedUIPanelAnimator.CreateDefaultOpenCurve();
    [SerializeField] private AnimationCurve panelCloseCurve = CurvedUIPanelAnimator.CreateDefaultCloseCurve();
    [SerializeField] private AnimationCurve panelFadeCurve = CurvedUIPanelAnimator.CreateDefaultFadeCurve();

    private readonly List<Button> dailyCloseButtons = new List<Button>();
    private readonly List<Button> shopCloseButtons = new List<Button>();

    private Button taskButton;
    private Button shopButton;
    private Button starsCountButton;
    private Button diamondCountButton;
    private GameObject dailyAndTasksPanel;
    private GameObject shopPanel;
    private UnityAction toggleDailyAction;
    private UnityAction toggleShopAction;
    private UnityAction openShopAction;
    private UnityAction closeDailyAction;
    private UnityAction closeShopAction;
    private float refreshListenersUntil;
    private bool warnedMissingDailyPanel;
    private bool warnedMissingShopPanel;

    private void Awake()
    {
        toggleDailyAction = ToggleDailyAndTasksPanel;
        toggleShopAction = ToggleShopPanel;
        openShopAction = OpenShopPanel;
        closeDailyAction = CloseDailyAndTasksPanel;
        closeShopAction = CloseShopPanel;

        ResolveObjects();

        if (hidePanelsOnStart)
            CloseAllPanelsImmediate();
    }

    private void OnEnable()
    {
        refreshListenersUntil = Time.unscaledTime + 2f;
        WireButtons();
    }

    private void Start()
    {
        ResolveObjects();
        WireButtons();

        if (hidePanelsOnStart)
            CloseAllPanelsImmediate();
    }

    private void LateUpdate()
    {
        if (Time.unscaledTime > refreshListenersUntil)
            return;

        ResolveObjects();
        WireButtons();
    }

    private void OnDisable()
    {
        UnwireButtons();
    }

    public void ToggleDailyAndTasksPanel()
    {
        ResolveObjects();

        if (dailyAndTasksPanel == null)
        {
            WarnMissingPanel(ref warnedMissingDailyPanel, "Daily And Tasks Panel");
            return;
        }

        SetDailyAndTasksPanelVisible(!dailyAndTasksPanel.activeSelf);
    }

    public void ToggleShopPanel()
    {
        ResolveObjects();

        if (shopPanel == null)
        {
            WarnMissingPanel(ref warnedMissingShopPanel, "Shop Panel");
            return;
        }

        SetShopPanelVisible(!shopPanel.activeSelf);
    }

    public void OpenDailyAndTasksPanel()
    {
        ResolveObjects();
        SetDailyAndTasksPanelVisible(true);
    }

    public void CloseDailyAndTasksPanel()
    {
        ResolveObjects();
        SetDailyAndTasksPanelVisible(false);
    }

    public void OpenShopPanel()
    {
        ResolveObjects();
        SetShopPanelVisible(true);
    }

    public void CloseShopPanel()
    {
        ResolveObjects();
        SetShopPanelVisible(false);
    }

    public void CloseAllPanels()
    {
        ResolveObjects();

        if (dailyAndTasksPanel != null)
            HidePanel(dailyAndTasksPanel);

        if (shopPanel != null)
            HidePanel(shopPanel);
    }

    private void SetDailyAndTasksPanelVisible(bool isVisible)
    {
        if (dailyAndTasksPanel == null)
        {
            WarnMissingPanel(ref warnedMissingDailyPanel, "Daily And Tasks Panel");
            return;
        }

        if (isVisible && closeOtherPanelsOnOpen && shopPanel != null)
            HidePanel(shopPanel);

        SetPanelVisible(dailyAndTasksPanel, isVisible);
        ResolveCloseButtons();
        WireCloseButtons();
    }

    private void SetShopPanelVisible(bool isVisible)
    {
        if (shopPanel == null)
        {
            WarnMissingPanel(ref warnedMissingShopPanel, "Shop Panel");
            return;
        }

        if (isVisible && closeOtherPanelsOnOpen && dailyAndTasksPanel != null)
            HidePanel(dailyAndTasksPanel);

        SetPanelVisible(shopPanel, isVisible);
        ResolveCloseButtons();
        WireCloseButtons();
    }

    private void SetPanelVisible(GameObject panel, bool isVisible)
    {
        if (isVisible)
            ShowPanel(panel);
        else
            HidePanel(panel);
    }

    private void ShowPanel(GameObject panel)
    {
        CurvedUIPanelAnimator.Show(
            panel,
            animatePanelTransitions ? panelOpenDuration : 0f,
            panelOpenCurve,
            panelFadeCurve);
    }

    private void HidePanel(GameObject panel)
    {
        if (animatePanelTransitions)
        {
            CurvedUIPanelAnimator.Hide(panel, panelCloseDuration, panelCloseCurve, panelFadeCurve);
            return;
        }

        CurvedUIPanelAnimator.HideImmediate(panel);
    }

    private void CloseAllPanelsImmediate()
    {
        ResolveObjects();

        CurvedUIPanelAnimator.HideImmediate(dailyAndTasksPanel);
        CurvedUIPanelAnimator.HideImmediate(shopPanel);
    }

    private void ResolveObjects()
    {
        taskButton = taskButton != null ? taskButton : FindButton(taskButtonNames);
        shopButton = shopButton != null ? shopButton : FindButton(shopButtonNames);
        starsCountButton = starsCountButton != null ? starsCountButton : FindButton(starsCountButtonNames);
        diamondCountButton = diamondCountButton != null ? diamondCountButton : FindButton(diamondCountButtonNames);
        dailyAndTasksPanel = dailyAndTasksPanel != null ? dailyAndTasksPanel : FindSceneObject(dailyAndTasksPanelNames);
        shopPanel = shopPanel != null ? shopPanel : FindSceneObject(shopPanelNames);
        ResolveCloseButtons();
    }

    private void ResolveCloseButtons()
    {
        ResolveCloseButtons(dailyAndTasksPanel, dailyCloseButtons);
        ResolveCloseButtons(shopPanel, shopCloseButtons);
    }

    private void ResolveCloseButtons(GameObject panel, List<Button> closeButtons)
    {
        closeButtons.Clear();

        if (panel == null)
            return;

        Button[] buttons = panel.GetComponentsInChildren<Button>(true);

        for (int i = 0; i < buttons.Length; i++)
        {
            if (IsCloseButtonName(buttons[i].name))
                closeButtons.Add(buttons[i]);
        }
    }

    private void WireButtons()
    {
        WireButton(taskButton, toggleDailyAction);
        WireButton(shopButton, toggleShopAction);
        WireButton(starsCountButton, openShopAction);
        WireButton(diamondCountButton, openShopAction);
        WireCloseButtons();
    }

    private void WireCloseButtons()
    {
        for (int i = 0; i < dailyCloseButtons.Count; i++)
            WireButton(dailyCloseButtons[i], closeDailyAction);

        for (int i = 0; i < shopCloseButtons.Count; i++)
            WireButton(shopCloseButtons[i], closeShopAction);
    }

    private void UnwireButtons()
    {
        UnwireButton(taskButton, toggleDailyAction);
        UnwireButton(shopButton, toggleShopAction);
        UnwireButton(starsCountButton, openShopAction);
        UnwireButton(diamondCountButton, openShopAction);

        for (int i = 0; i < dailyCloseButtons.Count; i++)
            UnwireButton(dailyCloseButtons[i], closeDailyAction);

        for (int i = 0; i < shopCloseButtons.Count; i++)
            UnwireButton(shopCloseButtons[i], closeShopAction);
    }

    private static void WireButton(Button button, UnityAction action)
    {
        if (button == null || action == null)
            return;

        button.onClick.RemoveListener(action);
        button.onClick.AddListener(action);
    }

    private static void UnwireButton(Button button, UnityAction action)
    {
        if (button == null || action == null)
            return;

        button.onClick.RemoveListener(action);
    }

    private Button FindButton(string[] names)
    {
        GameObject buttonObject = FindSceneObject(names);
        return buttonObject != null ? buttonObject.GetComponent<Button>() : null;
    }

    private GameObject FindSceneObject(string[] names)
    {
        if (names == null || names.Length == 0)
            return null;

        GameObject inactiveMatch = null;

        for (int sceneIndex = 0; sceneIndex < SceneManager.sceneCount; sceneIndex++)
        {
            Scene scene = SceneManager.GetSceneAt(sceneIndex);

            if (!scene.isLoaded)
                continue;

            GameObject[] roots = scene.GetRootGameObjects();

            for (int rootIndex = 0; rootIndex < roots.Length; rootIndex++)
            {
                GameObject match = FindInChildren(roots[rootIndex].transform, names, ref inactiveMatch);

                if (match != null)
                    return match;
            }
        }

        if (inactiveMatch != null)
            return inactiveMatch;

        Transform[] transforms = Resources.FindObjectsOfTypeAll<Transform>();

        for (int i = 0; i < transforms.Length; i++)
        {
            GameObject gameObject = transforms[i].gameObject;

            if (!gameObject.scene.IsValid() || !MatchesName(gameObject.name, names))
                continue;

            if (gameObject.activeInHierarchy)
                return gameObject;

            if (inactiveMatch == null)
                inactiveMatch = gameObject;
        }

        return inactiveMatch;
    }

    private GameObject FindInChildren(Transform parent, string[] names, ref GameObject inactiveMatch)
    {
        GameObject current = parent.gameObject;

        if (MatchesName(current.name, names))
        {
            if (current.activeInHierarchy)
                return current;

            if (inactiveMatch == null)
                inactiveMatch = current;
        }

        for (int i = 0; i < parent.childCount; i++)
        {
            GameObject match = FindInChildren(parent.GetChild(i), names, ref inactiveMatch);

            if (match != null)
                return match;
        }

        return null;
    }

    private bool IsCloseButtonName(string buttonName)
    {
        if (MatchesName(buttonName, closeButtonNames))
            return true;

        return buttonName.IndexOf("close", StringComparison.OrdinalIgnoreCase) >= 0;
    }

    private static bool MatchesName(string objectName, string[] names)
    {
        for (int i = 0; i < names.Length; i++)
        {
            if (string.Equals(objectName, names[i], StringComparison.OrdinalIgnoreCase))
                return true;
        }

        return false;
    }

    private void WarnMissingPanel(ref bool alreadyWarned, string panelName)
    {
        if (alreadyWarned)
            return;

        alreadyWarned = true;
        Debug.LogWarning($"{nameof(MainMenuPanelController)} could not find {panelName}.", this);
    }
}

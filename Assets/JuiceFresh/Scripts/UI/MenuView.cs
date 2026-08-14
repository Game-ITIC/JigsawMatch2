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
using UnityEngine.UI;
using VContainer;
using Views;

public class MenuView : MonoBehaviour, IPreload
{
    [SerializeField] private BuildingShopManager buildingShopManager;
    [SerializeField] private Button dailyButton;

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

    public Button DailyButton => dailyButton;
    public Button DailyRewardsButton => dailyButton;

    [Inject]
    private void Inject(StarModel starModel, GemModel gemModel, HealthSystem healthSystem)
    {
        _starModel = starModel;
        _gemModel = gemModel;
        _healthSystem = healthSystem;
    }

    public async UniTask Warmup()
    {
        RefreshCounterTextCaches();
        BindHud();

        await UniTask.Yield();
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

    private void RefreshCounterTextCaches()
    {
        _starsCountTexts.Clear();
        _diamondCountTexts.Clear();
        _lifeCountTexts.Clear();
        _lifeStatusTexts.Clear();

        AddCounterTextsByName(_starsCountTexts, "Stars Count Button", "StarsCountButton");
        AddCounterTextsByName(_diamondCountTexts, "Diamond Count Button Variant", "Dimond Count Button Variant", "Diamond Count Button");
        AddCounterTextsByName(_lifeCountTexts, "CountHelth", "CountHealth", "Life Count", "Lives Count");
        AddLifeStatusTexts();
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

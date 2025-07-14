using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Monobehaviours;

public class EventsListener : MonoBehaviour
{
    [SerializeField] private GameEventDispatcher gameEventDispatcher;

    void OnEnable()
    {
        LevelManager.OnMapState += OnMapState;
        LevelManager.OnEnterGame += OnEnterGame;
        LevelManager.OnLevelLoaded += OnLevelLoaded;
        LevelManager.OnMenuPlay += OnMenuPlay;
        LevelManager.OnMenuComplete += OnMenuComplete;
        LevelManager.OnTouchDetected += TouchDetected;
        LevelManager.OnWin += OnWin;
        LevelManager.OnLose += OnLose;
    }

    void OnDisable()
    {
        LevelManager.OnMapState -= OnMapState;
        LevelManager.OnEnterGame -= OnEnterGame;
        LevelManager.OnLevelLoaded -= OnLevelLoaded;
        LevelManager.OnMenuPlay -= OnMenuPlay;
        LevelManager.OnMenuComplete -= OnMenuComplete;
        LevelManager.OnTouchDetected -= TouchDetected;
        LevelManager.OnWin -= OnWin;
        LevelManager.OnLose -= OnLose;
    }

    #region GAME_EVENTS

    void OnMapState()
    {
    }

    void OnEnterGame()
    {
        //AnalyticsEvent("OnEnterGame", LevelManager.THIS.currentLevel);
        gameEventDispatcher.DispatchEnterGame();
    }

    void OnLevelLoaded()
    {
    }

    void OnMenuPlay()
    {
    }

    void OnMenuComplete()
    {
    }

    void TouchDetected()
    {
    }

    void OnWin()
    {
        //AnalyticsEvent("OnWin", LevelManager.THIS.currentLevel);
        // InitScript.Instance.AddGems(15);
        //InitScript.Instance.AddStars(3);
        // TinySauce.OnGameFinished(true, LevelManager.Score, LevelManager.THIS.currentLevel);
        gameEventDispatcher.DispatchWin();
    }

    void OnLose()
    {
        //AnalyticsEvent("OnLose", LevelManager.THIS.currentLevel);
        // AdmobManager.Instance.ShowInterstital();

        gameEventDispatcher.DispatchLose();
    }

    #endregion
}
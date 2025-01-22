using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DefaultNamespace;

public class EventsListener : MonoBehaviour
{
    void OnEnable()
    {
        LevelManager.OnMapState += OnMapState;
        LevelManager.OnEnterGame += OnEnterGame;
        LevelManager.OnLevelLoaded += OnLevelLoaded;
        LevelManager.OnMenuPlay += OnMenuPlay;
        LevelManager.OnMenuComplete += OnMenuComplete;
        LevelManager.OnStartPlay += OnStartPlay;
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
        LevelManager.OnStartPlay -= OnStartPlay;
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
        IronSourceManager.Instance.LoadInterstitial();
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

    void OnStartPlay()
    {
    }

    void OnWin()
    {
        //AnalyticsEvent("OnWin", LevelManager.THIS.currentLevel);
        InitScript.Instance.AddGems(15);
        //InitScript.Instance.AddStars(3);
        // TinySauce.OnGameFinished(true, LevelManager.Score, LevelManager.THIS.currentLevel);
        IronSourceManager.Instance.ShowInterstitial();
    }

    void OnLose()
    {
        //AnalyticsEvent("OnLose", LevelManager.THIS.currentLevel);
        // AdmobManager.Instance.ShowInterstital();
        IronSourceManager.Instance.ShowInterstitial();
    }

    #endregion

}
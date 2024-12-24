using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DefaultNamespace;
using KWCore;

//#if UNITY_ANALYTICS
//using UnityEngine.Analytics;
//#endif

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
        // TinySauce.OnGameStarted(LevelManager.THIS.currentLevel);
        // Umbrella.GameCore.ProgressManager.StartStage();
        //
        // AdmobManager.Instance.LoadInterstital();
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
        // AdmobManager.Instance.ShowInterstital();
        // TinySauce.OnGameFinished(true, LevelManager.Score, LevelManager.THIS.currentLevel);
        // Umbrella.GameCore.ProgressManager.CompleteStage(score: LevelManager.Score);
        IronSourceManager.Instance.ShowInterstitial();
    }

    void OnLose()
    {
        //AnalyticsEvent("OnLose", LevelManager.THIS.currentLevel);
        // AdmobManager.Instance.ShowInterstital();
        // TinySauce.OnGameFinished(false, LevelManager.Score, LevelManager.THIS.currentLevel);
        // Umbrella.GameCore.ProgressManager.FailStage(score: LevelManager.Score);
        IronSourceManager.Instance.ShowInterstitial();
    }

    #endregion

//    void AnalyticsEvent(string _event, int level)
//    {
//#if UNITY_ANALYTICS
//        Dictionary<string, object> dic = new Dictionary<string, object>();
//        dic.Add(_event, level);
//        Analytics.CustomEvent(_event, dic);

//#endif
//    }
}
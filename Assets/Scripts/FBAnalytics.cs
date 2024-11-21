using System.Collections;
using System.Collections.Generic;
using Facebook.Unity;
using UnityEngine;

public class FBAnalytics : MonoBehaviour
{
    private void Awake()
    {
        if (Application.platform == RuntimePlatform.IPhonePlayer)
        {
            Application.targetFrameRate = 60;
        }

        FB.Init(OnFacebookInitialized);
        
        DontDestroyOnLoad(gameObject);
    }

    private void OnFacebookInitialized()
    {
        if (FB.IsInitialized)
        {
            FB.ActivateApp();
        }
    }

    private void OnApplicationPause(bool paused)
    {
        if (!paused && FB.IsInitialized)
        {
            FB.ActivateApp();
        }
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Facebook;
using Facebook.Unity;
using FacebookGames;
using FacebookPlatformServiceClient;
using Misc;


public class FacebookSDK : MonoBehaviour
{
    void Awake()
    {
        if (!FB.IsInitialized)
        {
            // Initialize the Facebook SDK
            FB.Init(InitCallback, OnHideUnity);
        }
        else
        {
            // Already initialized, signal an app activation App Event
            FB.ActivateApp();
        }
       // Facebook.Unity.AppEventName.AchievedLevel;
    }

    private void InitCallback()
    {
        if (FB.IsInitialized)
        {
            // Signal an app activation App Event
            FB.ActivateApp();
            // Continue with Facebook SDK
            // ...
        }
        else
        {
            Debug.Log("Failed to Initialize the Facebook SDK");
        }
    }

    private void OnHideUnity(bool isGameShown)
    {
        if (!isGameShown)
        {
            // Pause the game - we will need to hide
            Time.timeScale = 0;
        }
        else
        {
            // Resume the game - we're getting focus again
            Time.timeScale = 1;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
      //  FacebookSDK.
      //  AppLinkData.fetchDeferredAppLinkData();

    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnEnable () {
        SaveData.LevelAchievedEvent += LogAchievedLevelEvent;
    }

    private void OnDisable () {
        SaveData.LevelAchievedEvent -= LogAchievedLevelEvent;
    }

    public void LogAchievedLevelEvent(string level)
    {
        var parameters = new Dictionary<string, object>();
        parameters[AppEventParameterName.Level] = level;

//        float currentLevel = Random.Range(1, 50);
  //      var softPurchaseParameters = new Dictionary<string, object>();
    //    softPurchaseParameters["fb_mobile_level_achieved"] = currentLevel;

        FB.LogAppEvent(
            AppEventName.AchievedLevel,
            null,
            parameters
            );

      // var test = Facebook.Unity.AppEventName.AchievedLevel;
      //  print("LEVEL CHECKED: " + test);
    }


}



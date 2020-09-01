using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Debug = UnityEngine.Debug;


namespace Misc {
    public class AdTimer : MonoBehaviour {
        private enum AdType {
            Regular,
            GoldMode
        }

        [SerializeField]
        private GameObject goldModePanel = default(GameObject);

        private static float _adTimerProgress;

        private const float AdInterval = 750f;

        private static readonly Queue <AdType> AdQueue = new Queue <AdType>(
            new[] {
                AdType.GoldMode
                // AdType.Regular,
                // AdType.Regular,
                // AdType.Regular,
                // AdType.Regular,
                // AdType.Regular
            });

        private void Update () {
            _adTimerProgress += Time.deltaTime;
        }

        private void OnEnable () {
            SceneManager.activeSceneChanged += CheckTimer;
        }

        private void OnDisable () {
            SceneManager.activeSceneChanged -= CheckTimer;
        }

        private void CheckTimer (Scene currentScene, Scene nextScene) {
            if (Preferences.GoldMode) return;

            if (!(_adTimerProgress >= AdInterval)) return;

            ShowAd();
            _adTimerProgress = 0;
        }

        private void ShowAd () {
            Debug.Log("Showing Ad!");

            AdType adType = AdQueue.Dequeue();
            AdQueue.Enqueue(adType);

            switch (adType) {
                case AdType.Regular:
                    // AppLovinIntegration apLovIntegr = FindObjectOfType <AppLovinIntegration>();
                    // if (apLovIntegr != null) apLovIntegr.ShowInterstitial();
                    // Debug.Log("Show Regular Ad!");
                    break;
                case AdType.GoldMode:
                    goldModePanel.SetActive(true);
                    Debug.Log("Showing Gold Mode!");
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}

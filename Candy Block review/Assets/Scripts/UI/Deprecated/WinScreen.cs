using System;
using UnityEngine;
// using Gameplay;
// using TMPro;


// using UnityEngine.Playables;


// using Random = UnityEngine.Random;


namespace UI.Deprecated {
    public class WinScreen : MonoBehaviour {
        public static event Action <bool> DimMusicEvent;

        [SerializeField]
        private GameObject winScreenPanel = default(GameObject);

        // [SerializeField]
        // private PlayableDirector playableDirector = default(PlayableDirector);

        // [SerializeField]
        // private LevelMap levelMap = default(LevelMap);

        // [Space]
        // [SerializeField]
        // private TextMeshProUGUI bearBubbleText = default(TextMeshProUGUI);

        // [SerializeField]
        // private string[] threeStarQuotes = default(string[]);

        // [SerializeField]
        // private string[] twoStarQuotes = default(string[]);

        // [SerializeField]
        // private string[] oneStarQuotes = default(string[]);

        // [SerializeField]
        // private string[] noStarQuotes = default(string[]);

        private void Awake () {
            winScreenPanel.SetActive(false);
        }

        // private void OnEnable () {
        // levelMap.MapCompleteEvent += OnMapComplete;
        // }

        // private void OnMapComplete () {
        // SetBearBubbleText();

        // winScreenPanel.SetActive(true);
        // playableDirector.Play();
        // }

        // private void SetBearBubbleText () {
        //     switch (HintManager.HintsUsed) {
        //         case 0:
        //             bearBubbleText.text = threeStarQuotes[Random.Range(0, threeStarQuotes.Length)];
        //             break;
        //         case 1:
        //             bearBubbleText.text = twoStarQuotes[Random.Range(0, twoStarQuotes.Length)];
        //             break;
        //         case 2:
        //             bearBubbleText.text = oneStarQuotes[Random.Range(0, oneStarQuotes.Length)];
        //             break;
        //         default:
        //             bearBubbleText.text = noStarQuotes[Random.Range(0, noStarQuotes.Length)];
        //             break;
        //     }
        // }

        public void MuteMusic () {
            DimMusicEvent?.Invoke(true);
            Debug.Log("Mute");
        }

        public void UnmuteMusic () {
            DimMusicEvent?.Invoke(false);
            Debug.Log("Unmute");
        }
    }
}

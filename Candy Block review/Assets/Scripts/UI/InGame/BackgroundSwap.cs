using Gameplay.Data;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;


namespace UI.InGame {
    /// <summary>
    /// Swaps in-game background image once in a while
    /// </summary>
    [RequireComponent(typeof(Image))]
    public class BackgroundSwap : MonoBehaviour {
        // TODO could use array to minimize repetition
        private static Sprite _currentBackground;
        private static int    _lastLevelIndex;

        [SerializeField]
        private int interval = default(int);

        [SerializeField]
        private Sprite[] Backgrounds = default(Sprite[]);

        private Image backgroundImage;

        private void Awake () {
            backgroundImage = GetComponent <Image>();
        }

        private void Start () {
            if (_currentBackground == null) {
                Swap();
                return;
            }

            backgroundImage.sprite = _currentBackground;

            if (Level.SelectedLevel == null) return;

            int levelIndex = Level.SelectedLevel.Index;

            if (levelIndex % interval == 0 && _lastLevelIndex != levelIndex) {
                Swap();
            }

            _lastLevelIndex = levelIndex;
        }

        private void Swap () {
            for (int i = 0; i < 5; i++) {
                Sprite bkg = Backgrounds[Random.Range(0, Backgrounds.Length)];
                if (bkg == _currentBackground) continue;
                backgroundImage.sprite = bkg;
                _currentBackground     = bkg;
                return;
            }
        }
    }
}

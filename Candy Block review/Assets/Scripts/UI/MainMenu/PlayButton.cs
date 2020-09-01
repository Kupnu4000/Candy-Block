using Gameplay;
using Gameplay.Data;
using Misc;
using TMPro;
using UnityEngine;
using UnityEngine.Events;


namespace UI.MainMenu {
    public class PlayButton : MonoBehaviour {
        [SerializeField]
        private UnityEvent OnStart = default(UnityEvent);

        [SerializeField]
        private TextMeshProUGUI levelText = default(TextMeshProUGUI);

        [SerializeField]
        private LevelMap levelMap = default(LevelMap);

        private void Start () {
            OnStart?.Invoke();
        }

        public void SetLastLevelText () {
            int levelIndex = SaveData.Instance.CurrentLevelIndex + 1;
            levelText.text = $"{(levelIndex + 1).ToString()}";
        }

        public void SetNextLevelText () {
            int nextLevelIndex = levelMap.Level.Index + 1;
            levelText.text = $"{(nextLevelIndex + 1).ToString()}";
        }

        public void SetLastLevelToPlay () {
            int levelIndex = SaveData.Instance.CurrentLevelIndex;
            // TODO check lenght
            Level.SelectedLevel = Level.Levels[levelIndex + 1];
        }

        public void SetNextLevelToPlay () {
            int nextLevelIndex = levelMap.Level.Index;
            // TODO check lenght
            Level.SelectedLevel = Level.Levels[nextLevelIndex + 1];
        }
    }
}

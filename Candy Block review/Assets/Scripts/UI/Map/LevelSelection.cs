using Gameplay.Data;
using UnityEngine;


namespace UI.Map {
    public class LevelSelection : MonoBehaviour {
        public Transform  ButtonGrid;
        public GameObject LevelButtonPrefab;

        private void Start () {
            for (int i = ButtonGrid.childCount; i > 0; --i) {
                DestroyImmediate(ButtonGrid.GetChild(0).gameObject);
            }

            for (int i = 0; i < Level.Levels.Length; i++) {
                GameObject button = Instantiate(LevelButtonPrefab, ButtonGrid);

                LevelButton levelButton = button.GetComponent <LevelButton>();

                levelButton.LevelNumberText.text = $"{(i + 1).ToString()}";

                levelButton.LevelToLoad = Level.Levels[i];

                // FIXME saving
                // if (i > GameData.Instance.CurrentLevelIndex) button.GetComponent <Button>().interactable = false;
            }
        }
    }
}

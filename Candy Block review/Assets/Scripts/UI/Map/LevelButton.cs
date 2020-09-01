using Gameplay.Data;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;


namespace UI.Map {
    public class LevelButton : MonoBehaviour {
        public TextMeshProUGUI LevelNumberText;
        public Level           LevelToLoad;

        public void LoadLevel () {
            Level.SelectedLevel = LevelToLoad;
            SceneManager.LoadScene("Game");
        }
    }
}

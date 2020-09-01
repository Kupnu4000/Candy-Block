using System;
using Gameplay;
using Gameplay.Data;
using TMPro;
using UnityEngine;


namespace UI.InGame {
    /// <summary>
    /// Shows currently played level
    /// </summary>
    public class LevelCounter : MonoBehaviour {
        public TextMeshProUGUI LevelText;

        private void Start () {
            LevelMap levelMap = FindObjectOfType <LevelMap>();

            if (levelMap == null) {
                Debug.LogError("No Level Map found", this);

                LevelText.text = "---";
                return;
            }

            if (levelMap.Level == null) {
                Debug.LogError("Level map has no level", this);

                LevelText.text = "---";
                return;
            }

            int levelIndex = Array.IndexOf(Level.Levels, levelMap.Level);
            LevelText.text = $"{(levelIndex + 1).ToString()}";
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using Gameplay.Data;
using UnityEngine;


namespace Misc {
    [Serializable]
    public class SaveData {
        public static event Action <string> LevelAchievedEvent = delegate {};

        private static readonly string SaveFilePath = Path.Combine(Application.persistentDataPath, "save.json");

        // FIXME DEV TIME ONLY!!
        private const int StartHintAmount = 10;

        #region Singleton
        private static SaveData _instance;
        public static  SaveData Instance => _instance ?? (_instance = new SaveData());

        private SaveData () => LoadState();
        #endregion

        [SerializeField]
        private int hints;

        public int Hints {
            get => hints;
            set {
                hints = value;
                SaveState();
            }
        }

        [SerializeField]
        private int currentLevelIndex;
        public int CurrentLevelIndex => currentLevelIndex;

        [SerializeField]
        private List <StarData> levelStarsData;

        private IDictionary <string, int> levelStars;

        public void SaveLevelProgress (Level level, int hintsUsed) {
            levelStars.TryGetValue(level.name, out int currentStars);

            int stars = Mathf.Clamp(3 - hintsUsed, 0, 3);

            bool dataChanged = false;

            if (stars > currentStars) {
                levelStars[level.name] = stars;
                dataChanged            = true;
            }

            if (level.Index > currentLevelIndex) {
                currentLevelIndex = level.Index;
                dataChanged       = true;
                LevelAchievedEvent.Invoke((currentLevelIndex + 2).ToString());
            }

            if (dataChanged) SaveState();
        }

        public void Reset () {
            hints             = StartHintAmount;
            currentLevelIndex = -1;
            levelStarsData    = new List <StarData>();
            levelStars        = new Dictionary <string, int>();
            SaveState();
        }

        public int? GetLevelStars (string levelName) {
            if (levelStars.ContainsKey(levelName)) {
                return levelStars[levelName];
            }

            return null;
        }

        private void LoadState () {
            if (File.Exists(SaveFilePath)) {
                string data = File.ReadAllText(SaveFilePath);
                JsonUtility.FromJsonOverwrite(data, this);

                levelStars = new Dictionary <string, int>();

                foreach (StarData starData in levelStarsData) {
                    levelStars.Add(starData.GetData());
                }

                // Debug.Log("Game Loaded!");
            } else {
                hints             = StartHintAmount;
                currentLevelIndex = -1;
                levelStars        = new Dictionary <string, int>();
            }
        }

        private void SaveState () {
            // TODO use temp file first, then overwrite currnet save if succeeded

            levelStarsData = new List <StarData>();

            foreach (KeyValuePair <string, int> pair in levelStars) {
                StarData starData = new StarData();
                starData.SetData(pair);
                levelStarsData.Add(starData);
            }

            string data = JsonUtility.ToJson(this, true);
            File.WriteAllText(SaveFilePath, data);

            Debug.Log("Game Saved!");
        }
    }

    [Serializable]
    public struct StarData {
        public string levelName;
        public int    stars;

        public KeyValuePair <string, int> GetData () {
            return new KeyValuePair <string, int>(levelName, stars);
        }

        public void SetData (KeyValuePair <string, int> data) {
            levelName = data.Key;
            stars     = data.Value;
        }
    }
}

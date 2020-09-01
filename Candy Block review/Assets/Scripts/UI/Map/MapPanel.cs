using Gameplay.Data;
using Misc;
using UnityEngine;
using UnityEngine.UI;


namespace UI.Map {
    public class MapPanel : MonoBehaviour {
        [SerializeField]
        private GameObject mapButtonPrefab = default(GameObject);

        private ScrollRect scrollRect;

        private void Start () {
            scrollRect = GetComponent <ScrollRect>();

            for (int i = 0; i < Level.Levels.Length; i++) {
                Level level = Level.Levels[i];
                int?  stars = SaveData.Instance.GetLevelStars(level.name);

                GameObject mapButtonGo = Instantiate(mapButtonPrefab, scrollRect.content);
                mapButtonGo.name = level.name;

                MapButton mapButton = mapButtonGo.GetComponent <MapButton>();
                mapButton.LevelToLoad     = level;
                mapButton.ButtonText.text = (i + 1).ToString();

                int index = SaveData.Instance.CurrentLevelIndex;

                if (i <= index) {
                    mapButton.ButtonType = MapButton.MapButtonType.Unlocked;
                } else {
                    mapButton.ButtonType = MapButton.MapButtonType.Locked;
                }

                mapButton.Stars = stars ?? 0;

                if (i == 0) mapButton.ButtonType = MapButton.MapButtonType.Unlocked;

                if (i == SaveData.Instance.CurrentLevelIndex + 1) {
                    mapButton.ButtonType = MapButton.MapButtonType.Current;
                }

                mapButton.Initialize();
            }

            Destroy(mapButtonPrefab);
        }
    }
}

using UnityEngine;
using UnityEngine.SceneManagement;


namespace Misc {
    [CreateAssetMenu(fileName = "General Methods", menuName = "Game Data/General Methods", order = 0)]
    public class TestMethods : ScriptableObject {
        public void ResetPlayerPrefs () {
            PlayerPrefs.DeleteAll();
        }

        public void ReloadScene () {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        public void ResetSaveData () {
            SaveData.Instance.Reset();
        }
    }
}

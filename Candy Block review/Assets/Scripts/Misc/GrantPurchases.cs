using Gameplay;
using UnityEngine;
using UnityEngine.SceneManagement;


namespace Misc {
    public class GrantPurchases : MonoBehaviour {
        public void GrantGoldMode () {
            print("GoldMode Granted");
            Preferences.GoldMode = true;
            SceneManager.LoadScene("Menu");
        }

        public void Grant20Hints () {
            HintManager.GrantHints(20);
        }
    }
}

using Misc;
using UnityEngine;


namespace UI.GoldMode {
    public class GoldModeButton : MonoBehaviour {
        private void Start () {
            if (Preferences.GoldMode) gameObject.SetActive(false);
        }
    }
}

using Gameplay;
using UnityEngine;


namespace Misc {
    public class RateUsScript : MonoBehaviour {
        public void RateUs () {
            #if UNITY_ANDROID
            Application.OpenURL("market://details?id=com.pliplay.CandyDandy");

            #elif UNITY_IPHONE
        Application.OpenURL("itms-apps://itunes.apple.com/app/idYOUR_ID");

            #endif
        }

        public void GiveHint () {
            HintManager.GrantHints();
        }
    }
}

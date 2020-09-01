using UnityEngine;


namespace Misc {
    public class PurchasesControl : MonoBehaviour {
        public void BuyHints (int credits) {
            //userCredits += credits;
            Debug.Log($"You received {credits.ToString()} Hints!");
        }
    }
}

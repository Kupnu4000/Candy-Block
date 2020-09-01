using System;
using UnityEngine;


namespace UI.InGame {
    /// <summary>
    /// Level restart button
    /// </summary>
    public class ResetButton : MonoBehaviour {
        public static event Action ResetLevelEvent = delegate {};

        public void ResetLevel () => ResetLevelEvent.Invoke();
    }
}

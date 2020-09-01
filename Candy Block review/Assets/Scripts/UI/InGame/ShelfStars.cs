using System.Collections.Generic;
using Gameplay;
using UnityEngine;


namespace UI.InGame {
    /// <summary>
    /// Star counter on top of game screen
    /// </summary>
    public class ShelfStars : MonoBehaviour {
        [SerializeField]
        private List <Animator> starAnimators = default(List <Animator>);
        private static readonly int Hidden = Animator.StringToHash("Hidden");

        private void OnEnable () {
            HintManager.HintShownEvent  += UpdateStars;
            ResetButton.ResetLevelEvent += OnResetLevel;
        }

        private void OnDisable () {
            HintManager.HintShownEvent  -= UpdateStars;
            ResetButton.ResetLevelEvent -= OnResetLevel;
        }

        private void OnResetLevel () {
            foreach (Animator star in starAnimators) {
                star.SetBool(Hidden, false);
            }
        }

        private void UpdateStars () {
            for (int i = 0; i < Mathf.Min(HintManager.HintsUsed, starAnimators.Count); i++) {
                starAnimators[i].SetBool(Hidden, true);
            }
        }
    }
}

using Misc;
using UnityEngine;
using UnityEngine.UI;


namespace UI.InGame {
    /// <summary>
    /// In-game setting menu
    /// </summary>
    public class SettingsMenu : MonoBehaviour {
        [SerializeField]
        private Toggle musicToggle = default(Toggle);

        [SerializeField]
        private Toggle soundToggle = default(Toggle);

        [Space]
        [SerializeField]
        private Animator animator = default(Animator);
        private static readonly int Show = Animator.StringToHash("Show");

        private void Start () {
            musicToggle.SetIsOnWithoutNotify(!Preferences.MuteMusic);
            soundToggle.SetIsOnWithoutNotify(!Preferences.MuteSound);
        }

        public void ShowMenu () => animator.SetBool(Show,  true);
        public void CloseMenu () => animator.SetBool(Show, false);

        public void ToggleMusic () {
            Preferences.MuteMusic = !musicToggle.isOn;
        }

        public void ToggleSound () {
            Preferences.MuteSound = !soundToggle.isOn;
        }
    }
}

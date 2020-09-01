using UnityEngine;


namespace UI.InGame {
    /// <summary>
    /// Shows Hint Purchase Menu
    /// </summary>
    public class HintsMenu : MonoBehaviour {
        [SerializeField]
        private Animator animator = default(Animator);
        private static readonly int Show = Animator.StringToHash("Show");


        public void ShowMenu () => animator.SetBool(Show,  true);
        public void CloseMenu () => animator.SetBool(Show, false);
    }
}

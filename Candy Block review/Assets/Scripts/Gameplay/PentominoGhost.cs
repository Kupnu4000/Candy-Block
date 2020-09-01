using Audio;
using UnityEngine;


namespace Gameplay {
    public class PentominoGhost : MonoBehaviour {
        public bool IsShownAsHint {get; private set;}

        [SerializeField]
        private Animator animator = default(Animator);
        private static readonly int Show = Animator.StringToHash("Show");
        private static readonly int Hide = Animator.StringToHash("Hide");

        [SerializeField]
        private Transform cellGroup = default(Transform);
        public Transform CellGroup => cellGroup;

        [SerializeField]
        public SpriteRenderer graphics;

        [SerializeField]
        private PentominoShape pentominoShape = default(PentominoShape);

        public PentominoShape PentominoShape {
            get => pentominoShape;

            #if UNITY_EDITOR
            set => pentominoShape = value;
            #endif
        }

        [SerializeField]
        private Transform[] cells = default(Transform[]);

        public Transform[] Cells {
            get => cells;

            #if UNITY_EDITOR
            set => cells = value;
            #endif
        }

        [Space, Header("Sounds")]
        [SerializeField]
        private AudioClip hintSound = default(AudioClip);

        private void Start () {
            graphics.enabled = false;
        }

        public void ShowAsHint () {
            graphics.enabled = true;
            animator.SetTrigger(Show);
            IsShownAsHint = true;

            AudioManager.PlayOnce(hintSound);
        }

        public void HideHint () {
            if (IsShownAsHint == false) return;

            animator.SetTrigger(Hide);
            IsShownAsHint = false;
        }
    }
}

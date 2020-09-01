using Gameplay;
using Misc;
using TMPro;
using UnityEngine;


namespace UI.InGame {
    public class HintButton : MonoBehaviour {
        [SerializeField]
        private TextMeshProUGUI hintCountText = default(TextMeshProUGUI);

        [SerializeField]
        private ParticleSystem particles = default(ParticleSystem);

        [SerializeField]
        private Animator highlightAnimator = default(Animator);
        private static readonly int Highlight = Animator.StringToHash("Highlight");

        [SerializeField]
        private float highlightDelay = default(float);
        private float highlightTimer;

        [SerializeField]
        private HintsMenu hintsMenu = default(HintsMenu);

        private void Start () {
            UpdateHintCountText();
        }

        private void OnEnable () {
            HintManager.HintShownEvent     += OnHintShown;
            HintManager.RanOutOfHintsEvent += OnRanOutOfHints;
            HintManager.HintsAdded         += OnHintsAdded;

            PentominoShape.PlacedEvent += ResetTimer;
        }

        private void OnDisable () {
            HintManager.HintShownEvent     -= OnHintShown;
            HintManager.RanOutOfHintsEvent -= OnRanOutOfHints;
            HintManager.HintsAdded         -= OnHintsAdded;

            PentominoShape.PlacedEvent -= ResetTimer;
        }

        private void OnRanOutOfHints () {
            hintsMenu.ShowMenu();
        }

        private void OnHintShown () {
            particles.Play();
            ResetTimer();
            UpdateHintCountText();
        }

        private void OnHintsAdded (int _) => UpdateHintCountText();

        private void UpdateHintCountText () {
            if (Preferences.GoldMode) {
                hintCountText.gameObject.SetActive(false);
                return;
            }

            hintCountText.text = SaveData.Instance.Hints.ToString();
        }

        private void ResetTimer () {
            highlightTimer = 0;
            highlightAnimator.SetBool(Highlight, false);
        }

        private void Update () {
            if (HintManager.HintsToBeUsed <= 0 || highlightAnimator.GetBool(Highlight)) return;

            if (highlightTimer < highlightDelay) {
                highlightTimer += Time.deltaTime;
            } else {
                highlightAnimator.SetBool(Highlight, true);
            }
        }
    }
}

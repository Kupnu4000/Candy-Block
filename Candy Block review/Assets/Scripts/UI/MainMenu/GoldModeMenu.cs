using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


namespace UI.MainMenu {
    public class GoldModeMenu : MonoBehaviour {
        private const float CloseButtonShowDelay = 5f;
        private const float CloseButtonDelay     = 3f;

        private float closeButtonTimer;

        [SerializeField]
        private GameObject timer = default(GameObject);

        [SerializeField]
        private Image timerFill = default(Image);

        [SerializeField]
        private TextMeshProUGUI timerText = default(TextMeshProUGUI);

        [SerializeField]
        private GameObject closeButton = default(GameObject);

        private void OnEnable () {
            Time.timeScale       = 0;
            timerFill.fillAmount = 0;
            timerText.text       = Mathf.CeilToInt(closeButtonTimer).ToString();
            closeButtonTimer     = CloseButtonDelay;

            StartCoroutine(Test());
        }

        private void OnDisable () {
            Time.timeScale = 1;
        }

        // FIXME
        private IEnumerator Test () {
            timer.SetActive(false);
            closeButton.SetActive(false);

            yield return new WaitForSecondsRealtime(CloseButtonShowDelay);
            timer.SetActive(true);

            while (closeButtonTimer > 0) {
                closeButtonTimer -= Time.unscaledDeltaTime;

                timerText.text       = Mathf.CeilToInt(closeButtonTimer).ToString();
                timerFill.fillAmount = closeButtonTimer / CloseButtonDelay;

                yield return null;
            }

            timer.SetActive(false);
            closeButton.SetActive(true);

            yield return null;
        }
    }
}

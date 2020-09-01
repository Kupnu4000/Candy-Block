using System.Collections;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


namespace UI.General {
    /// <summary>
    /// Used to fade screen and transition to another Unity scene
    /// </summary>
    public class SceneTransitionScreen : MonoBehaviour {
        [SerializeField]
        [Range(0.1f, 1f)]
        private float fadeInDuration = default(float);

        [SerializeField]
        private bool fadeIn = default(bool);

        [Space]
        [SerializeField]
        [Range(0.1f, 1f)]
        private float fadeOutDuration = default(float);

        [SerializeField]
        private bool fadeOut = default(bool);

        [Space]
        [SerializeField]
        private Image blind = default(Image);

        private void Start () {
            StartCoroutine(FadeIn());
        }


        /// <summary>
        /// Fade-In after scene load
        /// </summary>
        /// <returns></returns>
        private IEnumerator FadeIn () {
            if (fadeIn == false) {
                blind.enabled = false;
                yield break;
            }

            blind.enabled = true;
            blind.canvasRenderer.SetAlpha(1);
            blind.CrossFadeAlpha(0f, fadeInDuration, false);
            yield return new WaitForSeconds(fadeInDuration);
            blind.enabled = false;
        }

        /// <summary>
        /// Fade-Out before scene load
        /// </summary>
        /// <param name="sceneName"></param>
        /// <returns></returns>
        private IEnumerator FadeOut (string sceneName) {
            if (fadeOut == false) {
                SceneManager.LoadScene(sceneName);
                yield break;
            }

            blind.enabled = true;
            blind.canvasRenderer.SetAlpha(0);
            blind.CrossFadeAlpha(1f, fadeOutDuration, false);
            yield return new WaitForSeconds(fadeOutDuration);
            SceneManager.LoadScene(sceneName);
        }

        [UsedImplicitly]
        public void GoToScene (string sceneName) {
            StopAllCoroutines();
            StartCoroutine(FadeOut(sceneName));
        }
    }
}

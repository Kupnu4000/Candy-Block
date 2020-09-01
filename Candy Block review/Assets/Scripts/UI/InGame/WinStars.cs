using System.Collections;
using Gameplay;
using UnityEngine;


namespace UI.InGame {
    public class WinStars : MonoBehaviour {
        [SerializeField]
        private Star[] stars = default(Star[]);

        private void OnEnable () {
            LevelMap.ShowWinStarsEvent += OnShowWinStars;
        }

        private void OnDisable () {
            LevelMap.ShowWinStarsEvent -= OnShowWinStars;
        }

        private IEnumerator StarsSeq () {
            yield return new WaitForSeconds(0.4f);

            foreach (Star star in stars) {
                star.gameObject.SetActive(true);
                yield return new WaitForSeconds(0.4f);
            }

            yield return new WaitForSeconds(0.4f);

            Transform parent = stars[0].transform.parent;

            while (parent.transform.localScale.x > 0.001f) {
                Vector3 scale = parent.localScale;

                const float scaleSpeed = 15f;

                scale = Vector3.Lerp(
                    scale,
                    Vector3.zero,
                    Time.deltaTime * scaleSpeed);

                parent.localScale = scale;

                yield return null;
            }

            yield return null;
        }

        private void OnShowWinStars () {
            StopAllCoroutines();
            StartCoroutine(StarsSeq());
        }
    }
}

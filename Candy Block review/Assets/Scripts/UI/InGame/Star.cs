using Audio;
using Gameplay;
using JetBrains.Annotations;
using UnityEngine;


namespace UI.InGame {
    public class Star : MonoBehaviour {
        [SerializeField]
        private ParticleSystem particles = default(ParticleSystem);

        [SerializeField]
        private ParticleSystem particles2;

        [SerializeField]
        [Range(0, 2)]
        private int id = default(int);

        [Space, Header("Sounds")]
        [SerializeField]
        private AudioClip AppearanceSound = default(AudioClip);

        private void OnEnable () {
            if (HintManager.HintsUsed > id) {
                gameObject.SetActive(false);
                return;
            }

            ShowStar();
        }

        private void ShowStar () {
            ParticleBurst();
            AudioManager.PlayOnce(AppearanceSound, 0.5f);
        }

        [UsedImplicitly]
        private void ParticleBurst () {
            particles.Play();
            particles2.Play();
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using Misc;
using UnityEditor;
using UnityEngine;
using UnityEngine.Audio;
using Random = UnityEngine.Random;


namespace Audio {
    /// <summary>
    /// Centralized Audio Manager
    /// Responsible for playing All in-game audio, be it sfx, music or ui sounds.
    /// </summary>
    public class AudioManager : MonoBehaviour {
        private static AudioManager _instance;

        private static AudioManager Instance {
            get => _instance;
            set => _instance = value;
        }

        [Header("Pitch Variation")]
        [SerializeField]
        private float lowPitch = 0.975f;

        [SerializeField]
        private float highPitch = 1.025f;

        private AudioMixer  mixer;
        private AudioSource sfxSource;
        private AudioSource uiSource;
        private AudioSource musicSource;
        private AudioSource overrideMusicSource;

        private AudioMixerGroup uiMixerGroup;
        private AudioMixerGroup sfxMixerGroup;
        private AudioMixerGroup musicMixerGroup;

        // private AudioMixerSnapshot mainSnapshot;
        // private AudioMixerSnapshot lowPassSnapshot;

        // public static AudioMixerSnapshot MainSnapshot    => Instance.mainSnapshot;
        // public static AudioMixerSnapshot LowPassSnapshot => Instance.lowPassSnapshot;

        private Dictionary <AudioClip, AudioSource>   loopClipToSourceMap;
        private Dictionary <AudioSource, IEnumerator> sourceToCoroutineMap;

        private void Awake () {
            if (Instance) {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            loopClipToSourceMap  = new Dictionary <AudioClip, AudioSource>();
            sourceToCoroutineMap = new Dictionary <AudioSource, IEnumerator>();

            // TODO check for nulls
            mixer = AssetDatabase.LoadAssetAtPath <AudioMixer>("Assets/Audio/Master.mixer");
            uiMixerGroup    = mixer.FindMatchingGroups("Ui")[0];
            sfxMixerGroup   = mixer.FindMatchingGroups("Sfx")[0];
            musicMixerGroup = mixer.FindMatchingGroups("Music")[0];

            // mainSnapshot    = mixer.FindSnapshot("Default");
            // lowPassSnapshot = mixer.FindSnapshot("Lowpass");

            uiSource                       = gameObject.AddComponent <AudioSource>();
            uiSource.loop                  = false;
            uiSource.playOnAwake           = false;
            uiSource.outputAudioMixerGroup = uiMixerGroup;

            sfxSource                       = gameObject.AddComponent <AudioSource>();
            sfxSource.loop                  = false;
            sfxSource.playOnAwake           = false;
            sfxSource.outputAudioMixerGroup = sfxMixerGroup;

            musicSource                       = gameObject.AddComponent <AudioSource>();
            musicSource.loop                  = true;
            musicSource.playOnAwake           = false;
            musicSource.outputAudioMixerGroup = musicMixerGroup;

            overrideMusicSource                       = gameObject.AddComponent <AudioSource>();
            overrideMusicSource.loop                  = true;
            overrideMusicSource.playOnAwake           = false;
            overrideMusicSource.outputAudioMixerGroup = musicMixerGroup;
        }

        private void Start () {
            SetMuteSound();
            SetMuteMusic();
        }

        private void OnEnable () {
            Preferences.MuteSoundEvent += SetMuteSound;
            Preferences.MuteMusicEvent += SetMuteMusic;
        }

        private void OnDisable () {
            Preferences.MuteSoundEvent -= SetMuteSound;
            Preferences.MuteMusicEvent -= SetMuteMusic;
        }

        // public static void TransitionToSnapshot (AudioMixerSnapshot snapshot, float timeToReach) {
        //     snapshot.TransitionTo(timeToReach);
        // }

        /// <summary>
        /// Sets volume of Sfx Mixer group based on preferences
        /// </summary>
        private void SetMuteSound () {
            mixer.SetFloat("SfxVolume", Preferences.MuteSound ? -80 : 0);
        }

        /// <summary>
        /// Sets volume of Music Mixer group based on preferences
        /// </summary>
        private void SetMuteMusic () {
            mixer.SetFloat("MusicVolume", Preferences.MuteMusic ? -80 : 0);
        }

        /// <summary>
        /// Play sfx clip once
        /// </summary>
        /// <param name="clip">clip to play</param>
        /// <param name="volume">playback volume (optional)</param>
        /// <param name="randomPitch">fluctate picth randomly (optional)</param>
        /// <param name="delay">delay playback by t in seconds (optional)</param>
        public static void PlayOnce (AudioClip clip, float volume = 1f, bool randomPitch = false, float? delay = null) {
            RandomizePitch(randomPitch);

            if (delay == null) {
                Instance.sfxSource.PlayOneShot(clip, volume);
            } else {
                Instance.StartCoroutine(PlayDelayed(clip, volume, (float)delay));
            }
        }

        /// <summary>
        /// Play ui clip once
        /// </summary>
        /// <param name="clip"></param>
        public static void PlayUiSound (AudioClip clip) {
            Instance.uiSource.PlayOneShot(clip);
        }

        private static IEnumerator PlayDelayed (AudioClip clip, float volume, float delay) {
            yield return new WaitForSeconds(delay);
            Instance.sfxSource.PlayOneShot(clip, volume);
        }

        [UsedImplicitly]
        public static void PlayRandom (AudioClip[] clips, float volume = 1f, bool randomPitch = false) {
            int clipIndex = Random.Range(0, clips.Length);
            PlayOnce(clips[clipIndex], volume, randomPitch);
        }

        /// <summary>
        /// Play clip loop
        /// </summary>
        /// <param name="clip">clip to play</param>
        public static void PlayLoop (AudioClip clip) => GetLoopSource(clip).Play();

        /// <summary>
        /// Stop clip loop, that is already playing.
        /// </summary>
        /// <param name="clip">clip to stop</param>
        /// <param name="fadeDuration">fade out duration in seconds (optional)</param>
        public static void StopLoop (AudioClip clip, float? fadeDuration = null) {
            AudioSource source = GetLoopSource(clip);

            if (fadeDuration == null) {
                source.Stop();
            } else {
                if (Instance.sourceToCoroutineMap.ContainsKey(source))
                    Instance.StopCoroutine(Instance.sourceToCoroutineMap[source]);

                IEnumerator fadeCoroutine = Fade(source, (float)fadeDuration);
                Instance.sourceToCoroutineMap[source] = fadeCoroutine;
                Instance.StartCoroutine(fadeCoroutine);
            }
        }

        private static IEnumerator Fade (AudioSource source, float duration) {
            float       initialVolume = source.volume;
            const float targetVolume  = 0f;

            for (float time = 0f; time < duration; time += Time.deltaTime) {
                source.volume = Mathf.Lerp(initialVolume, targetVolume, time / duration);
                yield return null;
            }

            source.volume = targetVolume;
            source.Stop();
            source.volume = 1;
        }

        private static AudioSource GetLoopSource (AudioClip clip) {
            return Instance.loopClipToSourceMap.ContainsKey(clip)
                ? Instance.loopClipToSourceMap[clip]
                : AllocateNewAudioSource(clip);
        }

        private static AudioSource AllocateNewAudioSource (AudioClip clip) {
            AudioSource source = Instance.gameObject.AddComponent <AudioSource>();
            source.clip                        = clip;
            source.loop                        = true;
            source.playOnAwake                 = false;
            source.outputAudioMixerGroup       = Instance.sfxMixerGroup;
            Instance.loopClipToSourceMap[clip] = source;

            return source;
        }

        private static void RandomizePitch (bool randomPitch) {
            if (randomPitch)
                Instance.sfxSource.pitch = Random.Range(Instance.lowPitch, Instance.highPitch);
            else
                Instance.sfxSource.pitch = 1;
        }

        /// <summary>
        /// Play music clip
        /// </summary>
        /// <param name="clip">music clip to play</param>
        /// <param name="restart">to restart currently playing music pass true</param>
        public static void PlayMusic (AudioClip clip, bool restart = false) {
            AudioSource source = Instance.musicSource;

            if (source.clip != clip) {
                source.Stop();
                source.clip = clip;
            }

            if (restart) source.Stop();

            if (source.isPlaying == false) source.Play();
        }

        /// <summary>
        /// Not used
        /// </summary>
        /// <param name="clip"></param>
        public static void OverrideMusic ([CanBeNull] AudioClip clip) {
            AudioSource source = Instance.overrideMusicSource;

            if (clip == null) {
                source.Stop();
                ResumeMusic();
                return;
            }

            PauseMusic();

            source.clip = clip;
            source.Play();
        }

        /// <summary>
        /// Pause currently playing music
        /// </summary>
        public static void PauseMusic () => Instance.musicSource.Pause();

        /// <summary>
        /// Resume currently playing music;
        /// </summary>
        private static void ResumeMusic () => Instance.musicSource.Play();
    }
}

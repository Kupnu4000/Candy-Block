using System;
using UnityEngine;


namespace Misc {
    public static class Preferences {
        public static event Action MuteMusicEvent = delegate {};
        public static event Action MuteSoundEvent = delegate {};

        private const string MuteMusicKey = "Mute Music";
        private const string MuteSoundKey = "Mute Sound";

        private const string GoldModeKey = "GoldMode";

        public static bool MuteMusic {
            get => GetBoolFromPlayerPrefs(MuteMusicKey);
            set {
                StoreBoolToPlayerPrefs(MuteMusicKey, value);
                MuteMusicEvent.Invoke();
            }
        }

        public static bool MuteSound {
            get => GetBoolFromPlayerPrefs(MuteSoundKey);
            set {
                StoreBoolToPlayerPrefs(MuteSoundKey, value);
                MuteSoundEvent.Invoke();
            }
        }

        public static bool GoldMode {
            get => GetBoolFromPlayerPrefs(GoldModeKey);
            set => StoreBoolToPlayerPrefs(GoldModeKey, value);
        }

        private static bool GetBoolFromPlayerPrefs (string key) {
            return PlayerPrefs.GetInt(key, 0) == 1;
        }

        private static void StoreBoolToPlayerPrefs (string key, bool value) {
            PlayerPrefs.SetInt(key, value ? 1 : 0);
        }
    }
}

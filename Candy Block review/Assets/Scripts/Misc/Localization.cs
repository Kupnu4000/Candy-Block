using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;


namespace Misc {
    public static class Localization {
        public static Action <string> LanguageChanged;

        public static Dictionary <string, string> LocalizedText;

        static Localization () {
            LoadLocalizedText(PlayerPrefs.GetString("language", "english"));
        }

        public static void LoadLocalizedText (string fileName) {
            LocalizedText = new Dictionary <string, string>();

            TextAsset localization = Resources.Load <TextAsset>(Path.Combine("Localization Data", fileName));

            if (localization) {
                LocalizationData data = JsonUtility.FromJson <LocalizationData>(localization.text);

                foreach (LocalizationEntry entry in data.Entries) {
                    LocalizedText.Add(entry.Key, entry.Value);
                }

                PlayerPrefs.SetString("language", fileName);

                LanguageChanged?.Invoke(fileName);
            } else {
                Debug.LogError("Localization file \"" + fileName + "\" not found!");
            }
        }
    }


    [Serializable]
    public struct LocalizationData {
        public string              Language;
        public LocalizationEntry[] Entries;
    }


    [Serializable]
    public struct LocalizationEntry {
        public string Key;
        public string Value;
    }
}

using Misc;
using TMPro;
using UnityEngine;


namespace UI.General {
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class LocalizedText : MonoBehaviour {
        public string Key;

        private TextMeshProUGUI textComponent;

        private void Start () {
            textComponent = GetComponent <TextMeshProUGUI>();
            GetLocalizedValue(null);
        }

        private void OnEnable () {
            Localization.LanguageChanged += GetLocalizedValue;
            GetLocalizedValue(null);
        }

        private void OnDisable () {
            Localization.LanguageChanged -= GetLocalizedValue;
        }

        private void GetLocalizedValue (string language) {
            if (!textComponent) return;

            if (Localization.LocalizedText.ContainsKey(Key)) {
                textComponent.text = Localization.LocalizedText[Key];
            } else {
                Debug.LogWarning($"No key \"{Key}\" in localization data");
                Debug.Log(textComponent.text);
            }
        }
    }
}

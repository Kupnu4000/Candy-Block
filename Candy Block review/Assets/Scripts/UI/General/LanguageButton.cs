using Misc;
using UnityEngine;


namespace UI.General {
    public class LanguageButton : MonoBehaviour {
        public void ChangeLanguage (string language) {
            Localization.LoadLocalizedText(language);
        }
    }
}

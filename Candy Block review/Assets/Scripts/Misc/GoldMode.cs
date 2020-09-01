using UnityEngine;
using UnityEngine.UI;


namespace Misc {
    public class GoldMode : MonoBehaviour {
        [SerializeField]
        private Image image = default(Image);

        [SerializeField]
        private Sprite GoldModeSprite = default(Sprite);

        [Space]
        [SerializeField]
        private Button button = default(Button);

        [SerializeField]
        private Sprite buttonPressedSprite = default(Sprite);

        private void Start () {
            bool goldMode = Preferences.GoldMode;

            if (!goldMode) return;

            if (image != null && GoldModeSprite != null) image.sprite = GoldModeSprite;

            if (button == null || buttonPressedSprite == null) return;

            SpriteState spriteState = button.spriteState;
            spriteState.pressedSprite = buttonPressedSprite;
            button.spriteState        = spriteState;
        }
    }
}

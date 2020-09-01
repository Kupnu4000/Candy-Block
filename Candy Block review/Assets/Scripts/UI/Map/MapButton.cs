using System;
using Gameplay.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


namespace UI.Map {
    public class MapButton : MonoBehaviour {
        public enum MapButtonType {
            Unlocked,
            Locked,
            Current
        }

        private Button button;

        private Image buttonImage;

        [SerializeField]
        private Sprite lastLevelSprite = default(Sprite);

        [SerializeField]
        private Sprite lastLevelSpritePressed = default(Sprite);

        [SerializeField]
        private TextMeshProUGUI buttonText = default(TextMeshProUGUI);
        public TextMeshProUGUI ButtonText => buttonText;

        [SerializeField]
        private Image[] starFill = default(Image[]);

        [SerializeField]
        private GameObject starsContainer = default(GameObject);

        public int Stars {private get; set;}

        public Level LevelToLoad {private get; set;}

        public MapButtonType ButtonType {private get; set;}

        private void Awake () {
            buttonImage = GetComponent <Image>();
            button      = GetComponent <Button>();
        }

        public void SetLevelToLoad () {
            Level.SelectedLevel = LevelToLoad;
        }

        public void Initialize () {
            switch (ButtonType) {
                case MapButtonType.Locked:
                    button.interactable = false;
                    starsContainer.SetActive(false);
                    break;
                case MapButtonType.Unlocked:
                    button.interactable = true;
                    for (int i = 0; i < Stars; i++) starFill[i].enabled = true;
                    break;
                case MapButtonType.Current:
                    buttonImage.sprite = lastLevelSprite;
                    var spriteState = button.spriteState;
                    spriteState.pressedSprite = lastLevelSpritePressed;
                    button.spriteState        = spriteState;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}

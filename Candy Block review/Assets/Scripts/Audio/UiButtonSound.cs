using UnityEngine;
using UnityEngine.UI;


namespace Audio {
    /// <summary>
    /// Add any sound to default Unity Ui Button
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class UiButtonSound : MonoBehaviour {

        [SerializeField]
        private AudioClip sound = default(AudioClip);

        private Button button;

        private void Awake () => button = GetComponent <Button>();

        private void Start () {
            if (sound != null) return;

            GameObject go = gameObject;
            Debug.LogWarning($"{go.name} has no sound", go);
        }

        private void OnEnable () => button.onClick.AddListener(OnButtonClick);
        private void OnDisable () => button.onClick.RemoveListener(OnButtonClick);
        private void OnButtonClick () => AudioManager.PlayUiSound(sound);
    }
}

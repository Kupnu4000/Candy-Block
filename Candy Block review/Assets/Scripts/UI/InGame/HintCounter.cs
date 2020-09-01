// using Gameplay;
// using TMPro;
// using UnityEngine;
//
//
// public class HintCounter : MonoBehaviour {
//     [SerializeField]
//     private TextMeshProUGUI hintText = default(TextMeshProUGUI);
//
//     private void Start () => UpdateCounter();
//     private void OnEnable () => HintManager.HintShownEvent += UpdateCounter;
//     private void OnDisable () => HintManager.HintShownEvent -= UpdateCounter;
//     private void UpdateCounter () => hintText.text = SaveData.Instance.Hints.ToString();
// }

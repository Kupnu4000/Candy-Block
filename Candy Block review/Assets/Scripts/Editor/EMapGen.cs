using Misc;
using UnityEditor;
using UnityEngine;


namespace Editor {
    [CustomEditor(typeof(LevelMapGenerator))]
    public class EMapGen : UnityEditor.Editor {
        private LevelMapGenerator levelMapGenerator;

        private void OnEnable () => levelMapGenerator = (LevelMapGenerator)target;

        public override void OnInspectorGUI () {
            base.OnInspectorGUI();

            GUILayout.Label("Partial", EditorStyles.boldLabel);

            if (GUILayout.Button("Random Fill", GUILayout.ExpandWidth(true))) {
                levelMapGenerator.RandomFIll();
                levelMapGenerator.Fill();
                EditorUtility.SetDirty(levelMapGenerator.tilemap);
            }

            if (GUILayout.Button("Smooth", GUILayout.ExpandWidth(true))) {
                levelMapGenerator.Smooth();
                levelMapGenerator.Fill();
                EditorUtility.SetDirty(levelMapGenerator.tilemap);
            }

            GUILayout.Label("Full", EditorStyles.boldLabel);

            if (GUILayout.Button("Full Gen", GUILayout.ExpandWidth(true))) {
                levelMapGenerator.FullGen();
                levelMapGenerator.Fill();
                EditorUtility.SetDirty(levelMapGenerator.tilemap);
            }
        }
    }
}

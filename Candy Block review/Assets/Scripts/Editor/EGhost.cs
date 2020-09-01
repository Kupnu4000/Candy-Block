using System;
using System.Linq;
using Gameplay;
using UnityEditor;
using UnityEngine;


namespace Editor {
    [CustomEditor(typeof(PentominoGhost)), CanEditMultipleObjects]
    public class EGhost : UnityEditor.Editor {
        private PentominoGhost[] ghosts;

        private void OnEnable () {
            ghosts = Array.ConvertAll(targets, g => (PentominoGhost) g);
        }

        public override void OnInspectorGUI () {
            base.OnInspectorGUI();

            if (GUILayout.Button("Assemble", GUILayout.ExpandWidth(true))) {
                SetCells();
                FindShape();
                SetGraphics();
            }

            if (ghosts.Length > 1 || ghosts[0].PentominoShape == null) return;

            Texture2D texture = AssetPreview.GetAssetPreview(ghosts[0].PentominoShape.gameObject);
            GUILayout.Label(texture);
        }

        private void SetCells () {
            foreach (PentominoGhost ghost in ghosts) {
                PentominoCell[] cells = ghost.CellGroup.GetComponentsInChildren <PentominoCell>();

                if (cells.Length == 0)
                    Debug.LogWarning($"<color=blue>{ghost.name}</color> has no cells!", ghost);

                ghost.Cells = cells.Select(c => c.transform).ToArray();

                EditorUtility.SetDirty(ghost);
            }
        }

        private void FindShape () {
            PentominoShape[] shapes = Resources.LoadAll <PentominoShape>("Shapes");

            foreach (PentominoGhost ghost in ghosts) {
                string shapeName = $"Shape_{ghost.name.Split('_')[1]}";

                PentominoShape pentominoShape = shapes.FirstOrDefault(s => s.name == shapeName);

                if (pentominoShape == null)
                    Debug.LogWarning($"Shape <color=blue>{shapeName}</color> not found!", ghost);

                ghost.PentominoShape = pentominoShape;

                EditorUtility.SetDirty(ghost);
            }
        }

        private void SetGraphics () {
            Sprite[] sprites = Resources.LoadAll <Sprite>("Candies");

            foreach (PentominoGhost ghost in ghosts) {
                string spriteName = ghost.name.Split('_')[1];

                Sprite sprite = sprites.FirstOrDefault(s => s.name == spriteName);

                if (sprite == null)
                    Debug.LogWarning($"Sprite <color=blue>{spriteName}</color> not found!", ghost);

                SpriteRenderer sr = ghost.transform.Find("Graphics").GetComponent <SpriteRenderer>();
                sr.sprite = sprite;

                foreach (PentominoCell cell in ghost.GetComponentsInChildren <PentominoCell>())
                    cell.ShowBounds = !sprite;

                EditorUtility.SetDirty(ghost);
            }
        }
    }
}

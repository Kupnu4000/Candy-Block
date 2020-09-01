using System.Linq;
using Gameplay;
using UnityEditor;
using UnityEngine;


namespace Editor {
    [CustomEditor(typeof(PentominoShape))]
    public class EPentominoShape : UnityEditor.Editor {
        private PentominoShape pentominoShape;

        private void OnEnable () {
            pentominoShape = (PentominoShape) target;
        }

        public override void OnInspectorGUI () {
            base.OnInspectorGUI();

            if (GUILayout.Button("Assemble", GUILayout.ExpandWidth(true))) {
                SetCells();
                GenerateColliders();
                SetGraphics();
            }
        }

        private void SetCells () {
            PentominoCell[] cells = pentominoShape.CellGroup.GetComponentsInChildren <PentominoCell>();

            if (pentominoShape.Cells.Length == 0)
                Debug.LogWarning($"<color=blue>{pentominoShape.name}</color> has no cells!", pentominoShape);

            pentominoShape.Cells = cells.Select(c => c.transform).ToArray();

            EditorUtility.SetDirty(pentominoShape);
        }

        private void GenerateColliders () {
            if (pentominoShape.Cells.Length == 0) return;

            GameObject  subColliderGo = pentominoShape.SubCollider.gameObject;

            foreach (BoxCollider2D box in pentominoShape.GetComponents <BoxCollider2D>())
                DestroyImmediate(box);

            foreach (BoxCollider2D box in subColliderGo.GetComponents <BoxCollider2D>())
                DestroyImmediate(box);

            foreach (Transform cell in pentominoShape.Cells) {
                var position = cell.localPosition;

                BoxCollider2D box = pentominoShape.gameObject.AddComponent <BoxCollider2D>();
                box.usedByComposite = true;
                box.offset          = position;
                box.size            = Vector2.one;

                BoxCollider2D subBox = subColliderGo.AddComponent <BoxCollider2D>();
                subBox.usedByComposite = true;
                subBox.offset          = position;
                subBox.size            = new Vector2(
                    PentominoShape.SubColliderSize,
                    PentominoShape.SubColliderSize);
            }

            EditorUtility.SetDirty(pentominoShape);
        }

        private void SetGraphics () {
            string spriteName = pentominoShape.name.Split('_')[1];

            string graphicsPath = $"Assets/Resources/Candies/{spriteName}.png";
            string shadowPath   = $"Assets/Resources/Shadows/{spriteName}.png";
            string outlinePath  = $"Assets/Resources/Glow/{spriteName}.png";

            Sprite graphics = AssetDatabase.LoadAssetAtPath <Sprite>(graphicsPath);
            Sprite shadow   = AssetDatabase.LoadAssetAtPath <Sprite>(shadowPath);
            Sprite outline  = AssetDatabase.LoadAssetAtPath <Sprite>(outlinePath);

            pentominoShape.Graphics.sprite = graphics;
            pentominoShape.Shadow.sprite   = shadow;
            pentominoShape.Outline.sprite  = outline;

            if (graphics == null)
                Debug.LogWarning($"Graphics <color=blue>{pentominoShape.name}</color> not found!", pentominoShape);

            if (shadow == null)
                Debug.LogWarning($"Shadow <color=blue>{pentominoShape.name}</color> not found!", pentominoShape);

            if (outline == null)
                Debug.LogWarning($"Outline <color=blue>{pentominoShape.name}</color> not found!", pentominoShape);

            foreach (Transform cell in pentominoShape.Cells)
                cell.GetComponent <PentominoCell>().ShowBounds = !graphics;

            AlignGraphics();

            EditorUtility.SetDirty(pentominoShape);
        }

        private void AlignGraphics () {
            Vector2 graphicsPos = pentominoShape.SubCollider.bounds.center;
            graphicsPos = (Vector2)Vector2Int.RoundToInt(graphicsPos * 2) / 2;

            pentominoShape.GraphicsGroup.transform.localPosition = graphicsPos;
        }
    }
}

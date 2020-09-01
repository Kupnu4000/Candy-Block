using UnityEngine;


namespace Gameplay {
    public class PentominoCell : MonoBehaviour {
        #if UNITY_EDITOR
        public bool ShowBounds;
        public Color Color;

        private void OnDrawGizmos () {
            if (ShowBounds == false) return;

            Gizmos.color = Color;

            Vector3 position = transform.position;
            Vector2 size = Vector2.one * 0.85f;

            Gizmos.DrawCube(position, size);
            Gizmos.DrawWireCube(position, size);
        }
        #endif
    }
}

using UnityEngine;


namespace Gameplay {
    public class Roster : MonoBehaviour {
        [SerializeField]
        [Range(1, 10)]
        private int width = default(int);
        public int Width {
            get => width;

            #if UNITY_EDITOR
            set => width = value;
            #endif
        }

        [SerializeField]
        [Range(1, 10)]
        private int height = default(int);
        public int Height {
            get {return height;}

            #if UNITY_EDITOR
            set => height = value;
            #endif
        }

        [SerializeField]
        [Range(0, 6)]
        private int scale = default(int);
        public int Scale {
            get {return scale;}

            #if UNITY_EDITOR
            set => scale = value;
            #endif
        }

        public bool ShowBounds;

        private void OnDrawGizmos () {
            if (!ShowBounds) return;

            Vector3 position = transform.position;

            Gizmos.color = Color.green;

            Gizmos.DrawWireCube(
                position + new Vector3(0, -height / 2f, 0),
                new Vector3Int(width, height, 0)
            );
        }
    }
}

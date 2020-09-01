using UnityEngine;
using UnityEngine.Tilemaps;


namespace Gameplay {
    public class MapHightlight : MonoBehaviour {
        public TileBase HighlightTile;

        [SerializeField]
        private LevelMap levelMap = default(LevelMap);

        [SerializeField]
        private Tilemap tilemap = default(Tilemap);

        private void Start () {
            foreach (Vector3Int cellCoords in levelMap.MapCells.Keys) {
                tilemap.SetTile(cellCoords, HighlightTile);
            }
        }
    }
}

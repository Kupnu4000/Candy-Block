using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;
using Random = UnityEngine.Random;


namespace Misc {
    /// <summary>
    /// Generate random levels by randomly filling cells and smoothing
    /// the result with cellular automata
    /// </summary>
    public class LevelMapGenerator : MonoBehaviour {
        public Tilemap  tilemap = default(Tilemap);
        public TileBase tile    = default(TileBase);

        [Header("Map Settings")]
        public Vector2Int size = new Vector2Int(10, 10);

        public bool useRandomSeed;

        public int seed;

        [Range(1, 5)]
        public int smoothIterations = 2;

        [Range(0, 100)]
        public int fillPercent;

        [Header("Target Values")]
        [Range(10, 100)]
        public int targetCellCount = 30;

        [Range(0, 100)]
        public int pokeChance;

        private int cellSize;

        private int[,] map;

        /// <summary>
        /// randomly fill level cell grid
        /// </summary>
        public void RandomFIll () {
            if (useRandomSeed) seed = Random.Range(-10000, 10000);

            System.Random prng = new System.Random(seed);

            map = new int[size.x, size.y];

            for (int x = 0; x < size.x; x++) {
                for (int y = 0; y < size.y; y++) {
                    map[x, y] = prng.Next(0, 100) < fillPercent ? 1 : 0;
                }
            }

            map[size.x / 2, size.y / 2]     = 1;
            map[size.x / 2, size.y / 2 - 1] = 1;
            map[size.x / 2, size.y / 2 + 1] = 1;
            map[size.x / 2 - 1, size.y / 2] = 1;
            map[size.x / 2 + 1, size.y / 2] = 1;
        }

        /// <summary>
        /// smoothen level cell grid
        /// </summary>
        public void Smooth () {
            const int smoothChanse = 97;

            cellSize = 0;

            for (int x = 0; x < size.x; x++) {
                for (int y = 0; y < size.y; y++) {
                    int neighbourCount = GetNeighbourCount(x, y);

                    if (neighbourCount > 3 && Random.Range(0, 100) < smoothChanse) {
                        map[x, y] = 1;
                        cellSize++;
                    } else {
                        map[x, y] = 0;
                    }
                }
            }
        }

        /// <summary>
        /// get number of neighbours of given cell
        /// </summary>
        /// <param name="gridX">cell X</param>
        /// <param name="gridY">cell Y</param>
        /// <returns></returns>
        private int GetNeighbourCount (int gridX, int gridY) {
            int count = 0;

            for (int x = gridX - 1; x <= gridX + 1; x++) {
                for (int y = gridY - 1; y <= gridY + 1; y++) {
                    if (x < 0 || x >= size.x || y < 0 || y >= size.y) continue;

                    if (x != gridX || y != gridY) {
                        count += map[x, y];
                    }
                }
            }

            return count;
        }

        /// <summary>
        /// fill level cell grid tilemanp with tiles
        /// to represent the result of generation
        /// </summary>
        public void Fill () {
            tilemap.ClearAllTiles();
            tilemap.CompressBounds();

            bool direction = Random.Range(0, 100) < 50;

            for (int x = 0; x < size.x; x++) {
                for (int y = 0; y < size.y; y++) {
                    TileBase t;

                    if (direction) {
                        t = map[x, y] == 1 ? tile : null;
                    } else {
                        t = map[y, x] == 1 ? tile : null;
                    }

                    tilemap.SetTile(new Vector3Int(x, y, 0), t);
                }
            }

            CenterMap();
        }

        /// <summary>
        /// flip level cell grid if generator decides to do so
        /// </summary>
        /// <param name="inputArray"></param>
        private static void FlipArray (in int[,] inputArray) {
            int height = inputArray.GetLength(0);
            int width  = inputArray.GetLength(1);
            for (int i = 0; i < width; i++) {
                for (int j = 0; j < height / 2; j++) {
                    int temp = inputArray[j, i];
                    inputArray[j, i]              = inputArray[height - j - 1, i];
                    inputArray[height - j - 1, i] = temp;
                }
            }
        }

        /// <summary>
        /// Center cells on level cell grid.
        /// Required as generated results might be offsetted.
        /// </summary>
        [ContextMenu("Center Map")]
        public void CenterMap () {
            tilemap.CompressBounds();
            BoundsInt b      = tilemap.cellBounds;
            Vector3   offset = b.center;

            tilemap.ClearAllTiles();

            for (int x = 0; x < size.x; x++) {
                for (int y = 0; y < size.y; y++) {
                    TileBase t = map[x, y] == 1 ? tile : null;
                    tilemap.SetTile(new Vector3Int(x - (int)offset.x, y - (int)offset.y, 0), t);
                }
            }
        }

        /// <summary>
        /// Generate full level including all steps
        /// Fill random cells
        /// Smoothen grid
        /// Draw tiles on tilemap
        /// Poke holes
        /// </summary>
        public void FullGen () {
            int attempts    = 100;
            int targetCells = targetCellCount;

            bool poke             = Random.Range(0, 100) < pokeChance;
            int  holes            = Random.Range(1, 4);
            if (poke) targetCells += holes;

            bool success = false;

            while (attempts > 0) {
                attempts--;

                RandomFIll();
                for (int i = 0; i < smoothIterations; i++) Smooth();

                if (Mathf.Abs(cellSize - targetCells) > 2) continue;

                success = true;

                if (Random.Range(0, 100) < 50) FlipArray(map);

                break;
            }

            if (success == false) {
                for (int x = 0; x < size.x; x++) {
                    for (int y = 0; y < size.y; y++) {
                        map[x, y] = 0;
                    }
                }

                return;
            }

            if (!poke) return;

            IEnumerable <Vector2Int> pokeCandidates = GetCellsToPoke();

            System.Random rand = new System.Random();

            IEnumerable <Vector2Int> cellsToPoke = pokeCandidates.OrderBy(x => rand.Next()).Take(holes);

            foreach (Vector2Int holeCoords in cellsToPoke) {
                map[holeCoords.x, holeCoords.y] = 0;
                cellSize--;
            }
        }

        /// <summary>
        /// Get candidates to poke holes on level cell grid
        /// </summary>
        /// <returns></returns>
        private IEnumerable <Vector2Int> GetCellsToPoke () {
            List <Vector2Int> cellsToPoke = new List <Vector2Int>();

            for (int x = 0; x < size.x; x++) {
                for (int y = 0; y < size.y; y++) {
                    int n = GetNeighbourCount(x, y);
                    if (n == 8) cellsToPoke.Add(new Vector2Int(x, y));
                }
            }

            return cellsToPoke;
        }

        #if UNITY_EDITOR
        private void OnDrawGizmos () {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(transform.position, size.ToVector3Int());
            Handles.Label(size.ToVector3Int(), cellSize.ToString());
        }
        #endif
    }
}

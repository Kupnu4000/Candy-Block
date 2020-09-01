using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;


namespace Gameplay.Data {
    /// <summary>
    /// Game levels are stored as instances of this class
    /// </summary>
    [Serializable]
    public class Level : ScriptableObject {
        public static Level SelectedLevel;

        private static Level[] _levels;
        public static  Level[] Levels => _levels ?? (_levels = Resources.LoadAll <Level>("Levels"));

        public int Index => Array.IndexOf(_levels, this);

        public float              ScaleFactor;
        public BoundsInt          MapBounds;
        public List <LevelObject> GhostData   = new List <LevelObject>();
        public List <LevelObject> ShapeData   = new List <LevelObject>();
        public List <TileBase>    TilemapData = new List <TileBase>();

        public int CellCount => TilemapData.Count(tileBase => tileBase);

        /// <summary>
        /// Save level layout and data
        /// </summary>
        /// <param name="scale">scale of pentominoes. Dependant on total amount</param>
        /// <param name="map">tilemap that shows level background grid</param>
        #if UNITY_EDITOR
        public void SaveLevel (float scale, Tilemap map) {
            ScaleFactor = scale;

            TilemapData.Clear();
            map.CompressBounds();
            MapBounds = map.cellBounds;

            foreach (Vector3Int coords in map.cellBounds.allPositionsWithin) {
                TilemapData.Add(map.GetTile(coords));
            }

            GhostData.Clear();
            foreach (PentominoGhost ghost in FindObjectsOfType <PentominoGhost>()) {
                GhostData.Add(new LevelObject {
                    name     = ghost.name,
                    position = ghost.transform.localPosition
                });
            }

            ShapeData.Clear();
            foreach (PentominoShape shape in FindObjectsOfType <PentominoShape>()) {
                ShapeData.Add(new LevelObject {
                    name     = shape.name,
                    position = shape.transform.localPosition
                });
            }

            EditorUtility.SetDirty(this);
        }

        public static void LoadLevels () {
            _levels = Resources.LoadAll <Level>("Levels");
        }
        #endif

    }

    [Serializable]
    public struct LevelObject {
        public string  name;
        public Vector3 position;
    }
}

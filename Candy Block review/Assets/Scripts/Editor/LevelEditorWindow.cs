using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Gameplay;
using Gameplay.Data;
using JetBrains.Annotations;
using Misc;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;
using Debug = UnityEngine.Debug;
using Random = UnityEngine.Random;


namespace Editor {
    public class LevelEditorWindow : EditorWindow {
        private LevelMap levelMap;
        private LevelMapGenerator   levelMapGenerator;
        private Roster   roster;

        private static bool _autoSave = false;

        private int   scaleFactor = 20;
        private float usedScaleFactor;
        private float spread = 0.25f;

        private const int NavButtonHeight = 24;

        [MenuItem("Tools/Level Editor")]
        private static void ShowWindow () {
            EditorWindow window = GetWindow <LevelEditorWindow>("Level Editor");
            window.minSize = new Vector2(260, 210);
        }

        private void OnEnable () {
            Level.LoadLevels();
            levelMap                               =  FindObjectOfType <LevelMap>();
            roster                                 =  FindObjectOfType <Roster>();
            EditorSceneManager.sceneOpened         += OnSceneOpened;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;

            if (levelMap && levelMap.Level) {
                usedScaleFactor = levelMap.Level.ScaleFactor;
            } else {
                usedScaleFactor = 1f;
            }

            if (levelMap) levelMapGenerator = levelMap.GetComponent <LevelMapGenerator>();
        }

        private void OnDisable () {
            EditorSceneManager.sceneOpened         -= OnSceneOpened;
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
        }

        private void OnPlayModeStateChanged (PlayModeStateChange _) {
            if (EditorApplication.isPlaying) return;

            Level.LoadLevels();
            levelMap = FindObjectOfType <LevelMap>();
            roster   = FindObjectOfType <Roster>();
            Repaint();
        }

        private void OnSceneOpened (Scene scene, OpenSceneMode mode) {
            levelMap = FindObjectOfType <LevelMap>();
            roster   = FindObjectOfType <Roster>();
            Repaint();
        }

        private void OnGUI () {
            if (EditorApplication.isPlaying) {
                EditorGUILayout.HelpBox("Editing disabled in Play Mode", MessageType.Warning, true);
                return;
            }

            if (!levelMap) {
                EditorGUILayout.HelpBox("NO LEVEL MAP", MessageType.Warning, true);
                return;
            }

            if (GUILayout.Button("New Level", GUILayout.ExpandWidth(true), GUILayout.Height(NavButtonHeight))) {
                if (_autoSave) SaveLevel();

                CreateNewLevel();
            }

            EditorGUILayout.Space();

            if (levelMap.Level != null) {
                GUILayout.Label($"Current Level: {levelMap.Level.name}", EditorStyles.boldLabel);
            } else {
                levelMap.Level = Level.Levels[0];
                Debug.Log("Test");
            }

            EditorGUILayout.BeginHorizontal();
            NavigationButtons();
            EditorGUILayout.EndHorizontal();


            EditorGUILayout.Space();
            GUILayout.Label("Level Shape Generator", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            CellSizeButtons();
            EditorGUILayout.EndHorizontal();

            // _autoSave = EditorGUILayout.Toggle("Autosave", _autoSave);
            levelMapGenerator.targetCellCount  = EditorGUILayout.IntSlider("Targer Cell Count", levelMapGenerator.targetCellCount,  1, 100);
            levelMapGenerator.smoothIterations = EditorGUILayout.IntSlider("Smooth Iterations", levelMapGenerator.smoothIterations, 1, 5);
            levelMapGenerator.pokeChance       = EditorGUILayout.IntSlider("Poke Chance",       levelMapGenerator.pokeChance,       1, 100);

            if (GUILayout.Button("Shuffle Map", GUILayout.ExpandWidth(true), GUILayout.Height(32))) {
                levelMapGenerator.FullGen();
                levelMapGenerator.Fill();
                AutoGenerateLevel();
                ArrangeShapes();

                if (_autoSave) SaveLevel();
            }


            if (GUILayout.Button("Align Map", GUILayout.ExpandWidth(true), GUILayout.Height(NavButtonHeight))) {
                levelMap.Map.CompressBounds();

                BoundsInt  cellBounds;
                Vector3Int offset = (cellBounds = levelMap.Map.cellBounds).center.ToVector3Int();

                TileBase[] tiles = levelMap.Map.GetTilesBlock(cellBounds);


                cellBounds.SetMinMax(cellBounds.min - offset, cellBounds.max - offset);

                levelMap.Map.ClearAllTiles();

                levelMap.Map.SetTilesBlock(cellBounds, tiles);

                foreach (PentominoGhost ghost in FindObjectsOfType <PentominoGhost>()) {
                    ghost.transform.Translate(offset * -1);
                }

                // SaveLevel();
            }

            if (GUILayout.Button("Shuffle Shapes", GUILayout.ExpandWidth(true), GUILayout.Height(32))) {
                AutoGenerateLevel();

                if (_autoSave) SaveLevel();
            }

            if (!roster) {
                EditorGUILayout.HelpBox("Roster not found!", MessageType.Warning, true);
            } else {
                GUILayout.Label($"Cells: {levelMap.Level.CellCount.ToString()}",     EditorStyles.boldLabel);
                GUILayout.Label($"Shapes: {roster.transform.childCount.ToString()}", EditorStyles.boldLabel);

                EditorGUILayout.Space();

                EditorGUI.BeginChangeCheck();
                roster.Width  = EditorGUILayout.IntSlider(roster.Width,  1, 10);
                roster.Height = EditorGUILayout.IntSlider(roster.Height, 1, 10);

                scaleFactor = EditorGUILayout.IntSlider("Scale", scaleFactor, 10, 20);
                // spread = EditorGUILayout.Slider("Spread", spread, 0f, 0.25f);

                if (EditorGUI.EndChangeCheck()) {
                    EditorUtility.SetDirty(roster);
                }

                if (GUILayout.Button("Arrange Shapes", GUILayout.ExpandWidth(true), GUILayout.Height(32))) {
                    ArrangeShapes();

                    EditorUtility.SetDirty(roster);

                    if (_autoSave) SaveLevel();
                }
            }

            if (GUILayout.Button("Clear (Hold SHIFT)", GUILayout.ExpandWidth(true),
                GUILayout.Height(NavButtonHeight))) {
                if (Event.current.shift) {
                    levelMap.ClearGhosts();
                    EditorUtility.SetDirty(levelMap);

                    levelMap.ClearShapes();
                    EditorUtility.SetDirty(roster);

                    if (_autoSave) SaveLevel();
                }
            }

            if (GUILayout.Button("Save", GUILayout.ExpandWidth(true), GUILayout.Height(48))) {
                SaveLevel();
            }

            // if (GUILayout.Button("Remap Level Names", GUILayout.ExpandWidth(true), GUILayout.Height(48))) {
            //     RemapLevelNames();
            // }
        }

        private void CellSizeButtons () {
            if (GUILayout.Button("36",
                GUILayout.ExpandWidth(true), GUILayout.Height(NavButtonHeight))) {
                levelMapGenerator.targetCellCount = 36;
            }

            if (GUILayout.Button("40",
                GUILayout.ExpandWidth(true), GUILayout.Height(NavButtonHeight))) {
                levelMapGenerator.targetCellCount = 40;
            }

            if (GUILayout.Button("44",
                GUILayout.ExpandWidth(true), GUILayout.Height(NavButtonHeight))) {
                levelMapGenerator.targetCellCount = 44;
            }

            if (GUILayout.Button("48",
                GUILayout.ExpandWidth(true), GUILayout.Height(NavButtonHeight))) {
                levelMapGenerator.targetCellCount = 48;
            }
        }

        [UsedImplicitly]
        private void RemapLevelNames () {
            foreach (Level level in Level.Levels) {
                int oldIndex = Convert.ToInt16(level.name.Split('_')[1]);

                string path     = AssetDatabase.GenerateUniqueAssetPath("Assets/Resources/Levels/Level_0001.asset");
                int    newIndex = Convert.ToInt16(Path.GetFileName(path)?.Split('.')[0].Split('_')[1]);

                if (oldIndex <= newIndex) continue;

                string oldPath = AssetDatabase.GetAssetPath(level);

                AssetDatabase.RenameAsset(oldPath, Path.GetFileName(path));
            }

            AssetDatabase.SaveAssets();
            Level.LoadLevels();
        }

        private void SaveLevel () {
            if (levelMap.Level == null) {
                Debug.LogWarning("No level!");
                return;
            }

            levelMap.Level.SaveLevel(usedScaleFactor, levelMap.Map);
        }

        private void NavigationButtons () {
            void Navigate (int increment) {
                if (levelMap == null || levelMap.Level == null) return;

                int levelIndex;

                if (Event.current.shift) {
                    levelIndex = increment > 0 ? Level.Levels.Length - 1 : 0;
                } else {
                    levelIndex = levelMap.Level.Index;
                    levelIndex = Mathf.Clamp(levelIndex + increment, 0, Level.Levels.Length - 1);
                }

                if (_autoSave) SaveLevel();

                levelMap.Level = Level.Levels[levelIndex];

                spread = 0.25f;

                EditorUtility.SetDirty(levelMap);
            }

            if (GUILayout.Button("<<<",
                GUILayout.ExpandWidth(true), GUILayout.Height(NavButtonHeight)
            )) {
                Navigate(-100);
            }

            if (GUILayout.Button("<<",
                GUILayout.ExpandWidth(true),
                GUILayout.Height(NavButtonHeight)
            )) {
                Navigate(-10);
            }

            if (GUILayout.Button("<",
                GUILayout.ExpandWidth(true),
                GUILayout.Height(NavButtonHeight)
            )) {
                Navigate(-1);
            }

            if (GUILayout.Button(">",
                GUILayout.ExpandWidth(true),
                GUILayout.Height(NavButtonHeight)
            )) {
                Navigate(1);
            }

            if (GUILayout.Button(">>",
                GUILayout.ExpandWidth(true),
                GUILayout.Height(NavButtonHeight)
            )) {
                Navigate(10);
            }

            if (GUILayout.Button(">>>",
                GUILayout.ExpandWidth(true),
                GUILayout.Height(NavButtonHeight)
            )) {
                Navigate(100);
            }
        }

        private void CreateNewLevel () {
            Level asset = CreateInstance <Level>();

            AssetDatabase.CreateAsset(asset,
                AssetDatabase.GenerateUniqueAssetPath("Assets/Resources/Levels/Level_0001.asset"));
            AssetDatabase.SaveAssets();

            levelMap.Level = asset;
            EditorUtility.SetDirty(levelMap);

            Level.LoadLevels();

            Repaint();
        }

        private void AutoGenerateLevel () {
            HashSet <Vector3Int>[] patterns = LevelAutoGen.Generate(levelMap.Map).ToArray();

            if (!patterns.Any()) {
                Debug.LogWarning("No shapes!");
                return;
            }

            levelMap.ClearGhosts();
            levelMap.ClearShapes();

            foreach (HashSet <Vector3Int> pattern in patterns) {
                HashSet <Vector3Int> originatedPattern = MoveToOrigin(pattern);

                List <HashSet <Vector3Int>> variants = new List <HashSet <Vector3Int>>();

                foreach (HashSet <Vector3Int> ghostPattern in LevelAutoGen.PatternToGhostMap.Keys) {
                    if (originatedPattern.SetEquals(ghostPattern)) {
                        variants.Add(ghostPattern);
                    }
                }

                int variantIndex = Random.Range(0, variants.Count);

                GameObject ghostPrefab = LevelAutoGen.PatternToGhostMap[variants[variantIndex]];
                GameObject ghost       = (GameObject)PrefabUtility.InstantiatePrefab(ghostPrefab);

                BoundsInt ghostBounds = new BoundsInt {
                    position = new Vector3Int(
                        int.MaxValue,
                        int.MaxValue,
                        int.MaxValue)
                };

                foreach (Vector3Int coords in originatedPattern)
                    ghostBounds.size = Vector3Int.Max(ghostBounds.size, coords);

                ghostBounds.size += new Vector3Int(1, 1, 0);

                foreach (Vector3Int coord in pattern)
                    ghostBounds.position = Vector3Int.Min(ghostBounds.position, coord);

                ghost.transform.parent        = levelMap.GhostsGroup;
                ghost.transform.localPosition = ghostBounds.center;
            }

            FillRoster();
            ArrangeShapes();
        }

        private void FillRoster () {
            foreach (PentominoGhost ghost in FindObjectsOfType <PentominoGhost>()) {
                if (!ghost.PentominoShape) continue;

                GameObject shape = (GameObject)PrefabUtility.InstantiatePrefab(ghost.PentominoShape.gameObject);
                shape.transform.position = levelMap.RosterGroup.position;
                shape.transform.parent   = levelMap.RosterGroup;
            }
        }

        private static HashSet <Vector3Int> MoveToOrigin (HashSet <Vector3Int> pattern) {
            int minX = pattern.Min(coords => coords.x);
            int minY = pattern.Min(coords => coords.y);
            int minZ = pattern.Min(coords => coords.z);

            Vector3Int offset = new Vector3Int(minX, minY, minZ);

            HashSet <Vector3Int> result = new HashSet <Vector3Int>();

            foreach (Vector3Int coords in pattern)
                result.Add(coords - offset);

            return result;
        }

        private void ArrangeShapes () {
            bool IsSafeToPlace (IEnumerable <Transform> cells) {
                foreach (Transform cell in cells)
                    if (Physics2D.OverlapCircle(cell.position, spread * usedScaleFactor))
                        return false;

                return true;
            }

            usedScaleFactor = scaleFactor / 20f;

            PentominoShape[]      shapes        = FindObjectsOfType <PentominoShape>();
            List <PentominoShape> shapesToCheck = new List <PentominoShape>(shapes);

            foreach (PentominoShape shape in shapes) {
                shape.gameObject.SetActive(false);
                shape.transform.localScale =
                    new Vector3(usedScaleFactor, usedScaleFactor, 1);
            }

            int attempts = 10;

            while (shapesToCheck.Count > 0) {
                if (attempts <= 0) break;

                PentominoShape pentominoShape = shapesToCheck[Random.Range(0, shapesToCheck.Count)];
                pentominoShape.gameObject.SetActive(true);

                Bounds shapeBounds = pentominoShape.ShapeBounds();

                float     xMin = -roster.Width / 2f;
                float     xMax = roster.Width / 2f - shapeBounds.size.x;
                float     yMin = -roster.Height + shapeBounds.size.y - 1f;
                const int yMax = 0;

                List <Vector3> coords = new List <Vector3>();

                for (float y = yMax; y > yMin - usedScaleFactor; y -= usedScaleFactor) {
                    for (float x = xMin; x < xMax - usedScaleFactor; x += usedScaleFactor) {
                        coords.Add(new Vector3(x, y, 0));
                    }
                }

                bool shapePlaced = false;

                foreach (Vector3 coord in coords) {
                    pentominoShape.transform.localPosition = coord;

                    pentominoShape.gameObject.SetActive(false);
                    bool safeToPlace = IsSafeToPlace(pentominoShape.Cells);
                    pentominoShape.gameObject.SetActive(true);

                    if (safeToPlace) {
                        shapesToCheck.Remove(pentominoShape);
                        shapePlaced = true;
                        break;
                    }
                }

                if (!shapePlaced) attempts--;
            }

            foreach (PentominoShape shape in shapes) shape.gameObject.SetActive(true);

            Bounds shapesBounds = new Bounds(
                Vector3.zero,
                Vector3.zero);

            foreach (PentominoShape shape in shapes)
                shapesBounds.Encapsulate(shape.GetComponent <CompositeCollider2D>().bounds);

            Bounds rosterBounds = new Bounds(
                roster.transform.position,
                new Vector3(roster.Width, roster.Height, 0));

            var offset = shapesBounds.center - rosterBounds.center;

            foreach (PentominoShape shape in shapes)
                shape.transform.localPosition += new Vector3(-offset.x, -offset.y, 0);
        }
    }

    public static class LevelAutoGen {
        private static Dictionary <HashSet <Vector3Int>, GameObject> _patternToGhostMap;

        public static Dictionary <HashSet <Vector3Int>, GameObject> PatternToGhostMap {
            get {
                if (_patternToGhostMap == null) LoadGhostPatterns();
                return _patternToGhostMap;
            }
            private set => _patternToGhostMap = value;
        }

        private static void LoadGhostPatterns () {
            PatternToGhostMap = new Dictionary <HashSet <Vector3Int>, GameObject>();

            foreach (PentominoGhost ghost in Resources.LoadAll <PentominoGhost>("Ghosts")) {
                PatternToGhostMap[GetPattern(ghost)] = ghost.gameObject;
            }
        }

        // ReSharper disable once CognitiveComplexity
        private static List <HashSet <Vector3Int>> GetFittingPatterns (IEnumerable <HashSet <Vector3Int>> patterns,
                                                                       Vector3Int currentCoord,
                                                                       ICollection <Vector3Int> validCoords) {
            List <HashSet <Vector3Int>> localShapePatterns = new List <HashSet <Vector3Int>>(patterns);

            if (localShapePatterns.Count == 0) return null;

            List <HashSet <Vector3Int>> alteredPatterns = new List <HashSet <Vector3Int>>();

            HashSet <Vector3Int> pattern = localShapePatterns[Random.Range(0, localShapePatterns.Count)];

            foreach (Vector3Int patternCoord in pattern) {
                HashSet <Vector3Int> altPattern = new HashSet <Vector3Int>();

                bool validPattern = true;

                foreach (Vector3Int x in pattern) {
                    Vector3Int altX = x - patternCoord + currentCoord;

                    if (validCoords.Contains(altX) == false) {
                        validPattern = false;
                        break;
                    }

                    altPattern.Add(altX);
                }

                if (validPattern)
                    alteredPatterns.Add(altPattern);
            }

            // If no suitable pattern, remove this shape from candidates list
            if (alteredPatterns.Count == 0) {
                localShapePatterns.Remove(pattern);

                alteredPatterns = GetFittingPatterns(localShapePatterns, currentCoord, validCoords);
            }

            return alteredPatterns;
        }

        private static IEnumerable <Vector3Int> GetNeighbours (Vector3Int coord, bool diagonal = false) {
            Vector3Int[] neighbours = diagonal ? new Vector3Int[8] : new Vector3Int[4];

            neighbours[0] = coord.Modified(0,  1);
            neighbours[1] = coord.Modified(1,  0);
            neighbours[2] = coord.Modified(0,  -1);
            neighbours[3] = coord.Modified(-1, 0);

            if (!diagonal) return neighbours;

            neighbours[4] = coord.Modified(1,  1);
            neighbours[5] = coord.Modified(1,  -1);
            neighbours[6] = coord.Modified(-1, -1);
            neighbours[7] = coord.Modified(-1, 1);

            return neighbours;
        }

        // ReSharper disable once CognitiveComplexity
        public static IEnumerable <HashSet <Vector3Int>> Generate (Tilemap tilemap) {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            LoadGhostPatterns();

            List <HashSet <Vector3Int>> shapes = new List <HashSet <Vector3Int>>();

            List <Vector3Int> coordsToCheck = new List <Vector3Int>();

            tilemap.CompressBounds();
            BoundsInt bounds = tilemap.cellBounds;

            foreach (Vector3Int coords in bounds.allPositionsWithin) {
                if (tilemap.GetTile(coords) != null) coordsToCheck.Add(coords);
            }

            int totalCells = coordsToCheck.Count;
            int breakLimit = 10000;
            int tries      = 0;

            while (coordsToCheck.Count > 0) {
                breakLimit--;
                if (breakLimit <= 0) {
                    Debug.LogError("Broken!");
                    break;
                }

                Vector3Int currentCoord = coordsToCheck[0];

                List <HashSet <Vector3Int>> fittingShapes =
                    GetFittingPatterns(PatternToGhostMap.Keys, currentCoord, coordsToCheck);

                if (fittingShapes.Count == 0) Debug.LogError("No Shape Fit");

                while (fittingShapes.Count > 0) {
                    HashSet <Vector3Int> shape = fittingShapes[Random.Range(0, fittingShapes.Count)];
                    fittingShapes.Remove(shape);

                    if (shape.Count <= 2 && Random.Range(0, 1f) < 0.95f) continue;

                    // TEMPORARY PLACE SHAPE
                    foreach (Vector3Int coords in shape) coordsToCheck.Remove(coords);

                    // CHECK FOR DOTS TODO extract method
                    bool hasDots = false;

                    HashSet <Vector3Int> shapeOutline = new HashSet <Vector3Int>();

                    foreach (Vector3Int coords in shape) {
                        IEnumerable <Vector3Int> neighbours = GetNeighbours(coords);

                        foreach (Vector3Int neighbour in neighbours) {
                            if (coordsToCheck.Contains(neighbour)) shapeOutline.Add(neighbour);
                        }
                    }

                    foreach (Vector3Int coords in shapeOutline) {
                        if (!GetNeighbours(coords).Any(c => coordsToCheck.Contains(c))) {
                            hasDots = true;
                            break;
                        }
                    }

                    if (hasDots && tries < 100) {
                        foreach (Vector3Int coords in shape) coordsToCheck.Add(coords);
                        tries++;
                    } else {
                        tries = 0;
                        shapes.Add(shape);
                        break;
                    }
                }
            }

            stopwatch.Stop();
            Debug.Log($"Filled {totalCells} cells with {shapes.Count} shapes in {stopwatch.Elapsed.Milliseconds} ms!");

            return shapes;
        }

        private static HashSet <Vector3Int> GetPattern (PentominoGhost pentominoGhost) {
            int minX = pentominoGhost.Cells.Min(cell => Mathf.FloorToInt(cell.position.x));
            int minY = pentominoGhost.Cells.Min(cell => Mathf.FloorToInt(cell.position.y));

            Vector3Int offset = new Vector3Int(minX, minY, 0);

            HashSet <Vector3Int> pattern = new HashSet <Vector3Int>();

            foreach (Transform cell in pentominoGhost.Cells)
                pattern.Add(Vector3Int.FloorToInt(cell.position) - offset);

            return pattern;
        }
    }
}

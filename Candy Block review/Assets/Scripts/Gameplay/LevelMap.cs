using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Audio;
using Gameplay.Data;
using Misc;
using UI.InGame;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;
using Random = UnityEngine.Random;


namespace Gameplay {
    /// <summary>
    /// Level representation on Game screen
    /// </summary>
    public class LevelMap : MonoBehaviour {
        public static event Action ShowWinStarsEvent = delegate {};
        public static event Action MapBlinkEvent     = delegate {};

        public enum CellState {
            Vacant,
            Occupied
        }

        [SerializeField]
        [HideInInspector]
        private Level level;

        public PentominoGhost[] Ghosts {get; private set;}
        public PentominoShape[] Shapes {get; private set;}

        public Tilemap Map;

        [SerializeField]
        private Animator animator = default(Animator);

        [SerializeField]
        private Grid grid = default(Grid);

        [SerializeField]
        private Transform ghostsGroup = default(Transform);
        public Transform GhostsGroup => ghostsGroup;

        [SerializeField]
        private Transform rosterGroup = default(Transform);
        public Transform RosterGroup => rosterGroup;

        [Space, Header("Sounds")]
        [SerializeField]
        private AudioClip mapAppearSound = default(AudioClip);

        public readonly Dictionary <Vector2Int, CellState> MapCells = new Dictionary <Vector2Int, CellState>();

        public Level Level {
            get => level;
            set {
                level = value;
                LoadLevelContents();
            }
        }

        private void Awake () {
            if (Level.SelectedLevel == null) {
                LoadLevelContents();
            } else {
                Level = Level.SelectedLevel;
            }

            AlignMap();

            foreach (PentominoGhost ghost in Ghosts) {
                foreach (Transform cell in ghost.Cells) {
                    Vector2Int cellPos = grid.WorldToCell(cell.position).ToVector2Int();
                    MapCells[cellPos] = CellState.Vacant;
                }
            }
        }

        private void Start () {
            StartCoroutine(ShapesAppearSeq());
        }

        private                 bool canBeResetted;
        private static readonly int  OutHash      = Animator.StringToHash("Out");
        private static readonly int  LoopOutlineHash = Animator.StringToHash("Loop Outline");


        /// <summary>
        /// Shape appearance sequence on level start
        /// </summary>
        /// <returns></returns>
        private IEnumerator ShapesAppearSeq () {
            while (Math.Abs(Time.timeScale) < 0.01f) {
                yield return null;
            }

            canBeResetted = false;

            // AudioManager.PlayOnce(mapAppearSound);


            foreach (PentominoShape shape in Shapes) {
                shape.gameObject.SetActive(false);
                shape.Enabled = false;
            }

            yield return new WaitForSeconds(0.25f);

            List <PentominoShape> shapesToPop = new List <PentominoShape>(Shapes);

            while (shapesToPop.Count > 0) {
                PentominoShape pentominoShape = shapesToPop[Random.Range(0, shapesToPop.Count)];
                shapesToPop.Remove(pentominoShape);

                pentominoShape.Show();

                yield return new WaitForSeconds(Random.Range(0.05f, 0.1f));
            }

            foreach (PentominoShape shape in Shapes) shape.Enabled = true;


            PentominoShape shapeToBlink = Shapes[Random.Range(0, Shapes.Length)];
            shapeToBlink.ShapeAnimator.SetBool(LoopOutlineHash, true);

            canBeResetted = true;
            yield return null;
        }

        private void ResetShapes () {
            if (canBeResetted == false) return;
            StopAllCoroutines();
            StartCoroutine(ResetShapesSeq());
        }

        private IEnumerator ResetShapesSeq () {
            canBeResetted = false;

            foreach (PentominoShape shape in Shapes) shape.IsDraggable = false;

            foreach (PentominoShape shape in Shapes.Where(shape => shape.IsOnBoard)) {
                shape.MoveToBase();
                yield return new WaitForSeconds(Random.Range(0.05f, 0.1f));
            }

            foreach (PentominoShape shape in Shapes) shape.IsDraggable = true;

            canBeResetted = true;
            yield return null;
        }

        private void OnEnable () {
            PentominoShape.PlacedEvent += CheckForWin;

            ResetButton.ResetLevelEvent += ResetShapes;
        }

        private void OnDisable () {
            PentominoShape.PlacedEvent -= CheckForWin;

            ResetButton.ResetLevelEvent -= ResetShapes;
        }

        public void ClearGhosts () {
            for (int i = ghostsGroup.childCount; i > 0; --i)
                DestroyImmediate(ghostsGroup.GetChild(0).gameObject);
        }

        public void ClearShapes () {
            for (int i = rosterGroup.childCount; i > 0; --i)
                DestroyImmediate(rosterGroup.GetChild(0).gameObject);
        }

        private void ClearLevel () {
            ClearGhosts();
            ClearShapes();
            Map.ClearAllTiles();
        }

        private void LoadLevelContents () {
            if (level == null) {
                Debug.LogWarning("No Level to load!", this);
                return;
            }

            ClearLevel();

            List <PentominoGhost> ghosts = new List <PentominoGhost>();
            List <PentominoShape> shapes = new List <PentominoShape>();

            foreach (LevelObject ghost in level.GhostData) {
                GameObject prefab = Resources.Load <GameObject>($"Ghosts/{ghost.name}");

                #if UNITY_EDITOR
                GameObject ghostGo = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                #else
                GameObject ghostGo = Instantiate(prefab);
                #endif

                ghostGo.transform.SetParent(GhostsGroup);
                ghostGo.transform.localPosition = ghost.position;

                ghosts.Add(ghostGo.GetComponent <PentominoGhost>());
            }

            foreach (LevelObject shape in level.ShapeData) {
                GameObject prefab = Resources.Load <GameObject>($"Shapes/{shape.name}");

                #if UNITY_EDITOR
                GameObject shapeGo = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                #else
                GameObject shapeGo = Instantiate(prefab);
                #endif

                shapeGo.transform.SetParent(RosterGroup);
                shapeGo.transform.localPosition = shape.position;
                shapeGo.transform.localScale    = new Vector3(level.ScaleFactor, level.ScaleFactor, 1);

                shapes.Add(shapeGo.GetComponent <PentominoShape>());
            }

            Ghosts = ghosts.ToArray();
            Shapes = shapes.ToArray();

            int counter = 0;
            foreach (Vector3Int coords in level.MapBounds.allPositionsWithin) {
                Map.SetTile(coords, level.TilemapData[counter]);
                counter++;
            }
        }

        private void AlignMap () {
            Map.CompressBounds();

            Vector2 offset = Map.cellBounds.center;

            grid.transform.position -= (Vector3)offset;
        }

        private void CheckForWin () {
            bool isLevelComplete = !MapCells.Values.Contains(CellState.Vacant);

            if (isLevelComplete == false) return;

            SaveData.Instance.SaveLevelProgress(Level, HintManager.HintsUsed);

            StartCoroutine(WinSequence());
        }

        private IEnumerator WinSequence () {
            canBeResetted = false;

            foreach (PentominoShape shape in Shapes) {
                shape.Enabled = false;
                shape.transform.SetParent(Map.transform);
            }

            foreach (PentominoGhost ghost in Ghosts) {
                ghost.transform.SetParent(Map.transform);
            }

            MapBlinkEvent.Invoke();

            float interval = 0.35f / Shapes.Length;

            foreach (PentominoShape shape in Shapes) {
                shape.OutlineBlink();
                yield return new WaitForSeconds(interval);
            }

            ShowWinStarsEvent.Invoke();

            yield return new WaitForSeconds(1.8f);

            animator.SetTrigger(OutHash);
            AudioManager.PlayOnce(mapAppearSound, 0.32f);

            yield return new WaitForSeconds(1f);

            int nextLevelIndex = Level.Index;

            // TODO check lenght
            Level.SelectedLevel = Level.Levels[nextLevelIndex + 1];

            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
}

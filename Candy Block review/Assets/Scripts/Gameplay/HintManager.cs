using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;
using System.Linq;
using JetBrains.Annotations;
using Misc;
using UI.InGame;


namespace Gameplay {
    public class HintManager : MonoBehaviour {
        public static int HintsUsed;
        public static int HintsToBeUsed;

        public static event Action       HintShownEvent     = delegate {};
        public static event Action       RanOutOfHintsEvent = delegate {};
        public static event Action <int> HintsAdded         = delegate {};

        private readonly Dictionary <Vector2Int, PentominoGhost> cellToGhostMap =
            new Dictionary <Vector2Int, PentominoGhost>();

        [SerializeField]
        private Grid grid = default(Grid);

        [SerializeField]
        private LevelMap levelMap = default(LevelMap);

        private PentominoGhost[] ghosts;
        private PentominoShape[] shapes;

        private void Awake () {
            ghosts = levelMap.Ghosts;
            shapes = levelMap.Shapes;

            foreach (PentominoGhost ghost in ghosts) {
                PentominoCell[] ghostCells = ghost.GetComponentsInChildren <PentominoCell>();

                foreach (PentominoCell ghostCell in ghostCells) {
                    Vector2Int cellPos = grid.WorldToCell(ghostCell.transform.position).ToVector2Int();

                    cellToGhostMap[cellPos] = ghost;
                }
            }
        }

        private void Start () {
            HintsUsed     = 0;
            HintsToBeUsed = levelMap.Ghosts.Length;
        }

        private void OnEnable () {
            ResetButton.ResetLevelEvent += OnResetLevel;
        }

        private void OnDisable () {
            ResetButton.ResetLevelEvent -= OnResetLevel;
        }

        private void OnResetLevel () {
            HintsUsed     = 0;
            HintsToBeUsed = levelMap.Ghosts.Length;

            foreach (PentominoGhost ghost in ghosts) ghost.HideHint();
        }

        /// <summary>
        /// Show pentomino ghost on level grid
        /// </summary>
        [UsedImplicitly]
        public void ShowHint () {
            if (SaveData.Instance.Hints <= 0 && Preferences.GoldMode == false) {
                RanOutOfHintsEvent.Invoke();
                return;
            }

            PentominoGhost OccupiedGhost (PentominoShape shape) {
                PentominoGhost pentominoGhost = null;

                foreach (Transform cell in shape.Cells) {
                    Vector2Int cellGridPos = grid.WorldToCell(cell.position).ToVector2Int();

                    if (pentominoGhost == null) {
                        pentominoGhost = cellToGhostMap[cellGridPos];
                        continue;
                    }

                    if (pentominoGhost != cellToGhostMap[cellGridPos]) return null;
                }

                return pentominoGhost;
            }

            List <PentominoGhost> validGhosts = ghosts.Where(g => g.IsShownAsHint == false).ToList();

            IEnumerable <PentominoShape> shapesOnBoard = shapes.Where(s => s.IsOnBoard);

            foreach (PentominoShape shape in shapesOnBoard) {
                PentominoGhost pentominoGhost = OccupiedGhost(shape);
                if (pentominoGhost != null) validGhosts.Remove(pentominoGhost);
            }

            if (validGhosts.Count > 0) {
                PentominoGhost pentominoGhost = validGhosts[Random.Range(0, validGhosts.Count)];
                pentominoGhost.ShowAsHint();

                if (Preferences.GoldMode == false) {
                    SaveData.Instance.Hints--;
                }

                HintsToBeUsed--;
                HintsUsed++;

                HintShownEvent.Invoke();
            }
        }

        /// <summary>
        /// Grant player specified amount of hints and save game data
        /// </summary>
        /// <param name="amount">how mamy hints to grant</param>
        public static void GrantHints (int amount = 1) {
            SaveData.Instance.Hints += amount;
            HintsAdded.Invoke(amount);
        }

        #if UNITY_EDITOR
        private void Update () {
            if (Input.GetKeyDown(KeyCode.H)) GrantHints(10);
        }
        #endif
    }
}

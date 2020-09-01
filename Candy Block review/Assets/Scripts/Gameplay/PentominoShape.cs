using System;
using System.Collections;
using System.Collections.Generic;
using Misc;
using UnityEngine;
using UnityEngine.EventSystems;
using Random = UnityEngine.Random;
using Audio;


namespace Gameplay {
    public class PentominoShape : MonoBehaviour, IMoveHandler, IPointerDownHandler, IDragHandler, IPointerUpHandler {
        public const  float SubColliderSize       = 2;
        private const int   GraphicsSortingOffset = 10;

        public static event Action GrabbedEvent           = delegate {};
        public static event Action PlacedEvent            = delegate {};
        public static event Action PlacementRejectedEvent = delegate {};
        public static event Action OutlineBlinkEvent      = delegate {};

        [SerializeField]
        private Transform cellGroup = default(Transform);
        public Transform CellGroup => cellGroup;

        [SerializeField]
        private CompositeCollider2D subCollider = default(CompositeCollider2D);
        public CompositeCollider2D SubCollider => subCollider;

        [SerializeField]
        private Transform[] cells = default(Transform[]);

        public Transform[] Cells {
            get => cells;

            #if UNITY_EDITOR
            set => cells = value;
            #endif
        }

        [Space]
        [SerializeField]
        private Animator shapeAnimator = default(Animator);

        public Animator ShapeAnimator => shapeAnimator;

        private static readonly int OutlineTriggerHash = Animator.StringToHash("Outline");

        [Space]
        [SerializeField]
        private Transform graphicsGroup = default(Transform);
        public Transform GraphicsGroup => graphicsGroup;

        [SerializeField]
        private SpriteRenderer shadow = default(SpriteRenderer);
        public SpriteRenderer Shadow => shadow;

        [SerializeField]
        private SpriteRenderer outline = default(SpriteRenderer);
        public SpriteRenderer Outline => outline;

        [SerializeField]
        private SpriteRenderer graphics = default(SpriteRenderer);
        public SpriteRenderer Graphics => graphics;

        [SerializeField]
        private ParticleSystem particles = default(ParticleSystem);

        [SerializeField]
        private SpriteRenderer flare = default(SpriteRenderer);

        [Space]
        [SerializeField]
        private Transform pivot = default(Transform);

        [SerializeField]
        public Animator shadowAnimator;

        private static readonly int GrabHash    = Animator.StringToHash("Grab");
        private static readonly int ReleaseHash = Animator.StringToHash("Release");

        [SerializeField]
        private Animation flareAnimation = default(Animation);

        [Space, Header("Sounds")]
        [SerializeField]
        private AudioClip popSound = default(AudioClip);

        [SerializeField]
        private AudioClip grabSound = default(AudioClip);

        [SerializeField]
        private AudioClip placeSound = default(AudioClip);

        [SerializeField]
        private AudioClip rejectSound = default(AudioClip);

        private Camera    mainCamera;
        private Transform shapeTransform;
        private Grid      grid;

        private LevelMap levelMap;

        private const float TransitionSpeed = 20f;
        private const float SwayAmount      = 8f;
        private const float SwayMaxAngle    = 3;

        private const float MinBlinkInterval = 3f;
        private const float MaxBlinkInterval = 10f;

        public bool IsOnBoard {get; private set;}

        public  bool    Enabled {get; set;} = true;
        private bool    isDragged;
        private Vector2 targetPosition;
        private Vector2 offset;

        public bool IsDraggable = true;

        private Vector2 basePosition;
        private Vector2 initialPosition;
        private Vector3 baseScale;

        private readonly Dictionary <SpriteRenderer, int>
            sortingOrders = new Dictionary <SpriteRenderer, int>();

        private                 float blinkTimer;
        private static readonly int   LoopOutlineHash = Animator.StringToHash("Loop Outline");

        private void Awake () {
            mainCamera = Camera.main;
            levelMap   = FindObjectOfType <LevelMap>();
            grid       = levelMap.GetComponentInChildren <Grid>();

            sortingOrders[shadow]   = shadow.sortingOrder;
            sortingOrders[outline]  = outline.sortingOrder;
            sortingOrders[graphics] = graphics.sortingOrder;
            sortingOrders[flare]    = flare.sortingOrder;
        }

        private void Start () {
            shapeTransform = transform;
            basePosition   = shapeTransform.position;
            baseScale      = shapeTransform.localScale;

            particles.Stop();

            blinkTimer = Random.Range(MinBlinkInterval, MaxBlinkInterval);
        }

        private void OnEnable () {
            GrabbedEvent += OnGrabbed;
            SetPivot();
        }

        private void OnDisable () {
            GrabbedEvent -= OnGrabbed;
        }

        private void OnGrabbed () {
            shapeAnimator.SetBool(LoopOutlineHash, false);
        }

        private void Update () {
            Blink();
            Drag();
        }

        private void Drag () {
            if (isDragged == false || Enabled == false) return;

            Vector2 shapePos = shapeTransform.position;
            shapePos = Vector2.Lerp(shapePos, targetPosition, TransitionSpeed * Time.deltaTime);

            shapeTransform.position = shapePos;
            shapeTransform.localScale =
                Vector3.Lerp(shapeTransform.localScale, Vector3.one, TransitionSpeed * Time.deltaTime);

            Vector2 graphicsPos = graphicsGroup.position;
            float   distance    = Vector2.Distance(graphicsPos, targetPosition);
            distance -= Vector2.Distance(graphicsPos, shapePos);

            float angle = distance * SwayAmount;
            angle = Mathf.Clamp(angle, -SwayMaxAngle, SwayMaxAngle);

            if (graphicsGroup.position.x > targetPosition.x) angle = -angle;

            graphicsGroup.Rotate(Vector3.forward, angle);
            graphicsGroup.rotation = Quaternion.Lerp(graphicsGroup.rotation, Quaternion.identity,
                TransitionSpeed * Time.deltaTime);
        }

        private void SetPivot () {
            float   pivotOffset = -0.88f * baseScale.x;
            Bounds  bounds      = subCollider.bounds;
            Vector2 pivotPos    = new Vector2(bounds.center.x, bounds.min.y + pivotOffset);
            pivot.transform.position = pivotPos;
        }

        private void Blink () {
            if (!Enabled) return;

            blinkTimer -= Time.deltaTime;

            if (!(blinkTimer <= 0)) return;

            blinkTimer = Random.Range(MinBlinkInterval, MaxBlinkInterval);
            flareAnimation.Play();
        }

        private IEnumerator TransitionTween (Vector3 destination, Vector3 targetScale) {
            const float closeEnough = 0.05f;

            Enabled = false;

            float distance = Vector3.Distance(destination, shapeTransform.position);

            while (distance > closeEnough) {
                Vector3 position = shapeTransform.position;
                distance = Vector3.Distance(destination, position);

                position = Vector3.Lerp(
                    position,
                    destination,
                    TransitionSpeed * Time.deltaTime);

                shapeTransform.position = position;

                shapeTransform.localScale =
                    Vector3.Lerp(
                        shapeTransform.localScale,
                        targetScale,
                        TransitionSpeed * Time.deltaTime);

                graphicsGroup.rotation =
                    Quaternion.Lerp(
                        shapeTransform.rotation,
                        Quaternion.identity,
                        TransitionSpeed * Time.deltaTime);

                yield return null;
            }

            shapeTransform.position   = destination;
            graphicsGroup.rotation    = Quaternion.identity;
            shapeTransform.localScale = targetScale;

            Enabled = true;

            yield return null;
        }

        private void OccupyCells (IEnumerable <Vector2Int> cellCoords) {
            foreach (Vector2Int cell in cellCoords)
                levelMap.MapCells[cell] = LevelMap.CellState.Occupied;
        }

        // FIXME map should respond on it via event
        private void ReleaseCells () {
            foreach (Transform cell in cells) {
                Vector2Int cellPos = grid.WorldToCell(cell.position).ToVector2Int();

                if (levelMap.MapCells.ContainsKey(cellPos)) {
                    levelMap.MapCells[cellPos] = LevelMap.CellState.Vacant;
                }
            }
        }

        private void BringToFront () {
            shadow.sortingOrder   += GraphicsSortingOffset;
            outline.sortingOrder  += GraphicsSortingOffset;
            graphics.sortingOrder += GraphicsSortingOffset;
            flare.sortingOrder    += GraphicsSortingOffset;
        }

        private void SendToBack () {
            shadow.sortingOrder   = sortingOrders[shadow];
            outline.sortingOrder  = sortingOrders[outline];
            graphics.sortingOrder = sortingOrders[graphics];
            flare.sortingOrder    = sortingOrders[flare];
        }

        public Bounds ShapeBounds () => subCollider.bounds;

        public void OutlineBlink () {
            shapeAnimator.SetTrigger(OutlineTriggerHash);
            flareAnimation.Play();

            OutlineBlinkEvent.Invoke();
        }

        public void OnPointerDown (PointerEventData eventData) {
            if (Enabled == false && IsDraggable) return;

            offset = -pivot.localPosition;

            targetPosition =  mainCamera.ScreenToWorldPoint(eventData.position);
            targetPosition += offset;

            ReleaseCells();
            BringToFront();

            particles.Play();
            shadowAnimator.SetTrigger(GrabHash);

            isDragged = true;

            GrabbedEvent.Invoke();

            AudioManager.PlayOnce(grabSound);
        }

        public void OnDrag (PointerEventData eventData) {
            targetPosition =  mainCamera.ScreenToWorldPoint(eventData.position);
            targetPosition += offset;
        }

        public void OnPointerUp (PointerEventData eventData) {
            PointerUpWrapper();
        }

        private void PointerUpWrapper (bool moveToBase = false) {
            if (Enabled == false && IsDraggable) return;

            bool validPosition = true;

            List <Vector2Int> cellsToOccupy = new List <Vector2Int>();

            foreach (Transform cell in cells) {
                Vector2Int cellPos = grid.WorldToCell(cell.position).ToVector2Int();

                if (levelMap.MapCells.TryGetValue(cellPos, out LevelMap.CellState state) &&
                    state == LevelMap.CellState.Vacant) {
                    cellsToOccupy.Add(cellPos);
                    continue;
                }

                validPosition = false;
                break;
            }

            if (validPosition && moveToBase == false) {
                OccupyCells(cellsToOccupy);

                Vector2 shapePos = grid.transform.InverseTransformPoint(shapeTransform.position);

                shapePos = Vector2Int.RoundToInt(shapePos);

                shapePos = grid.transform.TransformPoint(shapePos);

                StartCoroutine(TransitionTween(shapePos, Vector3.one));

                PlacedEvent.Invoke();

                IsOnBoard = true;

                AudioManager.PlayOnce(placeSound);
            } else {
                StartCoroutine(TransitionTween(basePosition, baseScale));
                PlacementRejectedEvent.Invoke();

                IsOnBoard = false;

                AudioManager.PlayOnce(rejectSound);
            }

            SendToBack();

            particles.Stop();

            shadowAnimator.SetTrigger(ReleaseHash);

            isDragged = false;
        }

        public void MoveToBase () {
            ReleaseCells();
            PointerUpWrapper(true);
        }

        public void OnMove (AxisEventData eventData) {
            // Debug.Log($"Test");
        }

        private void OnDrawGizmos () {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(SubCollider.bounds.center, pivot.position);
        }

        public void Show () {
            gameObject.SetActive(true);

            AudioManager.PlayOnce(popSound);
        }
    }
}

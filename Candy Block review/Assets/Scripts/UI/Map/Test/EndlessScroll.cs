using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


namespace UI.Map {
    /// <summary>
    /// NOT USED
    /// </summary>
    [RequireComponent(typeof(ScrollRect))]
    public class EndlessScroll : UIBehaviour, IDragHandler, IEndDragHandler, IBeginDragHandler, IPointerClickHandler {
        private const int TotalItemCount = 235;

        [SerializeField]
        private RectTransform itemPrototypeTransform = default(RectTransform);

        private ScrollRect scrollRect;

        [SerializeField]
        private RectTransform viewportRect = default(RectTransform);
        private RectTransform contentRect;

        public GridLayoutGroup grid;

        protected override void Awake () {
            if (itemPrototypeTransform == null) {
                // FIXME bad description
                Debug.LogError("No item prototype!!!");
                return;
            }

            scrollRect = GetComponent <ScrollRect>();
            // viewportRect = scrollRect.viewport;
            contentRect = scrollRect.content;

            scrollRect.onValueChanged.AddListener(OnScroll);
        }

        protected override void Start () {
            StartCoroutine(OnSeedData());
        }

        private int itemCount;

        private IEnumerator OnSeedData () {
            yield return null;
            itemPrototypeTransform.gameObject.SetActive(false);

            int columns = grid.constraintCount;
            int rows    = Mathf.CeilToInt(viewportRect.rect.height / (grid.cellSize.y + grid.spacing.y));
            rows += 3;

            for (var i = 0; i < rows * columns; ++i) {
                var itemRect = Instantiate(itemPrototypeTransform, contentRect, false);
                itemRect.name                                            = i.ToString();
                itemRect.gameObject.GetComponentInChildren <Text>().text = (i + 1).ToString();
                itemRect.gameObject.SetActive(true);

                lastAdded++;

                itemCount++;
            }

            ResizeContent();

            Destroy(itemPrototypeTransform.gameObject);
        }

        private void ResizeContent () {
            contentRect.sizeDelta = new Vector2(contentRect.sizeDelta.x, RowHeight * 8);
        }

        private int currentRow;

        private int RowHeight => Mathf.RoundToInt(grid.cellSize.y + grid.spacing.y);

        private int lastAdded;

        private void OnScroll (Vector2 pos) {
            if (lastAdded >= TotalItemCount || lastAdded <= 0) {
                return;
            }

            var an  = contentRect.anchoredPosition.y;
            int row = Mathf.CeilToInt(an / RowHeight);

            if (row == currentRow) return;
            // if (row != currentRow && row <= 0) {
            int delta = currentRow - row;


            Permutate(delta);

            print(delta.ToString());


            UpdateCurrentRow();
        }

        private void Permutate (int delta) {
            int sign = Math.Sign(delta);

            Debug.Log($"{sign.ToString()}");
            Vector2 velocity = scrollRect.velocity;
            scrollRect.StopMovement();

            switch (sign) {
                case 1:
                    for (int i = 0; i < grid.constraintCount * delta; i++) {
                        lastAdded++;

                        Transform first = grid.transform.GetChild(0);
                        first.gameObject.GetComponentInChildren <Text>().text = lastAdded.ToString();
                        first.SetAsLastSibling();

                        if (lastAdded > TotalItemCount) {
                            first.gameObject.SetActive(false);
                        }

                    }

                    grid.padding.bottom   += RowHeight * delta;
                    contentRect.sizeDelta += new Vector2(0, RowHeight * delta);

                    break;
                case -1:
                    for (int i = 0; i > grid.constraintCount * delta; i--) {
                        lastAdded--;

                        Transform last = grid.transform.GetChild(grid.transform.childCount - 1);
                        last.gameObject.GetComponentInChildren <Text>().text = (lastAdded - itemCount + 1).ToString();
                        last.SetAsFirstSibling();

                        last.gameObject.SetActive(true);

                    }

                    grid.padding.top      += RowHeight * delta;
                    contentRect.sizeDelta += new Vector2(0, RowHeight * delta);

                    break;
            }

            scrollRect.velocity = velocity;
        }

        public void OnEndDrag (PointerEventData eventData) {
            RectOffset padding = grid.padding;
            int        p       = padding.bottom;
            padding.bottom               =  0;
            contentRect.anchoredPosition += new Vector2(0, p);
            contentRect.sizeDelta        -= new Vector2(0, p);
            padding.bottom               =  0;

            UpdateCurrentRow();

        }

        public void OnDrag (PointerEventData eventData) {}

        public void OnBeginDrag (PointerEventData eventData) {
            UpdateCurrentRow();
        }

        private void UpdateCurrentRow () {
            var an = contentRect.anchoredPosition.y;
            currentRow = Mathf.CeilToInt(an / RowHeight);
        }

        public void OnPointerClick (PointerEventData eventData) {}
    }
}

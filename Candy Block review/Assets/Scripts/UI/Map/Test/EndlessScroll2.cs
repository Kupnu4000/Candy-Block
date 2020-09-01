using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


namespace UI.Map {
    /// <summary>
    /// NOT USED
    /// </summary>
    public class EndlessScroll2 : UIBehaviour, IEndDragHandler {
        private const int TotalItemCount = 64;

        public GridLayoutGroup grid;
        public GameObject      itemPrototype;

        private ScrollRect    scrollRect;
        private RectTransform contentRect;
        private RectTransform viewportRect;

        private float rowHeight;
        private int   columns;

        protected override void Awake () {
            if (itemPrototype == null) {
                Debug.LogError("No item prototype!");
                return;
            }

            scrollRect   = GetComponent <ScrollRect>();
            contentRect  = scrollRect.content;
            viewportRect = scrollRect.viewport;

            columns = grid.constraintCount;

            rowHeight = grid.cellSize.y + grid.spacing.y;

            scrollRect.onValueChanged.AddListener(OnScroll);
        }

        protected override void Start () {
            itemPrototype.SetActive(false);

            int rows = Mathf.CeilToInt(viewportRect.rect.height / (grid.cellSize.y + grid.spacing.y));
            rows += 3;

            for (var i = 0; i < rows * columns; ++i) {
                var itemRect = Instantiate(itemPrototype, contentRect);
                itemRect.name                                            = i.ToString();
                itemRect.gameObject.GetComponentInChildren <Text>().text = (i + 1).ToString();
                itemRect.gameObject.SetActive(true);

                maxItemIndex++;
            }

            contentRect.sizeDelta = new Vector2(contentRect.sizeDelta.x, rowHeight * 8 - grid.spacing.y);


            Destroy(itemPrototype.gameObject);
        }


        private int deltaRow;
        private int currentRow;

        private void OnScroll (Vector2 pos) {
            CheckCurrentRow();
        }

        private int GetRowDelta () {
            UpdateCurrentRow();

            int delta = deltaRow - currentRow;
            deltaRow -= delta;
            return delta;
        }

        private void CheckCurrentRow () {
            int delta = GetRowDelta();

            if (delta == 0) return;

            switch (Mathf.Sign(delta)) {
                case 1:
                    ShiftUp(delta);
                    break;
                case -1:
                    ShiftDown(delta);
                    break;
            }
        }

        private int maxItemIndex;
        private int minItemIndex;

        private void ShiftUp (int delta) {
            if (currentRow >= -1) return;
            if (maxItemIndex >= TotalItemCount) return;

            for (int i = 0; i < columns * delta; i++) {
                maxItemIndex++;
                minItemIndex++;

                Transform first = grid.transform.GetChild(0);
                first.gameObject.GetComponentInChildren <Text>().text = maxItemIndex.ToString();
                first.SetAsLastSibling();

                if (maxItemIndex > TotalItemCount) first.gameObject.SetActive(false);
            }

            int padding = Mathf.RoundToInt(rowHeight * Mathf.Abs(delta));
            grid.padding.top      -= padding;
            grid.padding.bottom   += padding;
            contentRect.sizeDelta += new Vector2(0, padding);

            UpdateCurrentRow();
        }

        private void ShiftDown (int delta) {
            Debug.Log("Test");
            if (minItemIndex <= 0) return;

            for (int i = 0; i > columns * delta; i--) {
                maxItemIndex--;
                minItemIndex--;

                Transform last = grid.transform.GetChild(grid.transform.childCount - 1);
                last.gameObject.GetComponentInChildren <Text>().text = (minItemIndex + 1).ToString();
                last.SetAsFirstSibling();

                last.gameObject.SetActive(true);
            }

            int padding = Mathf.RoundToInt(rowHeight * Mathf.Abs(delta));
            grid.padding.top      += padding;
            grid.padding.bottom   -= padding;
            contentRect.sizeDelta -= new Vector2(0, padding);

            UpdateCurrentRow();
        }

        private void UpdateCurrentRow () {
            float pos = contentRect.anchoredPosition.y;
            currentRow = Mathf.CeilToInt(pos / rowHeight);
        }

        public void OnEndDrag (PointerEventData eventData) {
            // return;
            Vector2 velocity = scrollRect.velocity;
            scrollRect.StopMovement();

            RectOffset padding = grid.padding;
            int        p       = padding.bottom;
            padding.bottom               =  0;
            contentRect.anchoredPosition += new Vector2(0, p);
            contentRect.sizeDelta        -= new Vector2(0, p);
            padding.bottom               =  0;

            UpdateCurrentRow();

            scrollRect.velocity = velocity;

        }
    }
}

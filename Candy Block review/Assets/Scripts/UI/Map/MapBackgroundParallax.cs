using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;


namespace UI.Map {
    public class MapBackgroundParallax : MonoBehaviour {
        [SerializeField]
        private RectTransform[] shapes = default(RectTransform[]);

        [SerializeField]
        [Range(0.1f, 10)]
        private float speed = default(float);

        [SerializeField]
        private RectTransform content = default(RectTransform);
        private float ContentY => content.anchoredPosition.y;

        [SerializeField]
        private CanvasScaler canvasScaler = default(CanvasScaler);
        private Vector2 refRes;

        private float[] speeds;

        private Vector3[] startPositions;

        private void Start () {
            refRes = canvasScaler.referenceResolution;

            startPositions = new Vector3[shapes.Length];
            speeds         = new float[shapes.Length];

            for (int i = 0; i < shapes.Length; i++) {
                shapes[i].anchoredPosition3D =
                    new Vector3(
                        Random.Range(-refRes.x / 2, refRes.x / 2),
                        Random.Range(-refRes.y,     refRes.y),
                        Random.Range(-0.25f,        -0.1f)
                    );

                float scale = Random.Range(1f, 1.75f);
                shapes[i].localScale = new Vector2(scale, scale);

                startPositions[i] = shapes[i].anchoredPosition3D;

                speeds[i] = Random.Range(0.1f, speed);

                float r = Random.Range(0, 1f);
                if (r > 0.85) shapes[i].gameObject.SetActive(false);
                if (r > 0.5) speeds[i] *= -1;
            }
        }

        private void Update () {
            // TODO move shapes up when they are too low

            for (int i = 0; i < shapes.Length; i++) {
                RectTransform shape = shapes[i];

                float timeSin = Mathf.Sin(Time.time / 10 * speeds[i]) * 300;
                float timeCos = Mathf.Cos(Time.time / 10 * speeds[i]) * 300;

                Vector3 pos = new Vector3(
                    timeSin,
                    timeCos + -ContentY * shape.anchoredPosition3D.z,
                    0.0f);

                shape.anchoredPosition3D = startPositions[i] + pos;

                shape.Rotate(Vector3.forward, Time.deltaTime * speeds[i] * 10);

                if (shape.anchoredPosition.y < -refRes.y) {
                    startPositions[i].Set(
                        startPositions[i].x,
                        startPositions[i].y + refRes.y * 2,
                        startPositions[i].z
                    );
                }

                if (shape.anchoredPosition.y > refRes.y) {
                    startPositions[i].Set(
                        startPositions[i].x,
                        startPositions[i].y - refRes.y * 2,
                        startPositions[i].z
                    );
                }
            }
        }
    }
}

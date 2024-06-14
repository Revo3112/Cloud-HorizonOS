using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class EventNetflix : MonoBehaviour
{
    public RectTransform pointer; // Pointer
    public RectTransform[] rawImageObjects; // Array objek Raw Image
    public GameObject gyroObj;
    public RawImage Window; // Texture baru untuk diubah
    public Texture2D newWindowTexture; // Texture baru untuk diubah
    private Quaternion currentGyroRotation;
    private Quaternion targetGyroRotation;
    private Vector2[] originalSizes; // Ukuran asli objek Raw Image
    private Vector2[] targetSizes; // Ukuran target yang dituju
    private float smoothSpeed = 5f; // Kecepatan perubahan ukuran
    public CanvasGroup netflixCanvas; // PaneCanvas untuk di-fade out

    private Vector2 swipeStartPos; // Posisi awal swipe
    private bool isSwiping = false;

    // Start is called before the first frame update
    void Start()
    {
        // Menyimpan ukuran asli untuk setiap objek Raw Image
        originalSizes = new Vector2[rawImageObjects.Length];
        targetSizes = new Vector2[rawImageObjects.Length];

        for (int i = 0; i < rawImageObjects.Length; i++)
        {
            originalSizes[i] = rawImageObjects[i].sizeDelta;
            targetSizes[i] = originalSizes[i];
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Update rotasi gyro
        currentGyroRotation = gyroObj.transform.rotation;

        // Menentukan posisi layar dari pointer
        Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(null, pointer.position);

        // Loop melalui setiap objek Raw Image
        for (int i = 0; i < rawImageObjects.Length; i++)
        {
            // Memeriksa apakah pointer berada di atas objek Raw Image
            if (RectTransformUtility.RectangleContainsScreenPoint(rawImageObjects[i], screenPoint))
            {
                // Menentukan ukuran target yang baru
                targetSizes[i] = originalSizes[i] + new Vector2(2, 2);

                // Deteksi awal swipe
                if (Input.GetMouseButtonDown(0))
                {
                    swipeStartPos = Input.mousePosition;
                    isSwiping = true;
                }
                else if (Input.GetMouseButtonUp(0) && isSwiping)
                {
                    Vector2 swipeEndPos = Input.mousePosition;
                    Vector2 swipeDelta = swipeEndPos - swipeStartPos;

                    // Memeriksa apakah swipe ke kanan
                    if (swipeDelta.x > 400 && Mathf.Abs(swipeDelta.y) < 50) // Threshold bisa disesuaikan
                    {
                        // Ganti texture
                        Window.texture = newWindowTexture;
                    }

                    isSwiping = false;
                }

                // // Deteksi tap/click di layar
                // if (Input.GetMouseButtonDown(0))
                // {
                //     // Memeriksa apakah pointer berada di atas PaneCanvas
                //     if (RectTransformUtility.RectangleContainsScreenPoint(netflixCanvas.GetComponent<RectTransform>(), screenPoint))
                //     {
                //         // Fade out PaneCanvas
                //         StartCoroutine(FadeCanvasGroup(netflixCanvas, 1f, 0f, 0.5f));
                //     }
                // }
            }
            else
            {
                // Mengembalikan ukuran target ke ukuran asli
                targetSizes[i] = originalSizes[i];
            }

            // Mengubah ukuran objek secara smooth menggunakan Lerp
            rawImageObjects[i].sizeDelta = Vector2.Lerp(rawImageObjects[i].sizeDelta, targetSizes[i], smoothSpeed * Time.deltaTime);
        }
    }

    // Coroutine untuk fade CanvasGroup
    IEnumerator FadeCanvasGroup(CanvasGroup canvasGroup, float startAlpha, float endAlpha, float duration)
    {
        float startTime = Time.time;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime = Time.time - startTime;
            float alpha = Mathf.Lerp(startAlpha, endAlpha, elapsedTime / duration);
            canvasGroup.alpha = alpha;
            yield return null;
        }

        canvasGroup.alpha = endAlpha; // Pastikan alpha akhir yang benar
    }
}

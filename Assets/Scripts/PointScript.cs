using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PointScript : MonoBehaviour
{
    public RectTransform pointer; // Pointer
    public RectTransform[] rawImageObjects; // Array objek Raw Image
    public GameObject gyroObj;
    private Quaternion currentGyroRotation;
    private Quaternion targetGyroRotation;
    private Vector2[] originalSizes; // Ukuran asli objek Raw Image
    private Vector2[] targetSizes; // Ukuran target yang dituju
    private float smoothSpeed = 5f; // Kecepatan perubahan ukuran
    private bool isPaneCanvasActive = false;
    private bool isOtherCanvasActive = false;

    public CanvasGroup paneCanvas; // PaneCanvas untuk di-fade out
    public CanvasGroup otherCanvas; // Canvas lain untuk di-fade in
    public RawImage otherCanvasRawImage; // RawImage di otherCanvas
    public Texture[] newTextures; // Array Texture baru

    private int currentTextureIndex = 0; // Indeks Texture saat ini

    // Start is called before the first frame update
    void Start()
    {
        // Menyimpan ukuran asli untuk setiap objek Raw Image
        originalSizes = new Vector2[rawImageObjects.Length];
        targetSizes = new Vector2[rawImageObjects.Length];
        otherCanvas.alpha = 0f;

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
        isPaneCanvasActive = true;

        // Menentukan posisi layar dari pointer
        Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(null, pointer.position);

        // Loop melalui setiap objek Raw Image
        for (int i = 0; i < rawImageObjects.Length; i++)
        {
            // Memeriksa apakah pointer berada di atas objek Raw Image
            if (RectTransformUtility.RectangleContainsScreenPoint(rawImageObjects[i], screenPoint))
            {
                // Menentukan ukuran target yang baru
                targetSizes[i] = originalSizes[i] + new Vector2(5, 5);
                // Deteksi tap/click di layar
                if (Input.GetMouseButtonDown(0))
                {
                    isPaneCanvasActive = false;
                    // Memeriksa apakah pointer berada di atas PaneCanvas
                    if (RectTransformUtility.RectangleContainsScreenPoint(paneCanvas.GetComponent<RectTransform>(), screenPoint))
                    {
                        // Fade out PaneCanvas
                        StartCoroutine(FadeCanvasGroup(paneCanvas, 1f, 0f, 0.5f));

                        if (!isPaneCanvasActive)
                        {
                            otherCanvas.transform.rotation = currentGyroRotation;
                            otherCanvas.transform.position = gyroObj.transform.position + gyroObj.transform.forward * 100f;
                            // Fade in otherCanvas
                            StartCoroutine(FadeCanvasGroup(otherCanvas, 0f, 1f, 0.5f));
                            isOtherCanvasActive = true;
                        }
                    }
                }
            }
            else
            {
                // Mengembalikan ukuran target ke ukuran asli
                targetSizes[i] = originalSizes[i];
            }

            // Mengubah ukuran objek secara smooth menggunakan Lerp
            rawImageObjects[i].sizeDelta = Vector2.Lerp(rawImageObjects[i].sizeDelta, targetSizes[i], smoothSpeed * Time.deltaTime);
        }

        // Deteksi tap di otherCanvas
        if (!isPaneCanvasActive && isOtherCanvasActive && Input.GetMouseButtonDown(0))
        {
            if (RectTransformUtility.RectangleContainsScreenPoint(otherCanvas.GetComponent<RectTransform>(), screenPoint))
            {
                // Ganti texture dengan fade transition
                StartCoroutine(ChangeTextureWithFade(otherCanvasRawImage, newTextures[currentTextureIndex], 0.5f));
                // Update indeks texture
                currentTextureIndex = (currentTextureIndex + 1) % newTextures.Length;
            }
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

    // Coroutine untuk mengganti texture dengan fade transition
    IEnumerator ChangeTextureWithFade(RawImage rawImage, Texture newTexture, float duration)
    {
        float startAlpha = rawImage.color.a;
        float elapsedTime = 0f;

        // Fade out
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            Color color = rawImage.color;
            color.a = Mathf.Lerp(startAlpha, 0f, elapsedTime / duration);
            rawImage.color = color;
            yield return null;
        }

        // Ganti texture
        rawImage.texture = newTexture;

        elapsedTime = 0f;

        // Fade in
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            Color color = rawImage.color;
            color.a = Mathf.Lerp(0f, startAlpha, elapsedTime / duration);
            rawImage.color = color;
            yield return null;
        }

        Color finalColor = rawImage.color;
        finalColor.a = startAlpha;
        rawImage.color = finalColor; // Pastikan alpha akhir yang benar
    }
}

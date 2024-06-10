using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SwipeToVR : MonoBehaviour
{
    public CanvasGroup introScreenCanvas; // GameObject yang ingin di-swipe
    public string nextSceneName; // Nama scene yang ingin dipindahkan setelah fade out
    public float fadeOutDuration = 1f; // Durasi fade out dalam detik

    private bool isFadingOut = false;

    void Update()
    {
        // Cek jika sedang menekan layar (atau mouse)
        if (Input.GetMouseButtonDown(0))
        {
            // Mendapatkan posisi klik
            Vector3 clickPosition = Input.mousePosition;

            // Cek jika posisi klik berada di dalam area CanvasGroup
            if (RectTransformUtility.RectangleContainsScreenPoint(introScreenCanvas.GetComponent<RectTransform>(), clickPosition))
            {
                // Mulai fade out
                StartFadeOut();
            }
        }

        // Cek jika sedang fade out
        if (isFadingOut)
        {
            // Update alpha CanvasGroup
            introScreenCanvas.alpha -= Time.deltaTime / fadeOutDuration;

            // Cek jika fade out selesai
            if (introScreenCanvas.alpha <= 0)
            {
                // Wait for 2 seconds before starting the fade
                // Pindah ke scene lain
                SceneManager.LoadScene(nextSceneName);
            }
        }
    }

    void StartFadeOut()
    {
        isFadingOut = true;
    }
}

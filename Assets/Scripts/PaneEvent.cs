using UnityEngine;
using UnityEngine.UI;

public class PaneEvent : MonoBehaviour
{
    public GameObject paneCanvas; // PaneCanvas that needs to be displayed
    public GameObject[] otherCanvases; // Other canvases that should become transparent
    public GameObject gyroObj;
    private Quaternion currentGyroRotation;
    private Quaternion targetGyroRotation;
    private Vector3 paneCanvasOffset = new Vector3(0, 0, 100f); // Offset to position paneCanvas in front of gyroObj

    private bool isPaneCanvasActive = false; // Status to check if PaneCanvas is active or not

    private CanvasGroup paneCanvasGroup; // CanvasGroup component on paneCanvas
    private CanvasGroup[] otherCanvasGroups; // CanvasGroup components on other canvases
    private float fadeDuration = 0.5f; // Duration for fade transition
    private float transparencyDistanceThreshold = 50f; // Distance within which other canvases become transparent

    void Start()
    {
        // Initialize: PaneCanvas is not active
        paneCanvas.SetActive(false);
        paneCanvasGroup = paneCanvas.GetComponent<CanvasGroup>();

        otherCanvasGroups = new CanvasGroup[otherCanvases.Length];
        for (int i = 0; i < otherCanvases.Length; i++)
        {
            otherCanvasGroups[i] = otherCanvases[i].GetComponent<CanvasGroup>();
        }
    }

    void Update()
    {
        // Update gyro rotation
        currentGyroRotation = gyroObj.transform.rotation;

        // Check if the user double-tapped the screen
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Ended)
        {
            if (Input.GetTouch(0).tapCount == 2)
            {
                if (isPaneCanvasActive)
                {
                    // If PaneCanvas is active, deactivate it with fade out
                    StartCoroutine(FadeCanvas(paneCanvasGroup, 1f, 0f));
                    ResetOtherCanvasesTransparency(); // Reset transparency for other canvases
                    isPaneCanvasActive = false;
                }
                else
                {
                    // Update rotation and position to always face forward from gyroObj
                    paneCanvas.transform.rotation = currentGyroRotation;
                    paneCanvas.transform.position = gyroObj.transform.position + gyroObj.transform.forward * 100f;

                    // If PaneCanvas is not active, activate it with fade in
                    paneCanvas.SetActive(true);
                    StartCoroutine(FadeCanvas(paneCanvasGroup, 0f, 1f));
                    UpdateOtherCanvasesTransparency(); // Update transparency for other canvases
                    isPaneCanvasActive = true;
                }
            }
        }
    }

    // Function to fade in/out CanvasGroup
    private System.Collections.IEnumerator FadeCanvas(CanvasGroup canvasGroup, float startAlpha, float targetAlpha)
    {
        float startTime = Time.time;
        float elapsedTime = 0f;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime = Time.time - startTime;
            float normalizedTime = elapsedTime / fadeDuration;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, normalizedTime);
            yield return null;
        }

        canvasGroup.alpha = targetAlpha;

        // If targetAlpha is 0 (fade out), deactivate paneCanvas after transition
        if (targetAlpha == 0f)
        {
            paneCanvas.SetActive(false);
        }
    }

    // Function to set transparency for other canvases based on their distance to paneCanvas
    private void UpdateOtherCanvasesTransparency()
    {
        foreach (CanvasGroup canvasGroup in otherCanvasGroups)
        {
            GameObject otherCanvas = canvasGroup.gameObject;
            float distance = Vector3.Distance(paneCanvas.transform.position, otherCanvas.transform.position);
            if (distance < transparencyDistanceThreshold)
            {
                StartCoroutine(FadeCanvas(canvasGroup, canvasGroup.alpha, 0.3f)); // Fade to more transparent if within threshold
            }
            else
            {
                StartCoroutine(FadeCanvas(canvasGroup, canvasGroup.alpha, 1f)); // Fade to opaque if outside threshold
            }
        }
    }

    // Function to reset transparency for other canvases to opaque
    private void ResetOtherCanvasesTransparency()
    {
        foreach (CanvasGroup canvasGroup in otherCanvasGroups)
        {
            StartCoroutine(FadeCanvas(canvasGroup, canvasGroup.alpha, 1f)); // Fade to opaque
        }
    }
}

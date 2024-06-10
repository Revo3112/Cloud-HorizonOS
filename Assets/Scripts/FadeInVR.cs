using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FadeInVR : MonoBehaviour
{
    public float fadeDuration = 1.0f;
    public CanvasGroup newSceneCanvasGroup;
    void Start()
    {
        // Start fading in the canvas
        StartCoroutine(FadeInCanvas());
    }

    private IEnumerator FadeInCanvas()
    {
        newSceneCanvasGroup.alpha = 0;
        float timer = 0.0f;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            newSceneCanvasGroup.alpha = Mathf.Lerp(0, 1, timer / fadeDuration);
            yield return null;
        }

        newSceneCanvasGroup.alpha = 1;
    }
}

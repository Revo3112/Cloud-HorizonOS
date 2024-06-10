using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class FadeInWithWallpaper : MonoBehaviour
{
    public CanvasGroup newSceneCanvasGroup;
    public float fadeDuration = 1.0f;
    public Image wallpaperImage; // Reference to the Image component
    public Image helloText; // Reference to the Image component for hello text
    public Sprite[] wallpapers; // Array to hold wallpaper images
    public Sprite[] texts; // Array to hold hello text images
    public RectTransform swipeObject; // Reference to the RectTransform component for the swipe object
    public float swipeDuration = 2.0f; // Duration for the swipe object to complete one up and down cycle
    public float swipeDistance = 50.0f; // Distance the swipe object moves up and down

    void Start()
    {
        // Start fading in the canvas
        StartCoroutine(FadeInCanvas());

        // Change the wallpaper image randomly
        SetRandomWallpaper();

        // Start showing hello texts in order
        StartCoroutine(SetOrderHelloText());

        // Start moving the swipe object up and down
        StartCoroutine(MoveSwipeObject());
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

    private IEnumerator FadeText(Sprite textSprite)
    {
        helloText.sprite = textSprite;

        // Fade in
        helloText.canvasRenderer.SetAlpha(0.0f);
        helloText.CrossFadeAlpha(1.0f, fadeDuration, false);
        yield return new WaitForSeconds(fadeDuration);

        // Hold for a moment
        yield return new WaitForSeconds(1.0f);

        // Fade out
        helloText.CrossFadeAlpha(0.0f, fadeDuration, false);
        yield return new WaitForSeconds(fadeDuration);
    }

    private IEnumerator SetOrderHelloText()
    {
        while (true)
        {
            foreach (Sprite text in texts)
            {
                yield return StartCoroutine(FadeText(text));
            }
        }
    }

    private IEnumerator MoveSwipeObject()
    {
        Vector3 startPos = swipeObject.anchoredPosition;
        Vector3 endPos = startPos + new Vector3(0, swipeDistance, 0);

        while (true)
        {
            // Move up
            yield return MoveObject(swipeObject, startPos, endPos, swipeDuration / 2);
            // Wait at the top

            // Move down
            yield return MoveObject(swipeObject, endPos, startPos, swipeDuration - 1 / 2);
            // Wait at the bottom
            yield return new WaitForSeconds(1.0f);
        }
    }

    private IEnumerator MoveObject(RectTransform obj, Vector3 start, Vector3 end, float duration)
    {
        float timer = 0.0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float t = timer / duration;
            t = Mathf.Sin(t * Mathf.PI * 0.5f); // Smooth ease-in and ease-out
            obj.anchoredPosition = Vector3.Lerp(start, end, t);
            yield return null;
        }

        obj.anchoredPosition = end;
    }

    void SetRandomWallpaper()
    {
        // Check if the Image component is assigned
        if (wallpaperImage == null)
        {
            Debug.LogError("Wallpaper Image component is not assigned!");
            return;
        }

        // Check if there are wallpapers in the array
        if (wallpapers.Length == 0)
        {
            Debug.LogError("No wallpapers are assigned!");
            return;
        }

        // Randomly select an index from the wallpapers array
        int randomIndex = Random.Range(0, wallpapers.Length);

        // Set the selected wallpaper as the source image
        wallpaperImage.sprite = wallpapers[randomIndex];
    }
}

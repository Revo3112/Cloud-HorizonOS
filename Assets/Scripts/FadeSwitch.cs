using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FadeSwitch : MonoBehaviour
{
    public CanvasGroup introScreenCanvas;
    public float fadeDuration = 1.0f;
    public float waitDur = 1.0f;
    public string sceneToLoad;
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(SwitchSceneWithFade());
    }

    private IEnumerator SwitchSceneWithFade()
    {
        // Wait for 2 seconds before starting the fade
        yield return new WaitForSeconds(waitDur);

        // Start the fade out process
        yield return StartCoroutine(FadeOut());

        // Load the new scene
        SceneManager.LoadScene(sceneToLoad);
    }

    private IEnumerator FadeOut()
    {
        float timer = 0.0f;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            introScreenCanvas.alpha = Mathf.Lerp(1, 0, timer / fadeDuration);
            yield return null;
        }
        introScreenCanvas.alpha = 0;
    }
}

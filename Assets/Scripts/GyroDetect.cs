using System.Collections;
using UnityEngine;
using UnityEngine.Android;
using UnityEngine.UI;

public class GyroDetect : MonoBehaviour
{
    public GameObject gyroObj;
    public GameObject audioObj;
    public CanvasGroup instructionCanvasGroup;
    public CanvasGroup canvasVR;
    public CanvasGroup canvasCam;
    public RawImage VRrightCam; // RawImage untuk menampilkan tampilan UICam
    public RawImage VRleftCam; // RawImage untuk menampilkan kamera (dapat disesuaikan)
    public RawImage rightCam; // RawImage untuk menampilkan kamera belakang
    public Canvas UICanvas; // Canvas untuk UI
    public Canvas ElementUICanvas; // Canvas utk element AR
    public Camera UICam; // Kamera untuk UI Canvas
    private WebCamTexture backCam;
    private RenderTexture uiRenderTexture;

    private Quaternion currentGyroRotation;
    private Quaternion targetGyroRotation;

    public float rotationLerpSpeed = 10f;
    public float positionLerpSpeed = 2f; // Adjust the speed as per your requirement
    private bool isFading = false;
    private Vector3 targetElementUIPosition;

    // Variables for triple tap detection
    private int tapCount = 0;
    private float lastTapTime = 0f;
    private const float tapDelay = 0.3f; // Time allowed between taps

    void Start()
    {
        gyroObj.SetActive(false); // inisiasi awal
        audioObj.SetActive(false); // inisiasi awal
        instructionCanvasGroup.gameObject.SetActive(true);
        canvasVR.alpha = 1f; // Set initial opacity to 1
        canvasCam.alpha = 0f; // Inisiasi awal canvasCam tidak terlihat
        canvasCam.gameObject.SetActive(false); // Inisiasi awal canvasCam tidak aktif

        if (SystemInfo.supportsGyroscope)
        {
            Input.gyro.enabled = true;
        }

        currentGyroRotation = Quaternion.identity;
        targetGyroRotation = Quaternion.identity;

        // Request camera permission
        if (!Permission.HasUserAuthorizedPermission(Permission.Camera))
        {
            Permission.RequestUserPermission(Permission.Camera);
        }
        else
        {
            InitCamera();
        }

        // Inisialisasi RenderTexture
        uiRenderTexture = new RenderTexture(Screen.width, Screen.height, 24);
        UICam.targetTexture = uiRenderTexture;
        VRrightCam.texture = uiRenderTexture;
        VRleftCam.texture = VRrightCam.texture;

        // Set initial target position for ElementUICanvas
        targetElementUIPosition = ElementUICanvas.transform.position;
    }

    void Update()
    {
        DetectGyro();

        // Smoothly interpolate between current and target rotation
        currentGyroRotation = Quaternion.Lerp(currentGyroRotation, targetGyroRotation, rotationLerpSpeed * Time.deltaTime);
        gyroObj.transform.rotation = currentGyroRotation;

        // Update the rotation of canvasCam to match gyroObj's rotation
        canvasCam.transform.rotation = currentGyroRotation;
        ElementUICanvas.transform.rotation = currentGyroRotation;

        // Maintain canvasCam's view by adjusting its position
        MaintainCanvasCamView();

        MaintainElementUIView();

        // Interpolate the position of ElementUICanvas
        ElementUICanvas.transform.position = Vector3.Lerp(ElementUICanvas.transform.position, targetElementUIPosition, positionLerpSpeed * Time.deltaTime);

        // Deactivate instructionCanvasGroup
        instructionCanvasGroup.gameObject.SetActive(false);

        // Detect gyro motion
        if (!isFading && DetectGyroMotion())
        {
            StartCoroutine(FadeOutCanvasVR());
        }

        // Detect triple tap
        DetectTripleTap();
    }

    void DetectGyro()
    {
        if (SystemInfo.supportsGyroscope && Input.gyro.enabled)
        {
            gyroObj.SetActive(true); // aktivasi capsule
            audioObj.SetActive(true); // aktivasi capsule
            Camera.main.clearFlags = CameraClearFlags.Skybox;

            // Apply gyro rotation to gyroObj
            targetGyroRotation = GyroToUnity(Input.gyro.attitude);

            Quaternion screenRotation = Quaternion.identity;
            targetGyroRotation = screenRotation * Quaternion.Euler(90, 0, -70) * targetGyroRotation;

            instructionCanvasGroup.gameObject.SetActive(false);
        }
        else
        {
            Debug.LogWarning("Gyroscope not supported or enabled. supportsGyroscope: " + SystemInfo.supportsGyroscope + ", gyro.enabled: " + Input.gyro.enabled);
        }
    }

    private Quaternion GyroToUnity(Quaternion q)
    {
        return new Quaternion(q.x, q.y, -q.z, -q.w);
    }

    private bool DetectGyroMotion()
    {
        if (Input.gyro.userAcceleration.z > 0.5f || Input.gyro.userAcceleration.z < -0.5f)
        {
            return true;
        }
        return false;
    }

    private IEnumerator FadeOutCanvasVR()
    {
        isFading = true;
        float duration = 1f; // Fade duration
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            canvasVR.alpha = Mathf.Lerp(1f, 0f, elapsed / duration);
            yield return null;
        }

        canvasVR.alpha = 0f;
        canvasVR.gameObject.SetActive(false);
        ActivateCamera();
        isFading = false;
    }

    private void ActivateCamera()
    {
        if (backCam != null)
        {
            backCam.Play();
            rightCam.texture = backCam; // Set WebCamTexture to rightCam
            Color color = rightCam.color;
            color.a = 0f;
            rightCam.color = color;
            canvasCam.alpha = 1f; // Set canvasCam visible
            canvasCam.gameObject.SetActive(true);
        }
    }

    private void InitCamera()
    {
        if (backCam == null)
        {
            backCam = new WebCamTexture();
        }
        ActivateCamera();
    }

    private void MaintainCanvasCamView()
    {
        Vector3 gyroObjPosition = gyroObj.transform.position;
        canvasCam.transform.position = CalculateCanvasCamPosition(gyroObjPosition);
    }

    private void MaintainElementUIView()
    {
        Vector3 gyroObjPosition = gyroObj.transform.position;
        targetElementUIPosition = CalculateElementUIPosition(gyroObjPosition);
    }

    private Vector3 CalculateCanvasCamPosition(Vector3 gyroObjPosition)
    {
        float fixedDistance = 100f; // Set the fixed distance as per your requirement
        return gyroObjPosition + (gyroObj.transform.forward * fixedDistance);
    }

    private Vector3 CalculateElementUIPosition(Vector3 gyroObjPosition)
    {
        float fixedDistance = 100f; // Set the fixed distance as per your requirement
        return gyroObjPosition + (gyroObj.transform.forward * fixedDistance);
    }

    void OnApplicationFocus(bool hasFocus)
    {
        if (hasFocus && Permission.HasUserAuthorizedPermission(Permission.Camera))
        {
            InitCamera();
        }
    }

    void LateUpdate()
    {
        if (uiRenderTexture == null)
        {
            // Inisialisasi ulang RenderTexture jika null
            uiRenderTexture = new RenderTexture(Screen.width, Screen.height, 24);
            UICam.targetTexture = uiRenderTexture;
            VRrightCam.texture = uiRenderTexture;
            VRleftCam.texture = VRrightCam.texture;
        }

        // Pastikan UICam merekam ke RenderTexture
        if (UICam.targetTexture != uiRenderTexture)
        {
            UICam.targetTexture = uiRenderTexture;
            VRrightCam.texture = uiRenderTexture;
            VRleftCam.texture = VRrightCam.texture;
        }
    }

    private void DetectTripleTap()
    {
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Ended)
        {
            if (Time.time - lastTapTime < tapDelay)
            {
                tapCount++;
            }
            else
            {
                tapCount = 1; // Reset to 1 tap if time between taps is too long
            }

            lastTapTime = Time.time;

            if (tapCount == 3)
            {
                StartCoroutine(FadeInOutRightCam());
                tapCount = 0; // Reset tap count after triple tap detected
            }
        }
    }

    private IEnumerator FadeInOutRightCam()
    {
        float duration = 1f; // Fade duration
        float elapsed = 0f;

        if (canvasCam.gameObject.activeSelf)
        {
            // Fade out rightCam
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                Color color = rightCam.color;
                color.a = Mathf.Lerp(1f, 0f, elapsed / duration);
                rightCam.color = color;
                yield return null;
            }

            rightCam.color = new Color(rightCam.color.r, rightCam.color.g, rightCam.color.b, 0f); // Ensure full transparency
        }
        else
        {
            // Fade in rightCam
            canvasCam.gameObject.SetActive(true);
            elapsed = 0f; // Reset elapsed time
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                Color color = rightCam.color;
                color.a = Mathf.Lerp(0f, 1f, elapsed / duration);
                rightCam.color = color;
                yield return null;
            }

            rightCam.color = new Color(rightCam.color.r, rightCam.color.g, rightCam.color.b, 1f); // Ensure full opacity
        }
    }
}

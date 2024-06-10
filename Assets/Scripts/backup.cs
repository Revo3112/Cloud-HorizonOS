using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class backup : MonoBehaviour
{
    public GameObject gyroObj;
    public GameObject audioObj;
    public CanvasGroup instructionCanvasGroup;
    int count = 0;

    private Quaternion currentGyroRotation;
    private Quaternion targetGyroRotation;

    // Adjust the speed based on your preference
    public float rotationLerpSpeed = 10f;

    // Start is called before the first frame update
    void Start()
    {
        gyroObj.SetActive(false); // inisiasi awal
        audioObj.SetActive(false); // inisiasi awal
        if (SystemInfo.supportsGyroscope)
        {
            Input.gyro.enabled = true;
        }

        currentGyroRotation = Quaternion.identity;
        targetGyroRotation = Quaternion.identity;
    }

    void Update()
    {
        // "instructionCanvasGroup is active."
        DetectGyro();

        // Smoothly interpolate between current and target rotation
        currentGyroRotation = Quaternion.Lerp(currentGyroRotation, targetGyroRotation, rotationLerpSpeed * Time.deltaTime);

        gyroObj.transform.rotation = currentGyroRotation;

        // Deactivate instructionCanvasGroup
        instructionCanvasGroup.gameObject.SetActive(false);
    }

    void DetectGyro()
    {
        // Check if gyro is available and enabled
        if (SystemInfo.supportsGyroscope && Input.gyro.enabled)
        {
            gyroObj.SetActive(true); // aktivasi capsule
            audioObj.SetActive(true); // aktivasi capsule
            // Set the Clear Flags property of the main camera to "Skybox"
            Camera.main.clearFlags = CameraClearFlags.Skybox;

            // Apply gyro rotation to gyroObj
            targetGyroRotation = GyroToUnity(Input.gyro.attitude);

            // Mengonversi orientasi layar menjadi rotasi yang sesuai untuk memperbaiki masalah orientasi
            Quaternion screenRotation = Quaternion.identity;

            // Adjust gyro rotation based on screen orientation
            targetGyroRotation = screenRotation * Quaternion.Euler(90, 0, -70) * targetGyroRotation;

            // Deactivate instructionCanvasGroup
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
}
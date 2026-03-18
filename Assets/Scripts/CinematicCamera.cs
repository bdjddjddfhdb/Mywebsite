using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Cinematic handheld camera with shake effect and jagged cuts between viewpoints.
/// Simulates documentary/indie film style camera work for the Baburka 2028 scene.
/// </summary>
public class CinematicCamera : MonoBehaviour
{
    [Header("Handheld Shake")]
    [SerializeField] private float positionShakeIntensity = 0.02f;
    [SerializeField] private float rotationShakeIntensity = 0.5f;
    [SerializeField] private float shakeSpeed = 1.5f;
    [SerializeField] private float shakeDrift = 0.3f;

    [Header("Breathing Effect")]
    [SerializeField] private bool enableBreathing = true;
    [SerializeField] private float breathingSpeed = 0.4f;
    [SerializeField] private float breathingIntensity = 0.008f;

    [Header("Jagged Cuts")]
    [SerializeField] private bool enableAutoCuts = true;
    [SerializeField] private float minCutInterval = 3f;
    [SerializeField] private float maxCutInterval = 8f;
    [SerializeField] [Range(0f, 1f)] private float flashCutChance = 0.15f;
    [SerializeField] private float flashCutDuration = 0.3f;

    [Header("Camera Points")]
    [SerializeField] private List<Transform> cameraPoints = new List<Transform>();

    [Header("Post-Cut Effect")]
    [SerializeField] private float cutZoomPunch = 3f;
    [SerializeField] private float cutZoomRecoveryTime = 0.5f;

    private Camera cam;
    private float baseFOV;
    private float cutTimer;
    private float nextCutTime;
    private int currentPointIndex;
    private bool isFlashCut;
    private float flashCutTimer;
    private int flashCutReturnIndex;

    private float noiseOffsetX;
    private float noiseOffsetY;
    private float noiseOffsetZ;
    private float noiseOffsetRotX;
    private float noiseOffsetRotY;
    private float noiseOffsetRotZ;

    private float currentZoomPunch;
    private Vector3 basePosition;
    private Quaternion baseRotation;

    private void Awake()
    {
        cam = GetComponent<Camera>();
        if (cam == null)
        {
            cam = gameObject.AddComponent<Camera>();
        }
        baseFOV = cam.fieldOfView;
    }

    private void Start()
    {
        InitializeNoiseOffsets();

        if (cameraPoints.Count == 0)
        {
            GenerateDefaultCameraPoints();
        }

        if (cameraPoints.Count > 0)
        {
            ApplyCameraPoint(0);
        }

        ScheduleNextCut();
    }

    private void LateUpdate()
    {
        if (enableAutoCuts && cameraPoints.Count > 1)
        {
            HandleCutTiming();
        }

        HandleFlashCut();
        ApplyHandheldShake();
        ApplyBreathing();
        ApplyZoomPunch();
    }

    private void InitializeNoiseOffsets()
    {
        noiseOffsetX = Random.Range(0f, 1000f);
        noiseOffsetY = Random.Range(0f, 1000f);
        noiseOffsetZ = Random.Range(0f, 1000f);
        noiseOffsetRotX = Random.Range(0f, 1000f);
        noiseOffsetRotY = Random.Range(0f, 1000f);
        noiseOffsetRotZ = Random.Range(0f, 1000f);
    }

    private void GenerateDefaultCameraPoints()
    {
        Vector3[] positions = new Vector3[]
        {
            new Vector3(0f, 1.7f, 5f),
            new Vector3(-8f, 1.7f, 30f),
            new Vector3(8f, 1.7f, 25f),
            new Vector3(-5f, 1.7f, 60f),
            new Vector3(0f, 2.5f, 45f),
            new Vector3(3f, 1.5f, 48f),
            new Vector3(0f, 15f, 30f),
            new Vector3(-12f, 5f, 20f),
        };

        Vector3[] lookAts = new Vector3[]
        {
            new Vector3(0f, 1.5f, 60f),
            new Vector3(-9f, 1.2f, 48f),
            new Vector3(9.5f, 1.2f, 42f),
            new Vector3(-8f, 1.2f, 72f),
            new Vector3(0f, 1.5f, 50f),
            new Vector3(0f, 1.5f, 42.5f),
            new Vector3(0f, 0f, 60f),
            new Vector3(0f, 1.5f, 50f),
        };

        for (int i = 0; i < positions.Length; i++)
        {
            GameObject point = new GameObject("CamPoint_" + i);
            point.transform.position = positions[i];
            point.transform.LookAt(lookAts[i]);
            point.transform.parent = transform;
            cameraPoints.Add(point.transform);
        }
    }

    private void HandleCutTiming()
    {
        cutTimer += Time.deltaTime;
        if (cutTimer >= nextCutTime)
        {
            PerformCut();
        }
    }

    private void PerformCut()
    {
        if (Random.value < flashCutChance)
            PerformFlashCut();
        else
            PerformStandardCut();

        ScheduleNextCut();
    }

    private void PerformStandardCut()
    {
        int newIndex;
        int attempts = 0;
        do
        {
            newIndex = Random.Range(0, cameraPoints.Count);
            attempts++;
        } while (newIndex == currentPointIndex && attempts < 10);

        currentPointIndex = newIndex;
        ApplyCameraPoint(currentPointIndex);
        currentZoomPunch = cutZoomPunch * (Random.value > 0.5f ? 1f : -1f);
    }

    private void PerformFlashCut()
    {
        flashCutReturnIndex = currentPointIndex;
        isFlashCut = true;
        flashCutTimer = flashCutDuration;

        int flashIndex;
        int attempts = 0;
        do
        {
            flashIndex = Random.Range(0, cameraPoints.Count);
            attempts++;
        } while (flashIndex == currentPointIndex && attempts < 10);

        ApplyCameraPoint(flashIndex);
        currentZoomPunch = cutZoomPunch * 2f;
    }

    private void HandleFlashCut()
    {
        if (!isFlashCut) return;

        flashCutTimer -= Time.deltaTime;
        if (flashCutTimer <= 0f)
        {
            isFlashCut = false;
            ApplyCameraPoint(flashCutReturnIndex);
            currentZoomPunch = -cutZoomPunch;
        }
    }

    private void ApplyCameraPoint(int index)
    {
        if (index < 0 || index >= cameraPoints.Count) return;

        Transform point = cameraPoints[index];
        if (point == null) return;

        transform.position = point.position;
        transform.rotation = point.rotation;
        basePosition = point.position;
        baseRotation = point.rotation;
        currentPointIndex = index;
    }

    private void ScheduleNextCut()
    {
        cutTimer = 0f;
        nextCutTime = Random.Range(minCutInterval, maxCutInterval);
    }

    private void ApplyHandheldShake()
    {
        float time = Time.time * shakeSpeed;

        float shakeX = (Mathf.PerlinNoise(time + noiseOffsetX, 0f) - 0.5f) * 2f * positionShakeIntensity;
        float shakeY = (Mathf.PerlinNoise(0f, time + noiseOffsetY) - 0.5f) * 2f * positionShakeIntensity;
        float shakeZ = (Mathf.PerlinNoise(time + noiseOffsetZ, time) - 0.5f) * 2f * positionShakeIntensity;

        transform.position = basePosition + new Vector3(shakeX, shakeY, shakeZ);

        float rotX = (Mathf.PerlinNoise(time + noiseOffsetRotX, 0f) - 0.5f) * 2f * rotationShakeIntensity;
        float rotY = (Mathf.PerlinNoise(0f, time + noiseOffsetRotY) - 0.5f) * 2f * rotationShakeIntensity;
        float rotZ = (Mathf.PerlinNoise(time + noiseOffsetRotZ, time) - 0.5f) * 2f * rotationShakeIntensity * 0.5f;

        transform.rotation = baseRotation * Quaternion.Euler(rotX, rotY, rotZ);

        noiseOffsetX += Time.deltaTime * shakeDrift;
        noiseOffsetY += Time.deltaTime * shakeDrift;
        noiseOffsetZ += Time.deltaTime * shakeDrift;
    }

    private void ApplyBreathing()
    {
        if (!enableBreathing) return;
        float breathOffset = Mathf.Sin(Time.time * breathingSpeed * Mathf.PI * 2f) * breathingIntensity;
        transform.position += new Vector3(0f, breathOffset, 0f);
    }

    private void ApplyZoomPunch()
    {
        if (Mathf.Abs(currentZoomPunch) < 0.01f)
        {
            currentZoomPunch = 0f;
            return;
        }
        cam.fieldOfView = baseFOV + currentZoomPunch;
        currentZoomPunch = Mathf.Lerp(currentZoomPunch, 0f, Time.deltaTime / cutZoomRecoveryTime);
    }

    public void CutToPoint(int index)
    {
        if (index >= 0 && index < cameraPoints.Count)
        {
            ApplyCameraPoint(index);
            currentZoomPunch = cutZoomPunch;
        }
    }

    public void AddCameraPoint(Transform point)
    {
        if (point != null)
            cameraPoints.Add(point);
    }

    public void SetAutoCutsEnabled(bool enabled)
    {
        enableAutoCuts = enabled;
        if (enabled) ScheduleNextCut();
    }

    public void SetShakeIntensity(float posIntensity, float rotIntensity)
    {
        positionShakeIntensity = posIntensity;
        rotationShakeIntensity = rotIntensity;
    }
}

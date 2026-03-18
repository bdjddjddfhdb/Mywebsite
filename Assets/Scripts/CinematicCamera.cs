using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Cinematic handheld camera with shake effect and jagged cuts between viewpoints.
/// Simulates documentary/indie film style camera work for the Baburka 2028 scene.
/// </summary>
public class CinematicCamera : MonoBehaviour
{
    [Header("Handheld Shake")]
    [Tooltip("Intensity of the position shake (meters).")]
    [SerializeField] private float positionShakeIntensity = 0.02f;

    [Tooltip("Intensity of the rotation shake (degrees).")]
    [SerializeField] private float rotationShakeIntensity = 0.5f;

    [Tooltip("Speed of the shake oscillation.")]
    [SerializeField] private float shakeSpeed = 1.5f;

    [Tooltip("How much the shake wanders over time (lower = more steady).")]
    [SerializeField] private float shakeDrift = 0.3f;

    [Header("Breathing Effect")]
    [Tooltip("Simulate camera operator breathing.")]
    [SerializeField] private bool enableBreathing = true;

    [Tooltip("Breathing cycle speed.")]
    [SerializeField] private float breathingSpeed = 0.4f;

    [Tooltip("Breathing intensity (vertical movement in meters).")]
    [SerializeField] private float breathingIntensity = 0.008f;

    [Header("Jagged Cuts")]
    [Tooltip("Enable automatic cuts between camera points.")]
    [SerializeField] private bool enableAutoCuts = true;

    [Tooltip("Minimum time between cuts (seconds).")]
    [SerializeField] private float minCutInterval = 3f;

    [Tooltip("Maximum time between cuts (seconds).")]
    [SerializeField] private float maxCutInterval = 8f;

    [Tooltip("Chance of a very short 'flash' cut (0-1).")]
    [SerializeField] [Range(0f, 1f)] private float flashCutChance = 0.15f;

    [Tooltip("Duration of a flash cut (seconds).")]
    [SerializeField] private float flashCutDuration = 0.3f;

    [Header("Camera Points")]
    [Tooltip("List of transforms the camera cuts between. If empty, generates default points.")]
    [SerializeField] private List<Transform> cameraPoints = new List<Transform>();

    [Header("Post-Cut Effect")]
    [Tooltip("Brief zoom punch on cut (field of view change).")]
    [SerializeField] private float cutZoomPunch = 3f;

    [Tooltip("Duration of the zoom punch recovery.")]
    [SerializeField] private float cutZoomRecoveryTime = 0.5f;

    // Internal state
    private Camera cam;
    private float baseFOV;
    private float cutTimer;
    private float nextCutTime;
    private int currentPointIndex;
    private bool isFlashCut;
    private float flashCutTimer;
    private int flashCutReturnIndex;

    // Shake noise offsets (Perlin noise based)
    private float noiseOffsetX;
    private float noiseOffsetY;
    private float noiseOffsetZ;
    private float noiseOffsetRotX;
    private float noiseOffsetRotY;
    private float noiseOffsetRotZ;

    // Zoom punch state
    private float currentZoomPunch;

    // Original transform for each frame
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
        // Randomize Perlin noise sample positions so each session feels different
        noiseOffsetX = Random.Range(0f, 1000f);
        noiseOffsetY = Random.Range(0f, 1000f);
        noiseOffsetZ = Random.Range(0f, 1000f);
        noiseOffsetRotX = Random.Range(0f, 1000f);
        noiseOffsetRotY = Random.Range(0f, 1000f);
        noiseOffsetRotZ = Random.Range(0f, 1000f);
    }

    private void GenerateDefaultCameraPoints()
    {
        // Create default cinematic viewpoints around the Baburka district scene
        Vector3[] defaultPositions = new Vector3[]
        {
            new Vector3(0f, 1.7f, 5f),      // Street level, looking down the road
            new Vector3(-8f, 1.7f, 30f),     // Left sidewalk, watching Vitaliy
            new Vector3(8f, 1.7f, 25f),      // Right sidewalk, near Kirill
            new Vector3(-5f, 1.7f, 60f),     // Near Uliana
            new Vector3(0f, 2.5f, 45f),      // Elevated view of school entrance
            new Vector3(3f, 1.5f, 48f),      // Close-up on school entrance / Zavkhoz
            new Vector3(0f, 15f, 30f),       // Bird's eye establishing shot
            new Vector3(-12f, 5f, 20f),      // Building corner angle
        };

        Vector3[] defaultLookAts = new Vector3[]
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

        for (int i = 0; i < defaultPositions.Length; i++)
        {
            GameObject point = new GameObject("CamPoint_" + i);
            point.transform.position = defaultPositions[i];
            point.transform.LookAt(defaultLookAts[i]);
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
        // Decide if this is a flash cut
        if (Random.value < flashCutChance)
        {
            PerformFlashCut();
        }
        else
        {
            PerformStandardCut();
        }

        ScheduleNextCut();
    }

    private void PerformStandardCut()
    {
        // Pick a different camera point (avoid cutting to the same angle)
        int newIndex;
        int attempts = 0;
        do
        {
            newIndex = Random.Range(0, cameraPoints.Count);
            attempts++;
        } while (newIndex == currentPointIndex && attempts < 10);

        currentPointIndex = newIndex;
        ApplyCameraPoint(currentPointIndex);

        // Apply zoom punch effect
        currentZoomPunch = cutZoomPunch * (Random.value > 0.5f ? 1f : -1f);
    }

    private void PerformFlashCut()
    {
        flashCutReturnIndex = currentPointIndex;
        isFlashCut = true;
        flashCutTimer = flashCutDuration;

        // Cut to a random different angle
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

        // Position shake using Perlin noise for organic movement
        float shakeX = (Mathf.PerlinNoise(time + noiseOffsetX, 0f) - 0.5f) * 2f * positionShakeIntensity;
        float shakeY = (Mathf.PerlinNoise(0f, time + noiseOffsetY) - 0.5f) * 2f * positionShakeIntensity;
        float shakeZ = (Mathf.PerlinNoise(time + noiseOffsetZ, time) - 0.5f) * 2f * positionShakeIntensity;

        transform.position = basePosition + new Vector3(shakeX, shakeY, shakeZ);

        // Rotation shake — more subtle
        float rotX = (Mathf.PerlinNoise(time + noiseOffsetRotX, 0f) - 0.5f) * 2f * rotationShakeIntensity;
        float rotY = (Mathf.PerlinNoise(0f, time + noiseOffsetRotY) - 0.5f) * 2f * rotationShakeIntensity;
        float rotZ = (Mathf.PerlinNoise(time + noiseOffsetRotZ, time) - 0.5f) * 2f * rotationShakeIntensity * 0.5f;

        transform.rotation = baseRotation * Quaternion.Euler(rotX, rotY, rotZ);

        // Drift the noise offsets for variety over time
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

    /// <summary>
    /// Manually trigger a cut to a specific camera point index.
    /// </summary>
    public void CutToPoint(int index)
    {
        if (index >= 0 && index < cameraPoints.Count)
        {
            ApplyCameraPoint(index);
            currentZoomPunch = cutZoomPunch;
        }
    }

    /// <summary>
    /// Add a camera point at runtime.
    /// </summary>
    public void AddCameraPoint(Transform point)
    {
        if (point != null)
        {
            cameraPoints.Add(point);
        }
    }

    /// <summary>
    /// Toggle the auto-cut system on/off.
    /// </summary>
    public void SetAutoCutsEnabled(bool enabled)
    {
        enableAutoCuts = enabled;
        if (enabled)
        {
            ScheduleNextCut();
        }
    }

    /// <summary>
    /// Adjust shake intensity at runtime (e.g., for action sequences).
    /// </summary>
    public void SetShakeIntensity(float positionIntensity, float rotationIntensity)
    {
        positionShakeIntensity = positionIntensity;
        rotationShakeIntensity = rotationIntensity;
    }
}

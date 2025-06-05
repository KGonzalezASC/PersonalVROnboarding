using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR;
using UnityEngine.XR;
using UnityEngine.XR.Management;

public class SubsystemPrinterRefactored : MonoBehaviour
{
    private XRInputSubsystem openXRInputSubsystem;
    public GameObject boundaryFound;

    // Assign a prefab to spawn at each boundary point
    [Tooltip("Assign a prefab to instantiate at each boundary point.")]
    public GameObject boundaryMarkerPrefab;

    // Scale to apply to each spawned marker
    [Tooltip("Uniform scale for each spawned boundary marker.")]
    public float markerScale = 10f;

    private bool isChecking = false;

    [SerializeField]
    private DebugTextManager debugManager;

    // Keep track of spawned markers to clear them later
    private readonly List<GameObject> spawnedMarkers = new List<GameObject>();

    private void Awake()
    {
        InputSystem.onDeviceChange += OnDeviceChange;
        debugManager = FindObjectOfType<DebugTextManager>();
        if (debugManager == null)
            Debug.LogWarning("No DebugTextManager found in scene!");
    }

    private void OnEnable()
    {
        var existingHmd = InputSystem.GetDevice<XRHMD>();
        if (existingHmd != null)
        {
            LogLine("[SubsystemPrinter] HMD already connected at Enable.");
            StartSubsystemChecks();
        }
    }

    private void OnDisable()
    {
        InputSystem.onDeviceChange -= OnDeviceChange;
        if (openXRInputSubsystem != null)
        {
            openXRInputSubsystem.boundaryChanged -= OnBoundaryChanged;
            openXRInputSubsystem.trackingOriginUpdated -= GetHeadsetPosition;
        }
        ClearMarkers();
    }

    private void OnDeviceChange(UnityEngine.InputSystem.InputDevice device, InputDeviceChange change)
    {
        if (!(device is XRHMD)) return;

        switch (change)
        {
            case InputDeviceChange.Added:
            case InputDeviceChange.Reconnected:
                LogLine("[SubsystemPrinter] XRHMD connected/reconnected.");
                StartSubsystemChecks();
                break;

            case InputDeviceChange.Removed:
            case InputDeviceChange.Disconnected:
                LogLine("[SubsystemPrinter] XRHMD removed/disconnected.");
                isChecking = false;
                openXRInputSubsystem = null;
                ClearMarkers();
                break;
        }
    }

    private void StartSubsystemChecks()
    {
        if (!isChecking)
        {
            isChecking = true;
            StartCoroutine(CheckSubsystemsCoroutine());
        }
    }

    private IEnumerator CheckSubsystemsCoroutine()
    {
        LogLine("[SubsystemPrinter] Waiting for XR Loader to initialize...");
        var xrManager = XRGeneralSettings.Instance?.Manager;
        if (xrManager == null)
        {
            LogLine("[SubsystemPrinter] ERROR: XR General Settings / Manager is missing!");
            yield break;
        }

        while (xrManager.activeLoader == null || !xrManager.isInitializationComplete)
            yield return null;

        LogLine("[SubsystemPrinter] XR Loader initialized.");

        var inputSubsystems = new List<XRInputSubsystem>();
        SubsystemManager.GetSubsystems(inputSubsystems);

        if (inputSubsystems.Count == 0)
        {
            LogLine("[SubsystemPrinter] No XRInputSubsystems found even after loader init.");
            yield break;
        }

        foreach (var sub in inputSubsystems)
        {
            string id = sub.SubsystemDescriptor.id;
            bool running = sub.running;
            LogLine($"[XRInputSubsystem] id: {id} | running: {running}");

            if (id == "OpenXR Input")
            {
                openXRInputSubsystem = sub;

                var supportedOrigins = openXRInputSubsystem.GetSupportedTrackingOriginModes();
                LogLine($"    • Supported Tracking Origin Modes: {supportedOrigins}");

                var currentOrigin = openXRInputSubsystem.GetTrackingOriginMode();
                LogLine($"    • Current Tracking Origin Mode: {currentOrigin}");

                List<Vector3> boundaryPoints = new List<Vector3>();
                bool hasBoundary = openXRInputSubsystem.TryGetBoundaryPoints(boundaryPoints);
                LogLine($"    • Boundary supported: {hasBoundary} | point count: {boundaryPoints.Count}");

                if (boundaryPoints.Count > 0)
                {
                    SpawnMarkers(boundaryPoints);
                    for (int i = 0; i < boundaryPoints.Count; i++)
                        LogLine($"    • Boundary Point {i}: {boundaryPoints[i]}");

                    if (boundaryFound != null)
                        StartCoroutine(SpinGrowCube());
                }
                else
                {
                    ClearMarkers();
                }

                openXRInputSubsystem.boundaryChanged += OnBoundaryChanged;
                openXRInputSubsystem.trackingOriginUpdated += GetHeadsetPosition;
                LogLine("    → Stored a reference to the OpenXR Input subsystem.");
                break;
            }
        }
    }

    private void GetHeadsetPosition(XRInputSubsystem obj)
    {
        var xrHmd = InputSystem.GetDevice<XRHMD>();
        if (xrHmd != null)
        {
            Vector3 headPos = xrHmd.devicePosition.ReadValue();
            LogLine($"[GetHeadsetPosition] Raw devicePosition: {headPos}");
            LogLine($"[GetHeadsetPosition] Headset height (m): {headPos.y:F3}");
        }
        else
        {
            LogLine("[GetHeadsetPosition] No XRHMD device found.");
        }
    }

    private IEnumerator SpinGrowCube()
    {
        if (boundaryFound == null)
        {
            LogLine("[SubsystemPrinter] SpinGrowCube called but boundaryFound is null.");
            yield break;
        }

        LogLine("[SubsystemPrinter] Spinning and growing the cube...");
        Vector3 initialScale = boundaryFound.transform.localScale;
        Vector3 targetScale = initialScale * 100f;
        float duration = 1f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float t = elapsed / duration;
            boundaryFound.transform.localScale = Vector3.Lerp(initialScale, targetScale, t);
            float rotationThisFrame = 360f * (Time.deltaTime / duration);
            boundaryFound.transform.Rotate(Vector3.up, rotationThisFrame, Space.Self);
            elapsed += Time.deltaTime;
            yield return null;
        }

        boundaryFound.transform.localScale = targetScale;
        float overshoot = 360f - (360f * (elapsed - Time.deltaTime) / duration);
        boundaryFound.transform.Rotate(Vector3.up, overshoot, Space.Self);
        LogLine("[SubsystemPrinter] Cube spin‐grow complete.");
    }

    private void OnBoundaryChanged(XRInputSubsystem inputSubsystem)
    {
        List<Vector3> updatedPoints = new List<Vector3>();
        if (inputSubsystem.TryGetBoundaryPoints(updatedPoints))
        {
            LogLine($"[BoundaryChanged] New boundary point count: {updatedPoints.Count}");
            if (updatedPoints.Count > 0)
            {
                SpawnMarkers(updatedPoints);
                if (boundaryFound != null)
                    StartCoroutine(SpinGrowCube());
            }
            else
            {
                ClearMarkers();
            }
        }
        else
        {
            LogLine("[BoundaryChanged] Could not fetch boundary points.");
            ClearMarkers();
        }
    }

    private void SpawnMarkers(List<Vector3> boundaryPoints)
    {
        ClearMarkers();

        if (boundaryMarkerPrefab == null)
        {
            LogLine("[SubsystemPrinter] No boundaryMarkerPrefab assigned.");
            return;
        }

        foreach (var point in boundaryPoints)
        {
            var marker = Instantiate(boundaryMarkerPrefab, point, Quaternion.identity);
            marker.transform.localScale = Vector3.one * markerScale;
            spawnedMarkers.Add(marker);
        }
    }

    private void ClearMarkers()
    {
        foreach (var marker in spawnedMarkers)
        {
            if (marker != null)
                Destroy(marker);
        }
        spawnedMarkers.Clear();
    }

    private void LogLine(string line)
    {
        debugManager?.AddLine(line);
        Debug.Log(line);
    }
}

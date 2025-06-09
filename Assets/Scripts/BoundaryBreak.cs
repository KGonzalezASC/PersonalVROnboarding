using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR;
using UnityEngine.XR;
using UnityEngine.XR.Management;

public class SubsystemPrinterRefactored : MonoBehaviour
{
    private XRInputSubsystem _openXRInputSubsystem;

    // Assign a prefab to spawn at each boundary point
    [Tooltip("Assign a prefab to instantiate at each boundary point.")]
    public GameObject boundaryMarkerPrefab;
    

    private bool _isChecking = false;

    [SerializeField]
    private DebugTextManager debugManager;

    // Keep track of spawned markers to clear them later
    private readonly List<GameObject> _spawnedMarkers = new List<GameObject>();

    private void Awake()
    {
        InputSystem.onDeviceChange += OnDeviceChange;
        debugManager = FindFirstObjectByType<DebugTextManager>();
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
        if (_openXRInputSubsystem != null)
        {
            _openXRInputSubsystem.boundaryChanged -= OnBoundaryChanged;
            _openXRInputSubsystem.trackingOriginUpdated -= GetHeadsetPosition;
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
                _isChecking = false;
                _openXRInputSubsystem = null;
                ClearMarkers();
                break;
        }
    }

    private void StartSubsystemChecks()
    {
        if (!_isChecking)
        {
            _isChecking = true;
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
            string id = sub.subsystemDescriptor.id;
            bool running = sub.running;
            LogLine($"[XRInputSubsystem] id: {id} | running: {running}");

            if (id == "OpenXR Input")
            {
                _openXRInputSubsystem = sub;

                var supportedOrigins = _openXRInputSubsystem.GetSupportedTrackingOriginModes();
                LogLine($"    • Supported Tracking Origin Modes: {supportedOrigins}");

                var currentOrigin = _openXRInputSubsystem.GetTrackingOriginMode();
                LogLine($"    • Current Tracking Origin Mode: {currentOrigin}");

                List<Vector3> boundaryPoints = new List<Vector3>();
                bool hasBoundary = _openXRInputSubsystem.TryGetBoundaryPoints(boundaryPoints);
                LogLine($"    • Boundary supported: {hasBoundary} | point count: {boundaryPoints.Count}");

                if (boundaryPoints.Count > 0)
                {
                    SpawnMarkers(boundaryPoints);
                    for (int i = 0; i < boundaryPoints.Count; i++)
                        LogLine($"    • Boundary Point {i}: {boundaryPoints[i]}");
                }
                else
                {
                    ClearMarkers();
                }

                _openXRInputSubsystem.boundaryChanged += OnBoundaryChanged;
                _openXRInputSubsystem.trackingOriginUpdated += GetHeadsetPosition;
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


    private void OnBoundaryChanged(XRInputSubsystem inputSubsystem)
    {
        List<Vector3> updatedPoints = new List<Vector3>();
        if (inputSubsystem.TryGetBoundaryPoints(updatedPoints))
        {
            LogLine($"[BoundaryChanged] New boundary point count: {updatedPoints.Count}");
            if (updatedPoints.Count > 0)
            {
                SpawnMarkers(updatedPoints);
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
            marker.transform.localScale = Vector3.one;
            _spawnedMarkers.Add(marker);
        }
    }

    private void ClearMarkers()
    {
        foreach (var marker in _spawnedMarkers)
        {
            if (marker != null)
                Destroy(marker);
        }
        _spawnedMarkers.Clear();
    }

    private void LogLine(string line)
    {
        debugManager?.AddLine(line);
        Debug.Log(line);
    }
}


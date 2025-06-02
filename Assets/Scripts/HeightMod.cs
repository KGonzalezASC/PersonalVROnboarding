using System.Collections.Generic;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

public class HeightMarshal : MonoBehaviour
{
    [Tooltip(
      "Assign the 'UI Press' action from XRI Default Input Actions\n" +
      "→ XRI RightHand → UI Press (Quest A button)."
    )]
    [SerializeField] private InputActionReference primaryAButton;

    [Tooltip("Drag your XROrigin (Action-Based) here so vertical can be adjusted.")]
    [SerializeField] private XROrigin xrOrigin;

    private XRHMD     _xrHmd;
    private bool      _gotHmd             = false;
    private bool      _hmdWasDisconnected = false;

    private void Awake()
    {
        // Attempt to grab XRHMD immediately
        _xrHmd = InputSystem.GetDevice<XRHMD>();
        if (_xrHmd != null)
        {
            _gotHmd = true;
            Debug.Log("[HeightMarshal] XRHMD found in Awake().");
        }
        else
        {
            // Subscribe to InputSystem device changes
            InputSystem.onDeviceChange += OnDeviceChange;
        }

        // On startup, request Floor mode so that in a build the floor is correct
        RequestFloorMode();
    }

    private void OnEnable()
    {
        if (primaryAButton != null && primaryAButton.action != null)
        {
            primaryAButton.action.performed += OnAButtonPressed;
            primaryAButton.action.Enable();
        }
        else
        {
            Debug.LogWarning("HeightMarshal: A-button InputActionReference is not assigned or invalid.");
        }
    }

    private void OnDisable()
    {
        if (primaryAButton != null && primaryAButton.action != null)
        {
            primaryAButton.action.performed -= OnAButtonPressed;
            primaryAButton.action.Disable();
        }

        InputSystem.onDeviceChange -= OnDeviceChange;
    }

    // Correct signature for InputSystem.onDeviceChange
    private void OnDeviceChange(UnityEngine.InputSystem.InputDevice device, InputDeviceChange change)
    {
        if (device is XRHMD)
        {
            switch (change)
            {
                case InputDeviceChange.Removed:
                case InputDeviceChange.Disconnected:
                    _hmdWasDisconnected = true;
                    _gotHmd             = false;
                    _xrHmd              = null;
                    Debug.Log("[HeightMarshal] XRHMD was disconnected.");
                    break;

                case InputDeviceChange.Added:
                case InputDeviceChange.Reconnected:
                    _xrHmd = InputSystem.GetDevice<XRHMD>();
                    if (_xrHmd != null)
                    {
                        _gotHmd = true;
                        Debug.Log("[HeightMarshal] XRHMD reconnected & available.");
                    }
                    break;
            }
        }
    }

    private void OnAButtonPressed(InputAction.CallbackContext context)
    {
        // If the HMD was disconnected and just reconnected, recalibrate floor + adjust vertical
        if (_hmdWasDisconnected && _gotHmd)
        {
            ReinitializeFloorAndAdjustHeight();
            _hmdWasDisconnected = false;
        }

        // 1) Request/reaffirm Floor mode & optionally recenter in the Editor
        var subsystems = new List<XRInputSubsystem>();
        SubsystemManager.GetSubsystems(subsystems);

        foreach (var sub in subsystems)
        {
            if (sub.TrySetTrackingOriginMode(TrackingOriginModeFlags.Floor))
                Debug.Log($"[HeightMarshal] Requested Floor mode on {sub.subsystemDescriptor.id}");
            else
                Debug.LogWarning($"[HeightMarshal] Failed to request Floor on {sub.subsystemDescriptor.id}");

#if UNITY_EDITOR
            if (sub.TryRecenter())
                Debug.Log($"[HeightMarshal] Recentered tracking origin on {sub.subsystemDescriptor.id} (Editor only)");
            else
                Debug.LogWarning($"[HeightMarshal] Failed to recenter on {sub.subsystemDescriptor.id} (Editor only)");
#endif
        }

        // 2) Log the A-button press
        Debug.Log("A button pressed via XRI Default Input Actions");

        // 3) Read HMD devicePosition (raw device-space) and log head height
        if (_gotHmd && _xrHmd != null)
        {
            Vector3 headPos = _xrHmd.devicePosition.ReadValue();
            Debug.Log($"Head position (device space): {headPos}");
            Debug.Log($"Top of head position (actual height relative): {headPos.y + 0.14f}");
        }
        else
        {
            Debug.LogWarning("HeightMarshal: No XR HMD device found on button press.");
        }
    }

    private void RequestFloorMode()
    {
        var subsystems = new List<XRInputSubsystem>();
        SubsystemManager.GetSubsystems(subsystems);

        foreach (var sub in subsystems)
        {
            if (sub.TrySetTrackingOriginMode(TrackingOriginModeFlags.Floor))
                Debug.Log($"[HeightMarshal] Requested Floor mode on {sub.subsystemDescriptor.id}");
            else
                Debug.LogWarning($"[HeightMarshal] Failed to request Floor on {sub.subsystemDescriptor.id}");
        }
    }

    private void ReinitializeFloorAndAdjustHeight()
    {
        // 1) Re-request Floor mode & recenter if in Editor
        var subsystems = new List<XRInputSubsystem>();
        SubsystemManager.GetSubsystems(subsystems);

        foreach (var sub in subsystems)
        {
            if (sub.TrySetTrackingOriginMode(TrackingOriginModeFlags.Floor))
                Debug.Log($"[HeightMarshal] Requested Floor mode on {sub.subsystemDescriptor.id}");
            else
                Debug.LogWarning($"[HeightMarshal] Failed to request Floor on {sub.subsystemDescriptor.id}");

#if UNITY_EDITOR
            if (sub.TryRecenter())
                Debug.Log($"[HeightMarshal] Recentered tracking origin on {sub.subsystemDescriptor.id} (Editor only)");
            else
                Debug.LogWarning($"[HeightMarshal] Failed to recenter on {sub.subsystemDescriptor.id} (Editor only)");
#endif
        }

        // 2) Adjust only vertical of the XROrigin so camera is at floor level
        if (xrOrigin != null)
        {
            // Get the camera’s world Y position
            var camera = xrOrigin.Camera;
            if (camera != null)
            {
                float camWorldY = camera.transform.position.y;

                // Subtract that from the rig’s Y so the camera ends up at floor
                Vector3 rigPos = xrOrigin.transform.position;
                xrOrigin.transform.position = new Vector3(
                    rigPos.x,
                    rigPos.y - camWorldY,
                    rigPos.z
                );
                Debug.Log($"[HeightMarshal] XROrigin vertical adjusted by {-camWorldY:F3}m (camera floor-aligned).");
            }
            else
            {
                Debug.LogWarning("[HeightMarshal] xrOrigin.Camera is null; cannot adjust height.");
            }
        }
        else
        {
            Debug.LogWarning("[HeightMarshal] No XROrigin reference assigned to adjust vertical.");
        }
    }
}

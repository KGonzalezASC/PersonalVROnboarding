using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.OpenXR;


public class HeightMarshal : MonoBehaviour
{
    [Tooltip("Assign the 'UI Press' action from XRI Default Input Actions\n" + "→ XRI RightHand → UI Press (Quest A button).")]
    [SerializeField] private InputActionReference primaryAButton;
    [Tooltip("Drag your XROrigin (Action-Based) here for recenter calls.")]
    [SerializeField] private XROrigin xrOrigin;
    [SerializeField] private CharacterController characterController;

    
    [SerializeField] private float headTopOffset = 0.14f;/// Eye-level offset from tracked head to top of head (e.g. ~0.14m for Quest 3). Used only for SetAllowRecentering after first valid height.
    [SerializeField, ReadOnly] private Vector3 lastAdjustedHeight = Vector3.zero;
    [SerializeField] DebugTextManager debugManager;

    #region Private Fields

    private XRHMD _xrHmd;
    private bool _gotHmd;
    private bool _hmdWasDisconnected;
    private readonly List<XRInputSubsystem> _subsystems = new();

    #endregion

    //Adding a height accessible property to ensure that the avatar is scaled properly
    /*public float Height
    {
        get { return lastAdjustedHeight.y; }
    }*/

    private void Awake()
    {
        InputSystem.onDeviceChange += OnDeviceChange;
        SubsystemManager.GetSubsystems(_subsystems);
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
            Debug.LogWarning("[HeightMarshal] A-button InputActionReference is not assigned or invalid.");
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

    private void OnDeviceChange(UnityEngine.InputSystem.InputDevice device, InputDeviceChange change)
    {
        if (device is XRHMD)
        {
            switch (change)
            {
                case InputDeviceChange.Removed:
                case InputDeviceChange.Disconnected:
                    _hmdWasDisconnected = true;
                    _gotHmd = false;
                    _xrHmd = null;
                    break;

                case InputDeviceChange.Added:
                case InputDeviceChange.Reconnected:
                    _xrHmd = InputSystem.GetDevice<XRHMD>();
                    if (_xrHmd != null)
                        _gotHmd = true;

                    RequestFloorMode();
                    StartCoroutine(GetHeightRoutine());
                    break;
            }
        }
    }
    
    private void OnAButtonPressed(InputAction.CallbackContext context)
    {
        Debug.Log("Recentering before returning height data");
        if (_gotHmd && _xrHmd != null)
        {
            //I prefer device position over center eye position //provides raw tracking data/device space from the headset's center of mass including predictions from openxr runtime.
            Vector3 headPos = _xrHmd.devicePosition.ReadValue();
            //Debug.Log($"Head position (device space): {headPos}");
            //Debug.Log($"Top of head position (actual height relative): {headPos.y + 0.14f}");
            lastAdjustedHeight = new Vector3(headPos.x, headPos.y + 0.14f, headPos.z);
            RequestFloorMode(ref lastAdjustedHeight.y);
            ResizeCharacterController(lastAdjustedHeight.y);
        }
    }
    

    /// <summary>
    /// Request Floor mode on all XRInputSubsystems (baseline).
    /// </summary>
    private void RequestFloorMode()
    {
        foreach (var sub in _subsystems)
        {
            bool setFloor = sub.TrySetTrackingOriginMode(TrackingOriginModeFlags.Floor);
            if (setFloor)
                Debug.Log($"[HeightMarshal] Requested Floor mode (baseline) on {sub.subsystemDescriptor.id}");
            else
                Debug.LogWarning($"[HeightMarshal] Failed baseline RequestFloorMode on {sub.subsystemDescriptor.id}");
        }
    }

    /// <summary>
    /// Request Floor mode + allow OpenXR recentering, using computed height.
    /// </summary>
    private void RequestFloorMode(ref float height)
    {
        foreach (var sub in _subsystems)
        {
            bool setFloor = sub.TrySetTrackingOriginMode(TrackingOriginModeFlags.Floor);
            
            if (setFloor) {
                string message = $"[HeightMarshal] Requested Floor mode (height) on {sub.subsystemDescriptor.id}";
                //Debug.Log(message);
                LogLine(message);
            }
            else
                Debug.LogWarning($"[HeightMarshal] Failed height RequestFloorMode on {sub.subsystemDescriptor.id}");
        }

        if (height > 0.1f)
        {
            OpenXRSettings.SetAllowRecentering(true, height);
            //Debug.Log($"[HeightMarshal] OpenXRSetAllowRecentering(true, {height:F2}) called");
            LogLine($"[HeightMarshal] OpenXRSetAllowRecentering(true, {height:F2}) called");
        }
        else
        {
            OpenXRSettings.SetAllowRecentering(false, 0f);
            Debug.LogWarning("[HeightMarshal] OpenXRSetAllowRecentering disabled; height below threshold");
        }
    }
    

    private IEnumerator GetHeightRoutine()
    {
        const float minValidY = 0.1f;
        const float timeout = 5f;
        float timer = 0f;

        //Debug.Log("[HeightMarshal] Waiting for valid headset tracking...");
        yield return new WaitForEndOfFrame();

        while (_xrHmd != null && _gotHmd)
        {
            Vector3 headPos = _xrHmd.devicePosition.ReadValue();
            if (headPos.y > minValidY && !Mathf.Approximately(headPos.y, 0f))
            {
                float adjustedY = headPos.y + headTopOffset;
                lastAdjustedHeight = new Vector3(headPos.x, adjustedY, headPos.z);

                //Debug.Log($"[HeightMarshal] First valid head position (device space): {headPos}");
                //Debug.Log($"[HeightMarshal] Estimated true head height (device + offset): {adjustedY:F3}m");

                RequestFloorMode(ref adjustedY);
                ResizeCharacterController(adjustedY);
                AlignRigToSurfaceBelow();
                yield break;
            }

            if (timer > timeout)
            {
                Debug.LogWarning("[HeightMarshal] Timed out waiting for valid headset tracking.");
                yield break;
            }

            timer += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
    }
    
    /// <summary>
    /// Resize the CharacterController capsule so its height matches the user’s “true head height.”
    /// Also adjust the center to half the new height so the bottom sits on the floor.
    /// </summary>
    /// <param name="trueHeadHeight">Head-top height in meters (devicePosition.y + offset).</param>
    private void ResizeCharacterController(float trueHeadHeight)
    {
        if (characterController == null)
        {
            if (xrOrigin != null)
                characterController = xrOrigin.GetComponentInChildren<CharacterController>();
        }

        if (characterController != null)
        {
            // Constrain the minimum and maximum heights if desired (e.g., clamp between 1.2m and 2.5m)
            float newHeight = Mathf.Clamp(trueHeadHeight, 1.2f, 2.5f);

            // Apply the height
            characterController.height = newHeight;

            // Center the capsule so its bottom touches the floor (centerY = height/2)
            Vector3 newCenter = characterController.center;
            newCenter.y = newHeight / 2f;
            characterController.center = newCenter;

            //Debug.Log($"[HeightMarshal] CharacterController resized: Height={newHeight:F3}m, CenterY={newCenter.y:F3}m.");
            LogLine($"[HeightMarshal] CharacterController resized: Height={newHeight:F3}m, CenterY={newCenter.y:F3}m.");
        }
        else
        {
            Debug.LogWarning("[HeightMarshal] No CharacterController found; cannot resize capsule.");
        }
    }
    

    /// <summary>
    /// Cast a ray down from the CharacterController’s capsule center (in world space) to find the floor below.
    /// Then shift the XR Origin so that the bottom of the capsule (feet) sits exactly on that floor point.
    /// </summary>
    public void AlignRigToSurfaceBelow()
    {
        if (characterController == null)
        {
            // Make sure we have a reference to the CharacterController under XROrigin
            if (xrOrigin != null)
                characterController = xrOrigin.GetComponentInChildren<CharacterController>();
        }

        if (characterController == null)
        {
            Debug.LogWarning("[HeightMarshal] No CharacterController found; cannot align to surface below.");
            return;
        }

        // 1) Compute the capsule’s world‐space center:
        Vector3 ccCenterWorld = characterController.transform.TransformPoint(characterController.center);

        // 2) Raycast downward from that world‐space center:
        if (Physics.Raycast(ccCenterWorld, Vector3.down, out RaycastHit hitInfo, 5f))
        {
            // 3) Figure out where the capsule’s “feet” currently are in world space:
            float feetWorldY = ccCenterWorld.y - (characterController.height * 0.5f);

            // 4) Compute how much we need to move the XR Origin so feetWorldY == hitInfo.point.y
            float deltaY = hitInfo.point.y - feetWorldY;

            // 5) Shift the XR Origin by that delta:
            Vector3 rigPos = xrOrigin.transform.position;
            xrOrigin.transform.position = new Vector3(rigPos.x, rigPos.y + deltaY, rigPos.z);

            //Debug.Log($"[HeightMarshal] Aligned rig so capsule feet sit on Y={hitInfo.point.y:F3}. Moved root by ΔY={deltaY:F3}m.");
        }
        else
        {
            Debug.LogWarning("[HeightMarshal] No surface detected below CharacterController within 5m.");
        }
    }
    
    private void LogLine(string line)
    {
        debugManager?.AddLine(line);
        //Debug.Log(line);
    }
}


using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR;
using Unity.XR.CoreUtils;

[RequireComponent(typeof(CharacterController))]
public class BodyMover : MonoBehaviour
{
    [Tooltip("Reference to the CharacterController on this GameObject")]
    [SerializeField] private CharacterController cc;

    [Tooltip("Assign your XROrigin (Action-Based or Device-Based) here")]
    [SerializeField] private XROrigin xrOrigin;

    [Tooltip("Minimum HMD forward/backward speed (m/s) to count as a step")]
    [SerializeField] private float stepThreshold = 0.2f;

    [Tooltip("Multiplier to convert headset velocity into CharacterController motion")]
    [SerializeField] private float moveSpeedMultiplier = 1f;

    private XRHMD xrHmd;

    private void Awake()
    {
        if (cc == null)
            cc = GetComponent<CharacterController>();

        xrHmd = InputSystem.GetDevice<XRHMD>();
        if (xrHmd == null)
            Debug.LogWarning("[BodyMover] No XRHMD device found on Awake.");
    }

    private void OnEnable()
    {
        InputSystem.onDeviceChange += OnDeviceChange;
    }

    private void OnDisable()
    {
        InputSystem.onDeviceChange -= OnDeviceChange;
    }

    private void OnDeviceChange(InputDevice device, InputDeviceChange change)
    {
        if (device is XRHMD && 
            (change == InputDeviceChange.Added || change == InputDeviceChange.Reconnected))
        {
            xrHmd = (XRHMD)device;
        }
        else if (device is XRHMD && 
                 (change == InputDeviceChange.Removed || change == InputDeviceChange.Disconnected))
        {
            xrHmd = null;
        }
    }

    /*private void FixedUpdate()
    {
        if (xrHmd == null || xrOrigin == null)
            return;

        // Read the raw headset velocity (world-space m/s)
        Vector3 worldVel = xrHmd.device.v

        // Transform world velocity into the camera's local space
        Transform camT = xrOrigin.Camera.transform;
        Vector3 localVel = camT.InverseTransformDirection(worldVel);

        // We only care about horizontal movement (forward/back on z)
        float forwardSpeed = localVel.z;

        // If forward/backward speed exceeds threshold, move the CharacterController
        if (Mathf.Abs(forwardSpeed) > stepThreshold)
        {
            // Project the camera's forward onto the horizontal plane
            Vector3 forwardDir = camT.forward;
            forwardDir.y = 0f;
            forwardDir.Normalize();

            // Use the headset's forward speed as a base and apply multiplier
            Vector3 move = forwardDir * (forwardSpeed * moveSpeedMultiplier) * Time.fixedDeltaTime;

            cc.Move(move);

            // Align the CharacterController's center under the camera horizontally
            Vector3 camLocalPos = transform.InverseTransformPoint(camT.position);
            cc.center = new Vector3(camLocalPos.x, cc.height / 2f + cc.skinWidth, camLocalPos.z);
        }
        else
        {
            // Even if not moving, keep CC centered under the camera
            Vector3 camLocalPos = transform.InverseTransformPoint(camT.position);
            cc.center = new Vector3(camLocalPos.x, cc.height / 2f + cc.skinWidth, camLocalPos.z);
        }
    }*/
}

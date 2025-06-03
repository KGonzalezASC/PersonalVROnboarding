using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

public class RecenterOrigin : MonoBehaviour
{
    [SerializeField] private InputActionReference secondaryButton;
    public Transform target;

    private XROrigin _xrOrigin;
    private Camera   _mainCam;

    private void Awake()
    {
        _xrOrigin = GetComponent<XROrigin>();
        _mainCam  = Camera.main;  // assume your XR camera is the “MainCamera”
    }

    private void OnEnable()
    {
        if (secondaryButton?.action != null)
        {
            secondaryButton.action.performed += OnBButtonPressed;
            secondaryButton.action.Enable();
        }
    }

    private void OnDisable()
    {
        if (secondaryButton?.action != null)
        {
            secondaryButton.action.performed -= OnBButtonPressed;
            secondaryButton.action.Disable();
        }
    }

    private void OnBButtonPressed(InputAction.CallbackContext ctx)
    {
        RecenterXZ();
    }

    //i want to fix the y a different way
    private void RecenterXZ()
    {
        // 1) Read the camera’s current world‐Y:
        float currentCamY = _mainCam.transform.position.y;

        // 2) Build a “desired head position” that uses target.X,Z but keeps camera’s Y:
        Vector3 desiredHeadPos = new Vector3(
            target.position.x,
            currentCamY,
            target.position.z
        );

        // 3) MoveCameraToWorldLocation will shift only horizontally if Y is unchanged
        _xrOrigin.MoveCameraToWorldLocation(desiredHeadPos);

        // 4) (Optional) If you want to preserve orientation, skip MatchOriginUpCameraForward.
        //    Otherwise, you could still call MatchOriginUpCameraForward if you want to match target’s up/forward.
    }
}
using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class Resize : MonoBehaviour
{
    public InputActionReference resizeAction;

    [SerializeField] private GameObject playerAvatar;
    [SerializeField] private CharacterController characterController;
    [SerializeField] private Transform leftController;
    [SerializeField] private Transform rightController;
    
    [SerializeField] private Transform RightShoulder, RightUpperArm, RightLowerArm, RightHand;
    [SerializeField] private Transform LeftShoulder, LeftUpperArm, LeftLowerArm, LeftHand;

    private float heightScale;
    private float defaultHeight = 1.0f;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        resizeAction.action.Enable();
        resizeAction.action.performed += OnResize;
        resizeAction.action.performed += ResizeArms;
    }

    private void OnDestroy()
    {
        resizeAction.action.Disable();
        resizeAction.action.performed -= OnResize;
        resizeAction.action.performed -= ResizeArms;
    }
    
    //Resizes the avatar based on the height of the character controller
    void OnResize(InputAction.CallbackContext ctx)
    {
        heightScale = defaultHeight / characterController.height;
        playerAvatar.transform.localScale = Vector3.one * heightScale;
    }
    
    //Repositiong the joints based on the distance from the controllers to the shoulders 
    void ResizeArms(InputAction.CallbackContext ctx)
    {
        float leftArmLength = Vector3.Distance(leftController.position, LeftShoulder.position);
        Vector3 leftDirection = (leftController.position - LeftShoulder.position).normalized;
        float rightArmLength = Vector3.Distance(rightController.position, RightShoulder.position);
        Vector3 rightDirection = (rightController.position - RightShoulder.position).normalized;
        
        LeftUpperArm.position = LeftShoulder.position + leftDirection * (leftArmLength * 0.33f);
        LeftLowerArm.position = LeftShoulder.position + leftDirection * (leftArmLength * 0.66f);
        //LeftUpperArm.position = LeftShoulder.position + leftDirection * (leftArmLength * 0.33f);
        
        RightUpperArm.position = LeftShoulder.position + leftDirection * (leftArmLength * 0.33f);
        RightLowerArm.position = LeftShoulder.position + leftDirection * (leftArmLength * 0.66f);
        //LeftUpperArm.position = LeftShoulder.position + leftDirection * (leftArmLength * 0.33f);
    }
}

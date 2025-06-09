using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class Resize : MonoBehaviour
{
    public InputActionReference resizeAction;

    [SerializeField] private GameObject playerAvatar;
    //[SerializeField] private CharacterController characterController;
    [SerializeField] private Transform cameraHeight;        //player's eye level in the game
    [SerializeField] private Transform leftController;
    [SerializeField] private Transform rightController;
    
    [SerializeField] private Transform RightShoulder, RightUpperArm, RightLowerArm, RightHand;
    [SerializeField] private Transform LeftShoulder, LeftUpperArm, LeftLowerArm, LeftHand;

    private float heightScale;
    private float defaultHeight = 1.83f;
    public bool isManualResizing = false;

    private float defaultArm = 0.584f;  //model's arm length
    public float neckLength = 0.35f;        //average size for women is 0.35m and 0.41m for men
    
    
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
        heightScale = cameraHeight.position.y/ defaultHeight;
        playerAvatar.transform.localScale = new Vector3(heightScale, heightScale, heightScale);;
    }
    
    //scales arms based on controller's distance
    void ResizeArms(InputAction.CallbackContext ctx)
    {
        float actualLeftArmLength = Vector3.Distance(LeftShoulder.position, leftController.position);
        float actualRightArmLength = Vector3.Distance(RightShoulder.position, rightController.position);

        float leftArmRatio = actualLeftArmLength / defaultArm;
        float rightArmRatio = actualRightArmLength / defaultArm;

        // Only scale upper and lower arm bones
        LeftUpperArm.localScale = Vector3.one * leftArmRatio;
        LeftLowerArm.localScale = Vector3.one * leftArmRatio;

        RightUpperArm.localScale = Vector3.one * rightArmRatio;
        RightLowerArm.localScale = Vector3.one * rightArmRatio;
    }

    
}

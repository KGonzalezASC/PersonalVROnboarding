using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class Resize : MonoBehaviour
{
    public InputActionReference resizeAction;

    [SerializeField] private GameObject playerAvatar;
    //[SerializeField] private CharacterController characterController;
    [SerializeField] private Transform cameraHeight;
    [SerializeField] private Transform leftController;
    [SerializeField] private Transform rightController;
    
    [SerializeField] private Transform RightShoulder, RightUpperArm, RightLowerArm, RightHand;
    [SerializeField] private Transform LeftShoulder, LeftUpperArm, LeftLowerArm, LeftHand;

    private float heightScale;
    private float defaultHeight = 1.83f;

    private float defaultArm = 0.584f;
    
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        resizeAction.action.Enable();
        resizeAction.action.performed += OnResize;
       // resizeAction.action.performed += ResizeArms;
    }

    private void OnDestroy()
    {
        resizeAction.action.Disable();
        resizeAction.action.performed -= OnResize;
        //resizeAction.action.performed -= ResizeArms;
    }
    
    //Resizes the avatar based on the height of the character controller
    void OnResize(InputAction.CallbackContext ctx)
    {
        heightScale = cameraHeight.position.y/ defaultHeight;
        playerAvatar.transform.localScale = new Vector3(heightScale, heightScale, heightScale);;
    }
    
    //Repositiong the joints based on the distance from the controllers to the shoulders - joints does not move
    void ResizeArms(InputAction.CallbackContext ctx)
    {
        float armLength = Vector3.Distance(leftController.position, LeftShoulder.position);
        float armScale = armLength / defaultArm;
        
        
        Vector3 direction = (LeftLowerArm.position - leftController.position).normalized;
        Vector3 upperLength = LeftShoulder.position + direction * (armLength * 0.33f);
        Vector3 lowerLength = LeftUpperArm.position + direction * (armLength * 0.33f);

        float upperScale = armScale;
        float lowerScale =  armScale;

        LeftUpperArm.localScale = Vector3.one * armScale;
        LeftLowerArm.localScale = Vector3.one * armScale;
        
        RightUpperArm.localScale = Vector3.one * upperScale;
        RightLowerArm.localScale = Vector3.one * lowerScale;
    }
}

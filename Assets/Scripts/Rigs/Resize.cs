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
        //resizeAction.action.performed += ResizeArms;
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
    
    //basically, using trigo to calculate the arm length of the user and then resizing it with the 
    //model's arm length by the unit to make sure that it is properly scaled up
    void ResizeArms(InputAction.CallbackContext ctx)
    {
        //perpendicular to the left controller
        float headsetToShoulderDistance = Vector3.Distance(cameraHeight.position, LeftShoulder.position);
        float headsetToControllerDistance = neckLength; //Vector3.Distance(cameraHeight.position, leftController.position);
        float angle = Mathf.Asin(headsetToShoulderDistance / headsetToControllerDistance);
        float actualArmLength = headsetToControllerDistance * Mathf.Cos(angle);     //computes the actual length
        
        /*float armRatio = actualArmLength / defaultArm;  //finds the ratio
        RightShoulder.localScale = new Vector3(armRatio, armRatio, armRatio);
        RightUpperArm.localScale = new Vector3(armRatio, armRatio, armRatio);
        RightLowerArm.localScale = new Vector3(armRatio, armRatio, armRatio);
        
        //LeftShoulder.localScale = new Vector3(armRatio, armRatio, armRatio);
        LeftUpperArm.localScale = new Vector3(armRatio, armRatio, armRatio);
        LeftLowerArm.localScale = new Vector3(armRatio, armRatio, armRatio);*/
    }

    
}

using UnityEngine;
using UnityEngine.InputSystem;

public class Resize : MonoBehaviour
{
    public InputActionReference resizeAction;
    HeightMarshal heightMarshal;

    [SerializeField] private GameObject XRInteractionManager;
    [SerializeField] private Transform cameraHeight;        //player's eye level in the game
    [SerializeField] private Transform controller;
    [SerializeField] private CharacterController characterController;
    [SerializeField] private Transform shoulder, upperArm, lowerArm, hand;

    private float heightScale;
    private float defaultHeight = 1.78f;
    private float defaultArm = 0.71f;
    private float playerHeight = 1.0f;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        heightMarshal = XRInteractionManager.GetComponent<HeightMarshal>();
        //playerHeight = heightMarshal.Height;        //to get the accurate height
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
        heightScale = characterController.height / defaultHeight;
        this.transform.localScale = Vector3.one * heightScale;
       // playerHeight = heightMarshal.Height;
        Debug.LogWarning("Player height (CAM): " + cameraHeight.position+" , character controller's height: "+ characterController.height+", height marshall: "+ playerHeight);
    }
    
    //scales arms based on controller's distance
    void ResizeArms(InputAction.CallbackContext ctx)
    {
        //Uses similar logic to height
        float playerArmsDistance = Vector3.Distance(controller.position, shoulder.position);
        float armScale = playerArmsDistance / defaultArm;

        shoulder.localScale = upperArm.localScale = lowerArm.localScale = Vector3.one * armScale;
    }
}

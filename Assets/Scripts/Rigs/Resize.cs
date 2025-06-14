using UnityEngine;
using UnityEngine.InputSystem;

//Attach it on the controllers
public class Resize : MonoBehaviour
{
    public InputActionReference resizeAction;
    HeightMarshal heightMarshal;

    [SerializeField] private GameObject XRInteractionManager;
    [SerializeField] private Transform cameraHeight;        //player's eye level in the game
    [SerializeField] private Transform controller;
    [SerializeField] private CharacterController characterController;
    [SerializeField] private Transform shoulder, upperArm, lowerArm, hand, middleFinger;

    private float heightScale;
    private float defaultHeight = 1.78f;
    private float defaultArm = 0.71f;
    private float playerHeight = 1.0f;

    float headUnit = 10.0f;          
    const float upperArmRatio = 1.4f;
    const float lowerArmRatio = 1.7f;
    
    
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
        float actualArmLength = Vector3.Distance(shoulder.position, controller.position);
        float modelLength = Vector3.Distance(shoulder.position, middleFinger.position);

        headUnit = actualArmLength / (upperArmRatio + lowerArmRatio);

        //target lengths - based on player
        float targetUpper = headUnit * upperArmRatio;
        float targetLower = headUnit * lowerArmRatio;

        //get the model lengths- using the distance to get accurate information
        float modelUpper = modelLength * upperArmRatio;
        float modelLower = modelLength * lowerArmRatio;

        //scale factors to make sure it resizes properly
        float upperScale = targetUpper/modelUpper;
        float lowerScale = targetLower/modelLower;

        //upperArm.localScale = new Vector3(1, upperScale, 1);
        //lowerArm.localScale = new Vector3(1, lowerScale, 1);

        Debug.Log("actual arm distance: "+ actualArmLength  +
            ", model arm: " + modelLength+
            ", upper arm scale: " + upperArm.localScale +
            ", lower arm scale: " + lowerArm.localScale +
            ", head unit: " + headUnit);
        Debug.Log("Model upper length: " + modelUpper + ", Target upper length: " + targetUpper);
        Debug.Log("Upper scale: " + upperScale);

    }
}

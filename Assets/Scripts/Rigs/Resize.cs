using UnityEngine;
using UnityEngine.InputSystem;

//Attach it on the controllers
public class Resize : MonoBehaviour
{
    public InputActionReference resizeAction;

    [SerializeField] private GameObject playerAvatar;
    [SerializeField] private Transform cameraHeight;        //player's eye level in the game
    [SerializeField] private Transform controller;
    
    [SerializeField] private Transform shoulder, upperArm, lowerArm, hand, middleFinger;

    private float heightScale;
    private float defaultHeight = 1.83f;

    private float defaultArm = 0.584f;      //model's arm length
    public float headUnit = 10.0f;          //average head unit

    const float upperArmRatio = 1.4f;
    const float lowerArmRatio = 1.7f;
    
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        resizeAction.action.Enable();
        //resizeAction.action.performed += OnResize;
        resizeAction.action.performed += AdjustArmLength;
    }

    private void OnDestroy()
    {
        resizeAction.action.Disable();
        //resizeAction.action.performed -= OnResize;
        resizeAction.action.performed -= AdjustArmLength;
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

    void AdjustArmLength(InputAction.CallbackContext ctx)
    {
        float totalArmLength = Vector3.Distance(shoulder.position, controller.position);
        float upperRatio = 0.45f;
        float lowerRatio = 0.55f;

        float upperLength = totalArmLength * upperRatio;
        float lowerLength = totalArmLength * lowerRatio;

        Debug.Log("Bones before: upper arm: " + upperArm.localPosition+
            ", lower arm: "+ lowerArm.localPosition+
            ", hand: "+ hand.localPosition);

        // Adjust lowerArm position relative to upperArm
        Vector3 dirUpper = (lowerArm.position - upperArm.position).normalized;
        lowerArm.position = upperArm.position + dirUpper * upperLength;

        // Adjust hand position relative to lowerArm
        Vector3 dirLower = (hand.position - lowerArm.position).normalized;
        hand.position = lowerArm.position + dirLower * lowerLength;

        Debug.Log("Bones after: upper arm: " + upperArm.localPosition +
          ", lower arm: " + lowerArm.localPosition +
          ", hand: " + hand.localPosition);
    }
}

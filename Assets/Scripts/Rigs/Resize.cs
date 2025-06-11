using UnityEngine;
using UnityEngine.InputSystem;

//Attach it on the controllers
public class Resize : MonoBehaviour
{
    public InputActionReference resizeAction;

    [SerializeField] private GameObject playerAvatar;
    [SerializeField] private Transform cameraHeight;        //player's eye level in the game
    [SerializeField] private Transform controller;
    
    [SerializeField] private Transform shoulder, upperArm, lowerArm, hand;

    private float heightScale;
    private float defaultHeight = 1.83f;

    private float defaultArm = 0.584f;      //model's arm length
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
        float actualArmLength = Vector3.Distance(shoulder.position, controller.position);
        float armRatio = actualArmLength / defaultArm;

        upperArm.localScale = Vector3.one * armRatio;
        lowerArm.localScale = Vector3.one * armRatio;
    }

    
}

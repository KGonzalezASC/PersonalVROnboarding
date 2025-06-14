using UnityEngine;
using UnityEngine.InputSystem;

//Rescales arms based on controller and the mesh's shoulder distance
public class ArmsScalar : MonoBehaviour
{
    [SerializeField] private InputActionReference resizeAction;
    [SerializeField] private Transform shoulder, upperArm, lowerArm;
    [SerializeField] private Transform controller;  // left or right hand
    [SerializeField] private GameObject hint;       //to control joints placing
    public bool isLeft = false;                     //to fix the coordinate placing
    public float hintDistance = 0.3f;
    private float defaultArmLength = 0.71f;

    void OnEnable()
    {
        if (resizeAction != null)
            resizeAction.action.performed += ResizeArms;
    }

    void OnDisable()
    {
        if (resizeAction != null)
            resizeAction.action.performed -= ResizeArms;
    }

    //Rescales arms and moves the joint- hint does not move as expected
    void ResizeArms(InputAction.CallbackContext ctx)
    {
        float currentLength = Vector3.Distance(controller.position, shoulder.position);
        float scaleFactor = currentLength / defaultArmLength;
        // Offset the hint from shoulder position along the local right axis
        //float offsetAmount = currentLength / 2.0f * (isLeft ? -1f : 1f);
        Vector3 offsetDir = isLeft ? -shoulder.right : shoulder.right;

        shoulder.localScale = upperArm.localScale = lowerArm.localScale = Vector3.one * scaleFactor;
        hint.transform.position = shoulder.position + offsetDir * hintDistance;
    }
}

using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem.Controls;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class Door : MonoBehaviour
{
    [SerializeField] Rigidbody rb;
    [SerializeField] Rigidbody connectedRB;     //by what should the hinge rotate by
    [SerializeField] XRGrabInteractable grabInteractable;
    //[SerializeField] HingeJoint hingeJoint;

    bool canRotate = false;
    public float speed = 1.0f;
    public Transform hingePoint;    //at this point, door will rotate
    float zInitial = 0.0f;
    public const float doorWindowOffset = 0.15f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        zInitial = this.transform.position.z;
        //To move the door like a slider 
        rb.constraints = RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionY;
        rb.isKinematic = true;                      //just for sliding
        rb.detectCollisions = true;
        grabInteractable.trackRotation = false;     //disables rotation
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        //checks for the difference between every frame and sees if the difference is less than 0.01 
        //yes, then makes the hinge joint at that point
        if(canRotate)
        {
            //makes the hinge and adds it the door
            HingeJoint hingeJoint = this.AddComponent<HingeJoint>();
            hingeJoint.anchor = new Vector3 (0, 1f, 0);
            hingeJoint.axis = Vector3.up;
            hingeJoint.enableCollision = true;
            canRotate = false;
        }
    }

    //For sliding the door
    public void SlideDoor(SelectExitEventArgs args)
    {
        float distance = hingePoint.position.z - this.transform.position.z;
        if(distance < doorWindowOffset)
        {
            rb.isKinematic = false;
            grabInteractable.trackRotation = true;
            grabInteractable.trackPosition = false;
            rb.constraints &= ~(RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionY);
            canRotate = true;
            Debug.Log("is locked");
        }
        Debug.Log("distance: "+ distance);
    }
}

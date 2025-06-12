using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem.Controls;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class Door : MonoBehaviour
{
    private enum DoorState { Sliding, Hinged}
    private DoorState currentState = DoorState.Sliding;

    [SerializeField] Rigidbody rb;
    [SerializeField] Rigidbody connectedRB;     //by what should the hinge rotate by
    [SerializeField] XRGrabInteractable grabInteractable;

    bool canRotate = false;
    public Transform hingePoint;    //at this point, door will rotate
    public float doorWindowOffset = 0.3f;
    
    //hinge values
    public float hingeMin = -90f;
    public float hingeMax = 180f;

    private IXRSelectInteractor currentInteractor;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //To move the door like a slider 
        rb.constraints = RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionY;
       // rb.isKinematic = true;                      //just for sliding
        rb.detectCollisions = true;
        grabInteractable.trackRotation = false;     //disables rotation
    }

    //Get the direction of where the controller is pushed from the user


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

            CreateHinge();
            Debug.Log("is locked");
        }
        Debug.Log("distance: "+ distance);
    }

    //To get the user's controller's direction
    public void OnGrab(SelectEnterEvent args)
    {
        ///currentInteractor = args.
    }

    /// <summary>
    /// Destroys hinge from the gameobject
    /// </summary>
    void RemoveHinge()
    {
        HingeJoint hinge = GetComponent<HingeJoint>();
        if(hinge != null)
        {
            Destroy(GetComponent<HingeJoint>());
        }
    }

    /// <summary>
    /// To ensure that the hinge is created just once
    /// </summary>
    void CreateHinge()
    {
        if (!TryGetComponent<HingeJoint>(out _))
        {
            HingeJoint hingeJoint = gameObject.AddComponent<HingeJoint>();
            hingeJoint.connectedBody = connectedRB;
            hingeJoint.anchor = new Vector3(0, 1f, 0);
            hingeJoint.axis = Vector3.up;
            hingeJoint.enableCollision = true;

            JointLimits jointLimits = hingeJoint.limits;
            jointLimits.min = hingeMin;
            jointLimits.max = hingeMax;
            hingeJoint.useLimits = true;
            hingeJoint.limits = jointLimits;
        }
    }
}

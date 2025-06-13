using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem.Controls;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class Door : MonoBehaviour
{
    private enum DoorState { Sliding, Hinged }
    private DoorState currentState = DoorState.Sliding;

    [SerializeField] Rigidbody rb;
    [SerializeField] Rigidbody connectedRB;     //by what should the hinge rotate by
    [SerializeField] XRGrabInteractable grabInteractable;
    [SerializeField] BoxCollider windowBox;

    public Transform hingePoint;    //at this point, door will rotate
    public float doorWindowOffset = 0.3f;

    //hinge values
    public float hingeMin = -180.0f;
    public float hingeMax = 90f;
    float angleOffset = 1.0f;
    //if the angle difference is less than this then destroy hinge and put on the slider

    private IXRSelectInteractor currentInteractor;
    private Vector3 lastHandPosition;
    private Vector3 controllerVelocity;

    void Start()
    {
        //To move the door like a slider 
        rb.constraints = RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionY;
        // rb.isKinematic = true;                      //just for sliding
        rb.detectCollisions = true;
        grabInteractable.trackRotation = false;     //disables rotation
    }

    //Get the direction of where the controller is pushed from the user
    public void Update()
    {
    }

    //For sliding the door
    public void SlideDoor(SelectEnterEventArgs args)
    {
        if (this.transform.position.z - hingePoint.position.z <= 0.3f)
        {
            rb.isKinematic = false;
            grabInteractable.trackRotation = true;
            grabInteractable.trackPosition = false;
            rb.constraints &= ~(RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionY);

            CreateHinge();
        }
    }

    public void MaxPosition(SelectExitEventArgs args)
    {
        //To avoid it sliding forward
        if (this.transform.position.z - this.hingePoint.position.z >= doorWindowOffset)
        {
            this.rb.MovePosition(this.hingePoint.position);
        }
    }

    /// <summary>
    /// Destroys it from the gameobject
    /// </summary>
    void RemoveHinge()
    {
        HingeJoint hinge = GetComponent<HingeJoint>();
        if (hinge != null)
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
            hingeJoint.anchor = new Vector3(0, -1f, 0);
            hingeJoint.axis = Vector3.up;
            hingeJoint.enableCollision = true;

            JointLimits jointLimits = hingeJoint.limits;
            jointLimits.min = hingeMin;
            jointLimits.max = hingeMax;
            hingeJoint.useLimits = true;
            hingeJoint.extendedLimits = true;
            hingeJoint.limits = jointLimits;
        }
    }
}

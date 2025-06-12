using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class Door : MonoBehaviour
{
    [SerializeField] Rigidbody rb;
    [SerializeField] XRGrabInteractable grabInteractable;
    [SerializeField] HingeJoint joint;

    public float speed = 1.0f;
    public Transform hingePoint;    //at this point, door will rotate
    float zInitial = 0.0f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        zInitial = this.transform.position.z;
        //To move the door like a slider 
        rb.constraints = RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionY;
        //disables rotation
        grabInteractable.trackRotation = false;
        
    }

    // Update is called once per frame
    void Update()
    {
        //Once the distance is more than the Z sliding point
        if(this.transform.position.z >= hingePoint.position.z)
        {
            //removes the constraint
            this.transform.position = hingePoint.position;
            rb.constraints &= ~(RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionY);
            rb.constraints |= RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

            grabInteractable.trackRotation = true;  //enables rotation for hinges
        }
        /*else
        {
            rb.constraints |= RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionY;
            rb.constraints &= ~(RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ);

            grabInteractable.trackRotation = false;  //disables rotation for hinges
        }*/

        //to ensure that the user cannot pull the door away from the hinge
        if (this.transform.position.z == hingePoint.position.z)
        {
            this.transform.position = hingePoint.position;
        }

        //to avoid going backwards
        if(this.transform.position.z <= zInitial)
        {
            this.transform.position = new Vector3(this.transform.position.x, this.transform.position.y, zInitial); 
        }
    }
}

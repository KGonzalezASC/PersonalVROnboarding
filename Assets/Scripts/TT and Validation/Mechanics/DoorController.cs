using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Transformers;

public class DoorController : MonoBehaviour
{
    [SerializeField] GameObject slidingDoor;
    [SerializeField] GameObject hingedDoor;
    [SerializeField] GameObject slideHandle;
    [SerializeField] GameObject hingedHandle;

    [SerializeField] XRGeneralGrabTransformer grabTransformer;
    [SerializeField] XRGrabInteractable grabInteractable;
    public float offset = 0.1f;

    private Vector3 hingedDoorCoordinates;  //to decide when to disable/enable sliding door
    private bool isSliding = true;

    void OnEnable()
    {
        grabInteractable.selectEntered.AddListener(OnGrabStarted);
    }

    void OnDisable()
    {
        grabInteractable.selectEntered.RemoveListener(OnGrabStarted);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        grabTransformer = GetComponent<XRGeneralGrabTransformer>();

        hingedDoorCoordinates = hingedDoor.transform.position;  //saves values
        hingedDoor.SetActive(!isSliding);   //disables hinged door
    }

    // Update is called once per frame
    void Update()
    {
        if(hingedDoorCoordinates.z - slidingDoor.transform.position.z <= offset)
        {
            isSliding = false;
            slidingDoor.SetActive(isSliding);
            hingedDoor.SetActive(!isSliding);
        }
    }

    private void OnGrabStarted(SelectEnterEventArgs args)
    {
        if (isSliding)
        {
            grabTransformer.permittedDisplacementAxes = XRGeneralGrabTransformer.ManipulationAxes.Z;
            grabInteractable.trackPosition = true;
            grabInteractable.trackRotation = false;
            grabInteractable.attachTransform = slideHandle.transform;
        }
        else
        {
            grabTransformer.permittedDisplacementAxes = XRGeneralGrabTransformer.ManipulationAxes.All;
            grabInteractable.trackPosition = false;
            grabInteractable.trackRotation = true;
            grabInteractable.attachTransform = hingedHandle.transform;

           // grabInteractable.attachTransform.position = args.interactorObject.transform.position;
           // grabInteractable.attachTransform.rotation = args.interactorObject.transform.rotation;

            // Make sure the Rigidbody is dynamic and not constrained
            var rb = hingedDoor.GetComponent<Rigidbody>();
            rb.isKinematic = false;
            rb.constraints = RigidbodyConstraints.None;
        }
    }


}

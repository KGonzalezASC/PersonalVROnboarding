using UnityEngine;

[System.Serializable]
public class VRMap2
{
    public Transform vrTarget;
    public Transform ikTarget;
    public Vector3 trackingPositionOffset;
    public Vector3 trackingRotationOffset;

    //Maps the IK to the VR target positions including the offsets
    public void Map()
    {
        ikTarget.position = vrTarget.TransformPoint(trackingPositionOffset);
        ikTarget.rotation = vrTarget.rotation * Quaternion.Euler(trackingRotationOffset);     //maps the offset
    }
}

public class RigController : MonoBehaviour
{
    //What does this do? - helps in motion sickness maybe?
    [Range(0, 1)]
    public float turnSmoothness = 0.1f;
    public VRMap2 head;
    public VRMap2 leftHand;
    public VRMap2 rightHand;

    public Vector3 headBodyOffset;
    public float headYawOffset; //? what does yaw do?

    void LateUpdate()
    {
        //Moves the mesh accordingly to make sure it looks good 
        transform.position = head.ikTarget.position;
        float yaw = head.vrTarget.eulerAngles.y;
        transform.rotation = Quaternion.Lerp(transform.rotation, 
            Quaternion.Euler(transform.eulerAngles.x, yaw, transform.eulerAngles.z), turnSmoothness);

        //Maps the controller
        head.Map();
        rightHand.Map();
        leftHand.Map();
    }
}

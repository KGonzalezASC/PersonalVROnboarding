using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// Works but is very jittery when the headset moves
public class IKFootSolver : MonoBehaviour
{
    [Header("References")]
    [SerializeField] LayerMask terrainLayer;
    [SerializeField] Transform body;
    [SerializeField] Transform headset;
    [SerializeField] IKFootSolver otherFoot;

    [Header("Stepping Settings")]
    [SerializeField] float speed = 4;
    [SerializeField] float stepDistance = 0.2f;
    [SerializeField] float stepLength = 0.2f;
    [SerializeField] float sideStepLength = 0.1f;
    [SerializeField] float stepHeight = 0.3f;

    [Header("Offsets")]
    [SerializeField] Vector3 footOffset = default;
    [SerializeField] public Vector3 footRotOffset;
    [SerializeField] public float footYPosOffset = 0.1f;
    [SerializeField] float rayStartYOffset = 0;
    [SerializeField] float rayLength = 1.5f;

    private float footSpacing;
    private Vector3 oldPosition, currentPosition, newPosition;
    private Vector3 oldNormal, currentNormal, newNormal;
    private float lerp = 1f;

    private Vector3 lastHeadsetPos;
    public bool isMovingForward;

    private void Start()
    {
        footSpacing = transform.localPosition.x;
        currentPosition = newPosition = oldPosition = transform.position;
        currentNormal = newNormal = oldNormal = transform.up;
        lastHeadsetPos = headset.position;
    }

    private void LateUpdate()
    {
        // Position the foot
        transform.position = currentPosition + Vector3.up * footYPosOffset;
        transform.localRotation = Quaternion.Euler(footRotOffset);

        // Move the body (pelvis) under the headset (XZ only)
        Vector3 headsetFlat = new Vector3(headset.position.x, body.position.y, headset.position.z);
        body.position = headsetFlat;

        // Rotate body toward headset forward direction
        Vector3 flatForward = Vector3.ProjectOnPlane(headset.forward, Vector3.up).normalized;
        body.forward = Vector3.Lerp(body.forward, flatForward, Time.deltaTime * 5f);

        // Calculate headset speed
        Vector3 headVelocity = (headset.position - lastHeadsetPos) / Time.deltaTime;
        float headSpeed = new Vector2(headVelocity.x, headVelocity.z).magnitude;
        lastHeadsetPos = headset.position;

        // Raycast to detect ground for this foot
        Vector3 rayOrigin = body.position + (body.right * footSpacing) + Vector3.up * rayStartYOffset;
        Ray ray = new Ray(rayOrigin, Vector3.down);
        Debug.DrawRay(ray.origin, ray.direction * rayLength, Color.red);

        if (Physics.Raycast(ray, out RaycastHit info, rayLength, terrainLayer.value))
        {
            if (Vector3.Distance(newPosition, info.point) > stepDistance &&
                !otherFoot.IsMoving() &&
                lerp >= 1 &&
                headSpeed > 0.05f) // only step if head is moving
            {
                lerp = 0;
                Vector3 direction = Vector3.ProjectOnPlane(info.point - currentPosition, Vector3.up).normalized;
                float angle = Vector3.Angle(body.forward, body.InverseTransformDirection(direction));
                isMovingForward = angle < 50 || angle > 130;

                if (isMovingForward)
                {
                    newPosition = info.point + direction * stepLength + footOffset;
                }
                else
                {
                    newPosition = info.point + direction * sideStepLength + footOffset;
                }

                newNormal = info.normal;
            }
        }

        // Handle stepping
        if (lerp < 1)
        {
            Vector3 tempPosition = Vector3.Lerp(oldPosition, newPosition, lerp);
            tempPosition.y += Mathf.Sin(lerp * Mathf.PI) * stepHeight;

            currentPosition = tempPosition;
            currentNormal = Vector3.Lerp(oldNormal, newNormal, lerp);
            lerp += Time.deltaTime * speed;
        }
        else
        {
            oldPosition = newPosition;
            oldNormal = newNormal;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(newPosition, 0.1f);
    }

    public bool IsMoving()
    {
        return lerp < 1;
    }
}

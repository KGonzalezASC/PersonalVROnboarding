using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class BoundaryReader : MonoBehaviour
{
    private XRInputSubsystem xrInputSubsystem;
    private List<Vector3> boundaryPoints = new List<Vector3>();

    void Start()
    {
        // Acquire the active XRInputSubsystem
        var subsystems = new List<XRInputSubsystem>();
        SubsystemManager.GetSubsystems(subsystems);
        foreach (var sub in subsystems)
        {
            // Only assign if boundary data is supported
            if (sub.TryGetBoundaryPoints(boundaryPoints))
            {
                xrInputSubsystem = sub;
                break;
            }
        }

        if (xrInputSubsystem == null)
        {
            Debug.LogWarning("Boundary support not available on this device.");
        }
    }

    void Update()
    {
        if (xrInputSubsystem != null)
        {
            boundaryPoints.Clear();
            if (xrInputSubsystem.TryGetBoundaryPoints(boundaryPoints))
            {
                // boundaryPoints now contains the four corners (in XZ) of the play area
                foreach (var point in boundaryPoints)
                {
                    Debug.Log($"Boundary vertex: {point}");
                }
            }
            else
            {
                Debug.LogWarning("Failed to fetch boundary points this frame.");
            }
        }
    }
}
using UnityEngine;

[RequireComponent(typeof(InspectorTaskCompleter))]
public class ProximityTaskValidator : MonoBehaviour, ITaskValidator
{
    public TaskId  taskId;
    public Transform validZoneCenter;
    public float   validZoneRadius = 1f;
    public int     segmentCount    = 32;
    public float   lineWidth       = 0.01f;

    LineRenderer _lr;

    void Start()
    {
        _lr = gameObject.AddComponent<LineRenderer>();
        _lr.positionCount    = segmentCount + 1;
        _lr.loop             = true;
        _lr.useWorldSpace    = true;
        _lr.startWidth       = _lr.endWidth = lineWidth;
        _lr.material         = new Material(Shader.Find("Unlit/Color")); // or your custom
        _lr.material.color   = new Color(0f, 1f, 0f, 0.5f);

        // build the circle once
        for (int i = 0; i <= segmentCount; i++)
        {
            float angle = 2 * Mathf.PI * i / segmentCount;
            Vector3 p = new Vector3(
                Mathf.Cos(angle) * validZoneRadius,
                0,
                Mathf.Sin(angle) * validZoneRadius
            );
            _lr.SetPosition(i, validZoneCenter.position + p);
        }
    }

    void Update()
    {
        // keep it in sync if the center moves
        for (int i = 0; i <= segmentCount; i++)
        {
            float angle = 2 * Mathf.PI * i / segmentCount;
            Vector3 p = new Vector3(
                Mathf.Cos(angle) * validZoneRadius,
                0,
                Mathf.Sin(angle) * validZoneRadius
            );
            _lr.SetPosition(i, validZoneCenter.position + p);
        }
    }

    public bool Validate(SpeedrunTask task, GameObject context)
    {
        // 1) Log the incoming parameters
        Debug.Log($"[ProximityValidator] Validating Task.Id={task.Id}, expected={taskId}; Context='{context.name}' at {context.transform.position}");

        // 2) If this isn’t the task we care about, short-circuit to “passed”
        if (task.Id != taskId)
        {
            Debug.Log("[ProximityValidator] Task ID mismatch → automatically passing.");
            return true;
        }

        // 3) Grab positions
        Vector3 ctxPos = context.transform.position;
        Vector3 zonePos = validZoneCenter.position;

        // 4) Compute deltas
        float dx = ctxPos.x - zonePos.x;
        float dy = ctxPos.y - zonePos.y;
        float dz = ctxPos.z - zonePos.z;
        Debug.Log($"[ProximityValidator] Δ = ({dx:F3}, {dy:F3}, {dz:F3})");

        // 5) Compute straight-line distance (with an explicit sqrt)
        float dist = Mathf.Sqrt(dx * dx + dy * dy + dz * dz);
        Debug.Log($"[ProximityValidator] Distance = {dist:F3}, Radius = {validZoneRadius}");

        // 6) Compare and log result
        if (dist <= validZoneRadius)
        {
            Debug.Log("[ProximityValidator] Within radius → PASS");
            return true;
        }
        else
        {
            Debug.Log("[ProximityValidator] Outside radius → FAIL");
            return false;
        }
    }
    
    //optimized without prints
    /*public bool Validate(SpeedrunTask task, GameObject context)
    {
        if (task.Id != taskId) return true;
        Debug.Log($"ProximityValidator checking {context.name} at position {context.transform.position}");
        return Vector3.Distance(context.transform.position,
                   validZoneCenter.position)
               <= validZoneRadius;
    }*/

}
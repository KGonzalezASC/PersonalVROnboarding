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
        if (task.Id != taskId) return true;
        return Vector3.Distance(context.transform.position,
                   validZoneCenter.position)
               <= validZoneRadius;
    }
}
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class PriceChecker : MonoBehaviour
{
    [Header("Scan Settings")]
    [Tooltip("Origin point for the scan ray")]
    [SerializeField] private Transform scanPoint;

    [Tooltip("Maximum distance for the scan")]
    [SerializeField] private float maxScanDistance = 10f;

    [Tooltip("Which layers the scanner can hit")]
    [SerializeField] private LayerMask scanMask = ~0;

    [Header("Beam Settings")]
    [Tooltip("How long (seconds) the beam remains visible after firing")]
    [SerializeField] private float beamDuration = 0.2f;

    [Tooltip("Width of the beam at the scan point")]
    [SerializeField] private float startWidth = 0.05f;

    [Tooltip("Width of the beam at its max distance (usually zero for a perfect cone)")]
    [SerializeField] private float endWidth = 0.0f;

    private LineRenderer _line;

    // runtime state
    private bool  _beamActive;
    private float _beamLength;
    private float _beamDeactivateTime;
    [SerializeField]
    private StackErrors forcedDefect;
    
    [SerializeField]
    private InspectorTaskCompleter taskCompleter;

    void Awake()
    {
        _line = GetComponent<LineRenderer>();
        _line.positionCount = 2;
        _line.useWorldSpace  = true;
        _line.startWidth    = startWidth;
        _line.endWidth      = endWidth;
        _line.enabled       = false;
        forcedDefect = default(StackErrors).RandomValue();
    }

    /// <summary>
    /// Fires the scanner: raycasts forward, logs the result, and turns on
    /// a beam that follows the scanner’s current transform for a short time.
    /// </summary>
    public void Fire()
    {
        if (scanPoint == null)
        {
            Debug.LogWarning($"{name}: No scanPoint assigned.");
            return;
        }

        Vector3 origin = scanPoint.position;
        Vector3 dir    = scanPoint.forward;
        float   dist   = maxScanDistance;
        
        if (Physics.Raycast(origin, dir, out var hit, maxScanDistance, scanMask))
        {
            dist = hit.distance;
            Debug.Log($"[{name}] Scanned: {hit.collider.gameObject.name}");
            if (taskCompleter.taskId == TaskMarshal.Instance.Sequence.MandatoryProgression)
            {
                taskCompleter.Complete(hit.collider.gameObject);
            }
        }
        else
        {
            Debug.Log($"[{name}] Nothing within {maxScanDistance}m.");
        }

        // Record beam state
        _beamLength         = dist;
        _beamDeactivateTime = Time.time + beamDuration;
        _beamActive         = true;

        // Enable renderer (positions will be set in Update)
        _line.enabled = true;
    }

    void Update()
    {
        if (!_beamActive) return;
        if (scanPoint == null)
        {
            _line.enabled = false;
            _beamActive   = false;
            return;
        }

        // Update endpoints each frame so the beam follows your grab
        Vector3 origin = scanPoint.position;
        Vector3 end    = origin + scanPoint.forward * _beamLength;
        _line.SetPosition(0, origin);
        _line.SetPosition(1, end);

        // Turn it off when time’s up
        if (Time.time >= _beamDeactivateTime)
        {
            _beamActive   = false;
            _line.enabled = false;
        }
    }
}

using ZLinq;
using UnityEngine;
public class InspectorTaskCompleter : MonoBehaviour
{
    [Tooltip("Which task to complete when this is triggered")]
    public TaskId taskId;

    [Tooltip("Drag in a component on this or another GameObject that implements ITaskValidator")]
    [SerializeField] private MonoBehaviour primaryValidatorComponent;

    [Tooltip("Drag any additional components here that implement ITaskValidator")]
    [SerializeField] private MonoBehaviour[] additionalValidatorComponents;

    private ITaskValidator _primaryValidator;
    private ITaskValidator[] _additionalValidators;

    void Awake()
    {
        // 1) Resolve primary validator
        _primaryValidator = primaryValidatorComponent as ITaskValidator
                            ?? GetComponent<ITaskValidator>();

        // 2) Build a list of additional validators, filtering out any that don't implement the interface
        _additionalValidators = additionalValidatorComponents
            .AsValueEnumerable()                     // struct-based enumerable :contentReference[oaicite:8]{index=8}
            .OfType<ITaskValidator>()             
            .ToArray();    
    }

    /// <summary>
    /// Complete the task using this GameObject as the “actor.”
    /// </summary>
    public void Complete()
    {
        TryComplete(gameObject);
    }

    /// <summary>
    /// Complete the task using another GameObject as the “actor.”
    /// Returns true if the task was actually completed.
    /// </summary>
    public bool Complete(GameObject other)
    {
        return TryComplete(other);
    }

    /// <summary>
    /// Core workflow: fetch the sequence & task, run validators, then complete.
    /// </summary>
    private bool TryComplete(GameObject actor)
    {
        var tm  = TaskMarshal.Instance;
        var seq = tm.Sequence;
        if (seq == null)
        {
            Debug.LogWarning("No sequence set.");
            return false;
        }

        if (!seq.TryGetTask(taskId, out var task))
        {
            Debug.LogWarning($"Task '{taskId}' not found.");
            return false;
        }

        if (task.IsCompleted)
        {
            Debug.LogWarning($"Task '{taskId}' already done.");
            return false;
        }
        
        
        // ────────────────────────────────────────
        // ENFORCE IN-ORDER MANDATORY COMPLETION:
        if (task.IsMandatory && task.Id != seq.MandatoryProgression)
        {
            var m =
                $"Cannot complete mandatory task '{task.Id}' out of order. " +
                $"Next mandatory is '{seq.MandatoryProgression}'.";
            Debug.LogWarning(m);
            TaskMarshal.Instance.Print(m);
            return false;
        }
        // ──
        
        

        // 1) Primary validator
        if (_primaryValidator!= null && !_primaryValidator.Validate(task, actor))
        {
            Debug.LogWarning($"Validation failed for '{taskId}' (primary).");
            return false;
        }

        // 2) Additional validators
        foreach (var v in _additionalValidators)
        {
            if (!v.Validate(task, actor))
            {
                Debug.LogWarning(
                    $"Validation failed for '{taskId}' by {v.GetType().Name}.");
                return false;
            }
        }


        // 3) All checks passed → complete & advance
        tm.CompleteTask(task);
        if (task.IsMandatory)
            tm.StartNextMandatory();

        return true;
    }
}

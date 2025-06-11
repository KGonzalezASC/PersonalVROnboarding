using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(XRBaseInteractable))]
public class InspectorTaskCompleter : MonoBehaviour
{
    public TaskId taskId;

    XRBaseInteractable interactable;
    ITaskValidator     _validator;

    void Awake()
    {
        if (!interactable)
            interactable = GetComponent<XRBaseInteractable>();
        _validator ??= GetComponent<ITaskValidator>();
    }
    
    public void Complete()
    {
        var tm  = TaskMarshal.Instance;
        var seq = tm.Sequence;
        if (seq == null) { Debug.LogWarning("No sequence set."); return; }

        if (!seq.TryGetTask(taskId, out var task))
        {
            Debug.LogWarning($"Task '{taskId}' not found."); return;
        }
        if (task.IsCompleted)
        {
            Debug.LogWarning($"Task '{taskId}' already done."); return;
        }

        // 1) If a validator exists and it fails, block completion
        if (_validator != null && !_validator.Validate(task, gameObject))
        {
            Debug.LogWarning($"Validation failed for {taskId}"); 
            return;
        }

        // 2) Otherwise complete and advance
        tm.CompleteTask(task);
        if (task.IsMandatory) tm.StartNextMandatory();
    }
}
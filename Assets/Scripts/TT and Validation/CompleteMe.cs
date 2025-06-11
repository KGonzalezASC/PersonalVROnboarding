using UnityEngine;
using UnityEngine.Profiling;
using ZLinq;   // ← add this

public class InspectorTaskCompleter : MonoBehaviour
{
    [Tooltip("Exact Name of the SpeedrunTask to complete.")]
    public string taskName;

    // Cached lookup for the task
    private SpeedrunTask _cachedTask;

    private void Start()
    {
        var seq = TaskMarshal.Instance.Sequence;
        if (seq == null)
        {
            Debug.LogWarning("No sequence set on TaskMarshal.");
            return;
        }

        // Perform the lookup once on start
        _cachedTask = seq.AllTasks()
            .AsValueEnumerable()
            .FirstOrDefault(t => t.Name == taskName);

        if (_cachedTask == null)
            Debug.LogWarning($"Task '{taskName}' not found in sequence.");
    }

    public void Complete()
    {
        if (_cachedTask == null)
        {
            Debug.LogWarning($"Cannot complete task: '{taskName}' was not found or initialized.");
            return;
        }

        if (!_cachedTask.IsCompleted)
        {
            // Add any pre‐completion validation here if needed
            TaskMarshal.Instance.CompleteTask(_cachedTask);
        }
        else
        {
            Debug.LogWarning($"Task '{taskName}' is already completed.");
            return;
        }

        if (_cachedTask.IsMandatory)
        {
            TaskMarshal.Instance.StartNextMandatory();
        }
    }
}
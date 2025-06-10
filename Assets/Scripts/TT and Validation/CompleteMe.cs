using UnityEngine;
using UnityEngine.Profiling;
using ZLinq;   // ← add this


public class InspectorTaskCompleter : MonoBehaviour
{
    [Tooltip("Exact Name of the SpeedrunTask to complete.")]
    public string taskName;

    public void Complete()
    {
        var seq = TaskMarshal.Instance.Sequence;
        if (seq == null)
        {
            Debug.LogWarning("No sequence set on TaskMarshal.");
            return;
        }


        // Zero-allocating lookup:
        var task = seq.AllTasks()                   // IEnumerable<SpeedrunTask>
            .AsValueEnumerable()          // ZLinq extension
            .FirstOrDefault(t => t.Name == taskName);

        if (task != null && !task.IsCompleted)
            //add validation before starting next mandatory task and if not done correctly resume timer somehow. --task mandatory index start from where you left off.
        
        
            TaskMarshal.Instance.CompleteTask(task);
        if (task != null && task.IsMandatory)
            TaskMarshal.Instance.StartNextMandatory();
            //start next mandatory task if it is a mandatory task
            
        else
            Debug.LogWarning($"Task '{taskName}' not found or already completed.");
    }
}
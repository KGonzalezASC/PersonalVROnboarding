using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(InspectorTaskCompleter))]
public class TaskTargetValidator : MonoBehaviour, ITaskValidator
{
    [Tooltip("Which TaskId this validator applies to")]
    public TaskId taskId;

    [Tooltip("All the GameObjects that count as valid targets for this task.")]
    public GameObject[] targetObjects;

    [Tooltip("If true, you must scan *all* targetObjects before completion;\nif false, a single match is enough.")]
    public bool   requireAll = false;

    // Only used in 'requireAll' mode
    HashSet<GameObject> _seen = new HashSet<GameObject>();

    public bool Validate(SpeedrunTask task, GameObject context)
    {
        if (task.Id != taskId) 
            return true;

        // Does this context match any of our listed targets?
        bool isMatch = false;
        for (int i = 0; i < targetObjects.Length; i++)
        {
            if (context == targetObjects[i])
            {
                isMatch = true;
                if (requireAll)
                    _seen.Add(context); // record for all-scan
                break;
            }
        }

        if (!isMatch)
            //debug saying not a match
        //    Debug.LogWarning($"Task '{taskId}' failed validation: context '{context}' not a valid target.");
            return false;

        // If only one match needed, we succeed immediately
        if (!requireAll)
            return true;

        // Otherwise, we need to see each target at least once
        return _seen.Count >= targetObjects.Length;
    }
}

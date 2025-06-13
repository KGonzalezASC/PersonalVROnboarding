using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// A composite of of mandatory and free tasks
/// Mandatory tasks have to be done in order to start time tracking the next mandatory task.
/// Free tasks can be done in any order.
/// However, doing a free task during a mandatory task does not stop the timer for it.
/// Pluggy will pause the timer between talking about tasks throughout "demo mode" to build a time baseline.
///
/// every speedrun will include start, end, and expected time [and rather than having those expected times be pre-provided (when the game is fully ready to ship), these expected times are made
/// from the Pluggy "demo mode".]
/// </summary>

public enum TaskId
{
    VerifyStackID,
    DefectiveCheck,
    StackToCart,
    BleedResistor,
    RemoveTape,
    PourDrink,
    SetTable,
    FryMozzarella,
    COUNT  // must be last
}



public class SpeedrunSequence
{
    private readonly List<SpeedrunTask> _mandatoryTasks = new();
    private readonly List<SpeedrunTask> _freeTasks      = new();
	//lookup table
	  readonly SpeedrunTask[] _tasksById = new SpeedrunTask[(int)TaskId.COUNT];
    private int _nextMandatoryIndex;
    
    //need a BACKING STRUCTURE ON RESET: store total time and timeslices before making people redo the whole thing.
    
    public TaskId MandatoryProgression
    {
        get
        {
            if (_nextMandatoryIndex < _mandatoryTasks.Count)
                return _mandatoryTasks[_nextMandatoryIndex].Id;
            // No more mandatory tasks
            return default;
        }
    }


    public bool IsRunning=false;

    public IReadOnlyList<SpeedrunTask> MandatoryTasks => _mandatoryTasks;
    public IReadOnlyList<SpeedrunTask> FreeTasks => _freeTasks;

    public IEnumerable<SpeedrunTask> AllTasks()
        => _mandatoryTasks.Concat(_freeTasks);

    public float TotalAllTasksDuration
        => AllTasks()
            .Where(t => t.IsCompleted)
            .Sum(t => t.ActualEndTime - t.ActualStartTime);
    
    public event Action<SpeedrunTask>        TaskStarted;
    public event Action<SpeedrunTask, float> TaskCompleted;
    public event Action                      AllMandatoryCompleted;
    public event Action                      AllFreeCompleted;
    public event Action                      SequenceComplete;
    

#region Sequence Creation:

    /// <summary>Adds a mandatory task (auto-assigns its MandIndex).</summary>
    public SpeedrunTask AddMandatory(TaskId name, float expectedTime) {
        var task = new SpeedrunTask(name, expectedTime, true, _mandatoryTasks.Count);
        _mandatoryTasks.Add(task);
        _tasksById[(int)name] = task;
        return task;
    }

    public SpeedrunTask AddFree(TaskId name, float expectedTime) {
        var task = new SpeedrunTask(name, expectedTime, false);
        _freeTasks.Add(task);
        _tasksById[(int)name] = task;
        return task;
    }

    /// <summary>
    ///     Example: build a hypothetical pizza-making sequence.
    /// </summary>
    public static SpeedrunSequence BuildPizzaSequence() {
        var seq = new SpeedrunSequence();

        // Mandatory steps (will have MandIndex 0,1,2...)
        seq.AddMandatory(TaskId.VerifyStackID, 60f);
        seq.AddMandatory(TaskId.DefectiveCheck, 30f);
        seq.AddMandatory(TaskId.StackToCart, 20f);
        seq.AddMandatory(TaskId.BleedResistor, 120f);

        // Free tasks (MandIndex = -1 automatically)
        seq.AddFree(TaskId.RemoveTape, 45f);
        seq.AddFree(TaskId.PourDrink, 15f);
        seq.AddFree(TaskId.SetTable, -1f);
        seq.AddFree(TaskId.FryMozzarella, 90f);

        return seq;
    }
    
#endregion

#region Progression:

    public void StartNextMandatory() {
        if (_nextMandatoryIndex >= _mandatoryTasks.Count) return;

        var task = _mandatoryTasks[_nextMandatoryIndex];
        Debug.Log(task.Name);
        task.ActualStartTime = Time.time;
        TaskStarted?.Invoke(task);
    }

    
    public void StartFreeTask(SpeedrunTask task) {
        var freetask = _tasksById[(int)task.Id];
        if (freetask == null || freetask.IsCompleted) return;
        freetask.ActualStartTime = Time.time;
        TaskStarted?.Invoke(freetask);
    }
    
    public void CompleteTask(SpeedrunTask task) {
        if (task.IsCompleted) return;

        task.ActualEndTime = Time.time;
        float actualDur    = task.ActualEndTime - task.ActualStartTime;
        TaskCompleted?.Invoke(task, actualDur);

        if (task.IsMandatory) {
            _nextMandatoryIndex++;
            if (_nextMandatoryIndex >= _mandatoryTasks.Count)
                AllMandatoryCompleted?.Invoke();
        }
        else {
            if (_freeTasks.All(t => t.IsCompleted))
                AllFreeCompleted?.Invoke();
        }

        // if EVERYTHING is done:
        if (_mandatoryTasks.All(t => t.IsCompleted)
            && _freeTasks.All(t => t.IsCompleted)) {
            SequenceComplete?.Invoke();
        }
    }
    
    public bool TryGetTask(TaskId id, out SpeedrunTask task)
    {
        task = _tasksById[(int)id];
        return task != null;
    }
    
    public void ResetSequence()
    {
        // Reset mandatory pointer
        _nextMandatoryIndex = 0;

        // Clear all timing data but not overall time.
        foreach (var t in AllTasks())
        {
            t.ActualStartTime = 0f;
            t.ActualEndTime   = 0f;
        }
    }
    
    //TODO: add a reset that doesn't stop the timer just moves your tasks back to the start in case we need to repeat steps:
    
    
#endregion
}


/// <summary>
/// A single speedrun task (mandatory or free), with ordering metadata.
/// </summary>
public class SpeedrunTask
{ 
    public TaskId Id { get; }
    public string Name=> Id.ToString();
    public float ExpectedTime { get; } //Expected time <0 is effectively untimed
    public bool IsMandatory { get; }

    /// <summary>
    /// If mandatory: its zero-based order index (0 = first).  
    /// If free: set to –1.
    /// </summary>
    public int MandIndex { get; }
    public float ActualStartTime { get; set; }
    public float ActualEndTime   { get; set; }

    /// <summary>True once the end time has been recorded.</summary>
    public bool IsCompleted => ActualEndTime > 0f;

    public SpeedrunTask(TaskId id, float expectedTime, bool isMandatory, int mandIndex = -1)
    {
        Id            = id;
        ExpectedTime  = expectedTime;
        IsMandatory   = isMandatory;
        MandIndex     = isMandatory ? mandIndex : -1;
    }
}



using System;
using System.Linq;
using UnityEngine;

public class TaskMarshal : NonPersistantSingleton<TaskMarshal>
{
    [SerializeField] private DebugTextManager _debugTextManager;

    public SpeedrunSequence Sequence;
    private float            _sequenceStartTime;
    
    public float TotalElapsed
    {
        get
        {
            if (Sequence == null) return 0f;
            if (Sequence.IsRunning)
                return Time.time - _sequenceStartTime;
            // once finished, fall back to the sequence’s own total
            return Sequence.TotalAllTasksDuration;
        }
    }
    
    public void Start()
    {
        _debugTextManager.AddLine("Starting new sequence...");
        
        var pizzaSeq = SpeedrunSequence.BuildPizzaSequence();
        TaskMarshal.Instance.SetSequence(pizzaSeq);
        TaskMarshal.Instance.StartSequence();
    }
    
    private void OnDisable()
    {
        if (Sequence != null)
            Unsubscribe(Sequence);
    }

    /// <summary>
    /// Assigns a new sequence and hooks up logging events.
    /// </summary>
    public void SetSequence(SpeedrunSequence sequence)
    {
        if (Sequence != null)
            Unsubscribe(Sequence);

        Sequence = sequence;
        Subscribe(Sequence);
    }

    private void Subscribe(SpeedrunSequence seq)
    {
        seq.TaskStarted          += OnTaskStarted;
        seq.TaskCompleted        += OnTaskCompleted;
        seq.AllMandatoryCompleted += OnAllMandatoryCompleted;
        seq.AllFreeCompleted     += OnAllFreeCompleted;
        seq.SequenceComplete     += OnSequenceComplete;
    }

    private void Unsubscribe(SpeedrunSequence seq)
    {
        seq.TaskStarted          -= OnTaskStarted;
        seq.TaskCompleted        -= OnTaskCompleted;
        seq.AllMandatoryCompleted -= OnAllMandatoryCompleted;
        seq.AllFreeCompleted     -= OnAllFreeCompleted;
        seq.SequenceComplete     -= OnSequenceComplete;
    }

    private void OnTaskStarted(SpeedrunTask task)
    {
        _debugTextManager.AddLine(
            $"Starting '{task.Name}': benchmark {task.ExpectedTime:F1}s");
    }

    private void OnTaskCompleted(SpeedrunTask task, float actualDuration)
    {
        _debugTextManager.AddLine(
            $"Completed '{task.Name}': {actualDuration:F1}s (exp {task.ExpectedTime:F1}s)");
    }

    private void OnAllMandatoryCompleted()
    {
        _debugTextManager.AddLine("▶ All mandatory tasks done.");
    }

    private void OnAllFreeCompleted()
    {
        _debugTextManager.AddLine("▶ All free tasks done.");
    }

    private void OnSequenceComplete()
    {
        float total = Sequence.TotalAllTasksDuration;
        _debugTextManager.AddLine(
            $"✅ Sequence complete! Total time: {total:F1}s");
    }

    /// <summary>
    /// Kick off the sequence: begins the first mandatory task.
    /// </summary>
    public void StartSequence()
    {
        if (Sequence == null)
        {
            Debug.LogWarning("TaskMarshal: no sequence has been set.");
            return;
        }

        // start the first mandatory
        _sequenceStartTime = Time.time;
        Sequence.IsRunning = true;
        Sequence.StartNextMandatory();
    }

    /// <summary>
    /// Manually begin the next mandatory task in order.
    /// </summary>
    public void StartNextMandatory()
        => Sequence?.StartNextMandatory();

    /// <summary>
    /// Begin a specific free (anytime) task.
    /// </summary>
    public void StartFreeTask(SpeedrunTask freeTask)
        => Sequence?.StartFreeTask(freeTask);

    /// <summary>
    /// Mark whichever task was running as complete.
    /// </summary>
    public void CompleteTask(SpeedrunTask task)
        => Sequence?.CompleteTask(task);
    
    
    //LogLine wrapper:
    public void Print(string line)
        => _debugTextManager.AddLine(line);
    
}

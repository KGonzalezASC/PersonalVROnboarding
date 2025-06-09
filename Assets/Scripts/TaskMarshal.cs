using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TaskMarshal : NonPersistantSingleton<TaskMarshal>
{
    [SerializeField] private DebugTextManager _debugTextManager;

    private readonly List<SpeedrunInstruction> _instructions = new List<SpeedrunInstruction>();
    private int    _currentIndex = -1;
    private bool   _isRunning    = false;
    private float _runningOverallTime = 0f;

    /// <summary>Fired when a new instruction begins.</summary>
    public event Action<SpeedrunInstruction> InstructionStarted;
    /// <summary>Fired when an instruction completes; gives you the actual elapsed time.</summary>
    public event Action<SpeedrunInstruction, float> InstructionCompleted;
    /// <summary>Fired when you finish the last instruction in the list.</summary>
    public event Action SequenceComplete;

    /// <summary>
    /// Expose the loaded instructions read-only.
    /// </summary>
    public IReadOnlyList<SpeedrunInstruction> Instructions => _instructions;

    /// <summary>
    /// Remaining time on the current instruction.
    /// </summary>
    public float CurrentTimeLeft
    {
        get
        {
            if (!_isRunning || _currentIndex < 0 || _currentIndex >= _instructions.Count)
                return 0f;
            var inst = _instructions[_currentIndex];
            float elapsed = Time.time - inst.ActualStartTime;
            return Mathf.Max(0f, inst.BenchmarkDuration - elapsed);
        }
    }

    /// <summary>
    /// Total actual time
    /// </summary>
    public float TotalActualTimeSoFar
    {
        get
        {
            if (_isRunning)
                return Time.time - _runningOverallTime;

            // once done, you can still fall back to summing:
            float sum = 0f;
            foreach (var inst in _instructions)
            {
                sum += inst.ActualEndTime - inst.ActualStartTime;
            }
            return sum;
        }
    }

    /// <summary>
    /// Load a brand-new sequence (clears any existing).
    /// </summary>
    public void SetSequence(IEnumerable<SpeedrunInstruction> sequence)
    {
        _instructions.Clear();
        _instructions.AddRange(sequence);
        _currentIndex = -1;
        _isRunning    = false;
    }

    /// <summary>
    /// Kick off the sequence from the top.
    /// Fires InstructionStarted for #0.
    /// </summary>
    public void StartSequence()
    {
        if (_instructions.Count == 0) return;
        _currentIndex = -1;
        _isRunning    = true;
        _runningOverallTime = Time.time;
        AdvanceToNext();
    }

    /// <summary>
    /// Mark the current instruction done—records actual time, fires InstructionCompleted,
    /// logs performance, then steps to next or SequenceComplete.
    /// </summary>
    public void CompleteCurrent()
    {
        if (!_isRunning || _currentIndex < 0 || _currentIndex >= _instructions.Count)
            return;

        var inst = _instructions[_currentIndex];
        inst.ActualEndTime = Time.time;
        float actualDur   = inst.ActualEndTime - inst.ActualStartTime;

        inst.OnComplete?.Invoke();
        InstructionCompleted?.Invoke(inst, actualDur);
        _debugTextManager.AddLine(
            $"Completed '{inst.Name}': {actualDur:F2}s (expected {inst.BenchmarkDuration:F2}s)");

        AdvanceToNext();
    }

    private void AdvanceToNext()
    {
        _currentIndex++;
        if (_currentIndex >= _instructions.Count)
        {
            _isRunning = false;
            SequenceComplete?.Invoke();
            return;
        }

        var inst = _instructions[_currentIndex];
        inst.ActualStartTime = Time.time;
        inst.OnStart?.Invoke();
        InstructionStarted?.Invoke(inst);
        _debugTextManager.AddLine(
            $"Starting '{inst.Name}': benchmark {inst.BenchmarkDuration:F2}s");

        StartCoroutine(MonitorTimeout(inst));
    }

    private IEnumerator MonitorTimeout(SpeedrunInstruction inst)
    {
        yield return new WaitForSeconds(inst.BenchmarkDuration);
        if (inst.ActualEndTime <= 0f)
        {
            float actualDur = Time.time - inst.ActualStartTime;
            _debugTextManager.AddLine(
                $"[Timeout] '{inst.Name}' took {actualDur:F2}s (expected {inst.BenchmarkDuration:F2}s)");
        }
    }

    /// <summary>
    /// How many steps remain (including the current one).
    /// </summary>
    public int RemainingSteps =>
        _isRunning
            ? _instructions.Count - _currentIndex
            : 0;

    private void Start()
    {
        // example startup
        var t = new SpeedrunInstruction(
            "Test",
            20f,
            () => _debugTextManager.AddLine("testStarted"),
            () => _debugTextManager.AddLine("testComplete")
        );

        SetSequence(new List<SpeedrunInstruction> { t });
        StartSequence();
    }
}

/// <summary>
/// A single step in your sequence.
/// Carries a benchmark duration, a display name, and Actions to invoke on start/complete.
/// </summary>
public class SpeedrunInstruction
{
    public string Name { get; }
    public float  BenchmarkDuration { get; }
    public Action OnStart    { get; }
    public Action OnComplete { get; }

    internal float ActualStartTime { get; set; }
    internal float ActualEndTime   { get; set; }

    public SpeedrunInstruction(string name, float benchmarkSeconds, Action onStart = null, Action onComplete = null)
    {
        Name               = name;
        BenchmarkDuration  = benchmarkSeconds;
        OnStart            = onStart;
        OnComplete         = onComplete;
    }
}

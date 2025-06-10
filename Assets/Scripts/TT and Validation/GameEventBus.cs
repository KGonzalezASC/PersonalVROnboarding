using System;
using System.Collections.Generic;
using UnityEngine;

public enum RunEventType
{
    CoinPick,
    RawPizzaInOven,
    // … add more here …
    Count
}

public static class RunEventBus
{
    // Subscribers per event type
    private static readonly List<Action<object>>[] _subs =
        new List<Action<object>>[(int)RunEventType.Count];

    // Streak tracking
    private static RunEventType? _lastEvent;
    private static int           _currentStreak;

    static RunEventBus()
    {
        // Pre-allocate subscriber lists
        for (int i = 0; i < _subs.Length; i++)
            _subs[i] = new List<Action<object>>(4);
    }

    /// <summary>Subscribe a callback to an event.</summary>
    public static void Subscribe(RunEventType type, Action<object> callback)
        => _subs[(int)type].Add(callback);

    /// <summary>Unsubscribe a callback.</summary>
    public static void Unsubscribe(RunEventType type, Action<object> callback)
        => _subs[(int)type].Remove(callback);

    /// <summary>
    /// Publish an event: updates streak, then notifies subscribers.
    /// </summary>
    public static void Publish(RunEventType type, object payload = null)
    {
        // Update streak counter
        if (_lastEvent == type)
            _currentStreak++;
        else
        {
            _lastEvent     = type;
            _currentStreak = 1;
        }

        // Dispatch to subscribers
        var list = _subs[(int)type];
        for (int i = 0, n = list.Count; i < n; i++)
            list[i](payload);
    }

    /// <summary>
    /// How many times the last event has fired in direct succession.
    /// </summary>
    public static int CurrentStreak => _currentStreak;

    /// <summary>
    /// Which event is in the current streak (null if none yet).
    /// </summary>
    public static RunEventType? LastEventType => _lastEvent;
}
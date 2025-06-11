using UnityEngine;

/// <summary>
/// Strategy interface for per‐task validation.
/// </summary>
public interface ITaskValidator
{
    /// <summary>
    /// false to block completion (timer keeps running).
    /// </summary>
    bool Validate(SpeedrunTask task, GameObject context);
}
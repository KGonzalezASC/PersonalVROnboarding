using UnityEditor;
using UnityEngine;
using ZLinq;

[CustomEditor(typeof(TaskMarshal))]
[CanEditMultipleObjects]
public class TaskMarshalEditor : Editor
{
    void OnEnable()  => EditorApplication.update += Repaint;
    void OnDisable() => EditorApplication.update -= Repaint;

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        DrawPropertiesExcluding(serializedObject, "m_Script");
        serializedObject.ApplyModifiedProperties();

        var tm  = (TaskMarshal)target;
        var seq = tm.Sequence;

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Speedrun Overview", EditorStyles.boldLabel);

        if (seq == null)
        {
            EditorGUILayout.LabelField("No sequence set.");
            return;
        }

        // 1) Overall elapsed time
        int overallSecs = Mathf.FloorToInt(tm.TotalElapsed);
        EditorGUILayout.LabelField($"Overall Elapsed: {overallSecs} s");
        EditorGUILayout.Space();

        // 2) Mandatory tasks
        EditorGUILayout.LabelField("Mandatory Tasks", EditorStyles.boldLabel);
        var mandEnum = seq.MandatoryTasks.AsValueEnumerable();
        var firstPair = mandEnum
            .Select((task, idx) => (task, idx))
            .FirstOrDefault(pair => !pair.task.IsCompleted);
        int currentMandIdx = firstPair.task != null ? firstPair.idx : -1;

        for (int i = 0; i < seq.MandatoryTasks.Count; i++)
        {
            var task = seq.MandatoryTasks[i];
            bool isCurrent = (i == currentMandIdx);

            string label = $"{i + 1}. {task.Name} — {task.ExpectedTime:F1}s";
            if (isCurrent)
            {
                float elapsed = Time.time - task.ActualStartTime;
                float left    = Mathf.Max(0f, task.ExpectedTime - elapsed);
                label += $"   [Time Left: {left:F1}s]";
            }
            else if (task.IsCompleted)
            {
                float actual = task.ActualEndTime - task.ActualStartTime;
                label += $"   (Done: {actual:F1}s)";
            }
            EditorGUILayout.LabelField(label);
        }

        EditorGUILayout.Space();

        // 3) Free tasks
        EditorGUILayout.LabelField("Free Tasks", EditorStyles.boldLabel);
        foreach (var task in seq.FreeTasks)
        {
            bool isRunning = task.ActualStartTime > 0f && !task.IsCompleted;
            bool isTimed   = task.ExpectedTime > 0f;

            // Base label
            string label = $"- {task.Name}";
            if (isTimed)
                label += $" — {task.ExpectedTime:F1}s";

            if (isRunning)
            {
                if (isTimed)
                {
                    float elapsed = Time.time - task.ActualStartTime;
                    float left    = Mathf.Max(0f, task.ExpectedTime - elapsed);
                    label += $"   [Time Left: {left:F1}s]";
                }
                else
                {
                    label += "   [Running…]";
                }
            }
            else if (task.IsCompleted)
            {
                float actual = task.ActualEndTime - task.ActualStartTime;
                label += $"   (Done: {actual:F1}s)";
            }

            EditorGUILayout.LabelField(label);
        }
    }
}

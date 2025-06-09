using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(TaskMarshal))]
[CanEditMultipleObjects]
public class TaskMarshalEditor : Editor
{
    private void OnEnable()
    {
        // Keep repainting so we see the countdown update continuously
        EditorApplication.update += Repaint;
    }

    private void OnDisable()
    {
        EditorApplication.update -= Repaint;
    }

    public override void OnInspectorGUI()
    {
        // 1) Draw your serialized fields (except the auto-generated Script field)
        serializedObject.Update();
        DrawPropertiesExcluding(serializedObject, "m_Script");
        serializedObject.ApplyModifiedProperties();

        // 2) Grab the runtime data
        var tm = (TaskMarshal)target;
        var list = tm.Instructions;

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Runtime Instructions", EditorStyles.boldLabel);

        if (list.Count == 0)
        {
            EditorGUILayout.LabelField("No instructions loaded.");
        }
        else
        {
            // compute which one is current: count - remainingSteps
            int currentIdx = tm.RemainingSteps > 0
                ? list.Count - tm.RemainingSteps
                : -1;

            for (int i = 0; i < list.Count; i++)
            {
                var inst = list[i];
                // base label: "1. Name — 2.50s"
                string label = $"{i + 1}. {inst.Name} — {inst.BenchmarkDuration:F2}s";

                // if it's the active one, append "[Time Left: x.xx s]"
                if (i == currentIdx)
                {
                    float left = tm.CurrentTimeLeft;
                    label += $"    [Time Left: {left:F2}s]";
                }

                EditorGUILayout.LabelField(label);
            }
        }

        EditorGUILayout.Space();
        // show total actual time so far
        int totalSecs = Mathf.FloorToInt(tm.TotalActualTimeSoFar);
        EditorGUILayout.LabelField($"Total Time Elapsed: {totalSecs}s");
    }
}
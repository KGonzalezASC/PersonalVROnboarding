using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DebugTextManager : MonoBehaviour
{
    [Tooltip("Drag your TextMeshPro - Text (UI) component here.")]
    public TMP_Text debugText;
    
    public int maxLines = 50;
    private readonly Queue<string> lines = new Queue<string>();

    public void AddLine(string msg)
    {
        if (lines.Count >= maxLines)
            lines.Dequeue();
        lines.Enqueue(msg);
        RefreshText();
    }

    private void RefreshText()
    {
        debugText.text = string.Join("\n", lines.ToArray());
    }
}
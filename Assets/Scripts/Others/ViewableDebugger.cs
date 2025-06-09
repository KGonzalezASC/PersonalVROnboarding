using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Pool;

public class DebugTextManager : MonoBehaviour
{
    [Tooltip("Content RectTransform under the ScrollRect where lines will be added.")]
    [SerializeField] private RectTransform contentTransform;

    [Tooltip("Prefab containing a TMP_Text component for each line.")]
    [SerializeField] private GameObject linePrefab;

    [Tooltip("Maximum number of lines to keep in the scrollable content. Oldest lines will be recycled when this is exceeded.")]
    [SerializeField] private int maxLines = 100;

    [Tooltip("Number of test lines to create at start. Set to 0 to disable flood test.")]
    [SerializeField] private int floodTestCount;

    // Pool reference using the interface
    private IObjectPool<GameObject> _pool;

    // Simple counter to name each line in creation order
    private int _lineCounter = 1;

    private void Awake()
    {
        _pool = new ObjectPool<GameObject>(
            createFunc: () => Instantiate(linePrefab),
            actionOnGet: go =>
            {
                go.SetActive(true);
                go.transform.SetParent(contentTransform, false);
                go.transform.SetSiblingIndex(0); // still want newest on top
            },
            actionOnRelease: go => go.SetActive(false),
            actionOnDestroy: go => Destroy(go),
            collectionCheck: false,
            defaultCapacity: maxLines,
            maxSize: maxLines
        );
    }

    private void Start()
    {
        if (floodTestCount > 0)
            FloodTest(floodTestCount);
    }

    [ContextMenu("Flood Test Lines")]
    private void FloodTest(int count)
    {
        for (int i = 1; i <= count; i++)
            AddLine($"Test line {i}");
    }

    public void AddLine(string msg)
    {
        if (contentTransform == null || linePrefab == null)
        {
            Debug.LogWarning("ContentTransform or linePrefab not assigned.");
            return;
        }

        // Recycle oldest if we're at capacity
        if (contentTransform.childCount >= maxLines)
        {
            var oldest = contentTransform.GetChild(contentTransform.childCount - 1).gameObject;
            _pool.Release(oldest);
        }

        // Pull a line from the pool
        var go = _pool.Get();
        go.name = $"Line_{_lineCounter++}";

        // Set its text
        var tmp = go.GetComponent<TMP_Text>();
        if (tmp != null)
            tmp.text = msg;
        else
            Debug.LogWarning("linePrefab missing TMP_Text.");

        Canvas.ForceUpdateCanvases();
    }

    public void ClearAll()
    {
        for (int i = contentTransform.childCount - 1; i >= 0; i--)
            _pool.Release(contentTransform.GetChild(i).gameObject);

        Canvas.ForceUpdateCanvases();
    }
}

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

[RequireComponent(typeof(XRGazeInteractor))]  // or XRRayInteractor if you prefer
[RequireComponent(typeof(XRGazeInteractor))]
public class GazeCapsuleHighlighter : MonoBehaviour
{
    [Header("Cast Settings")]
    [SerializeField] private float capsuleHeight = 0.1f;
    [SerializeField] private float capsuleRadius = 0.02f;
    [SerializeField] private float maxDistance   = 10f;
    [SerializeField] private LayerMask layerMask = ~0;

    [Header("Highlight")]
    [Tooltip("Material used to highlight the gazed object.")]
    [SerializeField] private Material highlightMaterial;

    private XRGazeInteractor _gazeInteractor;
    private GameObject       _lastGazed;
    private Dictionary<Renderer, Material[]> _originalMats = new();

    void Awake()
    {
        _gazeInteractor = GetComponent<XRGazeInteractor>();
        if (_gazeInteractor == null)
            Debug.LogError("GazeCapsuleHighlighter requires an XRGazeInteractor.", this);
    }

    
    /// <summary>
    /// revise to interaction layers and or enums instead of physics layer mask entirely.
    /// figure out how to make less expensive
    /// </summary>
    void Update()
    {
        if (_gazeInteractor == null)
            return;

        // capsule endpoints centered on the ray origin
        var originT = _gazeInteractor.rayOriginTransform;
        Vector3 origin  = originT.position;
        Vector3 forward = originT.forward;
        float halfLen   = capsuleHeight * 0.5f;
        Vector3 p1      = origin + forward * halfLen;
        Vector3 p2      = origin - forward * halfLen;

        // cast forward
        if (Physics.CapsuleCast(p1, p2, capsuleRadius, forward,
                out var hit, maxDistance, layerMask, QueryTriggerInteraction.Ignore))
        {
            var hitGo = hit.collider.gameObject;

            // only highlight if NOT already grabbed by a near/far interactor
            // i.e. if this object has an XRBaseInteractable and isSelected == true, skip
            var xrInteractable = hit.collider.GetComponentInParent<XRBaseInteractable>();
            if (xrInteractable != null && xrInteractable.isSelected)
            {
                ClearHighlight();
                return;
            }

            if (hitGo != _lastGazed)
            {
                ClearHighlight();
                ApplyHighlight(hitGo);
                /*
                Debug.Log($"Gazed hit: {hitGo.name}");
                */
            }
        }
        else
        {
            ClearHighlight();
        }
    }

    private void ApplyHighlight(GameObject go)
    {
        _lastGazed = go;
        foreach (var r in go.GetComponentsInChildren<Renderer>())
        {
            // save originals
            _originalMats[r] = r.materials;
            // create array of highlight mats
            var h = new Material[r.materials.Length];
            for (int i = 0; i < h.Length; i++)
                h[i] = highlightMaterial;
            r.materials = h;
        }
    }

    private void ClearHighlight()
    {
        if (_lastGazed == null) return;
        foreach (var kv in _originalMats)
        {
            if (kv.Key != null)
                kv.Key.materials = kv.Value;
        }
        _originalMats.Clear();
        _lastGazed = null;
    }

    void OnDisable()
    {
        ClearHighlight();
    }
}
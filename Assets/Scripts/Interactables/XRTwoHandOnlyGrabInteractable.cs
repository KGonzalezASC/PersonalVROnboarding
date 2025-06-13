using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class TwoHandOnlyGrabInteractable : XRGrabInteractable
{
    [SerializeField] private bool isHeavy = false;

    [Header("Beneath-Check (optional)")]
    [SerializeField, Tooltip("Enable/disable raycasting downwards to report what's beneath.")]
    private bool enableBeneathCheck = false;
    [SerializeField, Tooltip("How far down to check for a hit beneath the interactable")]
    private float beneathRayDistance = 1.0f;
    [SerializeField, Tooltip("Which layers to include when checking beneath")]
    private LayerMask beneathRayMask = ~0;

    private float _initialY;
    private const float k_HeavyMassThreshold = 25f;

    protected override void Awake()
    {
        if (interactionManager == null)
            interactionManager = FindObjectOfType<XRInteractionManager>();
        if (interactionManager == null)
            Debug.LogError($"[{name}] needs an XRInteractionManager in the scene!", this);

        base.Awake();
        selectMode = InteractableSelectMode.Multiple;
        _initialY = transform.position.y;
    }

    public override void ProcessInteractable(XRInteractionUpdateOrder.UpdatePhase phase)
    {
        int count = interactorsSelecting.Count;

        // 1) Zero hands → default
        if (count == 0)
        {
            base.ProcessInteractable(phase);
            return;
        }

        // 2) One hand → force‐release
        if (count == 1)
            return;

        // 3) Two hands in Dynamic → check “heavy fall” first
        if (phase == XRInteractionUpdateOrder.UpdatePhase.Dynamic)
        {
            if (HeavyIsOnFloor())
                return;

            // a) Two‐hand midpoint & optional height lock
            ApplyTwoHandTransform();

            // b) Post‐move beneath‐check
            if (enableBeneathCheck && DidHeavyFall())
                return;
        }

        // 4) Finally, snap controllers to their attach points
        SnapControllersToAttach();
    }

    /// <summary>
    /// Returns true if we did an early heavy‐fall release, already called base.ProcessInteractable.
    /// </summary>
    private bool HeavyIsOnFloor()
    {
        if (isHeavy && enableBeneathCheck
            && Physics.Raycast(transform.position, Vector3.down, out var hit, beneathRayDistance, beneathRayMask)
            && HeavyFall(hit))
        {
            Debug.Log($"[{name}] Already on the floor—dropping.");
            base.ProcessInteractable(XRInteractionUpdateOrder.UpdatePhase.Dynamic);
            ReleaseAllInteractors();
            return true;
        }
        return false;
    }

    /// <summary>
    /// After moving, do the same check one more time.
    /// Returns true if we released and called base.ProcessInteractable.
    /// </summary>
    private bool DidHeavyFall()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out var hit, beneathRayDistance, beneathRayMask)
            && HeavyFall(hit))
        {
            ReleaseAllInteractors();
            base.ProcessInteractable(XRInteractionUpdateOrder.UpdatePhase.Dynamic);
            return true;
        }
        return false;
    }

    private void ApplyTwoHandTransform()
    {
        var a = interactorsSelecting[0].transform.parent.position;
        var b = interactorsSelecting[1].transform.parent.position;
        var mid = (a + b) * 0.5f;
        if (isHeavy) mid.y = _initialY;
        transform.position = mid;
        transform.rotation = Quaternion.LookRotation(Vector3.forward, Vector3.up);
    }

    private void SnapControllersToAttach()
    {
        Vector3 worldPrimary = attachTransform    != null ? attachTransform.position    : transform.position;
        Vector3 worldSecondary = secondaryAttachTransform != null ? secondaryAttachTransform.position : transform.position;

        // walk backwards if you need to remove during iteration
        foreach (var inter in interactorsSelecting)
        {
            if (inter == null) continue;
            bool right = inter.handedness == InteractorHandedness.Right;
            Vector3 target = right ? worldSecondary : worldPrimary;
            inter.transform.parent?.SetPositionAndRotation(target, inter.transform.parent.rotation);
        }
    }

    private void ReleaseAllInteractors()
    {
        for (int i = interactorsSelecting.Count - 1; i >= 0; i--)
        {
            var inter = interactorsSelecting[i];
            interactionManager.SelectExit(inter, this);

            if (inter is XRBaseInteractor xrBase)
            {
                // disable/re-enable so it truly drops and won’t snap back
                xrBase.enabled = false;
                xrBase.enabled = true;
            }
        }
    }

    private bool HeavyFall(RaycastHit hit)
    {
        return TryGetComponent<Rigidbody>(out var rb)
            && rb.mass > k_HeavyMassThreshold
            && hit.collider.gameObject.name == "Floor";
    }

    public override Transform GetAttachTransform(IXRInteractor interactor)
    {
        var hand = interactor is IXRSelectInteractor sel
            ? sel.handedness
            : InteractorHandedness.None;

        if (hand == InteractorHandedness.Right && secondaryAttachTransform != null)
            return secondaryAttachTransform;

        return attachTransform != null
            ? attachTransform
            : base.GetAttachTransform(interactor);
    }

    /*void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;

        foreach (var inter in interactorsSelecting)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(inter.transform.position, 0.01f);
        }

        if (interactorsSelecting.Count == 2)
        {
            var a = interactorsSelecting[0].transform.position;
            var b = interactorsSelecting[1].transform.position;
            var mid = (a + b) * 0.5f;
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(mid, 0.02f);
            Gizmos.DrawLine(a, b);
        }
    }*/
}

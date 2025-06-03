using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;


public class TwoHandOnlyGrabInteractable : XRGrabInteractable
{
    protected override void Awake()
    {
        // Ensure an InteractionManager exists
        if (interactionManager == null)
            interactionManager = FindObjectOfType<XRInteractionManager>();
        if (interactionManager == null)
            Debug.LogError($"[{name}] needs an XRInteractionManager in the scene!", this);

        base.Awake();

        // Allow more than one interactor
        selectMode = InteractableSelectMode.Multiple;
    }

/// <summary>
    /// Only process the built-in XRGrabInteractable logic when there are zero hands.
    /// Ignore one-hand touches entirely (force a release).
    /// If there are exactly two hands, run our custom two-hand logic.
    /// </summary>
    public override void ProcessInteractable(XRInteractionUpdateOrder.UpdatePhase phase)
    {
        int count = interactorsSelecting.Count;

        // 1) Zero hands → fall back to normal (so that you can start grabbing with one hand)
        //    That allows the object to enter the grabbed state if a hand touches it.
        if (count == 0)
        {
            base.ProcessInteractable(phase);
            return;
        }

        // 2) Exactly one hand → do NOTHING.  
        //    By NOT calling base.ProcessInteractable, we force the object to drop (run its release cleanup)
        //    as soon as that second hand disappears.  
        if (count == 1)
        {
            return;
        }

        // 3) Exactly two hands → override completely with our midpoint AND snapping logic.
        //    We never call base.ProcessInteractable here, because we want to bypass
        //    all built-in one-hand grab behavior and physics state changes.

        if (phase == XRInteractionUpdateOrder.UpdatePhase.Dynamic)
        {
            // Compute midpoint between the two hand‐parent positions:
            Vector3 a = interactorsSelecting[0].transform.parent.position;
            Vector3 b = interactorsSelecting[1].transform.parent.position;
            transform.position = (a + b) * 0.5f;

            // (You can adjust rotation however you like; here we lock it to world‐forward ↑)
            transform.rotation = Quaternion.LookRotation(Vector3.forward, Vector3.up);
        }

        // Now manually snap each controller’s root to its chosen attach point:
        Vector3 worldPrimary   = (attachTransform != null) 
                                  ? attachTransform.position 
                                  : transform.position;
        Vector3 worldSecondary = (secondaryAttachTransform != null) 
                                  ? secondaryAttachTransform.position 
                                  : transform.position;

        for (int i = 0; i < interactorsSelecting.Count; ++i)
        {
            // Cast to IXRSelectInteractor to read handedness
            var inter = interactorsSelecting[i] as IXRSelectInteractor;
            if (inter == null)
                continue;

            bool isRight = (inter.handedness == InteractorHandedness.Right);
            Vector3 targetPos = isRight ? worldSecondary : worldPrimary;

            // Move the controller’s parent so the controller appears at the attach point
            var controllerRoot = inter.transform.parent;
            if (controllerRoot != null)
                controllerRoot.position = targetPos;
        }
    }
    


    public override Transform GetAttachTransform(IXRInteractor interactor)
    {
        // Try to get the IXRSelectInteractor so we can read its handedness
        var selectInteractor = interactor as IXRSelectInteractor;
        var hand = selectInteractor != null
            ? selectInteractor.handedness
            : InteractorHandedness.None;

        // Choose by handedness: left = primary, right = secondary
        Transform chosen;
        if (hand == InteractorHandedness.Right && secondaryAttachTransform != null)
        {
            chosen = secondaryAttachTransform;
        }
        else
        {
            chosen = attachTransform != null 
                ? attachTransform 
                : base.GetAttachTransform(interactor);
        }
        
        //i need to 

        // Log it out
        /*string which = (chosen == secondaryAttachTransform) 
            ? "Secondary" 
            : "Primary";
        Debug.Log($"{gameObject.name}: {hand} hand → {which} AttachPoint ({chosen.name})");*/

        return chosen;
    }
    
    void OnDrawGizmos()
    {
            if (!Application.isPlaying)
                return;

            foreach (var inter in interactorsSelecting)
            {
                // This is the controller GameObject’s world position
                Vector3 controllerPos = inter.transform.position;
                Gizmos.color = Color.blue;
                Gizmos.DrawSphere(controllerPos, 0.01f);
            }

            // Now draw your midpoint for comparison
            if (interactorsSelecting.Count == 2)
            {
                var a = interactorsSelecting[0].transform.position;
                var b = interactorsSelecting[1].transform.position;
                var mid = (a + b) * 0.5f;
                Gizmos.color = Color.green;
                Gizmos.DrawSphere(mid, 0.02f);
                Gizmos.DrawLine(a, b);
            }
    }

    

    /*protected override void OnSelectEntered(SelectEnterEventArgs args)
    {
        base.OnSelectEntered(args);

        if (interactorsSelecting.Count == 1)
        {
            // First hand grabbed—lock transform in place if you like
            // TODO: e.g. disable movement or provide haptic feedback
        }
        else if (interactorsSelecting.Count == 2)
        {
            // Second hand grabbed—now “activate” the two-hand grab
            // TODO: e.g. unfreeze object, enable force application, etc.
        }
    }
    
    */
    

    /*
    protected override void OnSelectExited(SelectExitEventArgs args)
    {
        // Let XRGrabInteractable restore its internal state (colliders, Rigidbody)
        base.OnSelectExited(args);
    }
    */
    
}

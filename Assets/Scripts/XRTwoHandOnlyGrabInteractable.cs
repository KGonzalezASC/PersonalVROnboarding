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
    /// Only run the normal grab-movement logic when exactly two hands are holding.
    /// </summary>
    public override void ProcessInteractable(XRInteractionUpdateOrder.UpdatePhase updatePhase)
    {
        // Always call base to handle hover / selection state
        base.ProcessInteractable(updatePhase);

        // Only run our logic during the Dynamic phase
        if (updatePhase != XRInteractionUpdateOrder.UpdatePhase.Dynamic)
            return;

        // Need exactly two hands attached
        if (interactorsSelecting.Count < 2)
            return;

        // Get the two interactors
        var handA = interactorsSelecting[0];
        var handB = interactorsSelecting[1];

        // Their world‐space attach positions
        Vector3 posA = handA.GetAttachTransform(this).position;
        Vector3 posB = handB.GetAttachTransform(this).position;

        // 1) Position = midpoint
        transform.position = (posA + posB) * 0.5f;

        // 2) Rotation = look from A to B, with world up as the up‐vector
        Vector3 direction = posB - posA;
        if (direction.sqrMagnitude > Mathf.Epsilon)
            transform.rotation = Quaternion.LookRotation(direction, Vector3.up);
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
        string which = (chosen == secondaryAttachTransform) 
            ? "Secondary" 
            : "Primary";
        Debug.Log($"{gameObject.name}: {hand} hand → {which} AttachPoint ({chosen.name})");

        return chosen;
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
    
    
    

    protected override void OnSelectExited(SelectExitEventArgs args)
    {
        base.OnSelectExited(args);

        if (interactorsSelecting.Count < 2)
        {
            // One or zero hands remain—back to “not grabbed” state
            // TODO: e.g. freeze object, reset any partial two-hand state
        }
    }*/
}

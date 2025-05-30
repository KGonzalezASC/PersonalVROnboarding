using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(XRGrabInteractable))]
public class LockToGrabbingHand : XRGrabInteractable
{
    private InteractionLayerMask _originalInteractionLayers;
    // Precompute the Default layer’s mask (as an integer)
    private int _defaultMaskValue;    
    
    protected override void Awake()
    {
        // 1) If you haven't wired one in the Inspector, try to find one in the scene 
        if (interactionManager == null)
            interactionManager = FindObjectOfType<XRInteractionManager>();
        
        // 2) If it's STILL null, warn you rather than disable yourself
        if (interactionManager == null)
            Debug.LogError($"[{name}] needs an XRInteractionManager in the scene!", this);
        
        _defaultMaskValue = InteractionLayerMask.GetMask("Default");
        //set original interaction layers
        _originalInteractionLayers = interactionLayers;

        // 3) Call the base so all the normal registration logic still runs
        base.Awake();
        //do not setup event listeners here as it probably has overlap with the XRGrabInteractable and causes issues
        ensureOn();
        
    }

    private void ensureOn()
    {
        enabled = true;
        
    }

    protected override void OnSelectEntered(SelectEnterEventArgs args)
    {
        base.OnSelectEntered(args);
        Debug.Log("changing interaction layer mask");
        Debug.Log($"[{name}] grabbed by interactor: {args.interactorObject.transform.parent.name}");
        
         // 1) Get the grabbing hand’s layers
            var handLayers = args.interactorObject.interactionLayers;
            // 2) Pull out the raw int mask
            int handMaskValue = handLayers.value;

            // 3) Remove the Default bit
            handMaskValue &= ~_defaultMaskValue;

            // 4) Write it back into the struct
            handLayers.value = handMaskValue;

            // 5) Lock us to exactly that
            interactionLayers = handLayers;

    }
    
    protected override void OnSelectExited(SelectExitEventArgs args)
    {
        base.OnSelectExited(args);
        interactionLayers = _originalInteractionLayers;
    }
}
using System;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

[AddComponentMenu("XR/Custom Interaction Manager")]
[DisallowMultipleComponent]
[DefaultExecutionOrder(-100)]
public class CustomInteractionManager : XRInteractionManager
{
    // Called first when selection begins (before args are created)
    public override void SelectEnter(IXRSelectInteractor interactor, IXRSelectInteractable interactable)
    {
        // Check if it's a Near-Far Interactor (controller)
        if (IsControllerInteractor(interactor))
        {
            Debug.Log($"[Controller SelectEnter] interactor={interactor} interactable={interactable}");
            MyXRInteractionEvents.RaiseSelectForATask(interactor, interactable);
        }
        
        // Always call base - this ensures ALL interactions work normally
        base.SelectEnter(interactor, interactable);
    }

    // Called immediately afterwards with full event data
    protected override void SelectEnter(IXRSelectInteractor interactor,
        IXRSelectInteractable interactable,
        SelectEnterEventArgs args)
    {
        // Check if it's a Near-Far Interactor (controller)
        if (IsControllerInteractor(interactor))
        {
            Debug.Log($"[Controller SelectEnter w/ args] interactor={args.interactorObject} interactable={args.interactableObject}");
        }
        
        // Always call base - this ensures ALL interactions work normally
        base.SelectEnter(interactor, interactable, args);
    }

    public static bool IsControllerInteractor(IXRSelectInteractor interactor)
    {
        // Option 1: Check for Near-Far Interactor specifically
        if (interactor is NearFarInteractor)
            return true;

        // Option 2: Check for common controller interactors
        if (interactor is XRDirectInteractor || interactor is XRRayInteractor)
            return true;

        // Option 3: Check by name (if you have specific naming conventions)
        if (interactor.transform.name.Contains("Controller") || 
            interactor.transform.name.Contains("Hand"))
            return true;

        return false;
    }
}
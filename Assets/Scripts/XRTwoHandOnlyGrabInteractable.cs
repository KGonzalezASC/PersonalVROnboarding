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
    public override void ProcessInteractable(XRInteractionUpdateOrder.UpdatePhase phase)
    {
        // Only do anything if two hands are selecting
        if (interactorsSelecting.Count != 2)
            return;

        float dist=0;

        // Only handle movement on the Dynamic phase
        if (phase == XRInteractionUpdateOrder.UpdatePhase.Dynamic)
        {
            var a = interactorsSelecting[0].transform.parent.position;
            var b = interactorsSelecting[1].transform.parent.position;
            transform.position = (a + b) * 0.5f;
            dist = Vector3.Distance(a, b);

            /*
            Vector3 dir = b - a;
            if (dir.sqrMagnitude > Mathf.Epsilon)
                transform.rotation = Quaternion.LookRotation(dir, Vector3.up);*/
            transform.rotation = Quaternion.LookRotation(Vector3.forward, Vector3.up);
        }
        //To disable snap grap base.ProcessInteractable(phase); need to be excluded in favor of a custom implementation always
        //this means u can orient the object inital position as you see fit
        //base.ProcessInteractable(phase);
        
        Vector3 worldPrimary   = attachTransform            != null ? attachTransform.position           : transform.position;
        Vector3 worldSecondary = secondaryAttachTransform   != null ? secondaryAttachTransform.position   : transform.position;
        // For each interactor, teleport its parent so the controller lines up exactly with the attach point
        for (int i = 0; i < interactorsSelecting.Count; i++)
        {
            var inter = interactorsSelecting[i];
            // Decide which attach point goes with which hand by handedness
            var selectInter = inter as IXRSelectInteractor;
            bool isRight = selectInter != null && selectInter.handedness == InteractorHandedness.Right;

            // Pick the world-space target
            Vector3 target = isRight ? worldSecondary : worldPrimary;

            // Move the controller’s root so that the controller sits at the attach point
            // (Assumes the XR Controller GameObject is the parent of the interactor component.)
            var controllerRoot = inter.transform.parent;
            if (controllerRoot != null)
                controllerRoot.position = target;
        }
        
        //Debug.Log(dist);
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

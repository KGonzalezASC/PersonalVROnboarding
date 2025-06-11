using System;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public static class MyXRInteractionEvents
{

    public static event Action<IXRSelectInteractor, IXRSelectInteractable> TaskBasedInteraction;
    internal static void RaiseSelectForATask(IXRSelectInteractor interactor, IXRSelectInteractable interactable)
        => TaskBasedInteraction?.Invoke(interactor, interactable);
}
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using ZLinq;

[RequireComponent(typeof(XRGrabInteractable))]
public class LvlOneTsk1Helper : MonoBehaviour
{
    private XRGrabInteractable _self;
    private readonly HashSet<GameObject> _currentlyHeldObjects = new();

    [SerializeField] private InspectorTaskCompleter taskCompleter;
    
    private void OnEnable()
    {
        _self = GetComponent<XRGrabInteractable>();
        MyXRInteractionEvents.TaskBasedInteraction += OnAnyGrab;
    }

    private void OnDisable()
    {
        MyXRInteractionEvents.TaskBasedInteraction -= OnAnyGrab;
    }

    private void OnAnyGrab(IXRSelectInteractor arg1, IXRSelectInteractable arg2)
    {
        var grabbedGameObject = arg2.transform.gameObject;
        TaskMarshal.Instance.Print(line: $"Before: Currently held objects: {_currentlyHeldObjects.Count}");
        bool wasAdded = _currentlyHeldObjects.Add(grabbedGameObject);
        if (wasAdded)
        {
            arg2.selectExited.AddListener(RemoveFromHashset);
        }
        TaskMarshal.Instance.Print(line: $"After: Currently held objects: {_currentlyHeldObjects.Count}");
        if (_currentlyHeldObjects.Count >= 2)
        {
            string heldObjects = string.Join(", ",
                System.Linq.Enumerable.Select(_currentlyHeldObjects, obj => $"{obj.name}"));
            string msg = $"Two-handed grab detected! Holding: {heldObjects}";
            TaskMarshal.Instance.Print(msg);
            var results = taskCompleter.Complete(_currentlyHeldObjects.AsValueEnumerable().First(go => go !=_self.gameObject));
        }
    }

    private void RemoveFromHashset(SelectExitEventArgs args)
    {
        // Only remove from hashset if it's being released by a CONTROLLER (sockets are interactors and will fireregardless since the select exited listener was set)
        if (!CustomInteractionManager.IsControllerInteractor(args.interactorObject))
        {
            TaskMarshal.Instance.Print($"Ignoring non-controller release from: {args.interactorObject.GetType().Name} - keeping in hashset");
            return;
        }
        var releasedGameObject = args.interactableObject.transform.gameObject;
        bool wasRemoved = _currentlyHeldObjects.Remove(releasedGameObject);
        TaskMarshal.Instance.Print(args.interactorObject.transform.gameObject.name);
        TaskMarshal.Instance.Print(line: $"Released: {releasedGameObject.name}, was removed: {wasRemoved}");
        TaskMarshal.Instance.Print(line: $"Currently held objects: {_currentlyHeldObjects.Count}");
        // Remove the listener
        args.interactableObject.selectExited.RemoveListener(RemoveFromHashset);
    }
}
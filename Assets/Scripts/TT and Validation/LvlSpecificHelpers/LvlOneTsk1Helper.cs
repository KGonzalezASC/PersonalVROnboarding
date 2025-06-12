using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using ZLinq;

[RequireComponent(typeof(XRGrabInteractable))]
public class LvlOneTsk1Helper : MonoBehaviour
{
    XRGrabInteractable _thisInteractable;
    private readonly HashSet<XRGrabInteractable> _currentlyHeldObjects = new();

    [SerializeField] private InspectorTaskCompleter taskCompleter;

    void Awake()
    {
        _thisInteractable = _thisInteractable ? _thisInteractable : GetComponent<XRGrabInteractable>();
        //we need to explicitly drag in what completer(task) always since a gameobject can have multiple.
    }

    void OnEnable() => MyXRInteractionEvents.TaskBasedInteraction += OnAnyGrab;

    void OnDisable() => MyXRInteractionEvents.TaskBasedInteraction -= OnAnyGrab;


    private void OnAnyGrab(IXRSelectInteractor interactor, IXRSelectInteractable interactable)
    {
        var grabInteractable = interactable as XRGrabInteractable;
        // Add to our tracking set
        _currentlyHeldObjects.Add(grabInteractable);

        // Check if we now have 2 or more objects held
        if (_currentlyHeldObjects.Count >= 2)
        {
            string heldObjects = string.Join(", ",
                System.Linq.Enumerable.Select(_currentlyHeldObjects, obj => $"{obj.name}"));
            string msg = $"Two-handed grab detected! Holding: {heldObjects}";
            Debug.Log(msg);
            TaskMarshal.Instance.Print(msg);
            taskCompleter.Complete(_currentlyHeldObjects.AsValueEnumerable().First(go => go != _thisInteractable).gameObject);
        }

        // Listen for when this object is released
        grabInteractable!.selectExited.AddListener(OnObjectReleased);
    }

    public void OnObjectReleased(SelectExitEventArgs args)
    {
        var grabInteractable = args.interactableObject as XRGrabInteractable;
        if (grabInteractable != null)
        {
            _currentlyHeldObjects.Remove(grabInteractable);
            grabInteractable.selectExited.RemoveListener(OnObjectReleased);
        }
    }


}
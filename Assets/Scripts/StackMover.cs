using System;
using UnityEngine;
using UnityEngine.Serialization;

[RequireComponent(typeof(Rigidbody))]
//lets do it by forces as sockets will make it so that the stack could never fall
public sealed class StackMover : MonoBehaviour
{
    [Tooltip("Optional: drag in a Rigidbody. If left empty, this GameObject's Rigidbody will be used.")]
    [SerializeField] private Rigidbody rb;
    [SerializeField] private float force = 10f;
    [SerializeField] private InspectorTaskCompleter taskCompleter;


    void Awake()
    {
        // Cache the Rigidbody reference (allow override in Inspector)
        if (!rb)
            rb = GetComponent<Rigidbody>();
        // If you just want ONLY Y‐position + all rotations frozen:
        rb.constraints = RigidbodyConstraints.FreezePositionY
                         | RigidbodyConstraints.FreezeRotation;

    }

    private void FixedUpdate()
    {
        //MoveByForce(new Vector3(0f, 0f, force));
    }


    /// <summary>
    /// Applies an instantaneous impulse force in the XZ plane.
    /// </summary>
    /// <param name="force">Force vector; its Y component will be ignored.</param>
    public void MoveByForce(Vector3 force)
    {
        // Only apply XZ
        force.y = 0f;

        // If you want the impulse to ignore mass, consider ForceMode.VelocityChange
        rb.AddForce(force, ForceMode.Force);
    }

    private void OnCollisionEnter(Collision other)
    {
        if (taskCompleter.taskId == TaskId.SpreadSauce)
        {
             taskCompleter.Complete(other.gameObject); 
        }
        else
        {
            TaskMarshal.Instance.Print("Plop");
        }
    }
}
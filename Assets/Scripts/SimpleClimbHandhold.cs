using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Climbing;

/// <summary>
/// Attach to a prefab to make it a climbable handhold.
/// Automatically finds ClimbProvider and supports climbing via XR Toolkit.
/// </summary>
[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Rigidbody))]
public class SimpleClimbHandhold : ClimbInteractable
{
    protected override void Awake()
    {
        base.Awake();

        // Ensure Rigidbody is kinematic and uses gravity off
        var rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;

        // Automatically find the ClimbProvider if not already set
        if (climbProvider == null)
        {
            climbProvider = FindFirstObjectByType<ClimbProvider>();
#if UNITY_EDITOR
            if (climbProvider != null)
                Debug.Log($"ClimbProvider found for {name}", this);
            else
                Debug.LogWarning($"No ClimbProvider found in scene for {name}", this);
#endif
        }
    }

    protected override void OnValidate()
    {
        base.OnValidate(); // ✅ use override, not new
        if (climbTransform == null)
            climbTransform = transform;
    }
}

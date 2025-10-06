using UnityEngine;

public class ZeroGForObjects : MonoBehaviour
{
    [Tooltip("Assign the rigidbodies you want to float down slowly.")]
    public Rigidbody[] targets;

    [Header("Float behavior")]
    [Tooltip("Tiny downward acceleration (m/s^2). Earth is ~9.81. Try 0.05�0.5 for very slow float.")]
    public float downwardAcceleration = 0.25f;

    [Tooltip("Clamp downward speed to keep it floaty (m/s). Set 0 to disable clamping.")]
    public float maxDownwardSpeed = 0.4f;

    [Header("Damping (Rigidbody)")]
    public float linearDrag = 1.0f;     // slows overall drift
    public float angularDrag = 0.1f;    // tames spinning

    void Start()
    {
        foreach (var rb in targets)
        {
            if (!rb) continue;
            rb.useGravity = false;      // no real gravity
            rb.isKinematic = false;     // make sure forces can move it
            rb.linearDamping = linearDrag;
            rb.angularDamping = angularDrag;
        }
    }

    void FixedUpdate()
    {
        foreach (var rb in targets)
        {
            if (!rb) continue;

            // Apply a tiny, consistent "fake gravity"
            rb.AddForce(Vector3.down * downwardAcceleration, ForceMode.Acceleration);

            // Optional: cap downward speed for a soft float
            if (maxDownwardSpeed > 0f && rb.linearVelocity.y < -maxDownwardSpeed)
            {
                var v = rb.linearVelocity;
                v.y = -maxDownwardSpeed;
                rb.linearVelocity = v;
            }
        }
    }

    // Call this to restore normal gravity later.
    public void RestoreGravity()
    {
        foreach (var rb in targets)
        {
            if (!rb) continue;
            rb.useGravity = true;
        }
    }
}

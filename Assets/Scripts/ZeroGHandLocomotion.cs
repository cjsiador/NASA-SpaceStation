using UnityEngine;
using UnityEngine.XR;

[RequireComponent(typeof(Rigidbody))]
public class ZeroGHandLocomotion : MonoBehaviour
{
    [Header("Enable")]
    public bool isLocomotionActive = false;

    [Header("Hands")]
    public bool useLeftHand = true;
    public bool useRightHand = true;
    public bool requireGrip = true;

    [Header("Local Translation")]
    public float accelerationGain = 2.0f;   // m/s hand -> m/s^2 body
    public float maxSpeed = 4.0f;           // cap linear speed (m/s), <=0 = uncapped
    public float linearDrag = 0.5f;         // m/s^2 drag
    [Range(0f, 0.95f)] public float velocitySmoothing = 0.2f;
    public bool yawOnlyTranslate = true;    // flatten translation to local XZ

    [Header("Local Rotation (from controller twist)")]
    public bool enableRotation = true;
    public bool yawOnlyRotate = true;       // only rotate around local Y
    public float angularGain = 0.15f;       // rad/s hand -> rad/s^2 body
    public float maxAngularSpeed = 2.5f;    // cap (rad/s), ~143 deg/s; <=0 = uncapped
    public float angularDrag = 0.2f;        // rad/s^2 drag
    [Range(0f, 0.95f)] public float angularSmoothing = 0.2f;

    public enum CombineMode { Sum, Average };
    public CombineMode translationCombine = CombineMode.Sum;
    public CombineMode rotationCombine = CombineMode.Average;

    Rigidbody rb;

    // Smoothed world-space samples
    Vector3 vL, vR;   // linear velocities
    Vector3 wL, wR;   // angular velocities (rad/s)

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        rb.interpolation = RigidbodyInterpolation.Interpolate;

        // Allow rotation; optionally constrain for yaw-only
        ApplyRotationConstraints();
    }

    void OnValidate() => ApplyRotationConstraints();

    void ApplyRotationConstraints()
    {
        if (!enableRotation)
        {
            rb.constraints = RigidbodyConstraints.FreezeRotation;
            return;
        }

        if (yawOnlyRotate)
            rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        else
            rb.constraints = RigidbodyConstraints.None; // position still free; rotation all axes
    }

    void Update()
    {
        if (!isLocomotionActive) return;

        if (useLeftHand) SampleHand(XRNode.LeftHand, ref vL, ref wL);
        if (useRightHand) SampleHand(XRNode.RightHand, ref vR, ref wR);
    }

    void FixedUpdate()
    {
        if (!isLocomotionActive) return;

        // --- TRANSLATION (local) ---
        Vector3 localPush = Vector3.zero;
        int tContribs = 0;

        if (useLeftHand && HandActive(XRNode.LeftHand)) { localPush += -WorldToLocal(vL, yawOnlyTranslate); tContribs++; }
        if (useRightHand && HandActive(XRNode.RightHand)) { localPush += -WorldToLocal(vR, yawOnlyTranslate); tContribs++; }

        if (tContribs > 0 && translationCombine == CombineMode.Average)
            localPush /= tContribs;

        if (localPush.sqrMagnitude > 0f)
            rb.AddRelativeForce(localPush * accelerationGain, ForceMode.Acceleration);

        // Linear drag & cap (world-space)
        if (linearDrag > 0f && rb.linearVelocity.sqrMagnitude > 0f)
            rb.AddForce(-rb.linearVelocity.normalized * linearDrag, ForceMode.Acceleration);

        if (maxSpeed > 0f)
        {
            var v = rb.linearVelocity;
            float s = v.magnitude;
            if (s > maxSpeed) rb.linearVelocity = v * (maxSpeed / s);
        }

        // --- ROTATION (local torque) ---
        if (enableRotation)
        {
            Vector3 localTorque = Vector3.zero;
            int rContribs = 0;

            if (useLeftHand && HandActive(XRNode.LeftHand)) { localTorque += -WorldToLocal(wL, !yawOnlyRotate); rContribs++; }
            if (useRightHand && HandActive(XRNode.RightHand)) { localTorque += -WorldToLocal(wR, !yawOnlyRotate); rContribs++; }

            if (rContribs > 0 && rotationCombine == CombineMode.Average)
                localTorque /= rContribs;

            if (yawOnlyRotate)
                localTorque = new Vector3(0f, localTorque.y, 0f);

            if (localTorque.sqrMagnitude > 0f)
                rb.AddRelativeTorque(localTorque * angularGain, ForceMode.Acceleration);

            // Angular drag & cap (world-space angularVelocity)
            if (angularDrag > 0f && rb.angularVelocity.sqrMagnitude > 0f)
                rb.AddTorque(-rb.angularVelocity.normalized * angularDrag, ForceMode.Acceleration);

            if (maxAngularSpeed > 0f)
            {
                var w = rb.angularVelocity;
                float s = w.magnitude;
                if (s > maxAngularSpeed) rb.angularVelocity = w * (maxAngularSpeed / s);
            }
        }
    }

    // --- helpers ---

    // Convert world vector to local; optionally flatten Y (for translation) or preserve all (for rotation)
    Vector3 WorldToLocal(Vector3 worldVec, bool preserveY)
    {
        Vector3 local = rb.transform.InverseTransformDirection(worldVec);
        if (!preserveY) local.y = 0f;
        return local;
    }

    void SampleHand(XRNode node, ref Vector3 vSmooth, ref Vector3 wSmooth)
    {
        var dev = InputDevices.GetDeviceAtXRNode(node);
        if (!dev.isValid) return;

        if (dev.TryGetFeatureValue(CommonUsages.deviceVelocity, out Vector3 v))
            vSmooth = Smooth(vSmooth, v, velocitySmoothing);

        if (dev.TryGetFeatureValue(CommonUsages.deviceAngularVelocity, out Vector3 w))
            wSmooth = Smooth(wSmooth, w, angularSmoothing);
    }

    bool HandActive(XRNode node)
    {
        if (!requireGrip) return true;

        var dev = InputDevices.GetDeviceAtXRNode(node);
        if (!dev.isValid) return false;

        if (dev.TryGetFeatureValue(CommonUsages.gripButton, out bool gb)) return gb;
        if (dev.TryGetFeatureValue(CommonUsages.grip, out float gv)) return gv > 0.5f;
        return true;
    }

    static Vector3 Smooth(Vector3 current, Vector3 target, float a)
        => a <= 0f ? target : Vector3.Lerp(target, current, a);
}

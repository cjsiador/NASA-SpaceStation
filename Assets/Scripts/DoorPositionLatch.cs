using UnityEngine;
using UnityEngine.Events;

public class DoorPositionLatch : MonoBehaviour
{
    [Header("Door")]
    public Transform door;                 // Leave empty to use this object
    public bool useLocalRotation = true;   // Check local or world Y rotation

    [Header("Yaw window (degrees)")]
    public float minYawDeg = -92f;
    public float maxYawDeg = -89f;

    [Header("Levers")]
    public GameObject temporaryLever;      // will be disabled on lock
    public GameObject realLever;           // will be enabled on lock

    [Header("Hinge")]
    [Tooltip("Joint to remove when locked. Leave empty to auto-find on the door.")]
    public HingeJoint hingeToDisable;
    public bool autoFindHingeOnDoor = true;

    [Header("Freeze axes AFTER reaching yaw window")]
    public bool lockPosX = true;
    public bool lockPosY = true;
    public bool lockPosZ = true;
    public bool lockRotX = true;
    public bool lockRotY = true;
    public bool lockRotZ = true;

    Rigidbody _rb;
    bool _locked;

    void Reset() => door = transform;

    void Awake()
    {
        if (!door) door = transform;
        _rb = door.GetComponent<Rigidbody>();

        if (!hingeToDisable && autoFindHingeOnDoor)
        {
            hingeToDisable = door.GetComponent<HingeJoint>();
            if (!hingeToDisable) hingeToDisable = door.GetComponentInChildren<HingeJoint>();
        }
    }

    void Update()
    {
        if (_locked || !door) return;

        float yawSigned = GetSignedYawDeg(door, useLocalRotation);
        if (yawSigned >= minYawDeg && yawSigned <= maxYawDeg)
        {
            LockDoor();
        }
    }

    static float GetSignedYawDeg(Transform t, bool local)
    {
        float raw = local ? t.localEulerAngles.y : t.eulerAngles.y;
        // Convert to [-180, 180] to handle wraparound
        return Mathf.DeltaAngle(0f, raw);
    }

    void LockDoor()
    {
        if (_locked) return;
        _locked = true;

        // --- "Disable" the hinge by removing it ---
        if (hingeToDisable)
        {
            // Neutralize first (optional, for safety)
            hingeToDisable.useMotor = false;
            hingeToDisable.useSpring = false;
            hingeToDisable.useLimits = false;
            hingeToDisable.enableCollision = false;   // from Joint base class

            // Remove the component at end of frame
            Destroy(hingeToDisable);
        }

        // Swap levers
        if (temporaryLever) temporaryLever.SetActive(false);
        if (realLever) realLever.SetActive(true);

        // Freeze the door via Rigidbody constraints (if present)
        if (_rb)
        {
            _rb.linearVelocity = Vector3.zero;
            _rb.angularVelocity = Vector3.zero;

            // Preserve existing constraints and add freezes
            RigidbodyConstraints c = _rb.constraints;
            if (lockPosX) c |= RigidbodyConstraints.FreezePositionX;
            if (lockPosY) c |= RigidbodyConstraints.FreezePositionY;
            if (lockPosZ) c |= RigidbodyConstraints.FreezePositionZ;
            if (lockRotX) c |= RigidbodyConstraints.FreezeRotationX;
            if (lockRotY) c |= RigidbodyConstraints.FreezeRotationY;
            if (lockRotZ) c |= RigidbodyConstraints.FreezeRotationZ;
            _rb.constraints = c;
        }
        else
        {
            // Transform-only fallback: pin current pose each LateUpdate.
            _useTransformLock = true;
            _frozenPos = door.position;
            _frozenEuler = door.eulerAngles;
        }
    }

    // ---- Transform-only fallback (no Rigidbody) ----
    bool _useTransformLock;
    Vector3 _frozenPos, _frozenEuler;

    void LateUpdate()
    {
        if (!_useTransformLock) return;

        var p = door.position;
        if (lockPosX) p.x = _frozenPos.x;
        if (lockPosY) p.y = _frozenPos.y;
        if (lockPosZ) p.z = _frozenPos.z;
        door.position = p;

        var e = door.eulerAngles;
        if (lockRotX) e.x = _frozenEuler.x;
        if (lockRotY) e.y = _frozenEuler.y;
        if (lockRotZ) e.z = _frozenEuler.z;
        door.eulerAngles = e;
    }
}

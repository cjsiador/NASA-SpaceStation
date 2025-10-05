using UnityEngine;

public class GrabMove : MonoBehaviour
{
    public enum InputMode { Controllers, Hands }
    [Header("Rig & Anchors")]
    public Transform playerRoot;          // The transform to move (your rig root)
    public Transform leftHandAnchor;      // Left controller/hand anchor
    public Transform rightHandAnchor;     // Right controller/hand anchor

    [Header("Input")]
    public InputMode inputMode = InputMode.Controllers;
    [Tooltip("Hands: pinch threshold [0..1]. Lower = easier to detect pinch.")]
    [Range(0.1f, 0.9f)] public float pinchOn = 0.6f;
    [Range(0.05f, 0.95f)] public float pinchOff = 0.4f;

    [Header("Motion")]
    public bool lockY = true;             // Keep movement horizontal
    public bool enableTwoHandTurn = true; // Yaw rotate when two hands are grabbed
    public float turnSmoothing = 12f;     // Higher = snappier turn
    public float maxStep = 0.5f;          // Safety clamp per-frame movement (meters)

    // Optional scale (off by default)
    public bool enableTwoHandScale = false;
    public Vector2 scaleLimits = new Vector2(0.5f, 2.0f);

    // --- Internals ---
    CharacterController _cc;

    bool _leftGrabbing, _rightGrabbing;
    Vector3 _lStartWorld, _rStartWorld;
    Vector3 _rigStartPos;
    Quaternion _rigStartRot;
    Vector3 _twoStartVec;
    float _twoStartDist;
    float _rigStartScale = 1f;

#if OCULUS_INTEGRATION_PRESENT
    // OVR types are optional; compile guards let project build with/without them.
    OVRHand _leftHand, _rightHand;
#endif

    void Awake()
    {
        if (playerRoot == null) playerRoot = transform;
        _cc = playerRoot.GetComponent<CharacterController>();
#if OCULUS_INTEGRATION_PRESENT
        _leftHand  = leftHandAnchor  ? leftHandAnchor.GetComponentInChildren<OVRHand>()  : null;
        _rightHand = rightHandAnchor ? rightHandAnchor.GetComponentInChildren<OVRHand>() : null;
#endif
    }

    void Update()
    {

        Debug.Log("Right Controller Press : " + OVRInput.Get(OVRInput.Axis1D.PrimaryHandTrigger, OVRInput.Controller.RTouch));

        UpdateGrabStates();

        bool any = _leftGrabbing || _rightGrabbing;
        if (!any) return;

        // One-hand drag
        if (_leftGrabbing ^ _rightGrabbing)
        {
            Vector3 handNow = _leftGrabbing ? leftHandAnchor.position : rightHandAnchor.position;
            Vector3 handStart = _leftGrabbing ? _lStartWorld : _rStartWorld;

            Vector3 delta = handNow - handStart;
            if (lockY) delta = Flatten(delta);
            MoveRig(-ClampVec(delta, maxStep));
        }
        else // Two hands grabbed
        {
            Vector3 lNow = leftHandAnchor.position;
            Vector3 rNow = rightHandAnchor.position;

            // Translate by average hand movement
            Vector3 avgStart = 0.5f * (_lStartWorld + _rStartWorld);
            Vector3 avgNow = 0.5f * (lNow + rNow);
            Vector3 tDelta = avgNow - avgStart;
            if (lockY) tDelta = Flatten(tDelta);
            MoveRig(-ClampVec(tDelta, maxStep));

            // Yaw turn (optional)
            if (enableTwoHandTurn)
            {
                Vector3 vStart = _twoStartVec; vStart.y = 0f;
                Vector3 vNow = (rNow - lNow); vNow.y = 0f;
                if (vStart.sqrMagnitude > 1e-6f && vNow.sqrMagnitude > 1e-6f)
                {
                    float yaw = SignedYawBetween(vStart, vNow); // degrees
                    Quaternion target = Quaternion.AngleAxis(yaw, Vector3.up) * _rigStartRot;
                    playerRoot.rotation = Quaternion.Slerp(playerRoot.rotation, target, Time.deltaTime * turnSmoothing);
                }
            }

            // Scale (optional, disabled by default)
            if (enableTwoHandScale)
            {
                float dNow = Vector3.Distance(lNow, rNow);
                float scale = Mathf.Clamp((_twoStartDist <= 1e-6f) ? 1f : dNow / _twoStartDist, scaleLimits.x, scaleLimits.y);
                playerRoot.localScale = Vector3.one * (_rigStartScale * scale);
            }
        }
    }

    void UpdateGrabStates()
    {
        // Press logic with hysteresis for hands
        bool leftPressed = GetLeftPressed();
        bool rightPressed = GetRightPressed();

        // Transitions
        if (leftPressed && !_leftGrabbing) BeginLeftGrab();
        if (!leftPressed && _leftGrabbing) _leftGrabbing = false;

        if (rightPressed && !_rightGrabbing) BeginRightGrab();
        if (!rightPressed && _rightGrabbing) _rightGrabbing = false;

        // When both just became grabbing, snapshot two-hand state
        if (_leftGrabbing && _rightGrabbing && _twoStartVec == Vector3.zero)
        {
            _rigStartPos = playerRoot.position;
            _rigStartRot = playerRoot.rotation;
            _rigStartScale = playerRoot.localScale.x;
            _twoStartVec = (rightHandAnchor.position - leftHandAnchor.position);
            _twoStartDist = _twoStartVec.magnitude;
        }
        else if (!(_leftGrabbing && _rightGrabbing))
        {
            // Reset two-hand baselines when not both grabbing
            _twoStartVec = Vector3.zero;
            _twoStartDist = 0f;
        }
    }

    void BeginLeftGrab()
    {
        _leftGrabbing = true;
        _lStartWorld = leftHandAnchor.position;
        _rigStartPos = playerRoot.position;
        _rigStartRot = playerRoot.rotation;
        _rigStartScale = playerRoot.localScale.x;
    }

    void BeginRightGrab()
    {
        _rightGrabbing = true;
        _rStartWorld = rightHandAnchor.position;
        _rigStartPos = playerRoot.position;
        _rigStartRot = playerRoot.rotation;
        _rigStartScale = playerRoot.localScale.x;
    }

    void MoveRig(Vector3 worldDelta)
    {
        if (_cc != null && _cc.enabled)
        {
            // Preserve CharacterController grounding/step offset behavior
            _cc.Move(worldDelta);
        }
        else
        {
            playerRoot.position += worldDelta;
        }
    }

    // --- Input helpers ---
    bool GetLeftPressed()
    {
        if (inputMode == InputMode.Controllers)
        {
            return OVRInput.Get(OVRInput.Axis1D.PrimaryHandTrigger, OVRInput.Controller.LTouch) > 0.7f;


            //#if OCULUS_INTEGRATION_PRESENT
            //            return OVRInput.Get(OVRInput.Axis1D.PrimaryHandTrigger, OVRInput.Controller.LTouch) > 0.7f;
            //#else
            //            // Fallback: left mouse as a simple test
            //            return Input.GetMouseButton(0);
            //#endif
            //        }
            //        else // Hands
            //        {
            //#if OCULUS_INTEGRATION_PRESENT
            //            float s = _leftHand ? _leftHand.GetFingerPinchStrength(OVRHand.HandFinger.Index) : 0f;
            //            return _leftGrabbing ? (s > pinchOff) : (s > pinchOn);
            //#else
            //            return false;
            //#endif
        }
        return false;
    }

    bool GetRightPressed()
    {
        if (inputMode == InputMode.Controllers)
        {

            return OVRInput.Get(OVRInput.Axis1D.PrimaryHandTrigger, OVRInput.Controller.RTouch) > 0.7f;

            //#if OCULUS_INTEGRATION_PRESENT
            //            Debug.Log("Right Controller Press : " + OVRInput.Get(OVRInput.Axis1D.PrimaryHandTrigger, OVRInput.Controller.RTouch));
            //            return OVRInput.Get(OVRInput.Axis1D.PrimaryHandTrigger, OVRInput.Controller.RTouch) > 0.7f;
            //#else
            //            return Input.GetMouseButton(1);
            //#endif
            //        }
            //        else // Hands
            //        {
            //#if OCULUS_INTEGRATION_PRESENT
            //            float s = _rightHand ? _rightHand.GetFingerPinchStrength(OVRHand.HandFinger.Index) : 0f;
            //            return _rightGrabbing ? (s > pinchOff) : (s > pinchOn);
            //#else
            //            return false;
            //#endif
        }

        return false;
    }

    // --- Math helpers ---
    static Vector3 Flatten(Vector3 v) => new Vector3(v.x, 0f, v.z);
    static Vector3 ClampVec(Vector3 v, float max)
    {
        float m = v.magnitude;
        return (m > max && m > 1e-6f) ? v * (max / m) : v;
    }
    static float SignedYawBetween(Vector3 a, Vector3 b)
    {
        a.Normalize(); b.Normalize();
        float ang = Vector3.SignedAngle(a, b, Vector3.up);
        return ang;
    }
}

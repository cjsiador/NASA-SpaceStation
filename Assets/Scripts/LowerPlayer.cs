using System.Collections;
using UnityEngine;
using UnityEngine.Events; // optional for the event

public class LowerPlayer : MonoBehaviour
{
    public enum Axis { X, Y, Z }

    [Header("Movement")]
    public Axis axis = Axis.Y;
    public bool useLocalSpace = true;
    public float speed = 0.5f;
    public float targetValue = 0f;
    public float stopEpsilon = 0.001f;

    [Header("Start Behavior")]
    public bool lowerOnStart = false;

    [Header("Activation")]
    public GameObject movement;          // set this in Inspector
    public UnityEvent onReachedTarget;   // optional: hook other logic here

    Rigidbody _rb;
    Coroutine _moveRoutine;
    bool _reached; // prevents double-fire

    void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        if (_rb != null) _rb.interpolation = RigidbodyInterpolation.Interpolate;
    }

    void Start()
    {
        if (lowerOnStart) Lower();
    }

    public void Lower()
    {
        _reached = false;
        Stop();
        _moveRoutine = StartCoroutine(MoveToTarget(targetValue));
    }

    public void Stop()
    {
        if (_moveRoutine != null)
        {
            StopCoroutine(_moveRoutine);
            _moveRoutine = null;
        }
    }

    IEnumerator MoveToTarget(float target)
    {
        var wait = _rb ? new WaitForFixedUpdate() : null;

        while (true)
        {
            Vector3 pos = useLocalSpace ? transform.localPosition : transform.position;
            float curr = axis == Axis.X ? pos.x : axis == Axis.Y ? pos.y : pos.z;

            // reached?
            if (Mathf.Abs(curr - target) <= stopEpsilon)
            {
                // snap exactly once, then activate & exit
                pos = SetAxis(pos, target);
                ApplyPosition(pos);

                if (!_reached)
                {
                    _reached = true;
                    if (movement) movement.SetActive(true);
                    onReachedTarget?.Invoke();
                }
                yield break;
            }

            float dir = Mathf.Sign(target - curr);
            float step = speed * (_rb ? Time.fixedDeltaTime : Time.deltaTime) * dir;
            float next = Mathf.MoveTowards(curr, target, Mathf.Abs(step));

            pos = SetAxis(pos, next);
            ApplyPosition(pos);

            if (_rb) yield return wait;
            else yield return null;
        }
    }

    Vector3 SetAxis(Vector3 v, float value)
    {
        switch (axis)
        {
            case Axis.X: v.x = value; break;
            case Axis.Y: v.y = value; break;
            case Axis.Z: v.z = value; break;
        }
        return v;
    }

    void ApplyPosition(Vector3 pos)
    {
        if (useLocalSpace)
        {
            if (_rb) _rb.MovePosition(transform.parent ? transform.parent.TransformPoint(pos) : pos);
            else transform.localPosition = pos;
        }
        else
        {
            if (_rb) _rb.MovePosition(pos);
            else transform.position = pos;
        }
    }

    public void SetTargetAndLower(float newTargetValue)
    {
        targetValue = newTargetValue;
        Lower();
    }
}

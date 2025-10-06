using UnityEngine;
using UnityEngine.XR;

public class ControllerVelocity : MonoBehaviour
{
    [Tooltip("LeftHand or RightHand")]
    public XRNode leftNode = XRNode.RightHand;
    public XRNode rightNode = XRNode.RightHand;

    public Vector3 leftLinearVelocityLocal;   // m/s, controller/local space
    public Vector3 leftAngularVelocityLocal;  // rad/s, controller/local space
    
    public Vector3 leftLinearVelocityWorld;
    public Vector3 leftAngularVelocityWorld;

    // Optional: reference to your XR rig root to convert to world space
    public Transform xrOriginOrRig; // e.g., your XROrigin (or its CameraFloorOffsetObject)

    public Vector3 rightLinearVelocityLocal;   // m/s, controller/local space
    public Vector3 rightAngularVelocityLocal;  // rad/s, controller/local space

    public Vector3 rightLinearVelocityWorld;
    public Vector3 rightAngularVelocityWorld;

    public InputDevice leftDevice;
    public InputDevice rightDevice;

    void Update()
    {
        leftDevice = InputDevices.GetDeviceAtXRNode(leftNode);
        rightDevice = InputDevices.GetDeviceAtXRNode(rightNode);

        LeftControllerData(leftDevice);
        RightControllerData(rightDevice);
    }

    void LeftControllerData(InputDevice device)
    {
        if (!device.isValid) return;

        if (device.TryGetFeatureValue(CommonUsages.deviceVelocity, out var v))
            leftLinearVelocityLocal = v;

        if (device.TryGetFeatureValue(CommonUsages.deviceAngularVelocity, out var w))
            leftAngularVelocityLocal = w;

        // Convert to world space if your rig is scaled/rotated
        if (xrOriginOrRig != null)
        {
            leftLinearVelocityWorld = xrOriginOrRig.TransformDirection(leftLinearVelocityLocal);
            leftAngularVelocityWorld = xrOriginOrRig.TransformDirection(leftAngularVelocityLocal);
        }
        else
        {
            leftLinearVelocityWorld = leftLinearVelocityLocal;
            leftAngularVelocityWorld = leftAngularVelocityLocal;
        }
    }

    void RightControllerData(InputDevice device)
    {
        if (!device.isValid) return;

        if (device.TryGetFeatureValue(CommonUsages.deviceVelocity, out var v))
            rightLinearVelocityLocal = v;

        if (device.TryGetFeatureValue(CommonUsages.deviceAngularVelocity, out var w))
            rightAngularVelocityLocal = w;

        // Convert to world space if your rig is scaled/rotated
        if (xrOriginOrRig != null)
        {
            rightLinearVelocityWorld = xrOriginOrRig.TransformDirection(rightLinearVelocityLocal);
            rightAngularVelocityWorld = xrOriginOrRig.TransformDirection(rightAngularVelocityLocal);
        }
        else
        {
            rightLinearVelocityWorld = rightLinearVelocityLocal;
            rightAngularVelocityWorld = rightAngularVelocityLocal;
        }
    }
}

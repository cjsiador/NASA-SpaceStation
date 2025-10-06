using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhoneVibrate : MonoBehaviour
{
    [Header("Vibration Settings")]
    public float shakeAngle = 10f;       // max angle to rotate (left/right)
    public float shakeSpeed = 5f;        // how fast it vibrates

    [Header("Arrow & Player Proximity")]
    public GameObject arrowObject;       // arrow pointing where to go
    public Transform player;             // XR Rig root or the main camera
    public float activateDistance = 1.25f; // meters from arrow to enable movetool
    public GameObject movetool;          // UI/controls to show when close

    private bool isGrabbed = false;
    private bool hasActivatedMoveTool = false;
    private float shakeTimer = 0f;
    private Quaternion baseRotation;

    void Start()
    {
        baseRotation = transform.localRotation;

        // If player isn't assigned, try main camera (XR Origin's camera)
        if (!player && Camera.main) player = Camera.main.transform;

        // Optional: keep movetool hidden until we're close
        if (movetool) movetool.SetActive(false);
    }

    void Update()
    {
        // 1) Phone "vibration" motion stays the same
        if (!isGrabbed)
        {
            shakeTimer += Time.deltaTime * shakeSpeed;
            float yRotation = Mathf.Sin(shakeTimer) * shakeAngle;
            transform.localRotation = baseRotation * Quaternion.Euler(0, yRotation, 0);
        }

        // 2) Proximity check to arrow -> activate movetool
        if (!hasActivatedMoveTool && player && arrowObject)
        {
            float dist = Vector3.Distance(player.position, arrowObject.transform.position);
            if (dist <= activateDistance)
            {
                if (movetool) movetool.SetActive(true);
                hasActivatedMoveTool = true;
            }
        }
    }

    // Hook this to the XR Grab Interactable SelectEntered event on the PHONE (if desired)
    public void StopShaking()
    {
        isGrabbed = true;

        // Hide arrow once the player grabs the phone (as you had)
        if (arrowObject) arrowObject.SetActive(false);

        // If you also want movetool to show immediately on grab, keep this:
        if (movetool && !hasActivatedMoveTool)
        {
            movetool.SetActive(true);
            arrowObject.SetActive(true);
            hasActivatedMoveTool = true;
        }
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        if (arrowObject)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(arrowObject.transform.position, activateDistance);
        }
    }
#endif
}

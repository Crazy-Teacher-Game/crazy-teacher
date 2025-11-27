using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainDice : MonoBehaviour
{
    [SerializeField] private Transform diceToRotate; // Assign your UI 3D dice here

    private GameManager gameManager;

    [Header("Input -> Rotation")]
    [SerializeField] private float inputThreshold = 0.5f;
    [SerializeField] private float stepDuration = 0.15f; // seconds for a 90° step

    private bool gateCenterReturn = true; // require stick to return to center between steps
    private bool isRotating;

    [Header("Dice Face Mapping (set according to your model)")]
    [SerializeField] private int faceUp = 1;    // number on +Y local
    [SerializeField] private int faceDown = 6;  // number on -Y local
    [SerializeField] private int faceRight = 3; // number on +X local
    [SerializeField] private int faceLeft = 4;  // number on -X local
    [SerializeField] private int faceForward = 2; // number on +Z local
    [SerializeField] private int faceBack = 5;    // number on -Z local

    // Logical state tracking: current accumulated rotation
    private Quaternion logicalRotation = Quaternion.identity;

    void Start()
    {
        Transform target = diceToRotate != null ? diceToRotate : transform;
        // Initialize logical rotation with current rotation
        logicalRotation = target.rotation;
    }

    void Update()
    {
        Transform target = diceToRotate != null ? diceToRotate : transform;

        float h = Input.GetAxisRaw("P1_Horizontal");
        float v = Input.GetAxisRaw("P1_Vertical");

        // Reset gate when returned to near-center
        if (Mathf.Abs(h) < 0.2f && Mathf.Abs(v) < 0.2f)
        {
            gateCenterReturn = true;
        }

        if (isRotating || !gateCenterReturn)
        {
            return;
        }

        // Dice rolling controls - rotations in WORLD space
        // Horizontal (Q/D): Rotate around world Y axis
        // Vertical (Z/S): Rotate around world X axis

        // Right (D key): Rotate +90° around world Y
        if (h > inputThreshold)
        {
            StartCoroutine(RotateStep(target, Vector3.up, 90f));
            gateCenterReturn = false;
            return;
        }

        // Left (Q key): Rotate -90° around world Y
        if (h < -inputThreshold)
        {
            StartCoroutine(RotateStep(target, Vector3.up, -90f));
            gateCenterReturn = false;
            return;
        }

        // Up (Z key): Rotate +90° around world X
        if (v > inputThreshold)
        {
            StartCoroutine(RotateStep(target, Vector3.right, 90f));
            gateCenterReturn = false;
            return;
        }

        // Down (S key): Rotate -90° around world X
        if (v < -inputThreshold)
        {
            StartCoroutine(RotateStep(target, Vector3.right, -90f));
            gateCenterReturn = false;
            return;
        }
    }

    private IEnumerator RotateStep(Transform target, Vector3 axis, float degrees)
    {
        isRotating = true;

        Quaternion start = logicalRotation;
        // Apply rotation around world axis
        Quaternion delta = Quaternion.AngleAxis(degrees, axis);
        Quaternion end = delta * start;

        float t = 0f;
        while (t < stepDuration)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / stepDuration);
            target.rotation = Quaternion.Slerp(start, end, k);
            yield return null;
        }

        // Update logical rotation and apply it (normalized)
        logicalRotation = end;
        logicalRotation.Normalize();
        target.rotation = logicalRotation;

        isRotating = false;
    }

    public int GetTopFace()
    {
        Transform target = diceToRotate != null ? diceToRotate : transform;
        // Determine which local axis aligns most with world FORWARD (camera direction)
        // Using negative Z because Unity's forward is +Z, and we want the face facing the camera
        Vector3 localForward = target.InverseTransformDirection(-Vector3.forward);

        float ax = Mathf.Abs(localForward.x);
        float ay = Mathf.Abs(localForward.y);
        float az = Mathf.Abs(localForward.z);

        int result;

        if (ay >= ax && ay >= az)
        {
            result = localForward.y >= 0f ? faceUp : faceDown;
        }
        else if (ax >= ay && ax >= az)
        {
            result = localForward.x >= 0f ? faceRight : faceLeft;
        }
        else
        {
            // z is largest
            result = localForward.z >= 0f ? faceForward : faceBack;
        }

        return result;
    }

   
 
}

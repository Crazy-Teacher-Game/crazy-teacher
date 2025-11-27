using UnityEngine;
using TMPro;

/// <summary>
/// Helper script to debug and configure dice face mapping.
/// Attach this to your dice GameObject alongside MainDice.
/// Press Space to log the current orientation and detected face.
/// </summary>
public class DiceFaceDebugger : MonoBehaviour
{
    [SerializeField] private MainDice mainDice;
    [SerializeField] private TMP_Text debugText; // Optional: display on screen

    void Start()
    {
        if (mainDice == null)
        {
            mainDice = GetComponent<MainDice>();
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            LogCurrentFace();
        }

        if (debugText != null && mainDice != null)
        {
            int face = mainDice.GetTopFace();
            Vector3 rot = transform.rotation.eulerAngles;
            debugText.text = $"Face: {face}\nRot: ({rot.x:F0}, {rot.y:F0}, {rot.z:F0})";
        }
    }

    void LogCurrentFace()
    {
        if (mainDice == null) return;

        int detectedFace = mainDice.GetTopFace();
        Vector3 rotation = transform.rotation.eulerAngles;
        Vector3 localUp = transform.InverseTransformDirection(Vector3.up);

        Debug.Log("=== DICE FACE DEBUG ===");
        Debug.Log($"Detected Face: {detectedFace}");
        Debug.Log($"Rotation (Euler): ({rotation.x:F1}, {rotation.y:F1}, {rotation.z:F1})");
        Debug.Log($"Local Up Vector: ({localUp.x:F2}, {localUp.y:F2}, {localUp.z:F2})");
        Debug.Log("======================");

        // Guide for user
        Debug.Log("INSTRUCTIONS:");
        Debug.Log("1. Rotate the dice to show each face (1-6) pointing UP");
        Debug.Log("2. Press SPACE to log which face is detected");
        Debug.Log("3. If the detected face doesn't match the visible number, adjust the face mapping in MainDice inspector");
        Debug.Log("4. Standard die: 1 opposite 6, 2 opposite 5, 3 opposite 4");
    }

    void OnGUI()
    {
        // Draw simple on-screen help
        GUI.Label(new Rect(10, 10, 300, 40), "Press SPACE to debug current dice face");

        if (mainDice != null)
        {
            int face = mainDice.GetTopFace();
            GUI.Label(new Rect(10, 50, 300, 40), $"Current detected face: {face}");
        }
    }
}

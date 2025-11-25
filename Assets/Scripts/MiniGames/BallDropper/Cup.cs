using UnityEngine;
using TMPro;

public class Cup : MonoBehaviour
{
    private static int globalScore = 0;
    public TMP_Text scoreText;
    public Transform rotationCenter;
    public float rotationSpeedDegreesPerSecond = 70f;
    public Vector3 rotationAxis = Vector3.up;
    void Start()
    {
        UpdateScoreUI();
    }
    void Update()
    {
        if (rotationSpeedDegreesPerSecond == 0f || rotationCenter == null)
            return;

        float deltaDegrees = rotationSpeedDegreesPerSecond * Time.deltaTime;
        transform.RotateAround(
            rotationCenter.position,
            rotationAxis.normalized,
            deltaDegrees
        );
    }
    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("ball"))
        {
            globalScore++;
            UpdateScoreUI();
        }
    }
    void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            scoreText.text = "Points: " + globalScore;
        }
    }
}

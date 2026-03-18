using UnityEngine;
using TMPro;

public class Cup : MonoBehaviour
{
    private static int globalScore = 0;
    private static bool isWon = false;
    public TMP_Text scoreText;
    public Transform rotationCenter;
    public float rotationSpeedDegreesPerSecond = 70f;
    public Vector3 rotationAxis = Vector3.up;
    public AudioSource audioSource;
    public AudioClip rightInHoleSound;
    public int maxScore = 4;
    void Start()
    {
        globalScore = 0;
        isWon = false;
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
        if (GameManager.Instance == null) return;

        if (globalScore >= 4 && !isWon)
        {
            GameManager.Instance.NotifyWin();
            isWon = true;
            return;
        }
    }
    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("ball"))
        {
            globalScore++;
            Destroy(other.gameObject);
            UpdateScoreUI();
            float minPitch = 0.5f;
            float maxPitch = 1.0f;
            float t = Mathf.InverseLerp(1, 4, globalScore);
            audioSource.pitch = Mathf.Lerp(minPitch, maxPitch, t);
            audioSource.PlayOneShot(rightInHoleSound);
        }
    }

    void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            scoreText.text = globalScore + "/" + maxScore;
        }
    }
}

using UnityEngine;
using TMPro;

public class Cup : MonoBehaviour
{
    private static int globalScore = 0;
    public TMP_Text scoreText;
    private bool isWon = false;
    public GameManager gameManager;
    void Start()
    {
        UpdateScoreUI();
        gameManager.OnTimerEnded += HandleTimeout;
        gameManager.OnMinigameWon += AfterWin;
        gameManager.OnMinigameFailed += AfterFail;
    }
    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("ball"))
        {
            globalScore++;
            UpdateScoreUI();
        }
    }

    public void OnPlayerSucceeded()
    {
        gameManager.NotifyWin();
    }
    void HandleTimeout()
    {
        gameManager.NotifyFail();
    }
    void AfterWin()
    {
        gameManager.AddRound();
    }

    void AfterFail()
    {
        gameManager.LoseLife();
    }

    void Update()
    {
        if (globalScore == 4 && !isWon)
        {
            gameManager.NotifyWin();
            isWon = true;
            return;
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

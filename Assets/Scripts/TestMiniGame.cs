using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestMiniGame : MonoBehaviour
{
    [SerializeField] float durationSeconds = 5f;
    [SerializeField] float minDurationSeconds = 3f;
    [SerializeField] public GameManager gameManager;

    void Start()
    {
        gameManager.StartTimer(durationSeconds, minDurationSeconds);
        gameManager.OnTimerEnded += HandleTimeout;
    }

    void OnDestroy()
    {
        if (gameManager != null)
        {
            gameManager.OnTimerEnded -= HandleTimeout;
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
}

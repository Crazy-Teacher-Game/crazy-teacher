using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Platform : MonoBehaviour
{
    public float durationSeconds = 12f;
    public float minDurationSeconds = 8f;
    void Start()
    {
        if (!GameManager.Instance.TimerRunning)
        {
            GameManager.Instance.StartTimer(durationSeconds, minDurationSeconds);
        }
    }

    void OnEnable()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnTimerEnded += HandleTimeout;
            GameManager.Instance.OnMinigameWon += AfterWin;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("ball"))
        {
            Destroy(other.gameObject);
        }
    }

    void HandleTimeout()
    {
        GameManager.Instance.NotifyFail();
        GameObject[] balls = GameObject.FindGameObjectsWithTag("ball");
        foreach (GameObject ball in balls)
        {
            Destroy(ball);
        }
    }
    void AfterWin()
    {
        GameObject[] balls = GameObject.FindGameObjectsWithTag("ball");
        foreach (GameObject ball in balls)
        {
            Destroy(ball);
        }
    }

    void OnDisable()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnTimerEnded -= HandleTimeout;
            GameManager.Instance.OnMinigameWon -= AfterWin;
        }
    }
}

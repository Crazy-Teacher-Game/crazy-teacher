using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Platform : MonoBehaviour
{
    public float durationSeconds = 15f;
    void Start()
    {
        GameManager.Instance.StartTimer(durationSeconds);
        GameManager.Instance.OnTimerEnded += HandleTimeout;
        GameManager.Instance.OnMinigameWon += AfterWin;
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
        GameManager.Instance.AddRound();
        GameObject[] balls = GameObject.FindGameObjectsWithTag("ball");
        foreach (GameObject ball in balls)
        {
            Destroy(ball);
        }
    }
}

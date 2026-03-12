using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TimerGame : MonoBehaviour
{
    // pour le GM
    public float gameDuration = 30f;
    public float minDuration = 30f;

    public TMP_Text timerToFindText;
    public TMP_Text playerTimerText;

    private int timerToFind;
    private float playerTimer = 0f;
    private bool timerRunning = false;
    private bool gameEnded = false;
    private float startDelay = 1f;
    private float hideTextTime = 1.5f;

    void Start()
    {
        timerToFind = Random.Range(5, 11);
        timerToFindText.text = timerToFind.ToString();
        playerTimerText.text = "0.00";

        GameManager.Instance.StartTimer(gameDuration, minDuration);
        StartCoroutine(StartAfterDelay());

    }

    IEnumerator StartAfterDelay()
    {
        yield return new WaitForSeconds(startDelay);
        timerRunning = true;

        yield return new WaitForSeconds(hideTextTime);
        if (timerRunning)
        {
            SetTextOpacity(playerTimerText, 0f);
        }
    }

    void Update()
    {
        if (gameEnded) return;

        if (timerRunning)
        {
            playerTimer += Time.deltaTime;
            playerTimerText.text = playerTimer.ToString("F2");

            if (Input.GetButtonDown("P1_B1"))
            {
                timerRunning = false;
                SetTextOpacity(playerTimerText, 1f);
                //arrondi à l'entier le plus proche pour pas montrer notre marge d'erreur
                if (Mathf.Abs(playerTimer - timerToFind) <= 0.7f)
                {
                    playerTimerText.text = Mathf.CeilToInt(playerTimer) + "s";
                }
                else
                {
                    playerTimerText.text = playerTimer.ToString("F2") + "s";
                }

                gameEnded = true;
                StartCoroutine(GameEnd());
            }
        }
    }

    public IEnumerator GameEnd()
    {
                yield return new WaitForSeconds(2f);

                if (Mathf.Abs(playerTimer - timerToFind) <= 0.5f)
                {
                    GameManager.Instance.NotifyWin();
                }
                else
                {
                    GameManager.Instance.NotifyFail();
                }
    }

    void SetTextOpacity(TMP_Text text, float alpha)
    {
        Color c = text.color;
        c.a = alpha;
        text.color = c;
    }
}

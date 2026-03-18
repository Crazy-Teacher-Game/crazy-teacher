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

    public GameObject tennaDefault;
    public GameObject tennaWinPrefab;
    public AudioSource audioSource;
    public AudioClip buttonSound;
    public AudioClip winSound;
    public AudioClip loseSound;
    public Animator moveAnimator;

    public GameObject winPrefab;
    public GameObject losePrefab;

    public GameObject HidersPrefab;

    public TMP_Text timer2;
    public TMP_Text timer3;
    public TMP_Text timer4;

    private int timerToFind;
    private float playerTimer = 0f;
    private bool timerRunning = false;
    private bool gameEnded = false;
    private float startDelay = 1f;
    private float hideTextTime = 5f;

    void Start()
    {
        timerToFind = Random.Range(5, 11);
        timerToFindText.text = timerToFind.ToString();
        playerTimerText.text = "0.00";

        GameManager.Instance.OnTimerEnded += HandleTimerEnded;
        GameManager.Instance.StartTimer(gameDuration, minDuration);
        StartCoroutine(StartAfterDelay());
    }

    void OnDestroy()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnTimerEnded -= HandleTimerEnded;
    }

    void HandleTimerEnded()
    {
        if (gameEnded) return;
        gameEnded = true;
        GameManager.Instance.NotifyFail();
    }

    IEnumerator StartAfterDelay()
    {
        yield return new WaitForSeconds(startDelay);
        timerRunning = true;

        yield return new WaitForSeconds(hideTextTime);
        if (timerRunning)
        {
            SetTextOpacity(playerTimerText, 0f);
            SetTextOpacity(timer2, 0f);
            SetTextOpacity(timer3, 0f);
            SetTextOpacity(timer4, 0f);
        }
    }

    void Update()
    {
        if (gameEnded) return;

        if (timerRunning)
        {
            playerTimer += Time.deltaTime;
            playerTimerText.text = playerTimer.ToString("F2");
            timer2.text = playerTimer.ToString("F2");
            timer3.text = playerTimer.ToString("F2");
            timer4.text = playerTimer.ToString("F2");

            if (Input.GetButtonDown("P1_B1"))
            {
                HidersPrefab.SetActive(false);
                if (audioSource != null && buttonSound != null)
                    audioSource.PlayOneShot(buttonSound);
                if (moveAnimator != null)
                    moveAnimator.SetTrigger("Move");
                timerRunning = false;
                SetTextOpacity(playerTimerText, 1f);
                timer2.text = Random.Range(5, timerToFind).ToString();
                timer3.text = Random.Range(5, timerToFind).ToString();
                timer4.text = Random.Range(5, timerToFind).ToString();
                //arrondi à l'entier le plus proche pour pas montrer notre marge d'erreur
                if (Mathf.Abs(playerTimer - timerToFind) <= 0.7f)
                {
                    playerTimerText.text = Mathf.CeilToInt(playerTimer).ToString();
                    tennaDefault.SetActive(false);
                    tennaWinPrefab.SetActive(true);
                    //winPrefab.SetActive(true);
                    // if (audioSource != null && winSound != null)
                       // audioSource.PlayOneShot(winSound);
                }
                else
                {
                    playerTimerText.text = playerTimer.ToString("F2");
                    //losePrefab.SetActive(true);
                    //if (audioSource != null && loseSound != null)
                        // audioSource.PlayOneShot(loseSound);
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

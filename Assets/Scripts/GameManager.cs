using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class GameManager : MonoBehaviour
{
    [Header("Config Initiale")]
    [SerializeField] public int startingLives = 2;
    [SerializeField] public int difficulty = 1;
    [SerializeField] private int lives;
    public int Lives { get; private set; }

    [Header("Timer Partagé")]
    [SerializeField] private bool timerRunning;
    public bool TimerRunning { get; private set; }
    [SerializeField] private float remainingTime;
    public float RemainingTime { get; private set; }
    [SerializeField] private float duration;
    public float Duration { get; private set; }
    private Coroutine _timerCo;
    public event Action OnTimerEnded;
    public event Action<float> OnTimerTick;

    [Header("Leaderboard")]
    [SerializeField] private int roundsPlayed;
    public int RoundsPlayed { get; private set; }

    [Header("UI")]
    [SerializeField] public TimerUI timerUI;
    [SerializeField] private LivesUI livesUI;

    //ACTION À EFFECTUER À LA FIN D'UN MINI-JEU
    public event Action OnMinigameWon;
    public event Action OnMinigameFailed;

    void Awake()
    {
        Lives = startingLives;
        livesUI?.SetLives(Lives);
        RoundsPlayed = 0;
        if (livesText != null)
        {
            livesText.text = "Vies: " + Lives;
        }
        Debug.Log($"[GameManager] Awake - Lives={Lives}, Difficulty={difficulty}");
    }

    public void AddRound()
    {
        RoundsPlayed++;
        //ON POURRA RAJOUTER D'AUTRES ACTIONS AU CHANGEMENT DE ROUND ICI
    }

    //GESTION DES VIES
    public void LoseLife()
    {
        Lives--;
        Debug.Log($"[GameManager] LoseLife called - Lives now {Lives}, livesUI={(livesUI!=null)}, livesText={(livesText!=null)}");
        livesUI?.SetLives(Lives);
        if (livesText != null)
        {
            livesText.text = "Vies: " + Lives;
            Debug.Log($"[GameManager] Updated livesText to: {livesText.text}");
        }
        Debug.Log($"[GameManager] LoseLife -> {Lives} left");
    }

    public void ResetLives()
    {
        Lives = startingLives;
        livesUI?.SetLives(Lives);
        if (livesText != null)
        {
            livesText.text = "Vies: " + Lives;
        }
    }

    //GESTION DU TIMER
    public void StartTimer(float seconds)
    {
        StopTimer(); //pour être sur qu'on en a pas deux qui tournent
        Duration = Mathf.Max(0f, seconds);
        RemainingTime = Duration;
        TimerRunning = true;
        timerUI?.Show(Duration);
        _timerCo = StartCoroutine(CoTimer());
        Debug.Log($"[GameManager] StartTimer {Duration}s");
    }

    public void StopTimer()
    {
        if (_timerCo != null) StopCoroutine(_timerCo);
        _timerCo = null;
        TimerRunning = false;
        timerUI?.Hide();
        Debug.Log("[GameManager] StopTimer");
    }

    IEnumerator CoTimer()
    {
        while (RemainingTime > 0f)
        {
            RemainingTime -= Time.deltaTime;
            if (RemainingTime < 0f) RemainingTime = 0f;
            timerUI?.UpdateTime(RemainingTime, Duration);
            OnTimerTick?.Invoke(RemainingTime);
            yield return null;
        }
        TimerRunning = false;
        OnTimerEnded?.Invoke();
        Debug.Log("[GameManager] Timer ended event fired");
    }

    //ACTIONS QUI SE LANCENT QUAND ON GAGNE OU PERD UN MINI-JEU
    public void NotifyWin()
    {
        StopTimer();
        OnMinigameWon?.Invoke();
        Debug.Log("[GameManager] Minigame WON");
    }

    public void NotifyFail()
    {
        StopTimer();
        OnMinigameFailed?.Invoke();
        Debug.Log("[GameManager] Minigame FAILED");
    }

    //TOUT CE QUI EST EN DESSOUS : NE PAS UTILISER - A REFACTORER SUR MON MINI-JEU

    public TMP_Text livesText;

    void Update()
    {
        if (livesText != null)
        {
            livesText.text = "Vies: " + Lives;
        }
    }
}

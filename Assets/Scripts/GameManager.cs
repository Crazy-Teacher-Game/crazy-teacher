using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System;
using System.IO;

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
    private ScenesLoader scenesLoader;
    private AudioListener _activeAudioListener;

    // Back to menu manager
    private float afkTimer = 0f;
    private readonly float timeBeforeKick = 200f; //seconds
    private bool afk = false;
    private float quitTimer = 0f;
    private readonly float timeBeforeQuit = 2f;
    private bool quit = false;

    [DllImport("__Internal")]
    private static extern void BackToMenu();

    // Now you can call BackToMenu() in your methods
    public void GoBackToMenu()
    {
        BackToMenu();
        //Application.Quit();
    }
    //[END] Back to menu manager

    private string currentGame = "";

    // public void getRandomGame()
    // {
    //     string[] scenesList = Directory.GetFiles("Assets/Scenes/MiniGames", "*.unity");
    //     for (int i = 0; i < scenesList.Length; i++)
    //     {
    //         scenesList[i] = Path.GetFileNameWithoutExtension(scenesList[i]);
    //     }
    //     System.Random rand = new System.Random();
    //     int index = rand.Next(scenesList.Length);
    //     if (currentGame == scenesList[index])
    //     {
    //         index = (index + 1) % scenesList.Length;
    //     }
    //     if (Input.GetButton("P1_B6"))
    //     {   
    //         scenesLoader.LoadMiniGame(scenesList[index]);
    //     }
    //     currentGame = scenesList[index];
    // }

    void Awake()
    {
        scenesLoader = GetComponent<ScenesLoader>();
        Lives = startingLives;
        livesUI?.SetLives(Lives);
        RoundsPlayed = 0;
        Debug.Log($"[GameManager] Awake - Lives={Lives}, Difficulty={difficulty}");

        EnsureSingleAudioListener();
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += HandleSceneLoaded;
        EnsureSingleAudioListener();
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= HandleSceneLoaded;
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
        Debug.Log($"[GameManager] LoseLife called - Lives now {Lives}, livesUI={(livesUI != null)}");
        livesUI?.SetLives(Lives);
        Debug.Log($"[GameManager] LoseLife -> {Lives} left");
    }

    public void ResetLives()
    {
        Lives = startingLives;
        livesUI?.SetLives(Lives);
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

    void Update()
    {
        if (!HasExactlyOneActiveAudioListener())
        {
            EnsureSingleAudioListener();
        }

        if (Input.GetButtonDown("P1_B6"))
        {
            string nextGame = GetRandomGame();
            //ou alors le jeu que vous voulez tester comme ça :
            // nextGame = "PopTheBottle";
            scenesLoader.LoadMiniGame(nextGame);
            currentGame = nextGame;
        }
        if (Input.GetButtonDown("P1_B3"))
        {
            if (currentGame != "")
            {
                scenesLoader.UnloadMiniGame(currentGame);
            }
        }

        // Back to menu manager
        if (Input.GetButton("P1_Vertical") ||
            Input.GetButton("P1_Horizontal") ||
            Input.GetButton("P2_Vertical") ||
            Input.GetButton("P2_Horizontal") ||
            Input.GetButton("P1_Start") ||
            Input.GetButton("P1_B1") ||
            Input.GetButton("P1_B2") ||
            Input.GetButton("P1_B3") ||
            Input.GetButton("P1_B4") ||
            Input.GetButton("P1_B5") ||
            Input.GetButton("P1_B6") ||
            Input.GetButton("P2_Start") ||
            Input.GetButton("P2_B1") ||
            Input.GetButton("P2_B2") ||
            Input.GetButton("P2_B3") ||
            Input.GetButton("P2_B4") ||
            Input.GetButton("P2_B5") ||
            Input.GetButton("P2_B6") ||
            Input.GetButton("Coin"))
        {
            afkTimer = 0f;
        }

        if (afkTimer < timeBeforeKick)
        {
            afk = false;
        }

        if (afk || Input.GetButton("Coin"))
        {
            quit = true;
        }
        else
        {
            quit = false;
            quitTimer = 0f;
        }
        //[END] Back to menu manager
    }

    private string GetRandomGame()
    {
        string[] scenesList = Directory.GetFiles("Assets/Scenes/MiniGames", "*.unity");
        for (int i = 0; i < scenesList.Length; i++)
        {
            scenesList[i] = Path.GetFileNameWithoutExtension(scenesList[i]);
        }

        System.Random rand = new System.Random();
        int index = rand.Next(scenesList.Length);

        if (currentGame == scenesList[index])
        {
            index = (index + 1) % scenesList.Length;
        }

        return scenesList[index];

    }

    void FixedUpdate()
    {
        // Back to menu manager
        if (afkTimer < timeBeforeKick)
        {
            afkTimer += Time.fixedDeltaTime;
        }
        else
        {
            if (!afk)
            {
                Debug.Log("You will be kicked in 2 seconds");
                afk = true;
            }
        }

        if (quit)
        {
            quitTimer += Time.fixedDeltaTime;
        }
        else
        {
            quitTimer = 0f;
        }

        if (quitTimer >= timeBeforeQuit)
        {
            Debug.Log("QUITTING...");
            GoBackToMenu();
        }
        //[END] Back to menu manager
    }

    private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        EnsureSingleAudioListener();
    }

    private void EnsureSingleAudioListener()
    {
        var listeners = FindObjectsOfType<AudioListener>();
        AudioListener primary = null;

        for (int i = 0; i < listeners.Length; i++)
        {
            var listener = listeners[i];
            if (listener == null) continue;

            if (listener.isActiveAndEnabled)
            {
                if (primary == null)
                {
                    primary = listener;
                    continue;
                }

                listener.enabled = false;
                continue;
            }

            if (primary == null)
            {
                primary = listener;
                if (!primary.enabled) primary.enabled = true;
                continue;
            }

            listener.enabled = false;
        }

        if (primary == null)
        {
            primary = AttachListenerToMainCamera();
        }

        if (primary == null)
        {
            primary = GetComponent<AudioListener>() ?? gameObject.AddComponent<AudioListener>();
            primary.enabled = true;
        }

        if (!primary.enabled)
        {
            primary.enabled = true;
        }

        _activeAudioListener = primary;
    }

    private AudioListener AttachListenerToMainCamera()
    {
        var mainCamera = Camera.main;
        if (mainCamera == null) return null;

        var listener = mainCamera.GetComponent<AudioListener>();
        if (listener == null)
        {
            listener = mainCamera.gameObject.AddComponent<AudioListener>();
        }

        listener.enabled = true;
        return listener;
    }

    private bool HasExactlyOneActiveAudioListener()
    {
        var listeners = FindObjectsOfType<AudioListener>();
        int activeCount = 0;

        for (int i = 0; i < listeners.Length; i++)
        {
            var listener = listeners[i];
            if (listener == null) continue;

            if (listener.enabled && listener.gameObject.activeInHierarchy)
            {
                activeCount++;
                if (activeCount > 1)
                {
                    return false;
                }
            }
        }

        return activeCount == 1;
    }
}

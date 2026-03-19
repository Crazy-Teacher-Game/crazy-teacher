using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System;
using Anatidae;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    [Header("Config Initiale")]
    [SerializeField] public int startingLives = 2;
    [SerializeField] public int difficulty = 1;
    [SerializeField] private int _lives;
    public int Lives { get => _lives; private set => _lives = value; }

    [Header("Debug")]

    [SerializeField] public TMP_Text difficultyDebugText;

    [Header("Difficulté progressive")]
    [SerializeField] private float difficultyFactor = 0f;
    public float DifficultyFactor => difficultyFactor;
    [SerializeField] private int gamesBeforeDifficultyIncrease = 3;

    [Header("Timer Partagé")]
    [SerializeField] private bool _timerRunning;
    public bool TimerRunning { get => _timerRunning; private set => _timerRunning = value; }
    [SerializeField] private float _remainingTime;
    public float RemainingTime { get => _remainingTime; private set => _remainingTime = value; }
    [SerializeField] private float _duration;
    public float Duration { get => _duration; private set => _duration = value; }
    private Coroutine _timerCo;
    public event Action OnTimerEnded;
    public event Action<float> OnTimerTick;

    [Header("Leaderboard")]
    [SerializeField] private int _roundsPlayed;
    public int RoundsPlayed { get => _roundsPlayed; private set => _roundsPlayed = value; }
    public int currentRound = 0;
    public int Score { get; private set; }
    [SerializeField] private GameOverManager gameOverManager;

    [Header("UI")]
    [SerializeField] public TimerUI timerUI;
    [SerializeField] private LivesUI livesUI;

    [Header("Minigame Result UI")]
    [SerializeField] private GameObject winSprite;
    [SerializeField] private GameObject failSprite;
    [SerializeField] private float resultDisplayTime = 1.5f;

    //ACTION À EFFECTUER À LA FIN D'UN MINI-JEU
    public event Action OnMinigameWon;
    public event Action OnMinigameFailed;
    private ScenesLoader scenesLoader;
    private StartMenuLoader startMenuLoader;
    private AudioListener _activeAudioListener;
    private AudioSource audioSource;
    public AudioClip winSound;
    [SerializeField][Range(0f, 3f)] private float winSoundVolume = 1f;
    public AudioClip failSound;
    [SerializeField][Range(0f, 3f)] private float failSoundVolume = 1f;

    [Header("Audio - Menu Theme")]
    [SerializeField] private AudioClip menuIntroClip;
    [SerializeField][Range(0f, 3f)] private float menuIntroVolume = 1f;
    [SerializeField] private AudioClip menuLoopClip;
    [SerializeField][Range(0f, 3f)] private float menuLoopVolume = 1f;

    [Header("Audio - Game Theme")]
    [SerializeField] private AudioClip gameThemeClip;
    [SerializeField][Range(0f, 3f)] private float gameThemeVolume = 1f;
    [SerializeField] private float gameThemeFadeDuration = 0.5f;

    [Header("Audio - Game Over")]
    [SerializeField] private AudioClip gameOverSound1;
    [SerializeField][Range(0f, 3f)] private float gameOverSound1Volume = 1f;
    [SerializeField] private AudioClip gameOverSound2;
    [SerializeField][Range(0f, 3f)] private float gameOverSound2Volume = 1f;
    [SerializeField] private float gameOverSound2Delay = 0f;

    [Header("Audio - SFX")]
    [SerializeField] private AudioClip goSound;
    [SerializeField][Range(0f, 3f)] private float goSoundVolume = 1f;
    [SerializeField] private AudioClip descriptionSound;
    [SerializeField][Range(0f, 3f)] private float descriptionSoundVolume = 1f;

    private AudioSource musicSource;
    private AudioSource gameThemeSource;
    private Coroutine _menuThemeCo;
    private Coroutine _gameThemeFadeCo;

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
    public ControlType CurrentControlType { get; private set; }
    private bool isGameOver = false;
    private bool gameStarted = false;
    private bool replayTextShown = false;
    private bool isTransitioning = false;
    private bool minigameEnded = false;
    private bool highscoreInputWasShown = false;

    public TMP_Text descriptionText;
    private bool isDescriptionShowing = false;

    private static readonly string[] MinigameSceneNames =
    {
        "DropTheFish",
        "PopTheBottle",
        "SlotMachine",
        "MentalMath",
        "Dice",
        "FlashTheCar",
        "Loop",
        "ExplodeTheBalloon",
        "TimerGame",
        "TriPommePoire",

    };
    private List<string> _minigamePlaylist;
    private int _minigamePlaylistIndex;

    void Start()
    {
    }

    public void StartGame()
    {
        if (!gameStarted)
        {
            gameStarted = true;
            StopMenuTheme();
            if (goSound != null)
                audioSource.PlayOneShot(goSound, goSoundVolume);
            LoadNextMiniGame();
        }
    }


    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        scenesLoader = GetComponent<ScenesLoader>();
        startMenuLoader = GetComponent<StartMenuLoader>();
        if (startMenuLoader == null)
            startMenuLoader = FindObjectOfType<StartMenuLoader>();
        Lives = startingLives;
        livesUI?.SetLives(Lives);
        RoundsPlayed = 0;
        BuildAndShufflePlaylist();

        EnsureSingleAudioListener();

        audioSource = GetComponent<AudioSource>();

        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.playOnAwake = false;
        musicSource.loop = false;

        gameThemeSource = gameObject.AddComponent<AudioSource>();
        gameThemeSource.playOnAwake = false;
        gameThemeSource.loop = true;
    }

    public void RegisterGameOverManager(GameOverManager manager)
    {
        gameOverManager = manager;
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
        currentRound++;

        // Augmente la difficulté de 5% après chaque jeu, sauf les 3 premiers
        if (currentRound > gamesBeforeDifficultyIncrease)
        {
            difficultyFactor = Mathf.Min(1f, difficultyFactor + 0.05f);
        }
    }

    //GESTION DES VIES
    public void LoseLife()
    {
        Lives--;
        livesUI?.SetLives(Lives);
    }

    public void ResetLives()
    {
        Lives = startingLives;
        livesUI?.SetLives(Lives);
    }

    //GESTION DE LA DESCRIPTION
    public void ShowDescription(string sceneName)
    {
        string description = GameDescriptionDatabase.GetDescription(sceneName);
        if (!string.IsNullOrEmpty(description) && descriptionText != null)
        {
            StartCoroutine(CoShowDescription(description));
        }
    }

    public IEnumerator ShowDescriptionCoroutine(string sceneName)
    {
        string description = GameDescriptionDatabase.GetDescription(sceneName);
        if (!string.IsNullOrEmpty(description) && descriptionText != null)
        {
            yield return CoShowDescription(description);
        }
    }

    private IEnumerator CoShowDescription(string description)
    {
        isDescriptionShowing = true;
        descriptionText.text = description;
        descriptionText.gameObject.SetActive(true);

        if (descriptionSound != null)
            audioSource.PlayOneShot(descriptionSound, descriptionSoundVolume);

        // Relance l'animation si un Animator est présent
        Animator animator = descriptionText.GetComponent<Animator>();
        if (animator != null)
        {
            animator.Play(0, -1, 0f);
        }

        // Passer en Overlay pour garantir l'affichage par-dessus les sprites du mini-jeu
        Canvas canvas = descriptionText.GetComponentInParent<Canvas>();
        RenderMode originalMode = RenderMode.ScreenSpaceCamera;
        Camera originalCam = null;
        if (canvas != null)
        {
            originalMode = canvas.renderMode;
            originalCam = canvas.worldCamera;
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 999;
        }

        yield return new WaitForSeconds(1.85f);
        //retrait du repassage en world space, comme ça on a le timer au dessus de tous les jeux en tout

        descriptionText.gameObject.SetActive(false);
        isDescriptionShowing = false;
    }

    //GESTION DU TIMER
    public void StartTimer(float maxSeconds, float minSeconds)
    {
        StartCoroutine(CoStartTimer(maxSeconds, minSeconds));
    }

    private IEnumerator CoStartTimer(float maxSeconds, float minSeconds)
    {
        while (isDescriptionShowing)
            yield return null;

        StopTimer();
        float computed = (maxSeconds - minSeconds) * (1f - difficultyFactor) + minSeconds;
        Duration = Mathf.Max(0f, computed);
        RemainingTime = Duration;
        TimerRunning = true;
        timerUI?.Show(Duration);
        _timerCo = StartCoroutine(CoTimer());

        //debug
        if (difficultyDebugText != null)
            difficultyDebugText.text = $"DifficultyFactor: {difficultyFactor:0.0}";
    }

    public void RemoveTime(float seconds)
    {
        RemainingTime = Mathf.Max(0f, RemainingTime - seconds);
    }

    public void StopTimer()
    {
        if (_timerCo != null) StopCoroutine(_timerCo);
        _timerCo = null;
        TimerRunning = false;
        timerUI?.Hide();
    }

    IEnumerator CoTimer()
    {
        while (RemainingTime > 0f)
        {
            RemainingTime -= Time.deltaTime;
            timerUI?.UpdateTime(RemainingTime, Duration);
            OnTimerTick?.Invoke(RemainingTime);
            yield return null;
        }

        if (minigameEnded) yield break;

        TimerRunning = false;
        OnTimerEnded?.Invoke();
    }

    //ACTIONS QUI SE LANCENT QUAND ON GAGNE OU PERD UN MINI-JEU
    public void NotifyWin()
    {
        if (minigameEnded) return;
        minigameEnded = true;
        if (isTransitioning) return;
        isTransitioning = true;
        StartCoroutine(CoNotifyWin());
    }

    private IEnumerator CoNotifyWin()
    {
        StopTimer();
        FadeOutGameTheme();
        OnMinigameWon?.Invoke();
        int gained = Mathf.RoundToInt((1f + difficultyFactor) * 10f);
        Score += gained;
        if (winSound != null)
             audioSource.PlayOneShot(winSound, winSoundVolume);
        yield return StartCoroutine(ShowWinResult());
        yield return scenesLoader.UnloadMiniGame(currentGame);
        isTransitioning = false;
        AddRound();
        LoadNextMiniGame();
    }

    public void NotifyFail()
    {
        if (minigameEnded) return;
        minigameEnded = true;
        if (isTransitioning) return;
        isTransitioning = true;
        StartCoroutine(CoNotifyFail());
    }

    private IEnumerator CoNotifyFail()
    {
        StopTimer();
        FadeOutGameTheme();
        OnMinigameFailed?.Invoke();
        if (failSound != null)
            audioSource.PlayOneShot(failSound, failSoundVolume);
        yield return StartCoroutine(ShowFailResult());
        LoseLife();
        if (Lives > 0)
        {
            yield return scenesLoader.UnloadMiniGame(currentGame);
            isTransitioning = false;
            LoadNextMiniGame();
        }
        else
        {
            yield return scenesLoader.UnloadMiniGame(currentGame);
            isTransitioning = false;
            GameOver();
        }
    }

    public void SetControlType(ControlType type)
    {
        CurrentControlType = type;
    }

    private void LoadNextMiniGame()
    {
        minigameEnded = false;
        string nextGame = GetNextGameInPlaylist();
        currentGame = nextGame;
        PlayGameTheme();
        scenesLoader.LoadMiniGame(nextGame);
    }

    private void BuildAndShufflePlaylist()
    {
        _minigamePlaylist = new List<string>(MinigameSceneNames);
        ShufflePlaylist(_minigamePlaylist);
        _minigamePlaylistIndex = 0;
    }

    private static void ShufflePlaylist(List<string> list)
    {
        var rng = new System.Random();
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            (list[k], list[n]) = (list[n], list[k]);
        }
    }

    private string GetNextGameInPlaylist()
    {
        if (_minigamePlaylist == null || _minigamePlaylist.Count == 0)
            BuildAndShufflePlaylist();

        if (_minigamePlaylistIndex >= _minigamePlaylist.Count)
        {
            string lastPlayed = _minigamePlaylist[_minigamePlaylist.Count - 1];
            ShufflePlaylist(_minigamePlaylist);
            if (_minigamePlaylist.Count > 1 && _minigamePlaylist[0] == lastPlayed)
            {
                int swapIndex = new System.Random().Next(1, _minigamePlaylist.Count);
                (_minigamePlaylist[0], _minigamePlaylist[swapIndex]) =
                    (_minigamePlaylist[swapIndex], _minigamePlaylist[0]);
            }
            _minigamePlaylistIndex = 0;
        }

        return _minigamePlaylist[_minigamePlaylistIndex++];
    }

    private void GameOver()
    {
        isGameOver = true;
        StartCoroutine(CoGameOver());
    }

    private IEnumerator CoGameOver()
    {
        if (gameOverSound1 != null)
            audioSource.PlayOneShot(gameOverSound1, gameOverSound1Volume);
        if (gameOverSound2 != null)
            StartCoroutine(CoPlayGameOverSound2());

        yield return scenesLoader.LoadGameOverScene();
        yield return new WaitForSeconds(3f);
        LoadHighscoreInterface();
    }

    private IEnumerator CoPlayGameOverSound2()
    {
        if (gameOverSound2Delay > 0f)
            yield return new WaitForSeconds(gameOverSound2Delay);
        audioSource.PlayOneShot(gameOverSound2, gameOverSound2Volume);
    }

    // ─── Audio ───

    public void PlayMenuTheme()
    {
        if (_menuThemeCo != null) StopCoroutine(_menuThemeCo);
        _menuThemeCo = StartCoroutine(CoPlayMenuTheme());
    }

    private IEnumerator CoPlayMenuTheme()
    {
        if (menuIntroClip != null)
        {
            musicSource.clip = menuIntroClip;
            musicSource.loop = false;
            musicSource.volume = menuIntroVolume;
            musicSource.Play();
            yield return new WaitForSeconds(menuIntroClip.length);
        }
        if (menuLoopClip != null)
        {
            musicSource.clip = menuLoopClip;
            musicSource.loop = true;
            musicSource.volume = menuLoopVolume;
            musicSource.Play();
        }
    }

    public void StopMenuTheme()
    {
        if (_menuThemeCo != null) StopCoroutine(_menuThemeCo);
        _menuThemeCo = null;
        musicSource.Stop();
    }

    public void PlaySFX(AudioClip clip, float volume = 1f)
    {
        if (clip != null)
            audioSource.PlayOneShot(clip, volume);
    }

    private void PlayGameTheme()
    {
        if (gameThemeClip == null) return;
        if (_gameThemeFadeCo != null) StopCoroutine(_gameThemeFadeCo);
        _gameThemeFadeCo = null;

        // pitch = 1 + difficultyFactor → de x1 (diff=0) à x2 (diff=1)
        // AudioSource.pitch contrôle vitesse ET pitch simultanément
        float pitch = 1f + difficultyFactor;

        gameThemeSource.clip = gameThemeClip;
        gameThemeSource.volume = gameThemeVolume;
        gameThemeSource.pitch = pitch;
        gameThemeSource.loop = true;
        gameThemeSource.Play();
    }

    private void FadeOutGameTheme()
    {
        if (_gameThemeFadeCo != null) StopCoroutine(_gameThemeFadeCo);
        _gameThemeFadeCo = StartCoroutine(CoFadeOutGameTheme());
    }

    private IEnumerator CoFadeOutGameTheme()
    {
        if (gameThemeSource == null || !gameThemeSource.isPlaying)
            yield break;

        float startVolume = gameThemeSource.volume;
        float elapsed = 0f;

        while (elapsed < gameThemeFadeDuration)
        {
            elapsed += Time.deltaTime;
            gameThemeSource.volume = Mathf.Lerp(startVolume, 0f, elapsed / gameThemeFadeDuration);
            yield return null;
        }

        gameThemeSource.Stop();
        gameThemeSource.volume = gameThemeVolume;
        _gameThemeFadeCo = null;
    }

    private void RestartGame()
    {
        isGameOver = false;
        gameStarted = false;
        scenesLoader.UnloadGameOverScene();
        Lives = startingLives;
        livesUI?.SetLives(Lives);
        currentRound = 0;
        difficultyFactor = 0f;
        RoundsPlayed = 0;
        BuildAndShufflePlaylist();
        Score = 0;
        replayTextShown = false;
        isTransitioning = false;
        highscoreInputWasShown = false;
        OnTimerEnded = null;
        OnTimerTick = null;
        OnMinigameWon = null;
        OnMinigameFailed = null;

        if (startMenuLoader != null)
            startMenuLoader.ReloadMenu();
        else
            StartGame();
    }

    void Update()
    {
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

        if (isGameOver)
        {
            if (HighscoreManager.IsHighscoreInputScreenShown)
                highscoreInputWasShown = true;

            if (highscoreInputWasShown && !HighscoreManager.IsHighscoreInputScreenShown && !replayTextShown && gameOverManager != null)
            {
                gameOverManager.ShowReplayText();
                replayTextShown = true;
            }

            if (highscoreInputWasShown && !HighscoreManager.IsHighscoreInputScreenShown &&
                (Input.GetButtonDown("P1_B3") || Input.GetButtonDown("P2_B3")))
            {
                RestartGame();
            }

            return;
        }
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
            GoBackToMenu();
        }
        //[END] Back to menu manager
    }

    private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        EnsureSingleAudioListener();
        if (scene.name == "GameOverScene")
        {
            gameOverManager = FindObjectOfType<GameOverManager>();
        }
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

    public void RegisterTimerUI(TimerUI ui)
    {
        timerUI = ui;
    }

    public void RegisterLivesUI(LivesUI ui)
    {
        livesUI = ui;
        livesUI.SetLives(Lives);
    }

    private void LoadHighscoreInterface()
    {
        HighscoreManager.ShowHighscoreInput(Score);
    }

    private IEnumerator ShowWinResult()
    {
        winSprite.SetActive(true);
        yield return new WaitForSeconds(resultDisplayTime);
        winSprite.SetActive(false);
    }

    private IEnumerator ShowFailResult()
    {
        failSprite.SetActive(true);
        yield return new WaitForSeconds(resultDisplayTime);
        failSprite.SetActive(false);
    }

    public static class Config
    {
        public const string API_URL = "https://api.crazy-teacher.beauget.fr/leaderboard";
        public const string API_KEY = "ce7bc3a59c4bb8380cb893a573294f0a525b685a301f26e64722c02b94d06623";
    }

}

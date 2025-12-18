using UnityEngine;
using TMPro;
using System.Collections;

public class DiceNumberPrompt : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private MainDice mainDice; // Assign the MainDice in the scene
    [SerializeField] private TMP_Text targetText; // Assign your TMP Text
    private GameManager gameManager;

    [Header("Behavior")]
    [SerializeField] private bool autoNextOnMatch = true;
    [SerializeField] private float nextDelay = 0.1f;

    [Header("Game Settings")]
    [SerializeField] private float gameDuration = 30f; // Timer duration in seconds
    [SerializeField] private int targetSuccesses = 5; // Number of correct faces to win
    [SerializeField] private bool standaloneMode = false; // Enable to test without GameManager

    private int targetFace;
    private Coroutine _pendingNext;
    private int successCount = 0;
    private bool gameActive = false;
    private float standaloneTimer = 0f;
    private bool hasMatchedCurrentTarget = false;

    void Start()
    {
        if (mainDice == null)
        {
            mainDice = FindObjectOfType<MainDice>();
        }

        // Find GameManager - it might take a frame to load if in a different scene
        StartCoroutine(InitializeGameManager());
    }

    private IEnumerator InitializeGameManager()
    {
        // Try to find GameManager immediately
        gameManager = FindObjectOfType<GameManager>(true);

        // If not found, wait a few frames (scene might be loading)
        if (gameManager == null)
        {
            yield return null; // Wait one frame
            gameManager = FindObjectOfType<GameManager>(true);
        }

        if (gameManager == null)
        {
            standaloneMode = true;
        }
        else
        {
            // Subscribe to timer end event
            gameManager.OnTimerEnded += OnTimerEnded;
        }

        // Start the game immediately
        StartGame();
    }

    void OnDestroy()
    {
        // Unsubscribe from events to prevent memory leaks
        if (gameManager != null)
        {
            gameManager.OnTimerEnded -= OnTimerEnded;
        }
    }

    private void StartGame()
    {
        successCount = 0;
        gameActive = true;
        standaloneTimer = gameDuration;

        // Ensure text is enabled before setting content
        if (targetText != null)
        {
            targetText.enabled = true;
        }

        // Generate first target immediately
        GenerateNewTarget();

        if (!standaloneMode && gameManager != null)
        {
            gameManager.StartTimer(gameDuration);
        }
    }

    private void OnTimerEnded()
    {
        if (!gameActive) return;

        gameActive = false;

        if (!standaloneMode && gameManager != null)
        {
            gameManager.NotifyFail();
        }
    }

    private int lastDetectedFace = -1;

    void Update()
    {
        if (mainDice == null || !gameActive) return;

        // Handle standalone timer
        if (standaloneMode)
        {
            standaloneTimer -= Time.deltaTime;
            if (standaloneTimer <= 0f)
            {
                OnTimerEnded();
                return;
            }
        }

        int top = mainDice.GetTopFace();

        // Log only when the detected face changes (to avoid spam)
        if (top != lastDetectedFace)
        {
            lastDetectedFace = top;
        }

        if (top == targetFace)
        {
            if (autoNextOnMatch && _pendingNext == null && !hasMatchedCurrentTarget)
            {
                hasMatchedCurrentTarget = true;
                _pendingNext = StartCoroutine(CoNextAfterDelay());
            }
        }
        else
        {
            if (_pendingNext != null)
            {
                StopCoroutine(_pendingNext);
                _pendingNext = null;
            }
        }
    }

    public void GenerateNewTarget()
    {
        targetFace = Random.Range(1, 7); // 1..6 inclusive
        hasMatchedCurrentTarget = false; // Reset match flag for new target

        if (targetText != null)
        {
            targetText.text = targetFace.ToString();
            targetText.enabled = true;
            Canvas.ForceUpdateCanvases();
        }
    }

    private IEnumerator CoNextAfterDelay()
    {
        yield return new WaitForSeconds(nextDelay);
        _pendingNext = null;

        // Increment success count
        successCount++;

        // Check if player has won
        if (successCount >= targetSuccesses)
        {
            gameActive = false;

            if (!standaloneMode && gameManager != null)
            {
                gameManager.NotifyWin();
            }
        }
        else
        {
            // Generate next target
            GenerateNewTarget();
        }
    }
}



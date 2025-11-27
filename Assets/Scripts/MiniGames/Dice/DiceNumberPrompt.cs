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
            Debug.LogWarning("[DiceNumberPrompt] GameManager not found - running in STANDALONE MODE for testing");
            standaloneMode = true;
        }
        else
        {
            Debug.Log("[DiceNumberPrompt] GameManager found!");
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
            Debug.Log($"[DiceNumberPrompt] Game started with GameManager - Need {targetSuccesses} correct faces in {gameDuration}s");
        }
        else
        {
            Debug.Log($"[DiceNumberPrompt] Game started in STANDALONE mode - Need {targetSuccesses} correct faces in {gameDuration}s");
        }
    }

    private void OnTimerEnded()
    {
        if (!gameActive) return;

        gameActive = false;
        Debug.Log($"[DiceNumberPrompt] Timer ended - Score: {successCount}/{targetSuccesses}");

        if (!standaloneMode && gameManager != null)
        {
            gameManager.NotifyFail();
        }
        else
        {
            Debug.Log("[DiceNumberPrompt] GAME OVER - Time's up! (Standalone mode)");
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
            Debug.Log($"[DiceNumberPrompt] Detected face changed: {top} (target: {targetFace})");
            lastDetectedFace = top;
        }

        if (top == targetFace)
        {
            if (autoNextOnMatch && _pendingNext == null && !hasMatchedCurrentTarget)
            {
                Debug.Log($"[DiceNumberPrompt] MATCH! Top face {top} == target {targetFace}");
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
        Debug.Log($"[DiceNumberPrompt] Generated new target: {targetFace}");

        if (targetText != null)
        {
            targetText.text = targetFace.ToString();
            targetText.enabled = true;
            Canvas.ForceUpdateCanvases();
            Debug.Log($"[DiceNumberPrompt] Updated UI text to: {targetFace}");
        }
        else
        {
            Debug.LogWarning("[DiceNumberPrompt] targetText is NULL! Please assign TMP_Text in Inspector.");
        }
    }

    private IEnumerator CoNextAfterDelay()
    {
        yield return new WaitForSeconds(nextDelay);
        _pendingNext = null;

        // Increment success count
        successCount++;
        Debug.Log($"[DiceNumberPrompt] Success! Score: {successCount}/{targetSuccesses}");

        // Check if player has won
        if (successCount >= targetSuccesses)
        {
            gameActive = false;

            if (!standaloneMode && gameManager != null)
            {
                gameManager.NotifyWin();
                Debug.Log("[DiceNumberPrompt] Player won! Notifying GameManager.");
            }
            else
            {
                Debug.Log("[DiceNumberPrompt] YOU WIN! (Standalone mode)");
            }
        }
        else
        {
            // Generate next target
            GenerateNewTarget();
        }
    }
}



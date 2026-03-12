using UnityEngine;
using TMPro;
using System.Collections;

public class DiceNumberPrompt : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private MainDice mainDice; // Assign the MainDice in the scene
    [SerializeField] private TMP_Text targetText; // Assign your TMP Text
    [SerializeField] private TMP_Text progressText; // Affiche "X/5" pour la progression
    private GameManager gameManager;

    [Header("Behavior")]
    [SerializeField] private bool autoNextOnMatch = true;
    [SerializeField] private float nextDelay = 0.1f;

    [Header("Game Settings")]
    [SerializeField] private float gameDuration = 30f; // Timer max duration in seconds
    [SerializeField] private float gameMinDuration = 15f; // Timer min duration in seconds
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

        if (progressText == null)
        {
            var progressGO = GameObject.Find("progress-value");
            if (progressGO != null) progressText = progressGO.GetComponent<TMP_Text>();
        }

        StartCoroutine(InitializeGameManager());
    }

    private IEnumerator InitializeGameManager()
    {
        gameManager = FindObjectOfType<GameManager>(true);

        if (gameManager == null)
        {
            yield return null;
            gameManager = FindObjectOfType<GameManager>(true);
        }

        if (gameManager == null)
        {
            standaloneMode = true;
        }
        else
        {
            gameManager.OnTimerEnded += OnTimerEnded;
        }

        StartGame();
    }

    void OnDestroy()
    {
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

        if (targetText != null)
        {
            targetText.enabled = true;
        }

        GenerateNewTarget();
        UpdateProgressDisplay();

        if (!standaloneMode && gameManager != null)
        {
            gameManager.StartTimer(gameDuration, gameMinDuration);
        }
    }

    private void UpdateProgressDisplay()
    {
        if (progressText != null)
        {
            progressText.text = $"{successCount}/{targetSuccesses}";
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
        int currentFace = mainDice.GetTopFace();
        
        // Generate a target different from the current face
        do
        {
            targetFace = Random.Range(1, 7); // 1..6 inclusive
        } while (targetFace == currentFace);
        
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
        UpdateProgressDisplay();

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



using UnityEngine;

public class MiniGame_CalculManager : MonoBehaviour
{
    [SerializeField] private CalculLogic calculLogic;
    [SerializeField] private CalculUIManager calculUIManager;

    private int wrongAttempts;
    private int correctAnswers;
    private const int REQUIRED_CORRECT_ANSWERS = 3;
    private bool gameEnded = false;

    public int CorrectAnswers => correctAnswers;
    public int RequiredCorrectAnswers => REQUIRED_CORRECT_ANSWERS;

    void Start()
    {
        Debug.Log($"[MiniGame_CalculManager] Start() called - Instance ID: {GetInstanceID()}");
        if (calculLogic == null) calculLogic = FindObjectOfType<CalculLogic>();
        if (calculUIManager == null) calculUIManager = FindObjectOfType<CalculUIManager>();

        Debug.Log($"[MiniGame_CalculManager] Subscribing to OnTimerEnded - GameManager.Instance={(GameManager.Instance != null)}");
        GameManager.Instance.OnTimerEnded += HandleTimerEnded;
        GameManager.Instance.StartTimer(15f, 8f);
        wrongAttempts = 0;
        correctAnswers = 0;
        StartCoroutine(DelayedStart());
        Debug.Log($"[MiniGame_CalculManager] Start() complete - calculLogic={(calculLogic != null)}, calculUIManager={(calculUIManager != null)}");
    }

    private System.Collections.IEnumerator DelayedStart()
    {
        yield return null;
        GenerateNewCalculation();
    }

    void OnDestroy()
    {
        Debug.Log($"[MiniGame_CalculManager] OnDestroy() called - Instance ID: {GetInstanceID()}");
        if (GameManager.Instance != null)
        {
            Debug.Log($"[MiniGame_CalculManager] Unsubscribing from OnTimerEnded");
            GameManager.Instance.OnTimerEnded -= HandleTimerEnded;
        }
        else
        {
            Debug.LogWarning($"[MiniGame_CalculManager] OnDestroy - GameManager.Instance is NULL, cannot unsubscribe!");
        }
    }

    public void GenerateNewCalculation()
    {
        if (gameEnded || calculUIManager == null)
        {
            return;
        }
        var calcul = calculLogic.GenerateCalculation();
        calculUIManager.DisplayCalculation(calcul);
    }

    public void OnAnswerSelected(int index, bool correct)
    {
        if (gameEnded || GameManager.Instance.Lives <= 0)
        {
            return;
        }

        if (correct)
        {
            correctAnswers++;

            if (correctAnswers >= REQUIRED_CORRECT_ANSWERS)
            {
                gameEnded = true;
                GameManager.Instance.NotifyWin();
                return;
            }

            return;
        }

        wrongAttempts++;

        if (wrongAttempts >= 3)
        {
            gameEnded = true;
            GameManager.Instance.NotifyFail();
            return;
        }
    }

    private void HandleTimerEnded()
    {
        if (gameEnded) return;
        gameEnded = true;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.NotifyFail();
        }
    }
}

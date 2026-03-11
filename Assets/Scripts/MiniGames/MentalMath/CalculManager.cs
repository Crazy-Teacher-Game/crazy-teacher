using UnityEngine;

public class MiniGame_CalculManager : MonoBehaviour
{
    [SerializeField] private CalculLogic calculLogic;
    [SerializeField] private CalculUIManager calculUIManager;

    private int wrongAttempts;
    private int correctAnswers;
    private const int REQUIRED_CORRECT_ANSWERS = 3;

    public int CorrectAnswers => correctAnswers;
    public int RequiredCorrectAnswers => REQUIRED_CORRECT_ANSWERS;

    void Start()
    {
        Debug.Log($"[MiniGame_CalculManager] Start() called - Instance ID: {GetInstanceID()}");
        if (calculLogic == null) calculLogic = FindObjectOfType<CalculLogic>();
        if (calculUIManager == null) calculUIManager = FindObjectOfType<CalculUIManager>();

        Debug.Log($"[MiniGame_CalculManager] Subscribing to OnTimerEnded - GameManager.Instance={(GameManager.Instance != null)}");
        GameManager.Instance.OnTimerEnded += HandleTimerEnded;
        GameManager.Instance.StartTimer(15f);
        wrongAttempts = 0;
        correctAnswers = 0;
        GenerateNewCalculation();
        Debug.Log($"[MiniGame_CalculManager] Start() complete - calculLogic={(calculLogic != null)}, calculUIManager={(calculUIManager != null)}");
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
        if (calculUIManager == null)
        {
            return;
        }
        var calcul = calculLogic.GenerateCalculation();
        calculUIManager.DisplayCalculation(calcul);
    }

    public bool OnAnswerSelected(int index)
    {
        // Check if game is over (no lives left)
        if (GameManager.Instance.Lives <= 0)
        {
            return false;
        }
        
        bool correct = calculLogic.CheckAnswer(index);

        if (correct)
        {
            correctAnswers++;

            // Check win condition
            if (correctAnswers >= REQUIRED_CORRECT_ANSWERS)
            {
                GameManager.Instance.NotifyWin();
                return true;
            }

            // Generate next calculation if not yet won
            return true;
        }
        
        wrongAttempts++;

        if (wrongAttempts >= 3)
        {
            GameManager.Instance.NotifyFail();
            return false;
        }
        return false; 
    }

    private void HandleTimerEnded()
    {
        Debug.Log($"[MiniGame_CalculManager] HandleTimerEnded() called - Instance ID: {GetInstanceID()}, this={(this != null)}, gameObject={(gameObject != null)}");
        if (GameManager.Instance != null)
        {
            Debug.Log($"[MiniGame_CalculManager] Calling NotifyFail from HandleTimerEnded");
            GameManager.Instance.NotifyFail();
        }
        else
        {
            Debug.LogWarning($"[MiniGame_CalculManager] HandleTimerEnded - GameManager.Instance is NULL!");
        }
    }
}

using UnityEngine;

public class MiniGame_CalculManager : MonoBehaviour
{
    [SerializeField] private CalculLogic calculLogic;
    [SerializeField] private CalculUIManager calculUIManager;

    private int wrongAttempts;
    private int correctAnswers;
    private const int REQUIRED_CORRECT_ANSWERS = 3;

    void Start()
    {
        if (calculLogic == null) calculLogic = FindObjectOfType<CalculLogic>();
        if (calculUIManager == null) calculUIManager = FindObjectOfType<CalculUIManager>();
        
        GameManager.Instance.OnTimerEnded += HandleTimerEnded;
        GameManager.Instance.StartTimer(15f);
        wrongAttempts = 0;
        correctAnswers = 0;
        GenerateNewCalculation();
    }

    void OnDestroy()
    {
        GameManager.Instance.OnTimerEnded -= HandleTimerEnded;
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

        GameManager.Instance.LoseLife(); // visually update lives on each wrong answer

        // Check if game is over after losing a life
        if (GameManager.Instance.Lives <= 0)
        {
            GameManager.Instance.NotifyFail();
            return false;
        }

        if (wrongAttempts >= 3)
        {
            GameManager.Instance.NotifyFail();
            return false;
        }
        return false; 
    }

    private void HandleTimerEnded()
    {
        GameManager.Instance?.NotifyFail();
    }
}

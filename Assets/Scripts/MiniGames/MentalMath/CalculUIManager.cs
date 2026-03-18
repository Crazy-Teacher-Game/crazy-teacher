using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CalculUIManager : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_Text questionText;
    [SerializeField] private TMP_Text progressText;
    [SerializeField] private Image successImage;
    [SerializeField] private Image failImage;

    [Header("Answer Slots")]
    [SerializeField] private TMP_Text answerText1;
    [SerializeField] private TMP_Text answerText2;
    [SerializeField] private TMP_Text answerText3;
    [SerializeField] private Image answerLabelImage1;
    [SerializeField] private Image answerLabelImage2;
    [SerializeField] private Image answerLabelImage3;

    private MiniGame_CalculManager calculManager;
    private CalculLogic.CalculationData currentCalculation;
    private bool awaitingAnswer;

    // Sprites cached before fields are wired
    private Sprite _s1, _s2, _s3;

    void Awake()
    {
        calculManager = FindObjectOfType<MiniGame_CalculManager>();

        if (successImage != null) successImage.enabled = false;
        if (failImage != null) failImage.enabled = false;

        // Cache sprites now — fields not yet wired at this point
        _s1 = Resources.Load<Sprite>("Sprites/MentalMath/B1");
        _s2 = Resources.Load<Sprite>("Sprites/MentalMath/B2");
        _s3 = Resources.Load<Sprite>("Sprites/MentalMath/B3");
    }

    void Start()
    {
        // Fields are wired by Bootstrapper before Start() — apply sprites here
        if (answerLabelImage1 != null && _s1 != null) answerLabelImage1.sprite = _s1;
        if (answerLabelImage2 != null && _s2 != null) answerLabelImage2.sprite = _s2;
        if (answerLabelImage3 != null && _s3 != null) answerLabelImage3.sprite = _s3;
    }

    public void DisplayCalculation(CalculLogic.CalculationData data)
    {
        currentCalculation = data;
        if (questionText != null) questionText.text = data.Question;

        if (answerText1 != null) answerText1.text = data.Answers.Count > 0 ? data.Answers[0].ToString() : "";
        if (answerText2 != null) answerText2.text = data.Answers.Count > 1 ? data.Answers[1].ToString() : "";
        if (answerText3 != null) answerText3.text = data.Answers.Count > 2 ? data.Answers[2].ToString() : "";

        UpdateProgressDisplay();
        if (successImage != null) successImage.enabled = false;
        if (failImage != null) failImage.enabled = false;
        awaitingAnswer = true;
    }

    private void UpdateProgressDisplay()
    {
        if (calculManager == null)
            calculManager = FindObjectOfType<MiniGame_CalculManager>();

        if (progressText != null && calculManager != null)
            progressText.text = $"{calculManager.CorrectAnswers}/{calculManager.RequiredCorrectAnswers}";
    }

    void Update()
    {
        if (Input.GetButtonDown("P1_B1") || Input.GetKeyDown(KeyCode.F))
            CheckAnswer(0);
        if (Input.GetButtonDown("P1_B2") || Input.GetKeyDown(KeyCode.G))
            CheckAnswer(1);
        if (Input.GetButtonDown("P1_B3") || Input.GetKeyDown(KeyCode.H))
            CheckAnswer(2);
    }

    private void CheckAnswer(int index)
    {
        if (!awaitingAnswer)
        {
            return;
        }

        awaitingAnswer = false;

        if (calculManager != null)
        {
            var gm = FindObjectOfType<GameManager>();
            if (gm != null && gm.Lives <= 0) return;
        }

        if (calculManager == null)
        {
            calculManager = FindObjectOfType<MiniGame_CalculManager>();
            if (calculManager == null) return;
        }

        bool correct = (index == currentCalculation.CorrectAnswerIndex);

        if (successImage != null) successImage.enabled = correct;
        if (failImage != null) failImage.enabled = !correct;

        calculManager.OnAnswerSelected(index, correct);

        Invoke("Advance", 0.6f);
    }

    private void Advance()
    {
        if (this == null || calculManager == null) return;

        if (successImage != null) successImage.enabled = false;
        if (failImage != null) failImage.enabled = false;
        calculManager.GenerateNewCalculation();
        awaitingAnswer = true;
    }

    void OnDestroy()
    {
        CancelInvoke();
    }
}

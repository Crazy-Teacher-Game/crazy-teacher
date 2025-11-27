using UnityEngine;
using System.Collections.Generic;

public class CalculLogic : MonoBehaviour
{
    private int correctAnswerIndex;
    private int correctAnswer;

    public struct CalculationData
    {
        public string Question;
        public List<int> Answers;
    }

    public CalculationData GenerateCalculation()
    {
        int a, b, result;
        string operation;

        do
        {
            a = Random.Range(0, 11);
            b = Random.Range(0, 11);
        } while (a == 0 && b == 0);

        // Random operation: +, -, x, ÷
        float rand = Random.value;
        if (rand < 0.25f)
        {
            operation = "+";
            result = a + b;
        }
        else if (rand < 0.5f)
        {
            operation = "-";
            // Ensure result is non-negative
            if (a < b)
            {
                int temp = a;
                a = b;
                b = temp;
            }
            result = a - b;
        }
        else if (rand < 0.75f)
        {
            operation = "x";
            result = a * b;
        }
        else
        {
            operation = "÷";
            // Ensure clean division (no remainders)
            if (b == 0) b = 1;
            a = a * b; // Make a divisible by b
            result = a / b;
        }

        correctAnswer = result;

        List<int> answers = new List<int>();
        answers.Add(result);

        while (answers.Count < 3)
        {
            int wrong = result + Random.Range(-5, 6);
            if (wrong != result && wrong >= 0 && !answers.Contains(wrong))
                answers.Add(wrong);
        }

        // Mélange
        for (int i = 0; i < answers.Count; i++)
        {
            int rnd = Random.Range(i, answers.Count);
            (answers[i], answers[rnd]) = (answers[rnd], answers[i]);
        }

        correctAnswerIndex = answers.IndexOf(result);

        var data = new CalculationData
        {
            Question = $"{a} {operation} {b} = ?",
            Answers = answers
        };

        Debug.Log($"[CalculLogic] Generated: {data.Question} | Answers: {answers[0]}, {answers[1]}, {answers[2]} | CorrectIndex: {correctAnswerIndex}");
        
        return data;
    }

    public bool CheckAnswer(int index)
    {
        bool ok = index == correctAnswerIndex;
        Debug.Log($"[CalculLogic] Validate index {index} => {(ok ? "CORRECT" : "WRONG")} (correctIndex={correctAnswerIndex})");
        return ok;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class TimerUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI timeLabel;
    [SerializeField] private Image fillBar; //Faudra mettre une image en "Filled" radial/horizontal

    void Start()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.RegisterTimerUI(this);
        }
    }

    public void Show(float duration)
    {
        gameObject.SetActive(true);
        UpdateTime(duration, duration);
    }
    
    public void Hide()
    {
        gameObject.SetActive(false);
    }
    
    public void UpdateTime(float remaining, float duration)
    {
        if (timeLabel)
        {
            int r = Mathf.CeilToInt(remaining);
            timeLabel.text = r.ToString();
        }
        if (fillBar)
        {
            float t = duration <= 0.0001f ? 0f : Mathf.Clamp01(remaining / duration);
            fillBar.fillAmount = t;
            // Lerp color from green (full) to red (empty)
            // fillBar.color = Color.Lerp(Color.red, Color.green, t);
        }
    }

    // Helper to wire from COMMON_Canvas if needed
    public void SetRefs(TextMeshProUGUI label, Image bar)
    {
        timeLabel = label;
        fillBar = bar;
    }
   
}
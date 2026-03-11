using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class LivesUI : MonoBehaviour
{
    [SerializeField] private Image[] hearts; //2 ou + images de coeurs ou autre à assigner dans l'inspecteur
    private int lastShown = -1;

    void Start()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.RegisterLivesUI(this);
        }
    }

    public void SetLives(int lives)
    {
        if (hearts == null) return;
        if (lives == lastShown) return;
        for (int i = 0; i < hearts.Length; i++)
            hearts[i].enabled = i < lives;
        lastShown = lives;
        Debug.Log($"[LivesUI] SetLives -> {lives}");
    }
}
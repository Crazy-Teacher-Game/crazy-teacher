using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Manager : MonoBehaviour
{
    [SerializeField] private GameObject bottleObject;
    [SerializeField] private Sprite baseBottle;
    [SerializeField] private Sprite botteShaken;
    [SerializeField] private Sprite botteShaken2;
    private RectTransform bottleRectTransform;

    private GameManager gameManager;
    private SpriteRenderer bottleSpriteRenderer;

    private float bottleOffsetY = 0f;
    private float initializedBottleY = 0f;
    private int bottleState = 0;// -1: down, 0: middle, 1: up
    private int lastBottleState = 0;
    private int bottleSaturation = 0;
    private int bottleSaturationMax = 60; //facile : 30, moyen : 60, difficile : 100
    private float winPercentage = 0f;
    private bool gameEnded = false;

    // Start is called before the first frame update
    void Start()
    {
        bottleRectTransform = bottleObject.GetComponent<RectTransform>();
        if (bottleRectTransform != null)
        {
            initializedBottleY = bottleRectTransform.anchoredPosition.y;
        }
        else
        {
            initializedBottleY = bottleObject.transform.localPosition.y;
        }

        bottleSpriteRenderer = bottleObject.GetComponent<SpriteRenderer>();
        if (bottleSpriteRenderer == null)
        {
            Debug.LogError("[Manager] Bottle does not have a SpriteRenderer! Add one to the Bottle GameObject.");
        }

        // Find the GameManager in the scene
        gameManager = FindObjectOfType<GameManager>();
        if (gameManager == null)
        {
            Debug.LogError("[Manager] No GameManager found in the scene!");
        }

        gameManager.StartTimer(120);
    }

    // Update is called once per frame
    void Update()
    {
        if (gameEnded) return;

        // Guard against null gameManager
        if (gameManager == null) return;

        // Check for game end
        if (bottleSaturation >= bottleSaturationMax)
        {
            Debug.Log("Game Over! Bottle is full! You won !!");

            gameManager.NotifyWin();

            gameEnded = true;
            //logique de victoire
            return;
        }

        if (gameManager.RemainingTime <= 0f)
        {
            Debug.Log("Game Over! You loose gros nullos...");

            gameManager.NotifyFail();

            gameEnded = true;
            //logique de victoire
            return;
        }

        //Update bottle state
        if (Input.GetAxis("P1_Vertical") != 0)
        {
            if (Input.GetAxis("P1_Vertical") > 0)
            {
                bottleState = 1;
                if (bottleSpriteRenderer != null && botteShaken2 != null)
                    bottleSpriteRenderer.sprite = botteShaken2;
            }
            else
            {
                bottleState = -1;
                if (bottleSpriteRenderer != null && botteShaken != null)
                    bottleSpriteRenderer.sprite = botteShaken;
            }
        }
        else
        {
            bottleState = 0;
            if (bottleSpriteRenderer != null && baseBottle != null)
                bottleSpriteRenderer.sprite = baseBottle;
        }

        bottleOffsetY = bottleState;

        // Update bottle position
        if (bottleRectTransform != null)
        {
            Vector2 anchoredPosition = bottleRectTransform.anchoredPosition;
            anchoredPosition.y = initializedBottleY + bottleOffsetY;
            bottleRectTransform.anchoredPosition = anchoredPosition;
        }
        else
        {
            Vector3 localPosition = bottleObject.transform.localPosition;
            localPosition.y = initializedBottleY + bottleOffsetY;
            bottleObject.transform.localPosition = localPosition;
        }

        //Debug.Log("Bottle Position: " + bottleObject.transform.position);

        // Update bottle saturation
        //make the difference between last and current state
        //the difference is added to the saturation
        if (bottleState != lastBottleState)
        {
            bottleSaturation += Mathf.Abs(bottleState - lastBottleState);
            lastBottleState = bottleState;
            Debug.Log("Win Percentage: " + winPercentage + "%");
        }

        // Calculate win percentage
        winPercentage = (float)bottleSaturation / bottleSaturationMax * 100f;
    }

    // void FixedUpdate()
    // {
    //     if (Input.GetButton("P1_B1"))
    //     {
    //         bottleOffsetY += 0.1f;
    //     }
    // }
}

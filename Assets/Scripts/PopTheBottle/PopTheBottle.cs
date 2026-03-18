using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PopTheBottle : MonoBehaviour
{
    [SerializeField] private GameObject bottleObject;
    [SerializeField] private Sprite baseBottle;
    [SerializeField] private Sprite botteShaken;
    [SerializeField] private Sprite botteShaken2;
    [SerializeField] private Sprite bottlePopped;
    private RectTransform bottleRectTransform;

    private SpriteRenderer bottleSpriteRenderer;

    private float bottleOffsetY = 0f;
    private float initializedBottleY = 0f;
    private int bottleState = 0;// -1: down, 0: middle, 1: up
    private int lastBottleState = 0;
    [Header("UI")]
    [SerializeField] private RectTransform saturationBarFill; // Pivot en bas, hauteur max = hauteur souhaitée à 100%

    [Header("Difficulté")]
    [SerializeField] private int bottleSaturationMin = 30;
    [SerializeField] private int bottleSaturationMax = 80;
    private int bottleSaturation = 0;
    private int bottleSaturationTarget;
    private float winPercentage = 0f;
    private bool gameEnded = false;

    [Header("Audio - Ambiance")]
    [SerializeField] private AudioClip ambianceSound;
    [SerializeField][Min(0f)] private float ambianceVolume = 1f;
    [SerializeField] private float ambianceFadeInDuration = 1f;

    [Header("Audio - Effets")]
    [SerializeField] private AudioClip shakeDownSound;
    [SerializeField][Min(0f)] private float shakeDownVolume = 1f;
    [SerializeField] private AudioClip shakeUpSound;
    [SerializeField][Min(0f)] private float shakeUpVolume = 1f;
    [SerializeField] private AudioClip popSound;
    [SerializeField][Min(0f)] private float popVolume = 1f;

    [Header("Audio - Fade Global")]
    [SerializeField] private float globalFadeInDuration = 0.5f;
    [SerializeField] private float globalFadeOutDuration = 1f;

    private AudioSource ambianceAudioSource;
    private AudioSource fxAudioSource;

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
            Debug.LogError("[PopTheBottle] Bottle does not have a SpriteRenderer! Add one to the Bottle GameObject.");
        }

        float difficulty = GameManager.Instance != null ? GameManager.Instance.DifficultyFactor : 0f;
        bottleSaturationTarget = Mathf.RoundToInt(Mathf.Lerp(bottleSaturationMin, bottleSaturationMax, difficulty));

        // Ambiance (loop)
        if (ambianceSound != null)
        {
            ambianceAudioSource = gameObject.AddComponent<AudioSource>();
            ambianceAudioSource.clip = ambianceSound;
            ambianceAudioSource.loop = true;
            ambianceAudioSource.volume = 0f;
            ambianceAudioSource.Play();
            StartCoroutine(FadeAudio(ambianceAudioSource, 0f, ambianceVolume, ambianceFadeInDuration));
        }

        // Source effets
        fxAudioSource = gameObject.AddComponent<AudioSource>();
        fxAudioSource.volume = 0f;
        StartCoroutine(FadeAudio(fxAudioSource, 0f, 1f, globalFadeInDuration));

        GameManager.Instance.StartTimer(10f, 5f);
        GameManager.Instance.OnTimerEnded += HandleTimerEnded;
    }

    void Update()
    {
        if (gameEnded) return;

        // Check for win
        if (bottleSaturation >= bottleSaturationTarget)
        {
            if (bottleSpriteRenderer != null && bottlePopped != null)
                bottleSpriteRenderer.sprite = bottlePopped;
            if (fxAudioSource != null && popSound != null)
                fxAudioSource.PlayOneShot(popSound, popVolume);
            EndGame(true);
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
            anchoredPosition.y = initializedBottleY + bottleOffsetY * 2;
            bottleRectTransform.anchoredPosition = anchoredPosition;
        }
        else
        {
            Vector3 localPosition = bottleObject.transform.localPosition;
            localPosition.y = initializedBottleY + bottleOffsetY * 2;
            bottleObject.transform.localPosition = localPosition;
        }

        // Update bottle saturation
        if (bottleState != lastBottleState)
        {
            bottleSaturation += Mathf.Abs(bottleState - lastBottleState);

            if (fxAudioSource != null)
            {
                if (bottleState == -1 && shakeDownSound != null)
                    fxAudioSource.PlayOneShot(shakeDownSound, shakeDownVolume);
                else if (bottleState == 1 && shakeUpSound != null)
                    fxAudioSource.PlayOneShot(shakeUpSound, shakeUpVolume);
            }

            lastBottleState = bottleState;
        }

        // Calculate win percentage
        winPercentage = (float)bottleSaturation / bottleSaturationTarget * 100f;

        if (saturationBarFill != null)
        {
            float t = (float)bottleSaturation / bottleSaturationTarget * 10;
            saturationBarFill.localScale = new Vector3(1f, t, 1f);
        }
    }

    private void HandleTimerEnded()
    {
        if (!gameEnded)
            EndGame(false);
    }

    private void EndGame(bool win)
    {
        gameEnded = true;
        GameManager.Instance.OnTimerEnded -= HandleTimerEnded;

        if (ambianceAudioSource != null)
            StartCoroutine(FadeAudio(ambianceAudioSource, ambianceAudioSource.volume, 0f, globalFadeOutDuration));
        if (fxAudioSource != null)
            StartCoroutine(FadeAudio(fxAudioSource, fxAudioSource.volume, 0f, globalFadeOutDuration));

        if (win) GameManager.Instance.NotifyWin();
        else GameManager.Instance.NotifyFail();
    }

    private IEnumerator FadeAudio(AudioSource source, float from, float to, float duration)
    {
        if (source == null) yield break;

        if (duration <= 0f)
        {
            source.volume = to;
            yield break;
        }

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            source.volume = Mathf.Lerp(from, to, elapsed / duration);
            yield return null;
        }
        source.volume = to;
    }

    void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnTimerEnded -= HandleTimerEnded;
        }
    }
}

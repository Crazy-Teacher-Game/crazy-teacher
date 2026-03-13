using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Slotmachine : MonoBehaviour
{
    private const float MinSpinSpeed = 6f;
    private const float MaxSpinSpeed = 10f;
    private const float MaxDurationSeconds = 20f;
    private const float MinDurationSeconds = 6f;

    [SerializeField] private Wheel wheel1;
    [SerializeField] private Wheel wheel2;
    [SerializeField] private Wheel wheel3;
    [SerializeField] private FlashScreenIndicator screenIndicator;
    [SerializeField] private AudioClip endMiniGameSound;
    [SerializeField] private float endMiniGameVolume = 1f;
    [SerializeField] private AudioClip victorySound;
    [SerializeField] private float victoryVolume = 1f;
    [SerializeField] private float endNotifyDelay = 2f;
    [SerializeField] private GameObject fondgris;

    [Header("Audio")]
    [SerializeField] private AudioClip slotMachine7;
    [SerializeField] private float slotMachine7Volume = 1f;
    [SerializeField] private AudioClip slotMachineAmbiance;
    [SerializeField] private float slotMachineAmbianceVolume = 1f;
    [SerializeField] private AudioClip slotMachineStop;
    [SerializeField] private float slotMachineStopVolume = 1f;
    [SerializeField] private AudioClip slotMachineWheel;
    [SerializeField] private float slotMachineWheelVolume = 1f;
    [SerializeField] private float ambianceFadeInDuration = 1f;
    [SerializeField] private float ambianceFadeOutDuration = 1f;

    [Header("Background")]
    [SerializeField] private bool useRuntimeBackground = false;
    [SerializeField] private Color backgroundColor = new Color(0f, 0f, 0f, 0.25f);
    [SerializeField] private Sprite backgroundSprite;
    [SerializeField] private int backgroundSortingOrder = -200;

    private int level = 1;
    private bool gameEnded = false;
    private bool isResetting = false;
    private bool isStoppingCurrentWheel = false;
    private bool btnDownLastUpdate = false;
    private AudioSource endAudioSource;
    private AudioSource fxAudioSource;
    private AudioSource ambianceAudioSource;
    private AudioSource wheelAudioSource;
    private Coroutine ambianceFadeCoroutine;
    private bool wheelLoopPlaying = false;
    private GameObject backgroundCanvasGO;

    void Start()
    {
        float difficultyFactor = 0f;
        if (GameManager.Instance != null)
        {
            difficultyFactor = Mathf.Clamp01(GameManager.Instance.DifficultyFactor);
            GameManager.Instance.OnTimerEnded += HandleTimerEnded;
            GameManager.Instance.StartTimer(MaxDurationSeconds, MinDurationSeconds);
        }
        else
        {
            Debug.LogWarning("[Slotmachine] GameManager not found. Using default spin speed and no shared timer.");
        }

        float spinSpeed = Mathf.Lerp(MinSpinSpeed, MaxSpinSpeed, difficultyFactor);
        SetWheelsSpinSpeed(spinSpeed);

        if (useRuntimeBackground)
            CreateBackground();
        InitializeAudioSources();
        FadeAmbianceIn();

        // Wheels start spinning immediately
        if (wheel1 != null) wheel1.StartSpin();
        if (wheel2 != null) wheel2.StartSpin();
        if (wheel3 != null) wheel3.StartSpin();
    }

    private void SetWheelsSpinSpeed(float spinSpeed)
    {
        if (wheel1 != null) wheel1.SetSpinSpeed(spinSpeed);
        if (wheel2 != null) wheel2.SetSpinSpeed(spinSpeed);
        if (wheel3 != null) wheel3.SetSpinSpeed(spinSpeed);
    }

    void Update()
    {
        UpdateWheelLoopSound();

        if (gameEnded || isResetting || isStoppingCurrentWheel)
        {
            btnDownLastUpdate = Input.GetButton("P1_B1");
            return;
        }

        if (Input.GetButton("P1_B1"))
        {
            if (btnDownLastUpdate) return;
            btnDownLastUpdate = true;
            StartCoroutine(HandleCurrentWheelStop());
        }
        else
        {
            btnDownLastUpdate = false;
        }
    }

    private IEnumerator HandleCurrentWheelStop()
    {
        Wheel currentWheel = GetCurrentWheel();
        if (currentWheel == null)
            yield break;

        if (!currentWheel.Stop())
            yield break;

        isStoppingCurrentWheel = true;
        while (currentWheel.IsStopping && !gameEnded)
            yield return null;

        isStoppingCurrentWheel = false;
        if (gameEnded)
            yield break;

        bool pass = currentWheel.LastStopWasSeven;
        PlayStopResultSound(pass);
        if (!pass)
        {
            TriggerMiss();
            yield break;
        }

        if (level == 1)
        {
            level = 2;
        }
        else if (level == 2)
        {
            level = 3;
        }
        else
        {
            EndGame(true, FlashScreenIndicator.ScreenType.Parfait);
        }
    }

    private void InitializeAudioSources()
    {
        endAudioSource = gameObject.AddComponent<AudioSource>();
        endAudioSource.playOnAwake = false;

        fxAudioSource = gameObject.AddComponent<AudioSource>();
        fxAudioSource.playOnAwake = false;

        ambianceAudioSource = gameObject.AddComponent<AudioSource>();
        ambianceAudioSource.playOnAwake = false;
        ambianceAudioSource.loop = true;
        ambianceAudioSource.clip = slotMachineAmbiance;
        ambianceAudioSource.volume = 0f;
        if (slotMachineAmbiance != null)
            ambianceAudioSource.Play();

        wheelAudioSource = gameObject.AddComponent<AudioSource>();
        wheelAudioSource.playOnAwake = false;
        wheelAudioSource.loop = true;
        wheelAudioSource.clip = slotMachineWheel;
        wheelAudioSource.volume = slotMachineWheelVolume;
    }

    private void FadeAmbianceIn()
    {
        if (ambianceAudioSource == null || slotMachineAmbiance == null) return;
        if (ambianceFadeCoroutine != null)
            StopCoroutine(ambianceFadeCoroutine);
        ambianceFadeCoroutine = StartCoroutine(FadeAudio(ambianceAudioSource, 0f, slotMachineAmbianceVolume, ambianceFadeInDuration));
    }

    private void FadeAmbianceOut()
    {
        if (ambianceAudioSource == null) return;
        if (ambianceFadeCoroutine != null)
            StopCoroutine(ambianceFadeCoroutine);
        ambianceFadeCoroutine = StartCoroutine(FadeAudio(ambianceAudioSource, ambianceAudioSource.volume, 0f, ambianceFadeOutDuration));
    }

    private IEnumerator FadeAudio(AudioSource source, float from, float to, float duration)
    {
        if (source == null)
            yield break;

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

    private bool AnyWheelTurning()
    {
        bool wheel1Turning = wheel1 != null && (wheel1.IsSpinning || wheel1.IsStopping);
        bool wheel2Turning = wheel2 != null && (wheel2.IsSpinning || wheel2.IsStopping);
        bool wheel3Turning = wheel3 != null && (wheel3.IsSpinning || wheel3.IsStopping);
        return wheel1Turning || wheel2Turning || wheel3Turning;
    }

    private void UpdateWheelLoopSound()
    {
        if (wheelAudioSource == null || slotMachineWheel == null)
            return;

        wheelAudioSource.volume = slotMachineWheelVolume;
        bool shouldPlay = AnyWheelTurning();
        if (shouldPlay && !wheelLoopPlaying)
        {
            wheelAudioSource.Play();
            wheelLoopPlaying = true;
        }
        else if (!shouldPlay && wheelLoopPlaying)
        {
            wheelAudioSource.Stop();
            wheelLoopPlaying = false;
        }
    }

    private void PlayStopResultSound(bool isSeven)
    {
        if (fxAudioSource == null)
            return;

        if (isSeven && slotMachine7 != null)
            fxAudioSource.PlayOneShot(slotMachine7, slotMachine7Volume);

        if (slotMachineStop != null)
            fxAudioSource.PlayOneShot(slotMachineStop, slotMachineStopVolume);
    }

    private Wheel GetCurrentWheel()
    {
        if (level == 1) return wheel1;
        if (level == 2) return wheel2;
        if (level == 3) return wheel3;
        return null;
    }

    private void TriggerMiss()
    {
        if (gameEnded) return;
        isResetting = true;
        SetFondColor(new Color(1f, 0.42f, 0.42f, 1f));
        StartCoroutine(ResetAfterDelay());
    }

    private void SetFondColor(Color color)
    {
        if (fondgris != null)
        {
            var image = fondgris.GetComponent<Image>();
            if (image != null) { image.color = color; return; }
            var sr = fondgris.GetComponent<SpriteRenderer>();
            if (sr != null) { sr.color = color; return; }
        }
    }

    private IEnumerator ResetAfterDelay()
    {
        yield return new WaitForSeconds(0.5f);
        if (!gameEnded)
        {
            SetFondColor(new Color(1f, 1f, 1f, 1f));
            RestartStoppedWheelsOnly();
            level = 1;
        }
        isResetting = false;
    }

    private void RestartStoppedWheelsOnly()
    {
        RestartWheelIfStopped(wheel1);
        RestartWheelIfStopped(wheel2);
        RestartWheelIfStopped(wheel3);
    }

    private void RestartWheelIfStopped(Wheel wheel)
    {
        if (wheel == null) return;
        if (!wheel.IsSpinning && !wheel.IsStopping)
            wheel.StartSpin();
    }


    private void HandleTimerEnded()
    {
        EndGame(false, FlashScreenIndicator.ScreenType.Rate);
    }

    private void EndGame(bool isWin, FlashScreenIndicator.ScreenType screenType)
    {
        if (gameEnded) return;
        gameEnded = true;

        if (wheel1 != null) wheel1.Stop();
        if (wheel2 != null) wheel2.Stop();
        if (wheel3 != null) wheel3.Stop();

        FadeAmbianceOut();
        PlayEndScreenSound(screenType);

        if (screenIndicator != null)
            screenIndicator.ShowScreen(screenType);

        StartCoroutine(NotifyEndResultAfterDelay(isWin));
    }

    private IEnumerator NotifyEndResultAfterDelay(bool isWin)
    {
        yield return new WaitForSeconds(endNotifyDelay);

        System.Action notify = () =>
        {
            if (GameManager.Instance == null) return;
            if (isWin) GameManager.Instance.NotifyWin();
            else GameManager.Instance.NotifyFail();
        };

        notify();
    }

    private void PlayEndScreenSound(FlashScreenIndicator.ScreenType screenType)
    {
        if (endAudioSource == null) return;

        AudioClip clipToPlay = null;
        float volumeToPlay = 1f;
        switch (screenType)
        {
            case FlashScreenIndicator.ScreenType.Rate:
                clipToPlay = endMiniGameSound;
                volumeToPlay = endMiniGameVolume;
                break;
            case FlashScreenIndicator.ScreenType.Parfait:
                clipToPlay = victorySound;
                volumeToPlay = victoryVolume;
                break;
        }

        if (clipToPlay != null)
            endAudioSource.PlayOneShot(clipToPlay, volumeToPlay);
    }

    private void CreateBackground()
    {
        backgroundCanvasGO = new GameObject("SlotMachineBackgroundCanvas");
        Canvas canvas = backgroundCanvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.overrideSorting = true;
        canvas.sortingOrder = backgroundSortingOrder;
        backgroundCanvasGO.AddComponent<CanvasScaler>();

        GameObject panelGO = new GameObject("SlotMachineBackground");
        panelGO.transform.SetParent(backgroundCanvasGO.transform, false);
        RectTransform panelRect = panelGO.AddComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;

        Image panelImage = panelGO.AddComponent<Image>();
        panelImage.color = backgroundColor;
        if (backgroundSprite != null)
            panelImage.sprite = backgroundSprite;
    }

    void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnTimerEnded -= HandleTimerEnded;
        }

        if (ambianceFadeCoroutine != null)
        {
            StopCoroutine(ambianceFadeCoroutine);
            ambianceFadeCoroutine = null;
        }

        if (wheelAudioSource != null)
            wheelAudioSource.Stop();

        if (backgroundCanvasGO != null)
        {
            Destroy(backgroundCanvasGO);
        }
    }
}

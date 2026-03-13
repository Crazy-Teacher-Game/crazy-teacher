using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Slotmachine : MonoBehaviour
{
    [SerializeField] private Wheel wheel1;
    [SerializeField] private Wheel wheel2;
    [SerializeField] private Wheel wheel3;
    [SerializeField] private WheelFrame wheelFrame1;
    [SerializeField] private WheelFrame wheelFrame2;
    [SerializeField] private WheelFrame wheelFrame3;
    [SerializeField] float durationSeconds = 8f;
    [SerializeField] float minDurationSeconds = 4f;
    [SerializeField] private FlashScreenIndicator screenIndicator;
    [SerializeField] private AudioClip endMiniGameSound;
    [SerializeField] private AudioClip victorySound;
    [SerializeField] private float endNotifyDelay = 2f;

    [Header("Background")]
    [SerializeField] private Color backgroundColor = new Color(0f, 0f, 0f, 0.25f);
    [SerializeField] private Sprite backgroundSprite;
    [SerializeField] private int backgroundSortingOrder = -200;

    private int level = 1;
    private bool gameEnded = false;
    private bool isResetting = false;
    private bool isStoppingCurrentWheel = false;
    private bool btnDownLastUpdate = false;
    private AudioSource audioSource;
    private GameObject backgroundCanvasGO;

    void Start()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnTimerEnded += HandleTimerEnded;
            GameManager.Instance.StartTimer(durationSeconds, minDurationSeconds);
        }

        CreateBackground();
        audioSource = gameObject.AddComponent<AudioSource>();

        // Wheels start spinning immediately
        if (wheel1 != null) wheel1.StartSpin();
        if (wheel2 != null) wheel2.StartSpin();
        if (wheel3 != null) wheel3.StartSpin();
    }

    void Update()
    {
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
        SetAllFrames(Color.red);
        StartCoroutine(ResetAfterDelay());
    }

    private IEnumerator ResetAfterDelay()
    {
        yield return new WaitForSeconds(0.5f);
        if (!gameEnded)
        {
            SetAllFrames(Color.yellow);
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

    private void SetAllFrames(Color color)
    {
        wheelFrame1.SetColor(color);
        wheelFrame2.SetColor(color);
        wheelFrame3.SetColor(color);
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
        if (audioSource == null) return;

        AudioClip clipToPlay = null;
        switch (screenType)
        {
            case FlashScreenIndicator.ScreenType.Rate:
                clipToPlay = endMiniGameSound;
                break;
            case FlashScreenIndicator.ScreenType.Fini:
                clipToPlay = victorySound;
                break;
        }

        if (clipToPlay != null)
            audioSource.PlayOneShot(clipToPlay);
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
        if (backgroundCanvasGO != null)
        {
            Destroy(backgroundCanvasGO);
        }
    }
}

using UnityEngine;
using TMPro;

public class Loop : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform loupeTransform;
    [SerializeField] private SpriteRenderer objectRenderer;
    [SerializeField] private Material blurMaterial;

    [Header("Zoom")]
    [SerializeField] private float minScale = 0.5f;
    [SerializeField] private float maxScale = 2.0f;
    [SerializeField] private float zoomSpeed = 0.4f;

    [Header("Sweet Spot")]
    [SerializeField] private float minTargetScale = 0.7f;
    [SerializeField] private float maxTargetScale = 1.8f;
    [SerializeField] private float acceptableRange = 0.15f;
    [SerializeField] private float minimumStartDistance = 0.4f;

    [Header("Visual Feedback")]
    [SerializeField] private float blurFalloffRange = 0.6f;

    [Header("Validation")]
    [SerializeField] private float requiredTimeInZone = 0.2f;

    [Header("Timer")]
    [SerializeField] private float timerDuration = 10f;
    [SerializeField] private float minTimerDuration = 4f;

    [Header("Debug")]
    [SerializeField] private bool debugMode = false;
    [SerializeField] private TextMeshProUGUI debugText;

    private float currentScale;
    private float targetScale;
    private float timeInZone;
    private bool hasInteracted;
    private bool wasInZone;
    private bool gameEnded;
    private Material objectMaterial;

    void Start()
    {
        if (!ValidateReferences()) return;

        objectMaterial = new Material(blurMaterial);
        objectRenderer.material = objectMaterial;

        targetScale = PickTargetScale();
        currentScale = PickStartScale(targetScale);

        ApplyScale(currentScale);
        ApplyBlur(currentScale);

        if (GameManager.Instance == null)
        {
            Debug.LogError("[Loop] GameManager.Instance is null! Loop must be loaded through MainScene.");
            return;
        }

        GameManager.Instance.StartTimer(timerDuration, minTimerDuration);
        GameManager.Instance.OnTimerEnded += HandleTimerEnded;
    }

    void Update()
    {
        if (gameEnded || GameManager.Instance == null) return;

        HandleInput();
        ApplyBlur(currentScale);
        HandleValidation();

        if (debugMode) UpdateDebugDisplay();
    }

    private void HandleInput()
    {
        float verticalInput = Input.GetAxis("P1_Vertical");

        if (Mathf.Abs(verticalInput) <= 0.1f) return;

        hasInteracted = true;
        currentScale += verticalInput * zoomSpeed * Time.deltaTime;
        currentScale = Mathf.Clamp(currentScale, minScale, maxScale);
        ApplyScale(currentScale);
    }

    private void HandleValidation()
    {
        if (!hasInteracted) return;

        bool inZone = IsInAcceptableZone(currentScale);

        if (inZone)
        {
            timeInZone += Time.deltaTime;

            if (timeInZone >= requiredTimeInZone)
            {
                gameEnded = true;
                if (debugMode) Debug.Log($"[Loop] VICTORY! scale={currentScale:F3}, target={targetScale:F3}");
                GameManager.Instance.NotifyWin();
            }
        }
        else
        {
            timeInZone = 0f;
        }

        wasInZone = inZone;
    }

    private bool IsInAcceptableZone(float scale)
    {
        return Mathf.Abs(scale - targetScale) <= acceptableRange;
    }

    private void ApplyBlur(float scale)
    {
        float distance = Mathf.Abs(scale - targetScale);
        float blur = Mathf.Clamp01(distance / blurFalloffRange);
        objectMaterial.SetFloat("_BlurAmount", blur);
    }

    private void ApplyScale(float scale)
    {
        loupeTransform.localScale = Vector3.one * scale;
    }

    private float PickTargetScale()
    {
        float target = Random.Range(minTargetScale, maxTargetScale);
        return Mathf.Clamp(target, minScale + acceptableRange, maxScale - acceptableRange);
    }

    private float PickStartScale(float target)
    {
        float safeMin = minScale;
        float safeMax = maxScale;
        float exclusionLow = target - minimumStartDistance;
        float exclusionHigh = target + minimumStartDistance;

        float lowRangeSize = Mathf.Max(0f, exclusionLow - safeMin);
        float highRangeSize = Mathf.Max(0f, safeMax - exclusionHigh);
        float totalRange = lowRangeSize + highRangeSize;

        if (totalRange <= 0f) return safeMin;

        float roll = Random.Range(0f, totalRange);
        if (roll < lowRangeSize)
            return safeMin + roll;
        else
            return exclusionHigh + (roll - lowRangeSize);
    }

    private void HandleTimerEnded()
    {
        if (gameEnded) return;
        gameEnded = true;

        if (hasInteracted && IsInAcceptableZone(currentScale))
            GameManager.Instance?.NotifyWin();
        else
            GameManager.Instance?.NotifyFail();
    }

    private bool ValidateReferences()
    {
        if (loupeTransform == null) { Debug.LogError("[Loop] loupeTransform not assigned!"); return false; }
        if (objectRenderer == null) { Debug.LogError("[Loop] objectRenderer not assigned!"); return false; }
        if (blurMaterial == null)   { Debug.LogError("[Loop] blurMaterial not assigned!");   return false; }
        return true;
    }

    private void UpdateDebugDisplay()
    {
        float distance = Mathf.Abs(currentScale - targetScale);
        bool inZone = IsInAcceptableZone(currentScale);

        if (debugText != null)
        {
            string color = inZone ? "#00FF00" : "#FF0000";
            string status = inZone ? "IN ZONE" : "OUT OF ZONE";

            debugText.text =
                $"<color={color}><b>{status}</b></color>\n\n" +
                $"Scale: <b>{currentScale:F3}</b> | Target: <b>{targetScale:F3}</b>\n" +
                $"Distance: <b>{distance:F3}</b> | Range: <b>+/-{acceptableRange:F3}</b>\n" +
                $"Interacted: <b>{hasInteracted}</b>\n\n" +
                $"Time in Zone: <b>{timeInZone:F2}s</b> / {requiredTimeInZone:F2}s\n" +
                $"Progress: <b>{(timeInZone / requiredTimeInZone * 100f):F0}%</b>";
        }

        if (inZone && !wasInZone)
            Debug.Log($"[Loop] ENTERED ZONE | scale={currentScale:F3} target={targetScale:F3} dist={distance:F3}");
        else if (!inZone && wasInZone)
            Debug.Log($"[Loop] LEFT ZONE | scale={currentScale:F3} accumulated={timeInZone:F2}s");
    }

    void OnDestroy()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnTimerEnded -= HandleTimerEnded;

        if (objectMaterial != null)
            Destroy(objectMaterial);
    }

    void OnDisable()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnTimerEnded -= HandleTimerEnded;
    }
}

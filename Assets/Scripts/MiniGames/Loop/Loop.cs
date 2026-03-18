using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Loop : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform loupeTransform;
    [SerializeField] private SpriteRenderer objectRenderer;
    [SerializeField] private Material blurMaterial;

    [Header("Zoom Configuration")]
    [SerializeField] private float minScale = 0.5f;
    [SerializeField] private float maxScale = 2.0f;
    [SerializeField] private float zoomSpeed = 1.5f;
    private float currentScale = 1.0f;

    [Header("Sweet Spot Configuration")]
    [SerializeField] private float minTargetScale = 0.7f;
    [SerializeField] private float maxTargetScale = 1.8f;
    [SerializeField] private float acceptableRange = 0.005f;
    private float targetScale;

    [Header("Debug")]
    [SerializeField] private bool debugMode = false;
    [SerializeField] private TextMeshProUGUI debugText;
    private bool wasInZone = false;

    [Header("Validation")]
    private float timeInZone = 0f;
    [SerializeField] private float requiredTimeInZone = 0.7f;

    [Header("Timer")]
    [SerializeField] private float timerDuration = 12f;
    [SerializeField] private float minTimerDuration = 8f;

    private bool gameEnded = false;
    private Material objectMaterial;

    void Start()
    {
        if (loupeTransform == null)
        {
            Debug.LogError("[Loop] loupeTransform not assigned!");
            return;
        }

        if (objectRenderer == null)
        {
            Debug.LogError("[Loop] objectRenderer not assigned!");
            return;
        }

        if (blurMaterial == null)
        {
            Debug.LogError("[Loop] blurMaterial not assigned!");
            return;
        }

        objectMaterial = new Material(blurMaterial);
        objectRenderer.material = objectMaterial;

        targetScale = Random.Range(minTargetScale, maxTargetScale);
        targetScale = Mathf.Clamp(targetScale, minScale + acceptableRange, maxScale - acceptableRange);

        currentScale = 1.0f;

        float initialBlur = CalculateBlurFromScale(currentScale);
        objectMaterial.SetFloat("_BlurAmount", initialBlur);

        loupeTransform.localScale = new Vector3(currentScale, currentScale, currentScale);

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

        float verticalInput = Input.GetAxis("P1_Vertical");

        if (Mathf.Abs(verticalInput) > 0.1f)
        {
            currentScale += verticalInput * zoomSpeed * Time.deltaTime;
            currentScale = Mathf.Clamp(currentScale, minScale, maxScale);

            loupeTransform.localScale = new Vector3(currentScale, currentScale, currentScale);
        }

        float blurAmount = CalculateBlurFromScale(currentScale);
        objectMaterial.SetFloat("_BlurAmount", blurAmount);

        bool currentlyInZone = IsInAcceptableZone(currentScale);

        if (currentlyInZone)
        {
            timeInZone += Time.deltaTime;

            if (timeInZone >= requiredTimeInZone)
            {
                gameEnded = true;
                if (GameManager.Instance != null)
                {
                    if (debugMode) Debug.Log($"[Loop] 🎉 VICTORY! Final scale={currentScale:F4}, target was {targetScale:F4}");
                    GameManager.Instance.NotifyWin();
                }
            }
        }
        else
        {
            timeInZone = 0f;
        }

        if (debugMode)
        {
            UpdateDebugSystem(currentlyInZone);
        }

        wasInZone = currentlyInZone;
    }

    private float CalculateBlurFromScale(float scale)
    {
        float distanceFromTarget = Mathf.Abs(scale - targetScale);
        float normalizedDistance = distanceFromTarget / acceptableRange;
        float blurAmount = Mathf.Clamp01(normalizedDistance);
        return blurAmount;
    }

    private bool IsInAcceptableZone(float scale)
    {
        return Mathf.Abs(scale - targetScale) <= acceptableRange;
    }

    private void HandleTimerEnded()
    {
        if (!gameEnded)
        {
            gameEnded = true;
            if (GameManager.Instance != null)
            {
                GameManager.Instance.NotifyFail();
            }
        }
    }

    private void UpdateDebugSystem(bool currentlyInZone)
    {
        float distance = Mathf.Abs(currentScale - targetScale);
        
        if (debugText != null)
        {
            string statusColor = currentlyInZone ? "#00FF00" : "#FF0000";
            string statusText = currentlyInZone ? "IN ZONE ✅" : "OUT OF ZONE ❌";
            
            debugText.text = $"<color={statusColor}><b>{statusText}</b></color>\n\n" +
                             $"Current Scale: <b>{currentScale:F4}</b>\n" +
                             $"Target Scale: <b>{targetScale:F4}</b>\n" +
                             $"Distance: <b>{distance:F4}</b>\n" +
                             $"Acceptable Range: <b>±{acceptableRange:F4}</b>\n\n" +
                             $"Time in Zone: <b>{timeInZone:F2}s</b> / {requiredTimeInZone:F2}s\n" +
                             $"Progress: <b>{(timeInZone / requiredTimeInZone * 100f):F1}%</b>";
        }
        
        if (currentlyInZone && !wasInZone)
        {
            Debug.Log($"[Loop] ✅ <color=green>ENTERED ZONE</color> | Scale={currentScale:F4} | Target={targetScale:F4} | Distance={distance:F4}");
        }
        else if (!currentlyInZone && wasInZone)
        {
            Debug.Log($"[Loop] ❌ <color=red>LEFT ZONE</color> | Scale={currentScale:F4} | Time accumulated: {timeInZone:F2}s (RESET)");
        }
        else if (currentlyInZone && Mathf.FloorToInt(timeInZone / 0.5f) > Mathf.FloorToInt((timeInZone - Time.deltaTime) / 0.5f))
        {
            Debug.Log($"[Loop] ⏱️ <color=yellow>MAINTAINING ZONE</color> | TimeInZone={timeInZone:F2}s / {requiredTimeInZone:F2}s ({(timeInZone / requiredTimeInZone * 100f):F0}%)");
        }
    }

    void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnTimerEnded -= HandleTimerEnded;
        }

        if (objectMaterial != null)
        {
            Destroy(objectMaterial);
        }
    }
}

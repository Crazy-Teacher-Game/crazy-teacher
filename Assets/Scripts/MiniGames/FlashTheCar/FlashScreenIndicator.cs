using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class FlashScreenIndicator : MonoBehaviour
{
    public enum ScreenType
    {
        Rate,      // Voiture rapide ratée
        Innocent,  // Voiture normale flashée
        Parfait,   // Voiture rapide flashée (succès)
        Fini       // Fin du timer
    }

    [Header("Screen Sprites")]
    [SerializeField] private Sprite rateSprite;
    [SerializeField] private Sprite innocentSprite;
    [SerializeField] private Sprite parfaitSprite;
    [SerializeField] private Sprite finiSprite;

    [Header("UI References")]
    [SerializeField] private Canvas canvas;
    [SerializeField] private Image screenImage;

    [Header("Animation Settings")]
    [SerializeField] private float fadeInDuration = 0.5f;
    [SerializeField] private float displayDuration = 2.0f;
    [SerializeField] private float fadeOutDuration = 0.5f;
    [SerializeField] private float translateDistance = 300f; // Distance depuis le haut

    private Vector3 startPosition;
    private Vector3 centerPosition;
    private Coroutine currentScreenCoroutine;

    void Awake()
    {
        if (screenImage != null)
        {
            centerPosition = screenImage.rectTransform.anchoredPosition;
            startPosition = centerPosition + new Vector3(0, translateDistance, 0);
            screenImage.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Affiche un écran avec animation de fondu et translation
    /// </summary>
    /// <param name="type">Type d'écran à afficher</param>
    /// <param name="onComplete">Callback appelé après la fin de l'animation (3s total)</param>
    public void ShowScreen(ScreenType type, Action onComplete = null)
    {
        // Arrêter l'animation précédente si elle existe
        if (currentScreenCoroutine != null)
        {
            StopCoroutine(currentScreenCoroutine);
        }

        currentScreenCoroutine = StartCoroutine(ShowScreenRoutine(type, onComplete));
    }

    private IEnumerator ShowScreenRoutine(ScreenType type, Action onComplete)
    {
        // Sélectionner le sprite approprié
        Sprite selectedSprite = GetSpriteForType(type);
        if (selectedSprite == null)
        {
            Debug.LogWarning($"FlashScreenIndicator: No sprite assigned for type {type}");
            onComplete?.Invoke();
            yield break;
        }

        screenImage.sprite = selectedSprite;
        screenImage.gameObject.SetActive(true);

        // Phase 1: Fade in + Translation (0.5s)
        float elapsed = 0f;
        while (elapsed < fadeInDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeInDuration;

            // Animation de translation (haut vers centre)
            screenImage.rectTransform.anchoredPosition = Vector3.Lerp(startPosition, centerPosition, t);

            // Animation de fade in
            Color color = screenImage.color;
            color.a = Mathf.Lerp(0f, 1f, t);
            screenImage.color = color;

            yield return null;
        }

        // Assurer la position finale et l'alpha final
        screenImage.rectTransform.anchoredPosition = centerPosition;
        Color finalColor = screenImage.color;
        finalColor.a = 1f;
        screenImage.color = finalColor;

        // Phase 2: Display (2s)
        yield return new WaitForSeconds(displayDuration);

        // Phase 3: Fade out (0.5s)
        elapsed = 0f;
        while (elapsed < fadeOutDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeOutDuration;

            // Animation de fade out
            Color color = screenImage.color;
            color.a = Mathf.Lerp(1f, 0f, t);
            screenImage.color = color;

            yield return null;
        }

        // Désactiver l'image
        screenImage.gameObject.SetActive(false);

        // Appeler le callback
        onComplete?.Invoke();

        currentScreenCoroutine = null;
    }

    private Sprite GetSpriteForType(ScreenType type)
    {
        switch (type)
        {
            case ScreenType.Rate:
                return rateSprite;
            case ScreenType.Innocent:
                return innocentSprite;
            case ScreenType.Parfait:
                return parfaitSprite;
            case ScreenType.Fini:
                return finiSprite;
            default:
                return null;
        }
    }

    /// <summary>
    /// Arrête toute animation en cours et cache l'écran
    /// </summary>
    public void HideScreen()
    {
        if (currentScreenCoroutine != null)
        {
            StopCoroutine(currentScreenCoroutine);
            currentScreenCoroutine = null;
        }

        if (screenImage != null)
        {
            screenImage.gameObject.SetActive(false);
        }
    }
}

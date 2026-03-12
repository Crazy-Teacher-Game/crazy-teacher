using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FlashPhotoStrip : MonoBehaviour
{
    private const float PHOTO_WIDTH = 450f;
    private const float PHOTO_HEIGHT = 330f;
    private const float SPACING = 12f;
    private const float SLIDE_DURATION = 0.3f;

    private RectTransform container;
    private GameObject canvasGO;
    private readonly List<RectTransform> photoRects = new List<RectTransform>();
    private readonly List<Texture2D> textures = new List<Texture2D>();

    void Awake()
    {
        // Canvas dédié à la scène (sera détruit avec la scène)
        canvasGO = new GameObject("FlashPhotoCanvas");
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.overrideSorting = true;
        canvas.sortingOrder = -100;
        canvasGO.AddComponent<CanvasScaler>();

        // Container ancré en bas à droite, décalé de 10 % de la hauteur d'écran
        GameObject containerGO = new GameObject("PhotoContainer");
        containerGO.transform.SetParent(canvasGO.transform, false);
        container = containerGO.AddComponent<RectTransform>();
        container.anchorMin = new Vector2(1f, 0f);
        container.anchorMax = new Vector2(1f, 0f);
        container.pivot = new Vector2(1f, 0f);
        container.anchoredPosition = new Vector2(-30f, Screen.height * 0.2f);
        container.sizeDelta = new Vector2(PHOTO_WIDTH, 0f);
    }

    public void AddPhoto(Texture2D tex)
    {
        textures.Add(tex);

        // Faire monter toutes les photos existantes avant d'ajouter la nouvelle
        if (photoRects.Count > 0)
            StartCoroutine(ShiftPhotosUp());

        // Créer le nouveau polaroïd en bas (y = 0)
        GameObject polaroidGO = new GameObject("Polaroid_" + textures.Count);
        polaroidGO.transform.SetParent(container, false);
        RectTransform polaroidRT = polaroidGO.AddComponent<RectTransform>();
        polaroidRT.sizeDelta = new Vector2(PHOTO_WIDTH, PHOTO_HEIGHT);
        polaroidRT.anchorMin = new Vector2(0.5f, 0f);
        polaroidRT.anchorMax = new Vector2(0.5f, 0f);
        polaroidRT.pivot = new Vector2(0.5f, 0f);
        polaroidRT.localRotation = Quaternion.Euler(0f, 0f, Random.Range(-8f, 8f));

        Image bg = polaroidGO.AddComponent<Image>();
        bg.color = Color.white;

        GameObject photoGO = new GameObject("Photo");
        photoGO.transform.SetParent(polaroidGO.transform, false);
        RectTransform photoRT = photoGO.AddComponent<RectTransform>();
        photoRT.anchorMin = Vector2.zero;
        photoRT.anchorMax = Vector2.one;
        photoRT.offsetMin = new Vector2(10f, 35f);
        photoRT.offsetMax = new Vector2(-10f, -10f);

        RawImage rawImage = photoGO.AddComponent<RawImage>();
        rawImage.texture = tex;
        rawImage.color = new Color(0.8f, 0.8f, 0.8f, 1f); // luminosité réduite

        photoRects.Add(polaroidRT);
        StartCoroutine(SlideIn(polaroidRT));
    }

    // Monte toutes les photos existantes d'un cran
    private IEnumerator ShiftPhotosUp()
    {
        float step = PHOTO_HEIGHT + SPACING;
        var snapshot = new List<(RectTransform rt, float fromY)>();
        // Ne pas déplacer la dernière entrée de photoRects (la nouvelle, pas encore ajoutée ici)
        for (int i = 0; i < photoRects.Count; i++)
            snapshot.Add((photoRects[i], photoRects[i].anchoredPosition.y));

        float elapsed = 0f;
        while (elapsed < SLIDE_DURATION)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / SLIDE_DURATION);
            foreach (var (rt, fromY) in snapshot)
                rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, Mathf.Lerp(fromY, fromY + step, t));
            yield return null;
        }
        foreach (var (rt, fromY) in snapshot)
            rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, fromY + step);
    }

    // Slide-in horizontal depuis la droite, position Y fixe à 0
    private IEnumerator SlideIn(RectTransform rt)
    {
        float startX = PHOTO_WIDTH + 50f;
        float elapsed = 0f;
        while (elapsed < SLIDE_DURATION)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / SLIDE_DURATION);
            rt.anchoredPosition = new Vector2(Mathf.Lerp(startX, 0f, t), rt.anchoredPosition.y);
            yield return null;
        }
        rt.anchoredPosition = new Vector2(0f, rt.anchoredPosition.y);
    }

    public void DestroyTextures()
    {
        foreach (Texture2D tex in textures)
        {
            if (tex != null) Destroy(tex);
        }
        textures.Clear();
    }

    public void Cleanup()
    {
        DestroyTextures();
        if (canvasGO != null) Destroy(canvasGO);
    }
}

using UnityEngine;
using UnityEngine.UI;
using TMPro;

[DefaultExecutionOrder(-100)]
public class MentalMathBootstrapper : MonoBehaviour
{
	void Start()
	{
		EnsureMentalMathHUD();
	}

	private void EnsureMentalMathHUD()
	{
		var canvas = Object.FindObjectOfType<Canvas>();
		if (canvas == null)
		{
			return;
		}

		// Wire existing GameManager to existing TimerUI and LivesUI (from COMMON_Canvas)
		// IMPORTANT: utiliser true pour inclure les objets inactifs (TimerUI peut être caché par Hide())
		var gm = Object.FindObjectOfType<GameManager>();
		var timerUI = Object.FindObjectOfType<TimerUI>(true);
		var livesUI = Object.FindObjectOfType<LivesUI>(true);

		Debug.Log($"[MentalMathBootstrapper] Found timerUI={(timerUI != null)}, livesUI={(livesUI != null)}");

		// If no GameManager exists, create one
		if (gm == null)
		{
			var gmGO = new GameObject("GameManager");
			gm = gmGO.AddComponent<GameManager>();
			Object.DontDestroyOnLoad(gmGO);
			Debug.Log("[MentalMathBootstrapper] Created GameManager");
		}

		if (gm != null)
		{
			// Wire timerUI (public field)
			var timerUIField = typeof(GameManager).GetField("timerUI", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
			if (timerUIField != null)
			{
				timerUIField.SetValue(gm, timerUI);
			}

			// Wire livesUI (private field)
			var livesUIField = typeof(GameManager).GetField("livesUI", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
			if (livesUIField != null)
			{
				livesUIField.SetValue(gm, livesUI);
			}

			// Reinitialize lives display after wiring
			if (livesUI != null)
			{
				livesUI.SetLives(gm.Lives);
			}

			if (timerUI != null && !timerUI.gameObject.activeSelf)
			{
				Debug.Log("[MentalMathBootstrapper] TimerUI was inactive, reactivating");
				timerUI.gameObject.SetActive(true);
			}
		}

		// If COMMON_Canvas has specific children for time bar, wire them into TimerUI
		if (timerUI != null)
		{
			var labelGO = GameObject.Find("TimeLabel");
			var barGO = GameObject.Find("FillBar");
			var label = labelGO ? labelGO.GetComponent<TextMeshProUGUI>() : null;
			var bar = barGO ? barGO.GetComponent<Image>() : null;

			if (label != null && bar != null)
			{
				timerUI.SetRefs(label, bar);
			}
		}

		// Ensure MentalMath components exist and are wired
		EnsureMentalMath(canvas);
	}

	private void EnsureMentalMath(Canvas canvas)
	{
		var oldLogic = Object.FindObjectOfType<CalculLogic>();
		var oldUI = Object.FindObjectOfType<CalculUIManager>();
		var oldMgr = Object.FindObjectOfType<MiniGame_CalculManager>();

		if (oldLogic != null)
		{
			Debug.Log("[MentalMathBootstrapper] Destroying old CalculLogic");
			DestroyImmediate(oldLogic.gameObject);
		}
		if (oldUI != null)
		{
			Debug.Log("[MentalMathBootstrapper] Destroying old CalculUIManager");
			DestroyImmediate(oldUI.gameObject);
		}
		if (oldMgr != null)
		{
			Debug.Log("[MentalMathBootstrapper] Destroying old MiniGame_CalculManager");
			DestroyImmediate(oldMgr.gameObject);
		}

		// Créer les nouveaux composants sous ce Bootstrapper (qui sera détruit avec la scène)
		var logicGO = new GameObject("CalculLogic");
		logicGO.transform.SetParent(transform, false);
		var logic = logicGO.AddComponent<CalculLogic>();
		Debug.Log("[MentalMathBootstrapper] Created new CalculLogic");

		var uiGO = new GameObject("CalculUIManager");
		uiGO.transform.SetParent(transform, false);
		var ui = uiGO.AddComponent<CalculUIManager>();
		Debug.Log("[MentalMathBootstrapper] Created new CalculUIManager");

		var mgrGO = new GameObject("MiniGame_CalculManager");
		mgrGO.transform.SetParent(transform, false);
		var mgr = mgrGO.AddComponent<MiniGame_CalculManager>();
		Debug.Log("[MentalMathBootstrapper] Created new MiniGame_CalculManager");

		// Wire UI references by scene object names
		var calcTextObj = GameObject.Find("calculation-value");
		var propTextObj = GameObject.Find("proposition-value");
		var successImgObj = GameObject.Find("SucessImage");
		var failImgObj = GameObject.Find("FailImage");

		var calcText = calcTextObj ? calcTextObj.GetComponent<TMPro.TMP_Text>() : null;
		var refTMP = propTextObj ? propTextObj.GetComponent<TMPro.TMP_Text>() : null;
		var successImg = successImgObj ? successImgObj.GetComponent<Image>() : null;
		var failImg = failImgObj ? failImgObj.GetComponent<Image>() : null;

		// Hide feedback images immediately to match default state
		if (successImg != null) successImg.enabled = false;
		if (failImg != null) failImg.enabled = false;

		// Replace proposition-value with 3 image+text slots in a HorizontalLayoutGroup
		Image labelImg1 = null, labelImg2 = null, labelImg3 = null;
		TMPro.TMP_Text answerTxt1 = null, answerTxt2 = null, answerTxt3 = null;

		if (propTextObj != null)
		{
			var propRect = propTextObj.GetComponent<RectTransform>();
			var propParent = propTextObj.transform.parent;

			// Hide original text
			propTextObj.SetActive(false);

			// Outer container at the same position
			var container = new GameObject("AnswersContainer");
			container.transform.SetParent(propParent, false);
			var containerRect = container.AddComponent<RectTransform>();
			if (propRect != null)
			{
				containerRect.anchorMin = propRect.anchorMin;
				containerRect.anchorMax = propRect.anchorMax;
				containerRect.anchoredPosition = propRect.anchoredPosition;
				// Hauteur copiée, largeur calculée : 3 slots * 200 + 2 espaces * 30
				// 3 slots * 270 + 2 espaces * 30 = 870
				containerRect.sizeDelta = new Vector2(870, propRect.sizeDelta.y);
				containerRect.pivot = propRect.pivot;
			}
			var outerHLG = container.AddComponent<HorizontalLayoutGroup>();
			outerHLG.spacing = 30;
			outerHLG.childAlignment = TextAnchor.MiddleCenter;
			outerHLG.childControlWidth = false;
			outerHLG.childControlHeight = false;
			outerHLG.childForceExpandWidth = false;
			outerHLG.childForceExpandHeight = false;

			(labelImg1, answerTxt1) = CreateAnswerSlot(container.transform, 1, refTMP);
			(labelImg2, answerTxt2) = CreateAnswerSlot(container.transform, 2, refTMP);
			(labelImg3, answerTxt3) = CreateAnswerSlot(container.transform, 3, refTMP);
		}

		// Assign fields via reflection (private serialized)
		if (ui != null)
		{
			typeof(CalculUIManager).GetField("questionText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
				?.SetValue(ui, calcText);
			typeof(CalculUIManager).GetField("successImage", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
				?.SetValue(ui, successImg);
			typeof(CalculUIManager).GetField("failImage", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
				?.SetValue(ui, failImg);
			typeof(CalculUIManager).GetField("answerText1", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
				?.SetValue(ui, answerTxt1);
			typeof(CalculUIManager).GetField("answerText2", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
				?.SetValue(ui, answerTxt2);
			typeof(CalculUIManager).GetField("answerText3", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
				?.SetValue(ui, answerTxt3);
			typeof(CalculUIManager).GetField("answerLabelImage1", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
				?.SetValue(ui, labelImg1);
			typeof(CalculUIManager).GetField("answerLabelImage2", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
				?.SetValue(ui, labelImg2);
			typeof(CalculUIManager).GetField("answerLabelImage3", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
				?.SetValue(ui, labelImg3);
		}

		var gm = Object.FindObjectOfType<GameManager>();
		if (mgr != null)
		{
			Debug.Log($"[MentalMathBootstrapper] Wiring MiniGame_CalculManager with GameManager={(gm!=null)}");
			typeof(MiniGame_CalculManager).GetField("gameManager", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
				?.SetValue(mgr, gm);
			typeof(MiniGame_CalculManager).GetField("calculLogic", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
				?.SetValue(mgr, logic);
			typeof(MiniGame_CalculManager).GetField("calculUIManager", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
				?.SetValue(mgr, ui);
		}
	}

	private (Image, TMPro.TMP_Text) CreateAnswerSlot(Transform parent, int index, TMPro.TMP_Text refTMP)
	{
		// Slot: image + text côte à côte
		var slot = new GameObject($"AnswerSlot{index}");
		slot.transform.SetParent(parent, false);
		var slotRect = slot.AddComponent<RectTransform>();
		slotRect.sizeDelta = new Vector2(270, 90);

		var hlg = slot.AddComponent<HorizontalLayoutGroup>();
		hlg.spacing = 10;
		hlg.childAlignment = TextAnchor.MiddleCenter;
		hlg.childControlWidth = false;
		hlg.childControlHeight = false;
		hlg.childForceExpandWidth = false;
		hlg.childForceExpandHeight = false;

		// Image (label bouton B1/B2/B3)
		var imgGO = new GameObject($"B{index}Label");
		imgGO.transform.SetParent(slot.transform, false);
		var imgRect = imgGO.AddComponent<RectTransform>();
		imgRect.sizeDelta = new Vector2(90, 90);
		var img = imgGO.AddComponent<Image>();
		img.preserveAspect = true;

		// TMP (valeur de la réponse)
		var txtGO = new GameObject($"AnswerText{index}");
		txtGO.transform.SetParent(slot.transform, false);
		var txtRect = txtGO.AddComponent<RectTransform>();
		txtRect.sizeDelta = new Vector2(160, 80);
		var txt = txtGO.AddComponent<TMPro.TextMeshProUGUI>();

		if (refTMP != null)
		{
			txt.font = refTMP.font;
			txt.fontSize = refTMP.fontSize;
			txt.color = refTMP.color;
		}
		else
		{
			txt.fontSize = 36;
		}
		txt.alignment = TMPro.TextAlignmentOptions.Midline;

		return (img, txt);
	}
}

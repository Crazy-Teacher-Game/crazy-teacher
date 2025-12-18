using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MentalMathBootstrapper : MonoBehaviour
{
	void Awake()
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
		var gm = Object.FindObjectOfType<GameManager>();
		var timerUI = Object.FindObjectOfType<TimerUI>();
		var livesUI = Object.FindObjectOfType<LivesUI>();
		
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
		var logic = Object.FindObjectOfType<CalculLogic>();
		var ui = Object.FindObjectOfType<CalculUIManager>();
		var mgr = Object.FindObjectOfType<MiniGame_CalculManager>();

		// Ensure gameplay components exist (safe to create these; we only avoid creating HUD)
		if (logic == null)
		{
			var go = new GameObject("CalculLogic");
			go.transform.SetParent(canvas.transform, false);
			logic = go.AddComponent<CalculLogic>();
		}
		if (ui == null)
		{
			var go = new GameObject("CalculUIManager");
			go.transform.SetParent(canvas.transform, false);
			ui = go.AddComponent<CalculUIManager>();
		}
		if (mgr == null)
		{
			var go = new GameObject("MiniGame_CalculManager");
			go.transform.SetParent(canvas.transform, false);
			mgr = go.AddComponent<MiniGame_CalculManager>();
		}

		// Wire UI references by scene object names
		var calcTextObj = GameObject.Find("calculation-value");
		var propTextObj = GameObject.Find("proposition-value");
		var successImgObj = GameObject.Find("SucessImage");
		var failImgObj = GameObject.Find("FailImage");

		var calcText = calcTextObj ? calcTextObj.GetComponent<TMPro.TMP_Text>() : null;
		var propText = propTextObj ? propTextObj.GetComponent<TMPro.TMP_Text>() : null;
		var successImg = successImgObj ? successImgObj.GetComponent<Image>() : null;
		var failImg = failImgObj ? failImgObj.GetComponent<Image>() : null;

		// Hide feedback images immediately to match default state
		if (successImg != null) successImg.enabled = false;
		if (failImg != null) failImg.enabled = false;

		// Assign fields via reflection (private serialized)
		if (ui != null)
		{
			typeof(CalculUIManager).GetField("questionText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
				?.SetValue(ui, calcText);
			typeof(CalculUIManager).GetField("propositionsText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
				?.SetValue(ui, propText);
			typeof(CalculUIManager).GetField("successImage", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
				?.SetValue(ui, successImg);
			typeof(CalculUIManager).GetField("failImage", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
				?.SetValue(ui, failImg);
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
}

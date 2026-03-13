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

			// Réactiver le TimerUI s'il était caché
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
		// Détruire les anciens composants s'ils existent (ils peuvent avoir des références cassées après un rechargement)
		var oldLogic = Object.FindObjectOfType<CalculLogic>();
		var oldUI = Object.FindObjectOfType<CalculUIManager>();
		var oldMgr = Object.FindObjectOfType<MiniGame_CalculManager>();

		if (oldLogic != null)
		{
			Debug.Log("[MentalMathBootstrapper] Destroying old CalculLogic");
			Object.Destroy(oldLogic.gameObject);
		}
		if (oldUI != null)
		{
			Debug.Log("[MentalMathBootstrapper] Destroying old CalculUIManager");
			Object.Destroy(oldUI.gameObject);
		}
		if (oldMgr != null)
		{
			Debug.Log("[MentalMathBootstrapper] Destroying old MiniGame_CalculManager");
			Object.Destroy(oldMgr.gameObject);
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
		var allTMPs = Object.FindObjectsOfType<TMP_Text>(true);
		var calcText = System.Array.Find(allTMPs, t => t.gameObject.name == "calculation-value");
		var propText = System.Array.Find(allTMPs, t => t.gameObject.name == "proposition-value");
		var progressText = System.Array.Find(allTMPs, t => t.gameObject.name == "progress-value");

		var allImages = Object.FindObjectsOfType<Image>(true);
		var successImg = System.Array.Find(allImages, img => img.gameObject.name == "SucessImage");
		var failImg = System.Array.Find(allImages, img => img.gameObject.name == "FailImage");

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
			typeof(CalculUIManager).GetField("progressText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
				?.SetValue(ui, progressText);
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

	private Transform FindChildRecursive(Transform parent, string name)
	{
		foreach (Transform child in parent)
		{
			if (child.name == name)
				return child;
			
			var found = FindChildRecursive(child, name);
			if (found != null)
				return found;
		}
		return null;
	}
}

using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class ScenesLoader : MonoBehaviour
{
    public GameObject sceneContainer;

    public void LoadMiniGame(string sceneName)
    {
        ControlType controlType = GameControlsDatabase.GetControlType(sceneName);
        GameManager.Instance.SetControlType(controlType);
        StartCoroutine(LoadMiniGameSequence(sceneName, controlType));
    }

    private IEnumerator LoadMiniGameSequence(string sceneName, ControlType controlType)
    {
        yield return StartCoroutine(LoadTransitionSceneCoroutine("LoadingScene", controlType));
        yield return new WaitForSeconds(2f);
        yield return StartCoroutine(UnloadTransitionSceneCoroutine("LoadingScene"));
        yield return StartCoroutine(LoadMiniGameCoroutine(sceneName));
    }


    private IEnumerator LoadTransitionSceneCoroutine(string sceneName, ControlType type)
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
        while (!asyncLoad.isDone)
            yield return null;
        // UIManager.Instance.SwitchControls(type);

        Scene transitionScene = SceneManager.GetSceneByName(sceneName);
        foreach (GameObject go in transitionScene.GetRootGameObjects())
        {
            go.transform.SetParent(sceneContainer.transform, false);
        }
    }

    private IEnumerator UnloadTransitionSceneCoroutine(string sceneName)
    {
        AsyncOperation asyncUnload = SceneManager.UnloadSceneAsync(sceneName);
        while (!asyncUnload.isDone)
            yield return null;

        foreach (Transform child in sceneContainer.transform)
        {
            Destroy(child.gameObject);
        }
    }

    private IEnumerator LoadMiniGameCoroutine(string sceneName)
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
        while (!asyncLoad.isDone)
            yield return null;

        Scene miniScene = SceneManager.GetSceneByName(sceneName);
        foreach (GameObject go in miniScene.GetRootGameObjects())
        {
            if (go.name == "Main Camera" || go.name == "EventSystem")
            {
                Destroy(go);
                continue;
            }
            go.transform.SetParent(sceneContainer.transform, false);
        }

        // Ensure only one EventSystem exists after scene load
        EnsureSingleEventSystem();
    }

    private void EnsureSingleEventSystem()
    {
        var eventSystems = FindObjectsOfType<UnityEngine.EventSystems.EventSystem>();
        if (eventSystems.Length > 1)
        {
            Debug.LogWarning($"[ScenesLoader] Found {eventSystems.Length} EventSystems! Destroying duplicates...");
            for (int i = 1; i < eventSystems.Length; i++)
            {
                Destroy(eventSystems[i].gameObject);
                Debug.Log($"[ScenesLoader] Destroyed duplicate EventSystem: {eventSystems[i].name}");
            }
        }
    }

    public void UnloadMiniGame(string sceneName)
    {
        SceneManager.UnloadSceneAsync(sceneName);
        foreach (Transform child in sceneContainer.transform)
        {
            Destroy(child.gameObject);
        }
    }
}

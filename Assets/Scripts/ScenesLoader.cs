using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class ScenesLoader : MonoBehaviour
{
    public GameObject sceneContainer;

    public void LoadMiniGame(string sceneName)
    {
        Debug.Log($"[ScenesLoader] LoadMiniGame called: {sceneName}");
        ControlType controlType = GameControlsDatabase.GetControlType(sceneName);
        GameManager.Instance.SetControlType(controlType);
        StartCoroutine(LoadMiniGameSequence(sceneName, controlType));
    }

    private IEnumerator LoadMiniGameSequence(string sceneName, ControlType controlType)
    {
        Debug.Log($"[ScenesLoader] LoadMiniGameSequence START: {sceneName}");
        yield return StartCoroutine(LoadTransitionSceneCoroutine("LoadingScene", controlType));
        yield return new WaitForSeconds(2f);
        yield return StartCoroutine(UnloadTransitionSceneCoroutine("LoadingScene"));
        yield return StartCoroutine(LoadMiniGameCoroutine(sceneName));
        Debug.Log($"[ScenesLoader] LoadMiniGameSequence END: {sceneName}");
    }

    public void LoadGameOverScene()
    {
        StartCoroutine(LoadGameOverSequence());
    }

    private IEnumerator LoadGameOverSequence()
    {
        foreach (Transform child in sceneContainer.transform)
        {
            Destroy(child.gameObject);
        }
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("GameOverScene", LoadSceneMode.Additive);
        while (!asyncLoad.isDone)
            yield return null;

        Scene gameOverScene = SceneManager.GetSceneByName("GameOverScene");
        foreach (GameObject go in gameOverScene.GetRootGameObjects())
        {
            if (go.name == "EventSystem")
            {
                Destroy(go);
                continue;
            }
            if (go.name == "Main Camera")
            {
                var cam = go.GetComponent<Camera>();
                if (cam != null)
                {
                    cam.clearFlags = CameraClearFlags.Skybox;
                    cam.cullingMask = ~(1 << LayerMask.NameToLayer("UI_Global"));
                    cam.depth = 0;
                    Debug.Log($"[ScenesLoader] Mini-game camera configured (depth=0, cullingMask excludes UI_Global)");
                }
                var al = go.GetComponent<AudioListener>();
                if (al != null) al.enabled = false;

                go.transform.SetParent(sceneContainer.transform, true);
                continue;
            }
            go.transform.SetParent(sceneContainer.transform, true);
        }
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
            if (go.name == "EventSystem")
            {
                Destroy(go);
                continue;
            }
            if (go.name == "Main Camera")
            {
                var cam = go.GetComponent<Camera>();
                if (cam != null)
                {
                    cam.clearFlags = CameraClearFlags.Skybox;
                    cam.cullingMask = ~(1 << LayerMask.NameToLayer("UI_Global"));
                    cam.depth = 0;
                    Debug.Log($"[ScenesLoader] Mini-game camera configured (depth=0, cullingMask excludes UI_Global)");
                }
                var al = go.GetComponent<AudioListener>();
                if (al != null) al.enabled = false;

                go.transform.SetParent(sceneContainer.transform, true);
                continue;
            }
            go.transform.SetParent(sceneContainer.transform, true);
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
        Debug.Log($"[ScenesLoader] LoadMiniGameCoroutine START: {sceneName}");
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
        while (!asyncLoad.isDone)
            yield return null;

        Debug.Log($"[ScenesLoader] Scene {sceneName} loaded, reparenting objects...");
        Scene miniScene = SceneManager.GetSceneByName(sceneName);
        foreach (GameObject go in miniScene.GetRootGameObjects())
        {
            if (go.name == "EventSystem")
            {
                Destroy(go);
                continue;
            }
            if (go.name == "Main Camera")
            {
                var cam = go.GetComponent<Camera>();
                if (cam != null)
                {
                    cam.clearFlags = CameraClearFlags.Skybox;
                    cam.cullingMask = ~(1 << LayerMask.NameToLayer("UI_Global"));
                    cam.depth = 0;
                    Debug.Log($"[ScenesLoader] Mini-game camera configured (depth=0, cullingMask excludes UI_Global)");
                }
                var al = go.GetComponent<AudioListener>();
                if (al != null) al.enabled = false;

                go.transform.SetParent(sceneContainer.transform, true);
                continue;
            }
            go.transform.SetParent(sceneContainer.transform, true);
        }
        Debug.Log($"[ScenesLoader] LoadMiniGameCoroutine END: {sceneName}");
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

    public void UnloadGameOverScene()
    {
        StartCoroutine(UnloadGameOverSequence());
    }

    private IEnumerator UnloadGameOverSequence()
    {
        foreach (Transform child in sceneContainer.transform)
        {
            Destroy(child.gameObject);
        }

        yield return null;

        var asyncUnload = SceneManager.UnloadSceneAsync("GameOverScene");
        if (asyncUnload != null)
        {
            while (!asyncUnload.isDone)
                yield return null;
        }
    }

    public void UnloadMiniGame(string sceneName)
    {
        StartCoroutine(UnloadMiniGameCoroutine(sceneName));
    }

    private IEnumerator UnloadMiniGameCoroutine(string sceneName)
    {
        // Destroy children first
        foreach (Transform child in sceneContainer.transform)
        {
            Destroy(child.gameObject);
        }

        // Wait for destruction to complete
        yield return null;

        // Then unload scene
        var asyncUnload = SceneManager.UnloadSceneAsync(sceneName);
        if (asyncUnload != null)
        {
            while (!asyncUnload.isDone)
                yield return null;
        }
    }

    public void UnloadAndLoadMiniGame(string unloadScene, string loadScene)
    {
        Debug.Log($"[ScenesLoader] UnloadAndLoadMiniGame called: unload={unloadScene}, load={loadScene}");
        StartCoroutine(UnloadAndLoadSequence(unloadScene, loadScene));
    }

    private IEnumerator UnloadAndLoadSequence(string unloadScene, string loadScene)
    {
        Debug.Log($"[ScenesLoader] UnloadAndLoadSequence START: unload={unloadScene}, load={loadScene}");

        // First, fully unload the current scene
        int childCount = sceneContainer.transform.childCount;
        Debug.Log($"[ScenesLoader] Destroying {childCount} children in sceneContainer");
        foreach (Transform child in sceneContainer.transform)
        {
            Debug.Log($"[ScenesLoader] Destroying child: {child.name}");
            Destroy(child.gameObject);
        }

        // Wait for destruction to complete
        yield return null;
        Debug.Log($"[ScenesLoader] Children destroyed, now unloading scene: {unloadScene}");

        var asyncUnload = SceneManager.UnloadSceneAsync(unloadScene);
        if (asyncUnload != null)
        {
            while (!asyncUnload.isDone)
                yield return null;
            Debug.Log($"[ScenesLoader] Scene {unloadScene} unloaded successfully");
        }
        else
        {
            Debug.LogWarning($"[ScenesLoader] Could not unload scene: {unloadScene} (asyncUnload is null)");
        }

        // Now load the new scene
        Debug.Log($"[ScenesLoader] Now loading new scene: {loadScene}");
        ControlType controlType = GameControlsDatabase.GetControlType(loadScene);
        GameManager.Instance.SetControlType(controlType);
        yield return StartCoroutine(LoadMiniGameSequence(loadScene, controlType));
        Debug.Log($"[ScenesLoader] UnloadAndLoadSequence END");
    }
}

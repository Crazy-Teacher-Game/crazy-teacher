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
        GameManager.Instance.ShowDescription(sceneName);
    }

    public void LoadGameOverScene()
    {
        StartCoroutine(LoadGameOverSequence());
    }

    private IEnumerator LoadGameOverSequence()
    {
        foreach (Transform child in sceneContainer.transform)
        {
            child.gameObject.SetActive(false);
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
            child.gameObject.SetActive(false);
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
                }
                var al = go.GetComponent<AudioListener>();
                if (al != null) al.enabled = false;

                go.transform.SetParent(sceneContainer.transform, true);
                continue;
            }
            go.transform.SetParent(sceneContainer.transform, true);
        }
    }

    private void EnsureSingleEventSystem()
    {
        var eventSystems = FindObjectsOfType<UnityEngine.EventSystems.EventSystem>();
        if (eventSystems.Length > 1)
        {
            for (int i = 1; i < eventSystems.Length; i++)
            {
                Destroy(eventSystems[i].gameObject);
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
            child.gameObject.SetActive(false);
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
            child.gameObject.SetActive(false);
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
        StartCoroutine(UnloadAndLoadSequence(unloadScene, loadScene));
    }

    private IEnumerator UnloadAndLoadSequence(string unloadScene, string loadScene)
    {
        // First, fully unload the current scene
        int childCount = sceneContainer.transform.childCount;
        foreach (Transform child in sceneContainer.transform)
        {
            child.gameObject.SetActive(false);
            Destroy(child.gameObject);
        }

        // Wait for destruction to complete
        yield return null;

        var asyncUnload = SceneManager.UnloadSceneAsync(unloadScene);
        if (asyncUnload != null)
        {
            while (!asyncUnload.isDone)
                yield return null;
        }
        else
        {
            Debug.LogWarning($"[ScenesLoader] Could not unload scene: {unloadScene} (asyncUnload is null)");
        }

        // Now load the new scene
        ControlType controlType = GameControlsDatabase.GetControlType(loadScene);
        GameManager.Instance.SetControlType(controlType);
        yield return StartCoroutine(LoadMiniGameSequence(loadScene, controlType));
    }
}

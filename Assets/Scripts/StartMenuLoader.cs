using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class StartMenuLoader : MonoBehaviour
{
    public GameObject sceneContainer;
    private const float JoystickThreshold = 0.5f;
    private bool menuLoaded = false;
    private bool inputDetected = false;

    void Start()
    {
        StartCoroutine(LoadStartMenuSequence());
    }

    void Update()
    {
        if (menuLoaded && !inputDetected && DetectAnyInput())
        {
            inputDetected = true;
            StartCoroutine(UnloadStartMenuAndStartGame());
        }
    }

    private IEnumerator LoadStartMenuSequence()
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("HomePage", LoadSceneMode.Additive);
        while (!asyncLoad.isDone)
            yield return null;

        Scene homePageScene = SceneManager.GetSceneByName("HomePage");
        foreach (GameObject go in homePageScene.GetRootGameObjects())
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
                    cam.depth = 0;
                }
                var al = go.GetComponent<AudioListener>();
                if (al != null) al.enabled = false;

                go.transform.SetParent(sceneContainer.transform, true);
                continue;
            }
            go.transform.SetParent(sceneContainer.transform, true);
        }

        menuLoaded = true;
    }

    private IEnumerator UnloadStartMenuAndStartGame()
    {
        foreach (Transform child in sceneContainer.transform)
        {
            Destroy(child.gameObject);
        }

        yield return null;

        var asyncUnload = SceneManager.UnloadSceneAsync("HomePage");
        if (asyncUnload != null)
        {
            while (!asyncUnload.isDone)
                yield return null;
        }

        GameManager.Instance.StartGame();
    }

    private bool DetectAnyInput()
    {
        if (Input.GetButtonDown("P1_B1") || Input.GetButtonDown("P1_B2") || Input.GetButtonDown("P1_B3") ||
            Input.GetButtonDown("P1_B4") || Input.GetButtonDown("P1_B5") || Input.GetButtonDown("P1_B6") ||
            Input.GetButtonDown("P1_Start"))
        {
            return true;
        }

        if (Input.GetButtonDown("P2_B1") || Input.GetButtonDown("P2_B2") || Input.GetButtonDown("P2_B3") ||
            Input.GetButtonDown("P2_B4") || Input.GetButtonDown("P2_B5") || Input.GetButtonDown("P2_B6") ||
            Input.GetButtonDown("P2_Start"))
        {
            return true;
        }

        float p1Horizontal = Input.GetAxisRaw("P1_Horizontal");
        float p1Vertical = Input.GetAxisRaw("P1_Vertical");
        if (Mathf.Abs(p1Horizontal) > JoystickThreshold || Mathf.Abs(p1Vertical) > JoystickThreshold)
        {
            return true;
        }

        float p2Horizontal = Input.GetAxisRaw("P2_Horizontal");
        float p2Vertical = Input.GetAxisRaw("P2_Vertical");
        if (Mathf.Abs(p2Horizontal) > JoystickThreshold || Mathf.Abs(p2Vertical) > JoystickThreshold)
        {
            return true;
        }

        return false;
    }
}

using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverManager : MonoBehaviour
{
    public GameObject replayText;

    void Awake()
    {
        GameManager.Instance.RegisterGameOverManager(this);
    }

    public void ShowReplayText()
    {
        replayText.SetActive(true);
    }
}
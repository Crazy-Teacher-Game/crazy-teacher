using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Platform : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        GameManager.Instance.StartTimer(durationSeconds);
    }

    // void HandleTimeout()
    // {
    //     GameManager.Instance.NotifyFail();
    // }
    // void AfterWin()
    // {
    //     GameManager.Instance.AddRound();
    // }

    // void AfterFail()
    // {
    //     GameManager.Instance.LoseLife();
    // }

    // Update is called once per frame
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Platform : MonoBehaviour
{
    // Start is called before the first frame update
    public float durationSeconds = 10f;
    void Start()
    {
        GameManager.Instance.StartTimer(durationSeconds);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("ball"))
        {
            Destroy(other.gameObject);
        }
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

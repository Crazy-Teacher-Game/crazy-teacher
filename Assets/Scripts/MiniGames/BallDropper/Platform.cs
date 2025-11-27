using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Platform : MonoBehaviour
{
    [Header("Rotation")]
    public float rotationSpeedDegreesPerSecond = 70f;
    public Vector3 localRotationAxis = Vector3.up;
    public float durationSeconds = 10f;
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
    void Update()
    {
        if (rotationSpeedDegreesPerSecond == 0f)
        {
            return;
        }
        float deltaDegrees = rotationSpeedDegreesPerSecond * Time.deltaTime;
        transform.Rotate(localRotationAxis.normalized, deltaDegrees, Space.Self);
    }
}

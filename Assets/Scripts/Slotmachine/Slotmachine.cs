using System.Collections;
using UnityEngine;

public class Slotmachine : MonoBehaviour
{
    [SerializeField] private Wheel wheel1;
    [SerializeField] private Wheel wheel2;
    [SerializeField] private Wheel wheel3;
    [SerializeField] private WheelFrame wheelFrame1;
    [SerializeField] private WheelFrame wheelFrame2;
    [SerializeField] private WheelFrame wheelFrame3;
    [SerializeField] float durationSeconds = 8f;

    private int level = 1;
    private bool gameEnded = false;
    private bool isResetting = false;
    private bool btnDownLastUpdate = false;

    void Start()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnTimerEnded += HandleTimerEnded;
            GameManager.Instance.StartTimer(durationSeconds);
        }

        // Wheels start spinning immediately
        wheel1.StartSpin();
        wheel2.StartSpin();
        wheel3.StartSpin();
    }

    void Update()
    {
        if (gameEnded || isResetting)
        {
            btnDownLastUpdate = Input.GetButton("P1_B1");
            return;
        }

        if (Input.GetButton("P1_B1"))
        {
            if (btnDownLastUpdate) return;
            btnDownLastUpdate = true;
            if (level == 1)
            {
                bool pass = wheel1.Stop();
                if (pass)
                {
                    level = 2;
                }
                else
                {
                    TriggerMiss();
                }
            }
            else if (level == 2)
            {
                bool pass = wheel2.Stop();
                if (pass)
                {
                    level = 3;
                }
                else
                {
                    TriggerMiss();
                }
            }
            else if (level == 3)
            {
                bool pass = wheel3.Stop();
                if (pass)
                {
                    gameEnded = true;
                    GameManager.Instance.NotifyWin();
                }
                else
                {
                    TriggerMiss();
                }
            }
        }
        else
        {
            btnDownLastUpdate = false;
        }
    }

    private void TriggerMiss()
    {
        isResetting = true;
        SetAllFrames(Color.red);
        wheel1.Stop();
        wheel2.Stop();
        wheel3.Stop();
        StartCoroutine(ResetAfterDelay());
    }

    private IEnumerator ResetAfterDelay()
    {
        yield return new WaitForSeconds(0.5f);
        if (!gameEnded)
        {
            SetAllFrames(Color.yellow);
            wheel1.StartSpin();
            wheel2.StartSpin();
            wheel3.StartSpin();
            level = 1;
        }
        isResetting = false;
    }

    private void SetAllFrames(Color color)
    {
        wheelFrame1.SetColor(color);
        wheelFrame2.SetColor(color);
        wheelFrame3.SetColor(color);
    }

    private void HandleTimerEnded()
    {
        if (!gameEnded)
        {
            gameEnded = true;
            GameManager.Instance.NotifyFail();
        }
    }

    void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnTimerEnded -= HandleTimerEnded;
        }
    }
}

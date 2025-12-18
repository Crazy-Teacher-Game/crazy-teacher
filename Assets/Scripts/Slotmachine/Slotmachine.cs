using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Slotmachine : MonoBehaviour
{
    [SerializeField] private Wheel wheel1;
    [SerializeField] private Wheel wheel2;
    [SerializeField] private Wheel wheel3;
    [SerializeField] private WheelFrame wheelFrame1;
    [SerializeField] private WheelFrame wheelFrame2;
    [SerializeField] private WheelFrame wheelFrame3;
    [SerializeField] float durationSeconds = 5f; //DURÉE DU MINI JEU

    private int level = 1;
    private bool btnDownLastUpdate = false;

    // Start is called before the first frame update
    void Start()
    {
        GameManager.Instance.StartTimer(durationSeconds);
        GameManager.Instance.OnTimerEnded += HandleTimeout;
        GameManager.Instance.OnMinigameWon += AfterWin;
        GameManager.Instance.OnMinigameFailed += AfterFail;
    }

    public void OnPlayerSucceeded()
    {
        GameManager.Instance.NotifyWin();
    }

    // --- LOGIQUE D'ECHEC ---
    void HandleTimeout()
    {
        GameManager.Instance.NotifyFail();
    }

    // --- CE QU'ON VEUT FAIRE À LA FIN D'UN MINI-JEU ---
    void AfterWin()
    {
        GameManager.Instance.AddRound();
    }

    void AfterFail()
    {
        GameManager.Instance.LoseLife();
    }

    // Update is called once per frame
    void Update()
    {
        if (level == -1)
        {
            wheel1.Stop();
            wheel2.Stop();
            wheel3.Stop();

            level = 0;
        }

        if (Input.GetButton("P1_B1"))
        {
            if (btnDownLastUpdate) return;
            btnDownLastUpdate = true;
            if (level == 0)
            {
                //start all the wheels spinning again
                wheel1.StartSpin();
                wheel2.StartSpin();
                wheel3.StartSpin();
                level = 1;
            }
            else if (level == 1)
            {
                bool pass = wheel1.Stop();
                if (pass)
                {
                    level++;
                }
                else
                {
                    level = 0;
                }
            }
            else if (level == 2)
            {
                bool pass2 = wheel2.Stop();
                if (pass2)
                {
                    level++;
                }
                else
                {
                    level = 0;
                }
            }
            else if (level == 3)
            {
                bool pass3 = wheel3.Stop();
                if (pass3)
                {
                    GameManager.Instance.NotifyWin();
                    level = 0;
                }
                else
                {
                    level = 0;
                }
            }
        }
        else
        {
            btnDownLastUpdate = false;
        }
    }
}

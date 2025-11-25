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
    public GameManager gameManager;

    private int level = -1;
    private bool btnDownLastUpdate = false;

    // Start is called before the first frame update
    void Start()
    {
        gameManager.StartTimer(durationSeconds);
        gameManager.OnTimerEnded += HandleTimeout;
        gameManager.OnMinigameWon += AfterWin;
        gameManager.OnMinigameFailed += AfterFail;
    }

    public void OnPlayerSucceeded()
    {
        gameManager.NotifyWin();
    }

    // --- LOGIQUE D'ECHEC ---
    void HandleTimeout()
    {
        gameManager.NotifyFail();
    }

    // --- CE QU'ON VEUT FAIRE À LA FIN D'UN MINI-JEU ---
    void AfterWin()
    {
        gameManager.AddRound();
    }

    void AfterFail()
    {
        gameManager.LoseLife();
    }

    // Update is called once per frame
    void Update()
    {
        if (level == -1)
        {
            wheel1.Stop();
            wheel2.Stop();
            wheel3.Stop();

            Debug.Log("Press space to start the slot machine!");
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
                    Debug.Log("Level up! Now at level " + level);
                }
                else
                {
                    Debug.Log("Try again!");
                    level = 0;
                }
            }
            else if (level == 2)
            {
                bool pass2 = wheel2.Stop();
                if (pass2)
                {
                    level++;
                    Debug.Log("Level up! Now at level " + level);
                }
                else
                {
                    Debug.Log("Try again!");
                    level = 0;
                }
            }
            else if (level == 3)
            {
                bool pass3 = wheel3.Stop();
                if (pass3)
                {
                    Debug.Log("You win the jackpot!");
                    gameManager.NotifyWin();
                    level = 0;
                }
                else
                {
                    Debug.Log("Try again!");
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

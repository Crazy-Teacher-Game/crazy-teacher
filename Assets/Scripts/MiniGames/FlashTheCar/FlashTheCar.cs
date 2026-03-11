using System.Collections;
using UnityEngine;

public class FlashTheCar : MonoBehaviour
{
    [SerializeField] private GameObject car1;
    [SerializeField] private GameObject car2;
    [SerializeField] private GameObject car3;
    [SerializeField] private GameObject car4_race;
    [SerializeField] private GameObject car5_taxi;
    [SerializeField] private GameObject car6_police;
    private float fixedWorldX = 206.19f;
    private float normalSpeed = 0.5f;
    private float fastSpeed = 1.4f;
    private float startZ = 310f;

    // State variables
    private float destroyZ = -30f;
    private float zoneMin = 230f;
    private float zoneMax = 263f;
    private float timerDuration = 30f;
    private int requiredFlashes = 3;

    private GameObject[] cars;
    private float[] originalY;
    private float[] carSpeeds;
    private bool[] carIsFast;

    private int lastCarIndex = -1;
    private bool inputWindowOpen;
    private bool hasPressedThisTurn;
    private bool currentCarHasPassedZone;
    private bool hasTriggeredNextCar;
    private int flashCount = 0;
    private bool gameEnded = false;

    void Start()
    {
        cars = new GameObject[] { car1, car2, car3, car4_race, car5_taxi, car6_police };

        originalY = new float[6];
        carSpeeds = new float[6];
        carIsFast = new bool[6];

        for (int i = 0; i < 6; i++)
        {
            originalY[i] = cars[i].transform.position.y;
            cars[i].SetActive(false);
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnTimerEnded += HandleTimerEnded;
            GameManager.Instance.StartTimer(timerDuration);
        }
        LaunchNewCar();
    }

    private void LaunchNewCar()
    {
        int carIndex;
        do
        {
            carIndex = Random.Range(0, 6);
        } while (carIndex == lastCarIndex);

        lastCarIndex = carIndex;
        cars[carIndex].transform.position = new Vector3(fixedWorldX, originalY[carIndex], startZ);
        cars[carIndex].SetActive(true);
        carIsFast[carIndex] = (Random.value < 0.25f);
        carSpeeds[carIndex] = carIsFast[carIndex] ? fastSpeed : normalSpeed;

        inputWindowOpen = false;
        hasPressedThisTurn = false;
        currentCarHasPassedZone = false;
        hasTriggeredNextCar = false;
    }

    void FixedUpdate()
    {
        if (gameEnded) return;

        // Step 1 — Move all active cars and clean up
        for (int i = 0; i < 6; i++)
        {
            if (cars[i].activeSelf)
            {
                float newZ = cars[i].transform.position.z - carSpeeds[i];
                cars[i].transform.position = new Vector3(fixedWorldX, cars[i].transform.position.y, newZ);

                if (newZ < destroyZ)
                {
                    cars[i].SetActive(false);
                    cars[i].transform.position = new Vector3(fixedWorldX, originalY[i], startZ);
                }
            }
        }

        // Step 2 — Track current car (lastCarIndex) for input and triggering
        if (lastCarIndex >= 0 && cars[lastCarIndex].activeSelf)
        {
            float curZ = cars[lastCarIndex].transform.position.z;

            // Enter window
            if (carIsFast[lastCarIndex] && curZ >= zoneMin && curZ <= zoneMax && !hasPressedThisTurn)
            {
                inputWindowOpen = true;
            }

            // Exit window
            if (curZ < zoneMin && inputWindowOpen)
            {
                inputWindowOpen = false;
                if (!hasPressedThisTurn)
                {
                    LoseLifeAndCheck();
                }
            }

            // Passage marker
            if (curZ < zoneMin && !currentCarHasPassedZone)
            {
                currentCarHasPassedZone = true;
            }
        }

        // Step 3 — Trigger next car
        if (currentCarHasPassedZone && !AnyCarInZone() && !hasTriggeredNextCar && !gameEnded)
        {
            hasTriggeredNextCar = true;
            StartCoroutine(NextCarWithDelay());
        }
    }

    private bool AnyCarInZone()
    {
        for (int i = 0; i < 6; i++)
        {
            if (cars[i].activeSelf && cars[i].transform.position.z >= zoneMin && cars[i].transform.position.z <= zoneMax)
            {
                return true;
            }
        }
        return false;
    }

    void Update()
    {
        if (gameEnded) return;

        if (Input.GetButtonDown("P1_B1"))
        {
            if (inputWindowOpen && !hasPressedThisTurn)
            {
                // Good press
                hasPressedThisTurn = true;
                inputWindowOpen = false;
                flashCount++;
                if (flashCount >= requiredFlashes)
                {
                    gameEnded = true;
                    GameManager.Instance.NotifyWin();
                }
            }
            else if (!hasPressedThisTurn)
            {
                // Bad press
                hasPressedThisTurn = true;
                LoseLifeAndCheck();
            }
        }
    }

    private IEnumerator NextCarWithDelay()
    {
        yield return new WaitForSeconds(Random.Range(0f, 2f));
        if (!gameEnded)
        {
            LaunchNewCar();
        }
    }

    private void LoseLifeAndCheck()
    {
        GameManager.Instance.LoseLife();
        if (GameManager.Instance.Lives <= 0)
        {
            gameEnded = true;
            GameManager.Instance.NotifyFail();
        }
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

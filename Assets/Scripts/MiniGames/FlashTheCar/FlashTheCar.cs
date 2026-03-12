using System.Collections;
using UnityEngine;

public class FlashTheCar : MonoBehaviour
{
    [SerializeField] private Light flashLight;
    [SerializeField] private GameObject car1;
    [SerializeField] private GameObject car2;
    [SerializeField] private GameObject car3;
    [SerializeField] private GameObject car4_race;
    [SerializeField] private GameObject car5_taxi;
    [SerializeField] private GameObject car6_police;
    private float fixedWorldX = 206.19f;
    private float fixedWorldXFast = 211.4f;
    private float normalSpeed = 0.5f;
    private float fastSpeed = 1f;
    private float startZ = 310f;

    // State variables
    private float destroyZ = -30f;
    private float zoneMin = 230f;
    private float zoneMax = 263f;
    private float timerDuration = 30f;
    private float timerMinDuration = 10f;
    private int requiredFlashes = 3;
    private float spawnChance; // Calculated based on difficulty for fast cars

    private GameObject[] cars;
    private Transform[] carTransforms;
    private float[] originalY;
    private float[] carSpeeds;
    private float[] carWorldX;
    private bool[] carIsFast;

    private int lastCarIndex = -1;
    private bool isFirstCar = true;
    private bool inputWindowOpen;
    private bool hasPressedThisTurn;
    private bool currentCarHasPassedZone;
    private bool hasTriggeredNextCar;
    private bool anyFastCarPassedZone = false;
    private int flashCount = 0;
    private bool gameEnded = false;

    void Start()
    {
        cars = new GameObject[] { car1, car2, car3, car4_race, car5_taxi, car6_police };

        originalY = new float[6];
        carSpeeds = new float[6];
        carWorldX = new float[6];
        carIsFast = new bool[6];
        carTransforms = new Transform[6];

        for (int i = 0; i < 6; i++)
        {
            carTransforms[i] = cars[i].transform;
            originalY[i] = carTransforms[i].position.y;
            cars[i].SetActive(false);
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnTimerEnded += HandleTimerEnded;
            GameManager.Instance.StartTimer(timerDuration, timerMinDuration);
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
        cars[carIndex].SetActive(true);
        spawnChance = 0.33f + 0.33f * GameManager.Instance.DifficultyFactor; // 33% base, up to 66% at max difficulty
        carIsFast[carIndex] = !isFirstCar && (Random.value < spawnChance);
        isFirstCar = false;
        carSpeeds[carIndex] = carIsFast[carIndex] ? fastSpeed * (1f + GameManager.Instance.DifficultyFactor) : normalSpeed;
        carWorldX[carIndex] = carIsFast[carIndex] ? fixedWorldXFast : fixedWorldX;
        carTransforms[carIndex].position = new Vector3(carWorldX[carIndex], originalY[carIndex], startZ);

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
                float newZ = carTransforms[i].position.z - carSpeeds[i];
                carTransforms[i].position = new Vector3(carWorldX[i], carTransforms[i].position.y, newZ);

                if (newZ < destroyZ)
                {
                    cars[i].SetActive(false);
                    carTransforms[i].position = new Vector3(carWorldX[i], originalY[i], startZ);
                }
            }
        }

        // Step 2 — Track current car (lastCarIndex) for input and triggering
        if (lastCarIndex >= 0 && cars[lastCarIndex].activeSelf)
        {
            float curZ = carTransforms[lastCarIndex].position.z;

            // Enter window
            if (curZ >= zoneMin && curZ <= zoneMax && !hasPressedThisTurn)
            {
                inputWindowOpen = true;
            }

            // Exit window
            if (curZ < zoneMin && inputWindowOpen)
            {
                inputWindowOpen = false;
                if (carIsFast[lastCarIndex] && lastCarIndex != 5)
                {
                    anyFastCarPassedZone = true;
                    if (!hasPressedThisTurn)
                    {
                        // Fast non-police car missed → fail
                        gameEnded = true;
                        GameManager.Instance.NotifyFail();
                    }
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
            StartCoroutine(TriggerFlash());
            if (inputWindowOpen && !hasPressedThisTurn)
            {
                hasPressedThisTurn = true;
                inputWindowOpen = false;

                if (lastCarIndex == 5)
                {
                    // Police in zone → fail
                    gameEnded = true;
                    GameManager.Instance.NotifyFail();
                }
                else if (!carIsFast[lastCarIndex])
                {
                    // Normal car in zone → fail
                    gameEnded = true;
                    GameManager.Instance.NotifyFail();
                }
                else
                {
                    // Fast non-police → success
                    flashCount++;
                    if (flashCount >= requiredFlashes)
                    {
                        gameEnded = true;
                        GameManager.Instance.NotifyWin();
                    }
                }
            }
            // Press outside window → no consequence
        }
    }

    private IEnumerator TriggerFlash()
    {
        float elapsed = 0f;
        while (elapsed < 0.05f)
        {
            elapsed += Time.deltaTime;
            flashLight.intensity = Mathf.Lerp(0f, 3f, elapsed / 0.05f);
            yield return null;
        }
        flashLight.intensity = 3f;
        elapsed = 0f;
        while (elapsed < 0.5f)
        {
            elapsed += Time.deltaTime;
            flashLight.intensity = Mathf.Lerp(3f, 0f, elapsed / 0.5f);
            yield return null;
        }
        flashLight.intensity = 0f;
    }

    private IEnumerator NextCarWithDelay()
    {
        yield return new WaitForSeconds(Random.Range(0f, 2f));
        if (!gameEnded)
        {
            LaunchNewCar();
        }
    }

    private void HandleTimerEnded()
    {
        if (!gameEnded)
        {
            gameEnded = true;
            if (anyFastCarPassedZone)
                GameManager.Instance.NotifyFail();
            else
                GameManager.Instance.NotifyWin();
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

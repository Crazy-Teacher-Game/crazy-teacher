using System.Collections;
using UnityEngine;

public class FlashTheCar : MonoBehaviour
{
    [SerializeField] private Light flashLight;
    [SerializeField] private Camera gameCamera;
    [SerializeField] private AudioClip ambientSound;
    [SerializeField] private AudioClip carSound;
    [SerializeField] private AudioClip flashSound;
    [SerializeField] private float carSoundDelay = 0f; // Délai avant de jouer le son de la voiture (normale)
    [SerializeField] private float carSoundDelayFast = 0f; // Délai avant de jouer le son de la voiture (rapide)
    [SerializeField] private float carVisualDelay = 0f; // Délai entre le son et l'apparition de la voiture normale
    [SerializeField] private float carVisualDelayFast = 0f; // Délai entre le son et l'apparition de la voiture rapide
    [SerializeField] private float shakeDelay = 0f; // Délai avant la secousse (voiture normale)
    [SerializeField] private float shakeDelayFast = 0f; // Délai avant la secousse (voiture rapide)
    [SerializeField] private float shakeIntensity = 0.2f; // Amplitude de la secousse de caméra
    [SerializeField] private float shakeDuration = 0.3f; // Durée de la secousse
    private AudioSource audioSource;
    private AudioSource ambientAudioSource;
    private Vector3 cameraOriginalPosition;
    [SerializeField] private GameObject car1;
    [SerializeField] private GameObject car2;
    [SerializeField] private GameObject car3;
    [SerializeField] private GameObject car4_race;
    [SerializeField] private GameObject car5_taxi;
    // car6_police retiré
    private float fixedWorldX = 206.19f;
    private float fixedWorldXFast = 211.4f;
    private float normalSpeed = 0.5f;
    private float fastSpeed = 1f;
    private float startZ = 310f;

    // State variables
    private float destroyZ = -130f;
    private float zoneMin = 220f;
    private float zoneMax = 260f;
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

    private FlashPhotoStrip photoStrip;

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
        cars = new GameObject[] { car1, car2, car3, car4_race, car5_taxi };

        originalY = new float[5];
        carSpeeds = new float[5];
        carWorldX = new float[5];
        carIsFast = new bool[5];
        carTransforms = new Transform[5];

        for (int i = 0; i < 5; i++)
        {
            carTransforms[i] = cars[i].transform;
            originalY[i] = carTransforms[i].position.y;
            cars[i].SetActive(false);
        }

        if (gameCamera == null) gameCamera = Camera.main;
        cameraOriginalPosition = gameCamera.transform.position;
        photoStrip = gameObject.AddComponent<FlashPhotoStrip>();

        // Son d'ambiance (bouclé)
        if (ambientSound != null)
        {
            ambientAudioSource = gameObject.AddComponent<AudioSource>();
            ambientAudioSource.clip = ambientSound;
            ambientAudioSource.loop = true;
            ambientAudioSource.Play();
        }

        // Source pour les sons ponctuels (voiture, flash)
        audioSource = gameObject.AddComponent<AudioSource>();

        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnTimerEnded += HandleTimerEnded;
            GameManager.Instance.StartTimer(timerDuration, timerMinDuration);
        }
        LaunchNewCar();
    }

    private void LaunchNewCar()
    {
        StartCoroutine(LaunchCarRoutine());
    }

    private IEnumerator LaunchCarRoutine()
    {
        int carIndex;
        do
        {
            carIndex = Random.Range(0, 5);
        } while (carIndex == lastCarIndex || cars[carIndex].activeSelf);

        lastCarIndex = carIndex;
        spawnChance = 0.5f + 0.4f * GameManager.Instance.DifficultyFactor;
        carIsFast[carIndex] = !isFirstCar && (Random.value < spawnChance);
        isFirstCar = false;
        carSpeeds[carIndex] = carIsFast[carIndex] ? fastSpeed * (1f + GameManager.Instance.DifficultyFactor) : normalSpeed;
        carWorldX[carIndex] = carIsFast[carIndex] ? fixedWorldXFast : fixedWorldX;

        bool isFast = carIsFast[carIndex];
        float soundDelay = isFast ? carSoundDelayFast : carSoundDelay;
        float visualDelay = isFast ? carVisualDelayFast : carVisualDelay;

        // Jouer le son en avance sur le visuel
        if (carSound != null)
        {
            if (soundDelay > 0f) yield return new WaitForSeconds(soundDelay);
            if (!gameEnded) audioSource.PlayOneShot(carSound);
        }

        // Attendre avant d'activer la voiture visuellement
        if (visualDelay > 0f) yield return new WaitForSeconds(visualDelay);
        if (gameEnded) yield break;

        carTransforms[carIndex].position = new Vector3(carWorldX[carIndex], originalY[carIndex], startZ);
        cars[carIndex].SetActive(true);

        // Déclencher la secousse de caméra pour les voitures rapides
        if (isFast)
        {
            float shakeDelayToUse = shakeDelayFast;
            if (shakeDelayToUse > 0f) yield return new WaitForSeconds(shakeDelayToUse);
            if (!gameEnded) StartCoroutine(CameraShake());
        }

        inputWindowOpen = false;
        hasPressedThisTurn = false;
        currentCarHasPassedZone = false;
        hasTriggeredNextCar = false;
    }

    void FixedUpdate()
    {
        if (gameEnded) return;

        // Step 1 — Move all active cars and clean up
        for (int i = 0; i < 5; i++)
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
                if (carIsFast[lastCarIndex])
                {
                    anyFastCarPassedZone = true;
                    if (!hasPressedThisTurn)
                    {
                        // Voiture rapide ratée → fail
                        gameEnded = true;
                        photoStrip.Cleanup();
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
        for (int i = 0; i < 5; i++)
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

                // car6_police retiré, plus de vérification sur l'index 5
                if (!carIsFast[lastCarIndex])
                {
                    // Voiture normale dans la zone → fail
                    gameEnded = true;
                    photoStrip.Cleanup();
                    GameManager.Instance.NotifyFail();
                }
                else
                {
                    // Voiture rapide → success
                    flashCount++;
                    if (flashCount >= requiredFlashes)
                    {
                        gameEnded = true;
                        photoStrip.Cleanup();
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

        // Son du flash
        if (flashSound != null) audioSource.PlayOneShot(flashSound);

        // Capture la scène au pic du flash (lumière à pleine intensité)
        photoStrip.AddPhoto(CaptureFrame());

        elapsed = 0f;
        while (elapsed < 0.5f)
        {
            elapsed += Time.deltaTime;
            flashLight.intensity = Mathf.Lerp(3f, 0f, elapsed / 0.5f);
            yield return null;
        }
        flashLight.intensity = 0f;
    }

    private Texture2D CaptureFrame()
    {
        RenderTexture rt = RenderTexture.GetTemporary(Screen.width, Screen.height, 24);
        gameCamera.targetTexture = rt;
        gameCamera.Render();
        gameCamera.targetTexture = null;

        RenderTexture.active = rt;
        Texture2D snap = new Texture2D(rt.width, rt.height, TextureFormat.RGB24, false);
        snap.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
        snap.Apply();
        RenderTexture.active = null;
        RenderTexture.ReleaseTemporary(rt);
        return snap;
    }

    private IEnumerator CameraShake()
    {
        float elapsed = 0f;
        while (elapsed < shakeDuration)
        {
            float x = Random.Range(-1f, 1f) * shakeIntensity;
            float y = Random.Range(-1f, 1f) * shakeIntensity;
            gameCamera.transform.position = cameraOriginalPosition + new Vector3(x, y, 0);

            elapsed += Time.deltaTime;
            yield return null;
        }
        gameCamera.transform.position = cameraOriginalPosition;
    }

    private IEnumerator NextCarWithDelay()
    {
        yield return new WaitForSeconds(Random.Range(0f, 2f));
        if (!gameEnded)
        {
            StartCoroutine(LaunchCarRoutine());
        }
    }

    private void HandleTimerEnded()
    {
        if (!gameEnded)
        {
            gameEnded = true;
            photoStrip.Cleanup();
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
        if (photoStrip != null) photoStrip.Cleanup();
    }
}

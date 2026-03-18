using System;
using System.Collections;
using UnityEngine;
using TMPro;
using Random = UnityEngine.Random;

public class TriPommePoire : MonoBehaviour
{
    public GameObject fruitSpawner;

    [SerializeField] public GameObject[] redPrefabs;
    [SerializeField] public GameObject[] bluePrefabs;

    [SerializeField] public string currentFruitName;

    public int fruitsATrouverMin = 4;
    public int fruitsATrouverMax = 12;
    public int fruitsATrouver;
    public TMP_Text fruitsATrouverText;

    private GameObject lastSpawnedFruit;

    public float timerDuration = 5f;
    public float timerMinDuration = 3f;

    public AudioSource audioSource;
    public AudioClip correctSound;
    public AudioClip wrongSound;

    public GameObject wrongAnimationTarget;
    public string wrongAnimationTrigger = "Move";

    private bool hasReturnedToCenter = true;
    private bool gameEnded = false;

    void Start()
    {
        float difficulty = GameManager.Instance != null ? GameManager.Instance.DifficultyFactor : 0f;
        fruitsATrouver = Mathf.Clamp(Mathf.RoundToInt(Mathf.Lerp(fruitsATrouverMin, fruitsATrouverMax, difficulty)), fruitsATrouverMin, fruitsATrouverMax);
        SpawnRandomFruit(Vector3.zero);
        GameManager.Instance.OnTimerEnded += HandleTimeout;
        GameManager.Instance.StartTimer(timerDuration, timerMinDuration);
    }

    void Update()
    {
        if (gameEnded) return;

        float horizontalInput = Input.GetAxisRaw("P1_Horizontal");

        // Si le joystick est revenu au centre, on autorise un nouvel input
        if (Mathf.Abs(horizontalInput) < 0.2f)
        {
            hasReturnedToCenter = true;
        }
        // Si le joystick est à gauche et qu'on attend un fruit rouge
        else if (horizontalInput < -0.5f && hasReturnedToCenter && !gameEnded)
        {
            if (currentFruitName == "red")
            {
                fruitsATrouver--;
                audioSource.PlayOneShot(correctSound);
                SpawnRandomFruit(Vector3.left);
            }
            else
            {
                HandleWrongAnswer();
            }
            hasReturnedToCenter = false;
        }
        // Si le joystick est à droite et qu'on attend un fruit bleu
        else if (horizontalInput > 0.5f && hasReturnedToCenter && !gameEnded)
        {
            if (currentFruitName == "blue")
            {
                fruitsATrouver--;
                audioSource.PlayOneShot(correctSound);
                SpawnRandomFruit(Vector3.right);
            }
            else
            {
                HandleWrongAnswer();
            }
            hasReturnedToCenter = false;
        }

        fruitsATrouverText.text = "Formes à trier: " + fruitsATrouver;

        if (fruitsATrouver <= 0)
        {
            gameEnded = true;
            StartCoroutine(WinAfterDelay());
        }
    }

    void HandleWrongAnswer()
    {
        audioSource.PlayOneShot(wrongSound);
        GameManager.Instance.RemoveTime(1f);
        if (wrongAnimationTarget != null)
            wrongAnimationTarget.GetComponent<Animator>().SetTrigger(wrongAnimationTrigger);
    }

    IEnumerator WinAfterDelay()
    {
        yield return new WaitForSeconds(0.2f);
        GameManager.Instance.NotifyWin();
    }

    void HandleTimeout()
    {
        if (gameEnded) return;
        gameEnded = true;
        GameManager.Instance.NotifyFail();
    }

    void SpawnRandomFruit(Vector3 moveDirection)
    {
        if (lastSpawnedFruit != null)
        {
            StartCoroutine(MoveAndDestroyFruit(lastSpawnedFruit, moveDirection));
        }

        if (!gameEnded && fruitsATrouver > 0)
        {
            bool isRed = Random.Range(0, 2) == 0;
            GameObject[] pool = isRed ? redPrefabs : bluePrefabs;
            GameObject prefabToSpawn = pool[Random.Range(0, pool.Length)];
            lastSpawnedFruit = Instantiate(prefabToSpawn, fruitSpawner.transform);
            currentFruitName = isRed ? "red" : "blue";
        }


    }

    IEnumerator MoveAndDestroyFruit(GameObject fruit, Vector3 direction)
    {
        float duration = 0.1f;
        float elapsed = 0f;
        Vector3 startPos = fruit.transform.position;
        Vector3 endPos = startPos + direction * 300f;

        while (elapsed < duration)
        {
            fruit.transform.position = Vector3.Lerp(startPos, endPos, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        fruit.transform.position = endPos;
        Destroy(fruit);
    }

    void OnDisable()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnTimerEnded -= HandleTimeout;
    }
}
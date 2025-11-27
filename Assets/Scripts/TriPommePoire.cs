using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class TriPommePoire : MonoBehaviour
{   
    public GameObject fruitSpawner;

    [SerializeField] public GameObject fruit1Prefab;
    [SerializeField] public GameObject fruit2Prefab;

    [SerializeField] public string currentFruitName;

    public int fruitsATrouver = 6;
    public TMP_Text fruitsATrouverText;

    private GameObject lastSpawnedFruit;

    public float timerDuration = 5f;

    private bool hasFailed = false;
    private bool hasReturnedToCenter = true;

    void Start()
    {
        SpawnRandomFruit(Vector3.zero);
        GameManager.Instance.StartTimer(timerDuration);
    }

    void Update()
    {
        float horizontalInput = Input.GetAxisRaw("P1_Horizontal");
        // Gestion du timer et de l'échec
        if (GameManager.Instance.RemainingTime <= 0f && fruitsATrouver > 0)
        {
            Debug.Log("Temps écoulé, fruit manqué");
            if (hasFailed == false)
            {
                GameManager.Instance.LoseLife();
                hasFailed = true;
            }
        }

        fruitsATrouverText.text = "Fruits à trier: " + fruitsATrouver;
        if (fruitsATrouver > 0 && GameManager.Instance.RemainingTime > 0f)
        {
            // Si le joystick est revenu au centre, on autorise un nouvel input
            if (Mathf.Abs(horizontalInput) < 0.2f)
            {
                hasReturnedToCenter = true;
            }
            // Si le joystick est à gauche et qu'on attend un fruit rouge
            else if (horizontalInput < -0.5f && hasReturnedToCenter)
            {
                if (currentFruitName == "red")
                {
                    fruitsATrouver--;
                    Debug.Log("Fruit correct (gauche)");
                    SpawnRandomFruit(Vector3.left);
                }
                hasReturnedToCenter = false;
            }
            // Si le joystick est à droite et qu'on attend un fruit bleu
            else if (horizontalInput > 0.5f && hasReturnedToCenter)
            {
                if (currentFruitName == "blue")
                {
                    fruitsATrouver--;
                    Debug.Log("Fruit correct (droite)");
                    SpawnRandomFruit(Vector3.right);
                }
                hasReturnedToCenter = false;
            }
        }
    }

    void SpawnRandomFruit(Vector3 moveDirection)
    {
        if (lastSpawnedFruit != null)
        {
            StartCoroutine(MoveAndDestroyFruit(lastSpawnedFruit, moveDirection));
        }

        GameObject prefabToSpawn = Random.Range(0, 2) == 0 ? fruit1Prefab : fruit2Prefab;
        lastSpawnedFruit = Instantiate(prefabToSpawn, fruitSpawner.transform);

        if (prefabToSpawn == fruit1Prefab)
            currentFruitName = "red";
        else
            currentFruitName = "blue";
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
}
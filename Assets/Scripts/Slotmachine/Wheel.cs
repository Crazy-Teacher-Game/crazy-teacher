using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wheel : MonoBehaviour
{
    [SerializeField] private GameObject[] slotPrefabs;
    [SerializeField] private float stopSmoothDuration = 0.15f;

    private GameObject wheel;
    private GameObject[] instantiatedObjects;
    private float spaceBetweenObjects = 2f;
    private float spinSpeed = 8f;

    //spin index, random float between 0 and spaceBetweenObjects
    private float spinIndex;
    private int numObjects;
    private int offsetMargin = 3;

    private bool isSpinning = true;
    private bool isStopping = false;
    private float stopElapsed = 0f;
    private float stopStartSpinIndex = 0f;
    private float stopTargetSpinIndex = 0f;
    private bool lastStopWasSeven = false;

    public bool IsSpinning => isSpinning;
    public bool IsStopping => isStopping;
    public bool LastStopWasSeven => lastStopWasSeven;

    public bool Stop()
    {
        if (instantiatedObjects == null || instantiatedObjects.Length == 0)
            return false;

        numObjects = instantiatedObjects.Length;

        if (!isSpinning || isStopping)
            return false;

        float spacing = spaceBetweenObjects;
        float totalHeight = numObjects * spacing;
        float current = Mathf.Repeat(spinIndex, totalHeight);

        int selectedIndex = 0;
        float bestSignedDelta = float.MaxValue;
        for (int i = 0; i < numObjects; i++)
        {
            float baseTarget = Mathf.Repeat((i - offsetMargin) * spacing, totalHeight);
            float forwardDelta = baseTarget - current;
            if (forwardDelta < 0f) forwardDelta += totalHeight;

            // Pick the nearest notch on the ring: small backward motion is allowed.
            float signedDelta = forwardDelta;
            if (signedDelta > totalHeight * 0.5f)
                signedDelta -= totalHeight;

            if (Mathf.Abs(signedDelta) < Mathf.Abs(bestSignedDelta))
            {
                bestSignedDelta = signedDelta;
                selectedIndex = i;
            }
        }

        lastStopWasSeven = instantiatedObjects[selectedIndex].name == "Seven";
        stopStartSpinIndex = spinIndex;
        stopTargetSpinIndex = spinIndex + bestSignedDelta;
        stopElapsed = 0f;
        isStopping = true;
        return true;
    }

    public void StartSpin()
    {
        isStopping = false;
        isSpinning = true;
    }

    private void UpdateVisuals(float currentSpinIndex)
    {
        float spacing = spaceBetweenObjects;
        float totalHeight = numObjects * spacing;
        float minY = -offsetMargin * spacing;

        float loop = 0f;
        for (int i = 0; i < numObjects; i++)
        {
            float y = minY + Mathf.Repeat(loop - currentSpinIndex, totalHeight);
            instantiatedObjects[i].transform.localPosition = new Vector3(0f, y, 0f);
            loop += spacing;
        }
    }

    void Awake()
    {
        wheel = gameObject;
        spinIndex = Random.Range(0f, spaceBetweenObjects);
    }

    // Start is called before the first frame update
    void Start()
    {
        instantiatedObjects = new GameObject[slotPrefabs.Length];

        //shuffle the slotPrefabs array
        for (int i = 0; i < slotPrefabs.Length - 1; i++)
        {
            int j = Random.Range(i, slotPrefabs.Length);
            (slotPrefabs[i], slotPrefabs[j]) = (slotPrefabs[j], slotPrefabs[i]);
        }


        var prefabIndex = 0;
        foreach (GameObject prefab in slotPrefabs)
        {
            GameObject instance = Instantiate(prefab, wheel.transform);
            // Position the instance based on the index
            instance.transform.localPosition = new Vector3(0, -100, 0);
            //change name of the instance to the name of the prefab
            instance.name = prefab.name;

            instantiatedObjects[prefabIndex] = instance;
            prefabIndex++;
        }

    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (instantiatedObjects == null || instantiatedObjects.Length == 0)
            return;

        numObjects = instantiatedObjects.Length;

        float spacing = spaceBetweenObjects;
        float totalHeight = numObjects * spacing;

        if (isStopping)
        {
            stopElapsed += Time.deltaTime;
            float duration = Mathf.Max(0.01f, stopSmoothDuration);
            float t = Mathf.Clamp01(stopElapsed / duration);
            float eased = 1f - Mathf.Pow(1f - t, 3f); // Ease out cubic

            float current = Mathf.Lerp(stopStartSpinIndex, stopTargetSpinIndex, eased);
            spinIndex = current;

            if (t >= 1f)
            {
                spinIndex = Mathf.Repeat(stopTargetSpinIndex, totalHeight);
                isStopping = false;
                isSpinning = false;
            }
        }
        else if (isSpinning)
        {
            spinIndex = Mathf.Repeat(spinIndex + spinSpeed * Time.deltaTime, totalHeight);
        }

        UpdateVisuals(spinIndex);
    }
}

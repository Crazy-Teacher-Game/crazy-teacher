using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wheel : MonoBehaviour
{
    [SerializeField] private GameObject[] slotPrefabs;

    private GameObject wheel;
    private GameObject[] instantiatedObjects;
    private float spaceBetweenObjects = 2f;
    private float spinSpeed = 8f;

    //spin index, random float between 0 and spaceBetweenObjects
    private float spinIndex;
    private int numObjects;
    private int offsetMargin = 3;

    private bool isSpinning = true;

    public bool Stop()
    {
        isSpinning = false;
        //find the object nearest with the Y position nearest to 0 in instantiatedObjects
        //devide spinIndex by two then round to the ciel the value
        if (numObjects == 0)
        {
            Debug.Log("Error: numObjects is 0 in Wheel.Stop()");
            //Debug element name
            Debug.Log("Wheel name: " + wheel.name);
        }
        if (spaceBetweenObjects == 0f)
        {
            Debug.Log("Error: spaceBetweenObjects is 0 in Wheel.Stop()");
        }
        int nearestIndex = Mathf.RoundToInt(spinIndex / spaceBetweenObjects) % numObjects - offsetMargin - 1;
        if (nearestIndex < 0) nearestIndex += numObjects;
        string nearestObjectName = instantiatedObjects[nearestIndex].name;

        if (nearestObjectName == "Seven")
        {
            return true;
        }
        return false;
    }

    public void StartSpin()
    {
        isSpinning = true;
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

        Debug.Log("Initializing wheel with " + slotPrefabs.Length + " slot prefabs.");
        Debug.Log(instantiatedObjects);

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
        if (!isSpinning) return;
        numObjects = instantiatedObjects.Length;

        float spacing = spaceBetweenObjects;
        float totalHeight = numObjects * spacing;
        float minY = -offsetMargin * spacing;     // your chosen start offset

        // advance the scroll (positive = move down visually; flip sign if you want the other way)
        spinIndex = Mathf.Repeat(spinIndex + spinSpeed * Time.deltaTime, totalHeight);

        float loop = 0f;
        for (int i = 0; i < numObjects; i++)
        {
            // Wrap each item within [minY, minY + totalHeight)
            float y = minY + Mathf.Repeat(loop - spinIndex, totalHeight);
            instantiatedObjects[i].transform.localPosition = new Vector3(0f, y, 0f);
            loop += spacing;
        }

    }
}

using System.Collections;
using System.Collections.Generic;
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

    // Start is called before the first frame update
    void Start()
    {
        car1.transform.position = new Vector3(fixedWorldX, car1.transform.position.y, startZ);
        //Debug.Log($"worldX={car1.transform.position.x:F2} localX={car1.transform.localPosition.x:F2}", car1);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        //move the car on y with x at 213.22

        car1.transform.position = new Vector3(fixedWorldX, car1.transform.position.y, car1.transform.position.z - normalSpeed);
    }
}

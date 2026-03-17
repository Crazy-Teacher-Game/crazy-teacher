using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dropper : MonoBehaviour
{
    public GameObject ballPrefab;
    private int dropCount = 0;
    private int ballCount = 0;
    [SerializeField] float x = 0f;
    [SerializeField] float y = -0.5f;
    [SerializeField] float z = 0f;
    private bool canDrop = true;

    void Update()
    {
        float vertical = Input.GetAxis("P1_Vertical");
        if (vertical < 0 && canDrop)
        {
            DropBall();
            dropCount++;
            ballCount++;
            canDrop = false;
        }
        if (vertical == 0 )
        {
            canDrop = true;
        }
    }

    void DropBall()
    {
        Vector3 spawnPos = transform.position + new Vector3(x, y, z);

        Instantiate(ballPrefab, spawnPos, Quaternion.identity);
    }
}

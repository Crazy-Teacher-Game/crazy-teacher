using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Platform : MonoBehaviour
{
    [Header("Platform")]
    public GameManager gameManager;

    // Start is called before the first frame update
    void Start()
    {

    }
    
    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("ball"))
        {
            gameManager.LoseLife();
            Debug.Log("loose");
        }
    }
}

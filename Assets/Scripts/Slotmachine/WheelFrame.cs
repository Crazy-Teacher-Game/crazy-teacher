using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WheelFrame : MonoBehaviour
{
    private GameObject wheelFrame;

    void Awake()
    {
        wheelFrame = gameObject;
    }

    // Start is called before the first frame update
    void Start()
    {
        //set color to black with alpha 0.5
        Color color = new Color(0f, 0f, 0f, 0.5f);
        Renderer renderer = wheelFrame.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material.color = color;
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}

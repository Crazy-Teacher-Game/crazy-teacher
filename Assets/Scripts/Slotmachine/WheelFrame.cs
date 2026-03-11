using UnityEngine;

public class WheelFrame : MonoBehaviour
{
    private Renderer _renderer;

    void Awake()
    {
        _renderer = GetComponent<Renderer>();
    }

    void Start()
    {
        // Default color: yellow
        SetColor(Color.yellow);
    }

    public void SetColor(Color color)
    {
        if (_renderer != null)
        {
            _renderer.material.color = color;
        }
    }
}

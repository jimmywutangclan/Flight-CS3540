using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorGlow : MonoBehaviour
{
    public Color startColor;
    public Color endColor;
    public float speed = 1.0f;

    Renderer renderer;

    void Start()
    {
        renderer = GetComponent<Renderer>();
    }

    void Update()
    {
        float val = (Mathf.Sin(Time.time * speed) + 1.0f) / 2.0f;
        renderer.material.color = Color.Lerp(startColor, endColor, val);
    }
}

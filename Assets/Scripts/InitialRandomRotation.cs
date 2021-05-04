using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InitialRandomRotation : MonoBehaviour
{
    void Start()
    {
        transform.Rotate(new Vector3(Random.Range(0.0f, 360.0f), Random.Range(0.0f, 360.0f), Random.Range(0.0f, 360.0f)));
    }
}

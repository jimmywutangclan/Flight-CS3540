using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RadiatePlayer : MonoBehaviour
{
    [SerializeField]
    private float radiationMultiplier = 1.5f;
    [SerializeField]
    private float minDamage = 0.5f;
    
    public float RadiationMultiplier { get; set; }
    public float MinDamage { get; set; }
    public bool Radiate { get; set; }
    // Start is called before the first frame update
    void Start()
    {
        Radiate = false;
        RadiationMultiplier = radiationMultiplier;
        MinDamage = minDamage;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            Radiate = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
            Radiate = false;
    }
}

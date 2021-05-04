using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KillerLava : MonoBehaviour
{
    public int hurtAmount = -100;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            other.gameObject.GetComponent<PlayerStatistics>().ChangePlayerHealth(-100);
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerBossBattle : MonoBehaviour
{
    public GameObject fightManager;
    
    // Start is called before the first frame update
    void Start()
    {
        fightManager = GameObject.FindGameObjectWithTag("FightManager");    
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") && other.transform.position.x > 50)
        {
            fightManager.GetComponent<FightManager>().battleOccurring = true;
        }
        else if (other.CompareTag("Player") && other.transform.position.x < 50)
        {
            fightManager.GetComponent<FightManager>().battleOccurring = false;
        }
    }
}

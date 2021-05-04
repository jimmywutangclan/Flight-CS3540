using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyDrop : MonoBehaviour
{
    // loot that enemy drops (meat)
    public GameObject loot;
    // amount of loot that can be extracted before it runs dry
    public int dropAmount;
    public GameObject player;


    // Start is called before the first frame update
    void Start()
    {
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player");
        }
    }

    // Update is called once per frame
    void Update()
    {
        // player clicks and extracts drops from a seperate interaction script
        // everytime a loot is extracted, decrease drop amount by one
        if (dropAmount <= 0)
        {
            RunOut();
        }
    }

    // when drops run out, destroy object
    void RunOut()
    {
        Destroy(gameObject, 5.0f);
    }
}

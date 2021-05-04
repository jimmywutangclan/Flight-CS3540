using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FightManager : MonoBehaviour
{
    public bool beat = false;
    public bool battleOccurring = false;
    public bool startedFirst = false;
    public GameObject[] zombies;
    public GameObject enemyPrefab;
    public int waveSize;

    // Start is called before the first frame update
    void Start()
    {
        zombies = new GameObject[9];
    }

    // Update is called once per frame
    void Update()
    {
        if (battleOccurring)
        {
            if (!startedFirst)
            {
                Debug.Log("Got in");
                for (int i = 0; i < 9; i++)
                {
                    int xSpawn = Random.Range(120, 200);
                    int ySpawn = -165;
                    int zSpawn = Random.Range(-50, 100);
                    Vector3 spawn = new Vector3(xSpawn, ySpawn, zSpawn);
                    zombies[i] = Instantiate(enemyPrefab, spawn, gameObject.transform.rotation) as GameObject;
                    zombies[i].transform.parent = gameObject.transform;
                }
                startedFirst = true;
            }
        }
        else if (!battleOccurring)
        {
            startedFirst = false;
            for (int i = 0; i < 9; i++)
            {
                Destroy(zombies[i], 0f);
                zombies[i] = null;
            }
        }
    }
}

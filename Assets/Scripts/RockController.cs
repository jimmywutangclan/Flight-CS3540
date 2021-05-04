using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RockController : MonoBehaviour, IItemSource
{
    public ItemType yieldedItem = ItemType.Rock;

    public float expectedRocksYielded = 3;
    public int hitsRequired = 9;

    public GameObject minedRockPrefab;

    public ItemType requireItemEquippedOfType = ItemType.Pickaxe;

    private int hitsTaken = 0;
    private Transform parent;
    // Start is called before the first frame update
    void Start()
    {
        parent = GameObject.FindGameObjectWithTag("RockParent").transform;
        //gameObject.transform.parent = parent;
    }

    public bool ItemsAvailable()
    {
        return (hitsRequired - hitsTaken) > 0;
    }


    public bool AllowYieldWithEquippedItem(ItemType? type)
    {
        return requireItemEquippedOfType == type;
    }


    public IItem YieldItem()
    {
        hitsTaken++;

        IItem returnItem = null;

        float probability = Random.Range(0f, 1f);
        if (probability <= (expectedRocksYielded / hitsRequired))
            returnItem = ItemRegistry.Instantiate(yieldedItem);

        if (hitsTaken >= hitsRequired)
        {
            gameObject.SetActive(false);

            // Instantiates the mined rock prefab with the matching y rotation.
            GameObject rock = Instantiate(minedRockPrefab, transform.position, Quaternion.Euler(0, transform.rotation.y, 0));
            rock.transform.parent = parent;
            Destroy(gameObject, 0.1f);
        }

        return returnItem;
    }

    
}

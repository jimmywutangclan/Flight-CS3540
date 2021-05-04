using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnItems : MonoBehaviour
{
    public ItemType[] items;
    ItemCollector itemCollector;

    void Start()
    {
        itemCollector = GetComponent<ItemCollector>();
        foreach (ItemType type in items)
        {
            itemCollector.AddToInventory(ItemRegistry.Instantiate(type));
        }
    }
}

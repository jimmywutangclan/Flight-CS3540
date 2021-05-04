using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Random = System.Random;

public class ItemSource : MonoBehaviour, IItemSource
{
    public ItemType type;
    public int totalItems;
    public bool destroyOnEmpty = true;
    public float destroyDelay = 0.5f;

    public bool requireItemEquipped = false;
    public ItemType requireItemEquippedOfType = ItemType.Axe;

    public bool ItemsAvailable()
    {
        return totalItems > 0;
    }

    public bool AllowYieldWithEquippedItem(ItemType? type)
    {
        return !requireItemEquipped || requireItemEquippedOfType == type;
    }

    public IItem YieldItem()
    {
        if (totalItems == 0)
        {
            return null;
        }

        totalItems -= 1;

        if (totalItems == 0 && destroyOnEmpty)
        {
            Destroy(gameObject, destroyDelay);
        }

        return ItemRegistry.Instantiate(type);
    }
}

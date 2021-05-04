using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SingleItemSource : MonoBehaviour, IItemSource
{
    public ItemType itemType;
    public bool pickedUp = false;
    public bool destroyOnYield = true;
    public float destroyDelay = 0.5f;

    public bool ItemsAvailable()
    {
        return !pickedUp;
    }

    public bool AllowYieldWithEquippedItem(ItemType? type)
    {
        return true;
    }

    public IItem YieldItem()
    {
        pickedUp = true;
        if (destroyOnYield)
        {
            gameObject.SetActive(false);
            Invoke("DoDestroy", destroyDelay);
        }
        return ItemRegistry.Instantiate(itemType);
    }

    private void DoDestroy()
    {
        Destroy(gameObject);

    }
}

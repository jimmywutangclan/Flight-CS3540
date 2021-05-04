using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public class NewItemEvent : UnityEvent<IItem> {}

[Serializable]
public class RemoveItemEvent : UnityEvent<IItem> {}

public class ItemCollector : MonoBehaviour
{
    public UIManager uIManager;
    public List<IItem> inventory = new List<IItem>();

    public AudioClip pickupSound;
    public AudioClip wrongItemEquippedSound;

    public Transform raycastSource;
    private RaycastController raycast;

    public NewItemEvent NewItem;
    public RemoveItemEvent RemoveItem;

    private ItemEquipper itemEquipper;

    private float reach;

    void Start()
    {
        itemEquipper = GetComponent<ItemEquipper>();

        if (raycastSource != null)
        {
            raycast = raycastSource.parent.GetComponent<RaycastController>();
            if (raycast != null)
            {
                int inventoryLayerMask = 1 << LayerMask.NameToLayer("Interactive");
                raycast.AddLayerMask(inventoryLayerMask);
            }
        }

        reach = GetComponent<PlayerController>().GetReach();
    }

    public void AddToInventory(IItem item)
    {
        inventory.Add(item);
        NewItem?.Invoke(item);
    }
    public void RemoveFromInventory(IItem item)
    {
        inventory.Remove(item);
        RemoveItem?.Invoke(item);
    }

    public bool Craftable(CraftingRecipe recipe)
    {
        return recipe.MaterialsPresent(this.inventory);
    }

    public void Craft(CraftingRecipe recipe)
    {
        if (recipe.MaterialsPresent(inventory))
        {
            inventory = recipe.CraftFrom(inventory);
            AddToInventory(ItemRegistry.Instantiate(recipe.ProducesType));
        }
    }

    void Update()
    {
        if (Input.GetButtonDown("Fire1") && !uIManager.UIShown)
        {
            RaycastHit hitInfo = raycast.GetHitObject();

            if (hitInfo.collider != null && hitInfo.collider.CompareTag("Inventory") &&
                Vector3.Distance(hitInfo.point, transform.position) < reach)
            {
                AddItemFromSource(hitInfo.collider.gameObject.GetComponent<IItemSource>());
            }
        }
    }

    public bool AddItemFromSource(IItemSource itemSource)
    {
        if (itemSource == null)
        {
            return false;
        }

        if (!itemSource.ItemsAvailable() || !itemSource.AllowYieldWithEquippedItem(itemEquipper.EquippedItem?.Type))
        {
            if (wrongItemEquippedSound)
            {
                AudioSource.PlayClipAtPoint(wrongItemEquippedSound, transform.position);
            }
            return false;
        }

        IItem item = itemSource.YieldItem();
        if (item == null)
        {
            return false;
        }

        if (pickupSound)
        {
            AudioSource.PlayClipAtPoint(pickupSound, transform.position);
        }

        AddToInventory(item);
        return true;
    }

    public IItem GetFirstItemOfType(ItemType type)
    {
        return inventory.Find( item => item.Type == type);
    }
}

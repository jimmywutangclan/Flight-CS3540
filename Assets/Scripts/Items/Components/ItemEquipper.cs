using System;
using UnityEngine;
using UnityEngine.Events;

using Object = UnityEngine.Object;

[Serializable]
public class ItemEquippedEvent : UnityEvent<IItem> { }

public class ItemEquipper : MonoBehaviour
{
    public UIManager uIManager;
    public Transform parent;

    public IItem EquippedItem;

    Object equippedObject;

    public ItemEquippedEvent ItemEquipped;

    public AudioClip consumeSFX;

    public void Equip(IItem item)
    {
        if(item.Properties.Equipable)
        {
            RemoveEquipment();

            EquippedItem = item;
            equippedObject = Instantiate(item.Properties.EquipablePrefab, parent);
            ItemEquipped?.Invoke(item);
        }
    }
    public void RemoveEquipment()
    {
        if (equippedObject)
        {
            Destroy(equippedObject);
            EquippedItem = null;
            equippedObject = null;

            ItemEquipped?.Invoke(null);
        }
    }

    private void Update()
    {
        if (EquippedItem != null && EquippedItem.Properties.Consumable && !uIManager.UIShown)
        {
            if (Input.GetButtonDown("Fire1"))
            {
                ConsumableItem item = EquippedItem as ConsumableItem;
                uIManager.SetProgressBarCountUp(item.TimeToConsume, OnConsume);
            }
            if (Input.GetButtonUp("Fire1"))
            {
                uIManager.SetProgressBarHidden();
            }
        }
    }

    private void OnConsume()
    {
        var item = EquippedItem as ConsumableItem;
        if (item.Consumptions > 0)
        {
            if (consumeSFX) {
                AudioSource.PlayClipAtPoint(consumeSFX, transform.position);
            }

            item.Consume();
            PlayerStatistics playerStatistics = GetComponent<PlayerStatistics>();
            playerStatistics.ChangePlayerHealth(item.Properties.HealthOnConsume);
            playerStatistics.ChangePlayerHunger(item.Properties.HungerOnConsume);
            playerStatistics.ChangePlayerRadiation(item.Properties.RadiationOnConsume);

            if (item.Consumptions == 0)
            {
                RemoveEquipment();
                ItemCollector itemCollector = GetComponent<ItemCollector>();
                itemCollector.RemoveFromInventory(item);
            }
        }
    }
}

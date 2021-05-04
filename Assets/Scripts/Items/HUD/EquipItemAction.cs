using UnityEngine;

public class EquipItemAction : MonoBehaviour
{
    private ItemEquipper itemEquipper;
    private IItem item;

    public void SetItemEquipper(ItemEquipper equipper)
    {
        itemEquipper = equipper;
    }
    public void SetItem(IItem item)
    {
        this.item = item;
    }

    public void EquipItem()
    {
        if (itemEquipper)
        {
            if (itemEquipper.EquippedItem == item)
            {
                itemEquipper.RemoveEquipment();
            }
            else
            {
                itemEquipper.Equip(item);
            }
        }
    }
}

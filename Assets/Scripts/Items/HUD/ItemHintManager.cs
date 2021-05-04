using System.Collections.Generic;
using UnityEngine;

public class ItemHintManager : MonoBehaviour
{
    private static Dictionary<ItemType, string> ItemHelpByType = new Dictionary<ItemType, string>
    {
        { ItemType.Axe, "Use the axe on a tree with left click." },
        { ItemType.Berries, "Eat the berries with left click." },
    };

    public UIManager uIManager;

    public void OnItemEquipped(IItem item)
    {
        if (item == null)
        {
            uIManager.SetHintHidden();
            return;
        }

        string helpText;
        if (!ItemHelpByType.TryGetValue(item.Type, out helpText))
        {
            helpText = "";
        }

        if (!string.IsNullOrEmpty(helpText))
        {
            uIManager.SetIndefiniteHint(helpText);
        }
    }
}

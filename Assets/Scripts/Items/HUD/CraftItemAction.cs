using UnityEngine;

public class CraftItemAction : MonoBehaviour
{
    private ItemCollector itemCollector;
    private ItemType itemType;
    private CraftingRecipe craftingRecipe;

    public void SetItemCollector(ItemCollector collector)
    {
        itemCollector = collector;
    }
    public void SetItemType(ItemType itemType)
    {
        this.itemType = itemType;
        this.craftingRecipe = ItemRegistry.GetCraftingRecipeForType(itemType);
    }

    public void CraftItem()
    {
        if (itemCollector && craftingRecipe.MaterialsPresent(itemCollector.inventory))
        {
            itemCollector.Craft(craftingRecipe);
        }
    }
}

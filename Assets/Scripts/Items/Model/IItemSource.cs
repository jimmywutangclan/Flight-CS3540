public interface IItemSource
{
    bool ItemsAvailable();
    bool AllowYieldWithEquippedItem(ItemType? type);
    IItem YieldItem();
}

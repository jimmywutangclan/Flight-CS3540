using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

public enum ItemType
{
    Axe,
    Banana,
    Rock,
    Stick,
    Berries,
    Metal,
    Pickaxe,
    Meat,
    Iodine,
    GasMask,
    PotassiumIodine,
    Sword,
    Medkit
}

public class ItemProps
{
    const float BASE_DAMAGE = 5.0f;

    bool stackable;
    bool durable;
    bool consumable;
    float? damage;

    float hungerOnConsume;
    float healthOnConsume;
    float radiationOnConsume;

    string equipablePrefabPath;
    Object equipablePrefabCached;

    public bool Stackable { get => stackable; }
    public bool Durable { get => durable; }
    public bool Consumable { get => consumable; }

    public float? Damage { get => damage; }

    public float HungerOnConsume { get => hungerOnConsume; }
    public float HealthOnConsume { get => healthOnConsume; }
    public float RadiationOnConsume { get => radiationOnConsume; }

    public bool Equipable { get => !string.IsNullOrEmpty(equipablePrefabPath); }
    public string EquipablePrefabPath { get => equipablePrefabPath; }
    public Object EquipablePrefab {
        get
        {
            if (!Equipable)
            {
                return null;
            }

            if (equipablePrefabCached)
            {
                return equipablePrefabCached;
            }

            equipablePrefabCached = Resources.Load(equipablePrefabPath);
            return equipablePrefabCached;
        }
    }

    private ItemProps(bool isStackable, bool isDurable, bool isConsumable, string equipablePrefabResource, float? dmg = null, float hunger = 0.0f, float health = 0.0f, float radiation = 0.0f)
    {
        stackable = isStackable;
        durable = isDurable;
        consumable = isConsumable;
        equipablePrefabPath = equipablePrefabResource;
        damage = dmg;

        hungerOnConsume = hunger;
        healthOnConsume = health;
        radiationOnConsume = radiation;
    }

    public static ItemProps MakeNormalItem()
    {
        return new ItemProps(true, false, false, "");
    }

    public static ItemProps MakeDurableItem()
    {
        return new ItemProps(false, true, false, "");
    }
    public static ItemProps MakeEquipableDurableItem(string equipablePrefab, float? damage = null)
    {
        return new ItemProps(false, true, false, equipablePrefab, damage);
    }
    public static ItemProps MakeEquipableConsumableItem(string equipablePrefab, float hunger, float health, float radiation)
    {
        return new ItemProps(true, false, true, equipablePrefab, null, hunger, health, radiation);
    }
};

public interface ICraftingRecipe
{
    ItemType ProducesType { get; }

    bool MaterialsPresent(List<IItem> inventory);

    List<IItem> CraftFrom(List<IItem> inventory);
}

public class MaterialCost
{
    public ItemType Type { get; }
    public int Quantity { get; }

    public MaterialCost(ItemType type, int quantity)
    {
        this.Type = type;
        this.Quantity = quantity;
    }
}
public class CraftingRecipe : ICraftingRecipe
{
    ItemType resultItemType;
    List<MaterialCost> materials;

    public CraftingRecipe(ItemType resultType, List<MaterialCost> materials)
    {
        this.resultItemType = resultType;
        this.materials = materials;
    }

    public ItemType ProducesType { get => resultItemType; }

    public bool MaterialsPresent(List<IItem> inventory)
    {
        Dictionary<ItemType, int> numItemsOfTypeInInventory = inventory.GroupBy(
            item => item.Type
        ).ToDictionary(
            group => group.Key, group => group.Count()
        );

        return materials.All(material => {
            if (!numItemsOfTypeInInventory.ContainsKey(material.Type)) return false;
            return numItemsOfTypeInInventory[material.Type] >= material.Quantity;
        });
    }
    public List<IItem> CraftFrom(List<IItem> inputInventory)
    {
        List<IItem> inventory = inputInventory.ToList();
        foreach (MaterialCost material in materials)
        {
            removeMultiple(inventory, material.Quantity, item => item.Type == material.Type);
        }
        return inventory;
    }

    private void removeMultiple<T>(IList<T> list, int maxToRemove, Predicate<T> predicate)
    {
        IList<T> toRemove = list.Where(i => predicate(i)).Take(maxToRemove).ToList();
        foreach (T itemToRemove in toRemove)
        {
            list.Remove(itemToRemove);
        }
    }
}

public interface IItem
{
    ItemType Type { get; }
    ItemProps Properties { get; }
}

public class NormalItem : IItem
{
    ItemType itemType;
    ItemProps itemProps;

    public ItemType Type { get => itemType; }
    public ItemProps Properties { get => itemProps; }

    public NormalItem(ItemType type, ItemProps props)
    {
        itemType = type;
        itemProps = props;
    }
}

public class DurableItem : NormalItem
{
    // When durability resets to zero the item should break / become unusable
    float durability;

    public DurableItem(ItemType type, ItemProps props, float initialDurability = 1.0f) : base(type, props)
    {
        this.durability = initialDurability;
    }

    public void ApplyWear(float wear) {
        durability -= wear;
    }
}

public class ConsumableItem : NormalItem
{
    float timeToConsume;
    int consumptions;

    public float TimeToConsume { get => timeToConsume; }
    public int Consumptions { get => consumptions; }

    public ConsumableItem(ItemType type, ItemProps props, float timeToConsumeItem = 1.0f, int numConsumptions = 1) : base(type, props)
    {
        timeToConsume = timeToConsumeItem;
        consumptions = numConsumptions;
    }
    public void Consume()
    {
        if (consumptions > 0)
        {
            consumptions -= 1;
        }
    }
}

public static class ItemRegistry
{
    private static Dictionary<ItemType, ItemProps> ItemPropsByType = new Dictionary<ItemType, ItemProps>
    {
        { ItemType.Axe, ItemProps.MakeEquipableDurableItem("Equip/Axe", 7.0f) },
        { ItemType.Banana, ItemProps.MakeNormalItem() },
        { ItemType.Rock, ItemProps.MakeNormalItem() },
        { ItemType.Stick, ItemProps.MakeNormalItem() },
        { ItemType.Berries, ItemProps.MakeEquipableConsumableItem("Equip/Berries", 10.0f, 0.0f, 0.0f) },
        { ItemType.Metal, ItemProps.MakeNormalItem() },
        { ItemType.Pickaxe, ItemProps.MakeEquipableDurableItem("Equip/Pickaxe", 10.0f) },
        { ItemType.Meat, ItemProps.MakeEquipableConsumableItem("Equip/Meat", 30.0f, 0.0f, 0.0f) },
        { ItemType.Iodine, ItemProps.MakeNormalItem() },
        { ItemType.GasMask, ItemProps.MakeNormalItem() },
        { ItemType.PotassiumIodine, ItemProps.MakeEquipableConsumableItem("Equip/PotassiumIodine", 0.0f, 0.0f, -20.0f) },
        { ItemType.Sword, ItemProps.MakeEquipableDurableItem("Equip/Sword", 25.0f) },
        { ItemType.Medkit, ItemProps.MakeEquipableConsumableItem("Equip/Medkit", 0.0f, 50.0f, 0.0f) }
    };

    private static Dictionary<ItemType, CraftingRecipe> ItemRecipeByType = new Dictionary<ItemType, CraftingRecipe>
    {
        { ItemType.Axe, new CraftingRecipe(ItemType.Axe, new List<MaterialCost> { new MaterialCost(ItemType.Rock, 3), new MaterialCost(ItemType.Stick, 6) }) },
        { ItemType.Pickaxe, new CraftingRecipe(ItemType.Pickaxe, new List<MaterialCost> { new MaterialCost(ItemType.Rock, 6), new MaterialCost(ItemType.Stick, 6) }) },
        { ItemType.Sword, new CraftingRecipe(ItemType.Sword, new List<MaterialCost> { new MaterialCost(ItemType.Metal, 6), new MaterialCost(ItemType.Stick, 3) }) },
        { ItemType.PotassiumIodine, new CraftingRecipe(ItemType.PotassiumIodine, new List<MaterialCost> { new MaterialCost(ItemType.Iodine, 3) }) },
        { ItemType.Medkit, new CraftingRecipe(ItemType.Medkit, new List<MaterialCost> { new MaterialCost(ItemType.Iodine, 3), new MaterialCost(ItemType.Metal, 3) }) }
    };

    public static ItemProps GetItemPropsForType(ItemType type)
    {
        ItemProps props;
        if (ItemPropsByType.TryGetValue(type, out props))
        {
            return props;
        }

        Debug.LogWarning("Unable to find item " + type.ToString() + ". Using default item props.");
        return ItemProps.MakeNormalItem();
    }

    public static bool IsCraftable(ItemType type)
    {
        return ItemRecipeByType.ContainsKey(type);
    }
    public static CraftingRecipe GetCraftingRecipeForType(ItemType type)
    {
        CraftingRecipe recipe;
        if (ItemRecipeByType.TryGetValue(type, out recipe))
        {
            return recipe;
        }

        return null;
    }

    public static IItem Instantiate(ItemType type)
    {
        ItemProps props = GetItemPropsForType(type);
        if (props.Durable)
        {
            return new DurableItem(type, props);
        }
        if (props.Consumable)
        {
            return new ConsumableItem(type, props);
        }
        if (type == ItemType.GasMask)
        {
            GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerStatistics>().AquiredMask();
        }
        return new NormalItem(type, props);
    }
}

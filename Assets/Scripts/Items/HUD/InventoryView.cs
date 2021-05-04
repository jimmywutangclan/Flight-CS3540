using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class InventoryView : MonoBehaviour
{
    public ItemCollector itemCollector;
    public ItemEquipper itemEquipper;

    public GameObject inventoryPanel;
    public GameObject itemsDisplay;
    public GameObject craftingDisplay;

    public GameObject itemButtonPrefab;
    public GameObject craftingButtonPrefab;
    public Color disabledCraftingButtonColor = Color.gray;
    public Color enabledCraftingButtonColor = Color.white;
    public float buttonOffset = 50.0f;

    Dictionary<ItemType, GameObject> craftingButtons;

    public UIManager uIManager;

    public GameObject reticle;

    private void Start()
    {
        inventoryPanel.SetActive(false);

        InitializeCraftingPanel();
        UpdateCraftingDisplay();
    }
    private void OnEnable()
    {
        itemCollector.NewItem.AddListener(OnInventoryUpdate);
        itemCollector.RemoveItem.AddListener(OnInventoryUpdate);
    }
    private void OnDisable()
    {
        itemCollector.NewItem.RemoveListener(OnInventoryUpdate);
        itemCollector.RemoveItem.RemoveListener(OnInventoryUpdate);
    }

    private void OnInventoryUpdate(IItem _)
    {
        UpdateInventoryDisplay();
        UpdateCraftingDisplay();
    }

    private void InitializeCraftingPanel()
    {
        if(craftingButtons != null)
        {
            foreach (GameObject btn in craftingButtons.Values)
            {
                Destroy(btn);
            }
        }
        craftingButtons = new Dictionary<ItemType, GameObject>();

        int craftingButtonIndex = 0;
        foreach (ItemType type in Enum.GetValues(typeof(ItemType)))
        {
            if (ItemRegistry.IsCraftable(type)) {
                craftingButtons.Add(type, AddCraftingButton(type, craftingButtonIndex, false));
                craftingButtonIndex++;
            }
        }
    }
    private void UpdateCraftingDisplay()
    {
        if (craftingButtons == null)
        {
            return;
        }
        foreach (ItemType itemType in craftingButtons.Keys)
        {
            GameObject btn = craftingButtons[itemType];
            CraftingRecipe itemRecipe = ItemRegistry.GetCraftingRecipeForType(itemType);
            if (itemCollector.Craftable(itemRecipe))
            {
                btn.SetActive(true);
                btn.GetComponent<Button>().interactable = true;
            }
            else
            {
                btn.GetComponent<Button>().interactable = false;
            }
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab)) {
            uIManager.UIShown = !uIManager.UIShown;

            if (uIManager.UIShown) {
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;

                reticle.SetActive(false);

                // TODO subscribe to ItemCollector so we can update when the inventory changes
                UpdateInventoryDisplay();
            }
            else
            {
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;

                reticle.SetActive(true);
            }

            inventoryPanel.SetActive(uIManager.UIShown);
        }
    }

    public void UpdateInventoryDisplay()
    {
        foreach (Transform child in itemsDisplay.transform)
        {
            Destroy(child.gameObject);
        }

        int numInventorySlots = 0;

        List<IItem> inventory = itemCollector.inventory;
        var nonStackableItems = inventory.Where(item => !item.Properties.Stackable);
        foreach (IItem item in nonStackableItems)
        {
            AddItemButton(item, numInventorySlots++);
        }

        var stackableItems = inventory.Where(item => item.Properties.Stackable);
        var itemStacks = stackableItems.GroupBy(item => item.Type);
        foreach (var stack in itemStacks)
        {
            AddStackedItemButton(itemCollector, stack.Key, stack.Count(), numInventorySlots++);
        }
    }

    public GameObject AddCraftingButton(ItemType itemType, int offset, bool initiallyActive)
    {
        GameObject btn = Instantiate(craftingButtonPrefab, craftingDisplay.transform);
        btn.SetActive(initiallyActive);

        btn.transform.position -= new Vector3(0.0f, offset * buttonOffset, 0.0f);
        Vector3 position = new Vector3(0.0f, 0.0f, offset * buttonOffset);
        btn.GetComponentInChildren<Text>().text = itemType.ToString();

        btn.GetComponent<Button>().interactable = true;
        CraftItemAction btnClick = btn.GetComponent<CraftItemAction>();
        btnClick.SetItemType(itemType);
        btnClick.SetItemCollector(itemCollector);

        return btn;
    }
    private void AddItemButton(IItem item, int offset)
    {
        GameObject btn = Instantiate(itemButtonPrefab, itemsDisplay.transform);
        btn.transform.position -= new Vector3(0.0f, offset * buttonOffset, 0.0f);
        btn.GetComponentInChildren<Text>().text = item.Type.ToString();

        if (item.Properties.Equipable)
        {
            btn.GetComponent<Button>().interactable = true;
            EquipItemAction btnClick = btn.GetComponent<EquipItemAction>();
            btnClick.SetItem(item);
            btnClick.SetItemEquipper(itemEquipper);
        }
        else
        {
            btn.GetComponent<Button>().interactable = false;
        }
    }

    private void AddStackedItemButton(ItemCollector itemCollector, ItemType type, int quantity, int offset)
    {
        GameObject btn = Instantiate(itemButtonPrefab, itemsDisplay.transform);
        btn.transform.position -= new Vector3(0.0f, offset * buttonOffset, 0.0f);
        btn.GetComponentInChildren<Text>().text = type.ToString() + "(" + quantity.ToString() + ")";

        if (ItemRegistry.GetItemPropsForType(type).Equipable)
        {
            btn.GetComponent<Button>().interactable = true;
            EquipItemAction btnClick = btn.GetComponent<EquipItemAction>();
            btnClick.SetItem(itemCollector.GetFirstItemOfType(type));
            btnClick.SetItemEquipper(itemEquipper);
        }
        else
        {
            btn.GetComponent<Button>().interactable = false;
        }
    }
}

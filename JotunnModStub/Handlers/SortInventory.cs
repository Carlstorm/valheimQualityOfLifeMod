using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class SortInventory : MonoBehaviour
{
    private static ItemManager ItemManager = new ItemManager();
    private Coroutine animateColorsCoroutine;

    private readonly static float fadeDuration = 0.3f;
    private readonly static float delayDuration = 0.5f;

    public static SortInventory instance { get; private set; }

    public void Awake()
    {
        instance = this;
    }

    public void SortContainerInventory()
    {
        SortContainerInventoryInternal(InventoryGui.instance?.m_currentContainer?.GetInventory(), InventoryGui.instance?.m_containerGrid);
    }

    public void SortPlayerInventory()
    {
        SortPlayerInventoryInternal(Player.m_localPlayer?.GetInventory(), InventoryGui.instance?.m_playerGrid);
    }

    private void SortContainerInventoryInternal(Inventory inventory, InventoryGrid inventoryGrid)
    {
        if (inventory == null || inventoryGrid == null) return;

        var inventoryElements = FindInventoryElements(inventoryGrid.transform);
        var items = inventory.GetAllItems();
        var stackedItems = StackItems(items);

        List<ItemDrop.ItemData> itemsToSort = stackedItems;

        var sortItems = itemsToSort.Select((item, i) => new InventorySortItem(i, inventory.m_width, ItemManager.GetColor(item.m_shared.m_itemType), ItemManager.GetCategory(item.m_shared.m_itemType), item)).ToList();

        sortItems.Sort((a, b) =>
        {
            int categoryComparison = ItemManager.GetCategorySortOrder(a.Category).CompareTo(ItemManager.GetCategorySortOrder(b.Category));
            return categoryComparison != 0 ? categoryComparison : a.Index.CompareTo(b.Index);
        });

        itemsToSort = sortItems.Select(si => si.Item).ToList();

        for (int x = 0; x < itemsToSort.Count; x++)
        {
            itemsToSort[x].m_gridPos = new Vector2i(x % inventory.m_width, x / inventory.m_width);
        }

        inventory.m_inventory = itemsToSort;
        StartAnimateColorsCoroutine(sortItems, inventoryElements, inventory);
    }

    private void SortPlayerInventoryInternal(Inventory inventory, InventoryGrid inventoryGrid)
    {
        if (inventory == null || inventoryGrid == null) return;

        var inventoryElements = FindInventoryElements(inventoryGrid.transform);
        var items = inventory.GetAllItems();
        var stackedItems = StackItems(items);

        List<ItemDrop.ItemData> topItems = stackedItems.Where(item => item?.m_gridPos.y == 0).ToList();
        List<ItemDrop.ItemData> itemsToSort = stackedItems.Where(item => item?.m_gridPos.y > 0).ToList();

        var sortItems = itemsToSort.Select((item, i) => new InventorySortItem(i, inventory.m_width, ItemManager.GetColor(item.m_shared.m_itemType), ItemManager.GetCategory(item.m_shared.m_itemType), item)).ToList();

        sortItems.Sort((a, b) =>
        {
            int categoryComparison = ItemManager.GetCategorySortOrder(a.Category).CompareTo(ItemManager.GetCategorySortOrder(b.Category));
            return categoryComparison != 0 ? categoryComparison : a.Index.CompareTo(b.Index);
        });

        itemsToSort = sortItems.Select(si => si.Item).ToList();

        for (int x = 0; x < itemsToSort.Count; x++)
        {
            itemsToSort[x].m_gridPos = new Vector2i(x % inventory.m_width, 1 + x / inventory.m_width);
        }

        inventory.m_inventory = topItems.Concat(itemsToSort).ToList();
        StartAnimateColorsCoroutine(sortItems, inventoryElements, inventory);
    }

    private void StartAnimateColorsCoroutine(List<InventorySortItem> sortItems, List<GameObject> gridElements, Inventory inventory)
    {
        if (animateColorsCoroutine != null)
        {
            StopCoroutine(animateColorsCoroutine);
            ResetColors(gridElements);
        }
        animateColorsCoroutine = StartCoroutine(AnimateColors(sortItems, gridElements, inventory));
    }

    private List<ItemDrop.ItemData> StackItems(IEnumerable<ItemDrop.ItemData> items)
    {
        var stackedItems = new List<ItemDrop.ItemData>();

        foreach (var item in items)
        {
            if (item == null) continue;

            var existingStack = stackedItems.FirstOrDefault(i => i.m_shared.m_name == item.m_shared.m_name && i.m_stack < i.m_shared.m_maxStackSize);
            if (existingStack != null)
            {
                int stackableAmount = existingStack.m_shared.m_maxStackSize - existingStack.m_stack;
                int transferAmount = Mathf.Min(stackableAmount, item.m_stack);
                existingStack.m_stack += transferAmount;
                item.m_stack -= transferAmount;
                if (item.m_stack > 0) stackedItems.Add(item);
            }
            else
            {
                stackedItems.Add(item);
            }
        }

        return stackedItems;
    }

    private List<GameObject> FindInventoryElements(Transform transform)
    {
        var inventoryElements = new List<GameObject>();

        foreach (Transform child in transform)
        {
            if (child.name.StartsWith("InventoryElement(Clone)"))
            {
                inventoryElements.Add(child.gameObject);
            }
            inventoryElements.AddRange(FindInventoryElements(child));
        }

        return inventoryElements;
    }

    private int GetGridIndex(ItemDrop.ItemData item, int width)
    {
        return item.m_gridPos.y * width + item.m_gridPos.x;
    }

    private void ResetColors(List<GameObject> gridElements)
    {
        foreach (var element in gridElements)
        {
            var image = element.GetComponent<Image>();
            var selectable = element.GetComponent<Selectable>();
            if (selectable != null && image != null)
            {
                ColorBlock colorBlock = selectable.colors;
                image.color = ColorSettings.ContainerSlotDefault;
                colorBlock.normalColor = ColorSettings.ContainerSlotDefaultNormal;
                selectable.colors = colorBlock;
            }
        }
    }

    private IEnumerator AnimateColors(List<InventorySortItem> inventorySortItems, List<GameObject> gridElements, Inventory inventory)
    {
        float elapsedTime = 0f;

        // Set the new colors initially
        foreach (var info in inventorySortItems)
        {
            var index = GetGridIndex(info.Item, info.GridWidth);
            if (index >= 0 && index < gridElements.Count)
            {
                var element = gridElements[index];
                var image = element.GetComponent<Image>();
                var selectable = element.GetComponent<Selectable>();
                if (selectable != null && image != null)
                {
                    ColorBlock colorBlock = selectable.colors;
                    image.color = info.Color;
                    colorBlock.normalColor = ColorSettings.ContainerSlotHighligtNormal;
                    selectable.colors = colorBlock;
                }
            }
        }

        // Wait for the specified delay
        yield return new WaitForSeconds(delayDuration);

        // Fade back to the original colors
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;

            foreach (var info in inventorySortItems)
            {
                var index = GetGridIndex(info.Item, info.GridWidth);
                if (index >= 0 && index < gridElements.Count)
                {
                    var element = gridElements[index];
                    var image = element.GetComponent<Image>();
                    var selectable = element.GetComponent<Selectable>();
                    if (selectable != null && image != null)
                    {
                        ColorBlock colorBlock = selectable.colors;
                        Color currentColor = Color.Lerp(info.Color, ColorSettings.ContainerSlotDefault, elapsedTime / fadeDuration);
                        Color currentNormal = Color.Lerp(ColorSettings.ContainerSlotHighligtNormal, ColorSettings.ContainerSlotDefaultNormal, elapsedTime / fadeDuration);

                        image.color = currentColor;
                        colorBlock.normalColor = currentNormal;
                        selectable.colors = colorBlock;
                    }
                }
            }

            // Ensure that colors are reset for any elements that no longer exist
            foreach (var element in gridElements)
            {
                if (!inventory.GetAllItems().Any(item => item != null && item.m_gridPos.x == element.transform.GetSiblingIndex() % inventory.m_width && item.m_gridPos.y == element.transform.GetSiblingIndex() / inventory.m_width))
                {
                    var image = element.GetComponent<Image>();
                    var selectable = element.GetComponent<Selectable>();
                    if (selectable != null && image != null)
                    {
                        ColorBlock colorBlock = selectable.colors;
                        image.color = ColorSettings.ContainerSlotDefault;
                        colorBlock.normalColor = ColorSettings.ContainerSlotDefaultNormal;
                        selectable.colors = colorBlock;
                    }
                }
            }

            yield return null;
        }

        animateColorsCoroutine = null;
    }
}

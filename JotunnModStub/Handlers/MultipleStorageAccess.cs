using Jotunn.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.UIElements;
using UnityEngine;
using System.Collections;
using static MultipleStorageAccess;
using System.ComponentModel;
using BepInEx;

public class MultipleStorageAccess : MonoBehaviour
{
    private static ItemManager ItemManager = new ItemManager();

    private Coroutine colorChangeCoroutine;

    private static List<Container> _nearbyContainers = new List<Container>();

    public static MultipleStorageAccess instance;

    public string searchTerm = string.Empty;

    public ItemCategory? category = null;

    public class AggregationResult
    {
        public Container TargetContainer { get; set; }
        public ItemDrop.ItemData ClonedItem { get; set; }

        public AggregationResult(Container targetContainer, ItemDrop.ItemData clonedItem)
        {
            TargetContainer = targetContainer;
            ClonedItem = clonedItem;
        }
    }

    public void Awake()
    {
        instance = this;
    }

    public void UpdateOnContainerChange(List<Container> nearbyContainers)
    {
        _nearbyContainers.Clear();
        _nearbyContainers.AddRange(nearbyContainers);

        if (MultipleStorageAccessGui.instance != null && MultipleStorageAccessGui.instance.IsVisible)
        {
            UpdatePanel();
        }
        UpdateButton();
    }

    private List<QuickStorageItem> FilterItems(List<QuickStorageItem> originalItems)
    {
        if (!searchTerm.IsNullOrWhiteSpace()) {
            return originalItems
                .Where(item => item.ItemData.m_shared.m_name.IndexOf(searchTerm, StringComparison.OrdinalIgnoreCase) >= 0)
                .OrderBy(item => item.ItemData.m_shared.m_name)
                .ToList();
        }

        if (category != null)
        {
            return originalItems
                .Where(item => item.Category == category)
                .OrderBy(item => item.ItemData.m_shared.m_name)
                .ToList();
        }

        return originalItems
            .OrderBy(item => item.ItemData.m_shared.m_name)
            .ToList();
    }

    public void UpdatePanel()
    {
        var cont = _nearbyContainers;
        if (cont == null || !cont.Any())
        {
            MultipleStorageAccessGui.instance.SetItems(new List<QuickStorageItem>());
            return;
        }

        var quickStorageItems = GetContainerItems();
        var filteredItems = FilterItems(quickStorageItems);

        int totalFilledSlots = 0;
        int totalEmptySlots = 0;

        foreach (var container in cont)
        {
            var inventory = container.GetInventory();
            int containerFilledSlots = 0;
            int containerEmptySlots = 0;

            foreach (var item in inventory.GetAllItems())
            {
                if (item != null && item.m_stack > 0)
                {
                    containerFilledSlots++;
                }
            }

            int totalSlots = inventory.GetWidth() * inventory.GetHeight();
            containerEmptySlots = totalSlots - containerFilledSlots;

            totalFilledSlots += containerFilledSlots;
            totalEmptySlots += containerEmptySlots;
        }

        int totalItemsNStacks = 0;
        foreach (var item in quickStorageItems)
        {
            totalItemsNStacks += item.CombinedStackCount;
        }

        Dictionary<ItemCategory, int> categoryCounts = GetCategoryCounts(quickStorageItems);

        MultipleStorageAccessGui.instance.SetContainerDetialsCounts(cont.Count, totalEmptySlots, totalFilledSlots, totalItemsNStacks);
        MultipleStorageAccessGui.instance.SetCategoryCounts(categoryCounts);
        MultipleStorageAccessGui.instance.SetItems(filteredItems);
    }

    public static Dictionary<ItemCategory, int> GetCategoryCounts<T>(List<T> items) where T : QuickStorageItem
    {
        return items
            .GroupBy(item => item.Category)
            .ToDictionary(group => group.Key, group => group.Count());
    }

    public void OnContainerClick()
    {
        if (InventoryGui.instance.m_dragItem != null)
        {
            if (InventoryGui.instance.m_dragInventory == Player.m_localPlayer.GetInventory())
            {
                TryPlaceStackThroughQuickAccess(InventoryGui.instance.m_dragItem, true);
                return;
            }
            InventoryGui.instance.CloseContainer();
            return;
        }
    }

    public void OnItemClick(QuickStorageItem item, BaseEventData eventData)
    {
        Jotunn.Logger.LogError("Item clicked: " + item.ItemData.m_shared.m_name);

        var pointerEventData = eventData as PointerEventData;
        if (pointerEventData.button == PointerEventData.InputButton.Right)
        {
            InventoryGui.instance.CloseContainer();
            return;
        }

        if (pointerEventData.button == PointerEventData.InputButton.Middle)
        {
            return;
        }

        if (InventoryGui.instance.m_dragItem != null)
        {
            if (InventoryGui.instance.m_dragInventory == Player.m_localPlayer.GetInventory())
            {
                TryPlaceStackThroughQuickAccess(InventoryGui.instance.m_dragItem, true);
                return;
            }
            InventoryGui.instance.CloseContainer();
            return;
        }

        //var pointerEventData = eventData as PointerEventData;

        // Logic to add item(s) to the player's inventory

        // Assuming you have access to the player's inventory:
        var playerInventory = Player.m_localPlayer.GetInventory();

        if (playerInventory != null && item.CombinedStackCount > 0)
        {

            int splitAmount = item.CombinedStackCount < item.ItemData.m_shared.m_maxStackSize ? item.CombinedStackCount : item.ItemData.m_shared.m_maxStackSize;
            Container suitableContainer = FindContainerWithMaxStacks(item);

            if (suitableContainer == null)
            {
                return;
            }

            var aggregationResult = AggregateItemsToContainer(item, suitableContainer, splitAmount);

            if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
            {
                AddStackToInventory(aggregationResult.ClonedItem, playerInventory, aggregationResult.TargetContainer);
                return;
            }

            if (InventoryGui.instance.m_currentContainer != null)
            {
                InventoryGui.instance.CloseContainer();
            }

            InventoryGui.instance.m_currentContainer = suitableContainer;

            if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
            {
                InventoryGui.instance.ShowSplitDialog(aggregationResult.ClonedItem, aggregationResult.TargetContainer.GetInventory());
            }
            else
            {
                InventoryGui.instance.SetupDragItem(aggregationResult.ClonedItem, aggregationResult.TargetContainer.GetInventory(), splitAmount);
            }
        }
    }

    private Container FindContainerWithMaxStacks(QuickStorageItem quickStorageItem)
    {
        Container maxContainer = null;
        int maxStack = 0;

        foreach (var pair in quickStorageItem.ItemContainerPairs)
        {
            if (pair.Item.m_stack > maxStack)
            {
                maxStack = pair.Item.m_stack;
                maxContainer = pair.Container;
            }
        }

        return maxContainer;
    }

    private AggregationResult AggregateItemsToContainer(QuickStorageItem quickStorageItem, Container targetContainer, int requiredAmount)
    {
        // Find the existing item stack in the target container
        var targetInventory = targetContainer.GetInventory();
        var targetItemStack = targetInventory.GetAllItems()
            .FirstOrDefault(item => item.m_shared.m_name == quickStorageItem.ItemContainerPairs[0].Item.m_shared.m_name);

        if (targetItemStack == null)
        {
            targetItemStack = quickStorageItem.ItemContainerPairs[0].Item.Clone();
            targetItemStack.m_stack = 0;
            targetInventory.AddItem(targetItemStack);
        }

        int remainingAmount = requiredAmount - targetItemStack.m_stack;
        int maxStackSize = targetItemStack.m_shared.m_maxStackSize;

        if (remainingAmount <= 0)
        {
            return new AggregationResult(targetContainer, targetItemStack);
        }

        // Prioritize combining stacks within the target container
        foreach (var item in targetInventory.GetAllItems()
            .Where(i => i.m_shared.m_name == targetItemStack.m_shared.m_name && i != targetItemStack).ToList())
        {
            if (remainingAmount <= 0)
                break;

            int itemsToMove = Mathf.Min(item.m_stack, remainingAmount);
            remainingAmount -= itemsToMove;
            targetItemStack.m_stack += itemsToMove;
            item.m_stack -= itemsToMove;

            if (item.m_stack <= 0)
            {
                targetInventory.RemoveItem(item);
            }
        }

        // Aggregate items from other containers
        foreach (var pair in quickStorageItem.ItemContainerPairs)
        {
            if (remainingAmount <= 0)
                break;

            var containerInventory = pair.Container.GetInventory();
            var containerItem = containerInventory.GetItem(pair.Item.m_shared.m_name);

            if (containerItem != null)
            {
                int itemsToMove = Mathf.Min(containerItem.m_stack, remainingAmount);
                remainingAmount -= itemsToMove;
                containerItem.m_stack -= itemsToMove;
                targetItemStack.m_stack += itemsToMove;

                if (containerItem.m_stack <= 0)
                {
                    containerInventory.RemoveItem(containerItem);
                    pair.Item.m_stack = 0;  // Clear stack in pair
                }
            }

            containerInventory.Changed();
        }

        targetInventory.Changed();

        return new AggregationResult(targetContainer, targetItemStack);
    }


    private void AddStackToInventory(ItemDrop.ItemData itemToMove, Inventory playerInventory, Container itemContainer)
    {
        if (playerInventory == null || itemToMove == null || itemToMove.m_stack <= 0)
        {
            return;
        }

        // Determine how much can actually be added to the player's inventory
        int remainingStackSize = itemToMove.m_stack;
        int maxStackSize = itemToMove.m_shared.m_maxStackSize;

        // Calculate the space available in the player's inventory
        int availableSpace = 0;

        foreach (var slot in playerInventory.GetAllItems())
        {
            if (slot.m_shared.m_name == itemToMove.m_shared.m_name)
            {
                availableSpace += (maxStackSize - slot.m_stack);
            }
        }

        int emptySlots = playerInventory.GetWidth() * playerInventory.GetHeight() - playerInventory.GetAllItems().Count;
        availableSpace += emptySlots * maxStackSize;

        if (availableSpace <= 0)
        {
            return;
        }

        remainingStackSize = Mathf.Min(itemToMove.m_stack, availableSpace);

        // Add the items to the player's inventory
        var itemsToAdd = itemToMove.Clone();
        itemsToAdd.m_stack = remainingStackSize;
        playerInventory.AddItem(itemsToAdd);

        // Remove the added items from the known container
        itemToMove.m_stack -= remainingStackSize;
        if (itemToMove.m_stack <= 0)
        {
            itemContainer.GetInventory().RemoveItem(itemToMove);
        }

        // Update the inventories
        itemContainer.GetInventory().Changed();

        // Log the action and update the UI to reflect the new count
        Jotunn.Logger.LogWarning("Stack taken: " + itemsToAdd.m_shared.m_name + " (" + itemsToAdd.m_stack + ")");
    }

    public void TryPlaceStackThroughQuickAccess(ItemDrop.ItemData itemData, bool fromDrag)
    {
        var playerInventory = Player.m_localPlayer.GetInventory();
        var containers = _nearbyContainers;
        int remainingStack = itemData.m_stack;

        // Track changed containers to update them later
        var changedContainers = new HashSet<Container>();

        // Try to add to existing stacks first
        foreach (var container in containers)
        {
            var containerInventory = container.GetInventory();
            foreach (var containerItem in containerInventory.GetAllItems())
            {
                if (containerItem.m_shared.m_name == itemData.m_shared.m_name && containerItem.m_stack < containerItem.m_shared.m_maxStackSize)
                {
                    int stackSpace = containerItem.m_shared.m_maxStackSize - containerItem.m_stack;
                    int addAmount = Mathf.Min(stackSpace, remainingStack);
                    containerItem.m_stack += addAmount;
                    remainingStack -= addAmount;
                    changedContainers.Add(container);
                    if (remainingStack <= 0)
                        break;
                }
            }
            if (remainingStack <= 0)
                break;
        }

        // Add to empty slots in containers that already have the item
        if (remainingStack > 0)
        {
            foreach (var container in containers)
            {
                var containerInventory = container.GetInventory();
                foreach (var containerItem in containerInventory.GetAllItems())
                {
                    if (containerItem.m_shared.m_name == itemData.m_shared.m_name)
                    {
                        var itemClone = itemData.Clone();
                        itemClone.m_stack = Mathf.Min(itemClone.m_stack, remainingStack);
                        if (containerInventory.CanAddItem(itemClone))
                        {
                            containerInventory.AddItem(itemClone);
                            remainingStack -= itemClone.m_stack;
                            changedContainers.Add(container);
                            if (remainingStack <= 0)
                                break;
                        }
                    }
                }
                if (remainingStack <= 0)
                    break;
            }
        }

        // Add to any empty slots in any container
        if (remainingStack > 0)
        {
            foreach (var container in containers)
            {
                var containerInventory = container.GetInventory();
                var itemClone = itemData.Clone();
                itemClone.m_stack = remainingStack;
                if (containerInventory.CanAddItem(itemClone))
                {
                    containerInventory.AddItem(itemClone);
                    remainingStack = 0;
                    changedContainers.Add(container);
                    break;
                }
            }
        }

        // Remove the item from the player's inventory
        if (remainingStack < itemData.m_stack)
        {
            itemData.m_stack = remainingStack; // Update the stack size of the clicked item
            if (itemData.m_stack <= 0)
            {
                playerInventory.RemoveItem(itemData);
                if (fromDrag)
                {
                    InventoryGui.instance.SetupDragItem(null, null, 1);
                }
            }
            else
            {
                IndicateFull();
                if (fromDrag)
                {
                    InventoryGui.instance.m_dragAmount = itemData.m_stack;
                }
            }

            foreach (var container in changedContainers)
            {
                container.GetInventory().Changed();
            }
            Jotunn.Logger.LogWarning("Item moved to containers: " + itemData.m_shared.m_name);
        }
        else
        {
            Jotunn.Logger.LogWarning("No space available in nearby containers for item: " + itemData.m_shared.m_name);
            IndicateFull();
        }

    }

    private void UpdateButton()
    {
        if (!_nearbyContainers.Any())
        {
            ExtraInventoryButtonsGui.instance.HideMultipleAccessButton();
        } else
        {
            ExtraInventoryButtonsGui.instance.ShowMultipleAccessButton(_nearbyContainers.Count);
        }
    }
    private void IndicateFull()
    {
        if (colorChangeCoroutine != null)
        {
            StopCoroutine(colorChangeCoroutine);
        }
        colorChangeCoroutine = StartCoroutine(ChangeTextColorForASecond(Color.red, 0.5f));
    }

    private IEnumerator ChangeTextColorForASecond(Color color, float duration)
    {
        MultipleStorageAccessGui.instance.CountColorFull();
        yield return new WaitForSeconds(duration);
        MultipleStorageAccessGui.instance.ResetCountColor();
        colorChangeCoroutine = null;
    }

    private List<QuickStorageItem> GetContainerItems()
    {
        Dictionary<string, QuickStorageItem> itemCounts = new Dictionary<string, QuickStorageItem>();

        var containers = _nearbyContainers;
        foreach (Container container in containers)
        {
            var inventory = container.GetInventory();
            foreach (var item in inventory.GetAllItems())
            {
                string key = $"{item.m_shared.m_name}_{item.m_quality}_{item.m_durability}";

                if (itemCounts.ContainsKey(key))
                {
                    itemCounts[key].CombinedStackCount += item.m_stack;
                    itemCounts[key].ItemContainerPairs.Add(new ItemContainerPair(item, container));
                }
                else
                {
                    var quickStorageItem = new QuickStorageItem(item, item.m_stack, ItemManager.GetCategory(item.m_shared.m_itemType));
                    quickStorageItem.ItemContainerPairs.Add(new ItemContainerPair(item, container));
                    itemCounts[key] = quickStorageItem;
                }
            }
        }

        List<QuickStorageItem> combinedItemList = new List<QuickStorageItem>(itemCounts.Values);
        return combinedItemList;
    }
}
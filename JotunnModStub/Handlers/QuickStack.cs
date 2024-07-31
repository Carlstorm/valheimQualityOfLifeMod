using System.Collections.Generic;
using UnityEngine;

public class QuickStack : MonoBehaviour
{
    private static List<Container> _nearbyContainers = new List<Container>();

    public static QuickStack instance { get; private set; }

    public void Awake()
    {
        instance = this;
    }

    public void UpdateOnContainerChange(List<Container> nearbyContainers)
    {
        _nearbyContainers = new List<Container>(nearbyContainers);
    }

    public void DoStack()
    {
        Player player = Player.m_localPlayer;
        Inventory playerInventory = player.GetInventory();

        // Cache the items of each container to avoid repeated GetAllItems calls
        var containerItemsCache = new Dictionary<string, List<ItemDrop.ItemData>>();
        var presentItemNames = new HashSet<string>();

        foreach (Container container in _nearbyContainers)
        {
            Inventory containerInventory = container.GetInventory();
            foreach (var item in containerInventory.GetAllItems())
            {
                if (!containerItemsCache.TryGetValue(item.m_shared.m_name, out var itemList))
                {
                    itemList = new List<ItemDrop.ItemData>();
                    containerItemsCache[item.m_shared.m_name] = itemList;
                }
                itemList.Add(item);
                presentItemNames.Add(item.m_shared.m_name);
            }
        }

        StackItems(playerInventory, containerItemsCache, presentItemNames);
    }

    private void StackItems(Inventory playerInventory, Dictionary<string, List<ItemDrop.ItemData>> containerItemsCache, HashSet<string> presentItemNames)
    {
        var playerItems = playerInventory.GetAllItems();
        var itemsToRemove = new List<ItemDrop.ItemData>();

        foreach (var playerItem in playerItems)
        {
            if (!presentItemNames.Contains(playerItem.m_shared.m_name))
                continue;

            if (containerItemsCache.TryGetValue(playerItem.m_shared.m_name, out var containerItems))
            {
                // Prioritize stacking into existing stacks and adding to containers with the same item type
                foreach (var container in _nearbyContainers)
                {
                    var containerInventory = container.GetInventory();
                    foreach (var containerItem in containerItems)
                    {
                        if (containerItem.m_shared.m_name == playerItem.m_shared.m_name && containerItem.m_stack < containerItem.m_shared.m_maxStackSize)
                        {
                            int amountToMove = Mathf.Min(playerItem.m_stack, containerItem.m_shared.m_maxStackSize - containerItem.m_stack);
                            containerItem.m_stack += amountToMove;
                            playerItem.m_stack -= amountToMove;
                            if (playerItem.m_stack <= 0)
                            {
                                itemsToRemove.Add(playerItem);
                                break;
                            }
                        }
                    }

                    // Try to add remaining items to containers that already have the item type
                    if (playerItem.m_stack > 0 && containerItems.Exists(item => item.m_shared.m_name == playerItem.m_shared.m_name))
                    {
                        if (containerInventory.GetEmptySlots() > 0)
                        {
                            var newItem = playerItem.Clone();
                            int amountToMove = Mathf.Min(playerItem.m_stack, newItem.m_shared.m_maxStackSize);
                            newItem.m_stack = amountToMove;
                            playerItem.m_stack -= amountToMove;
                            containerInventory.AddItem(newItem);

                            if (playerItem.m_stack <= 0)
                            {
                                itemsToRemove.Add(playerItem);
                                break;
                            }
                        }
                    }
                }
            }

            // Add remaining items as new stacks if there are empty slots in any container
            if (playerItem.m_stack > 0)
            {
                foreach (var container in _nearbyContainers)
                {
                    var containerInventory = container.GetInventory();
                    if (containerInventory.GetEmptySlots() > 0)
                    {
                        var newItem = playerItem.Clone();
                        int amountToMove = Mathf.Min(playerItem.m_stack, newItem.m_shared.m_maxStackSize);
                        newItem.m_stack = amountToMove;
                        playerItem.m_stack -= amountToMove;
                        containerInventory.AddItem(newItem);

                        if (playerItem.m_stack <= 0)
                        {
                            itemsToRemove.Add(playerItem);
                            break;
                        }
                    }
                }
            }
        }

        // Remove items from player inventory after iteration
        foreach (var itemToRemove in itemsToRemove)
        {
            playerInventory.RemoveItem(itemToRemove);
        }
    }
}

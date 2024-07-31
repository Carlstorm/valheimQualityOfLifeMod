using System.Collections.Generic;
using UnityEngine;

public class CombineItemsMod : MonoBehaviour
{
    private readonly float itemRadius = 12f; // Radius to group items
    private HashSet<ItemDrop> processedItems = new HashSet<ItemDrop>();
    private HashSet<ItemDrop> specialActionItems = new HashSet<ItemDrop>();

    private List<string> buggyItems = new List<string>()
    {
        "$item_trophy_deer"
    };

    public void UpdateOnItemDropChange(List<ItemDrop> nearbyItemDrops)
    {
        processedItems.Clear(); // Clear the processed items set at the beginning of each update
        List<List<ItemDrop>> groups = GroupItems(nearbyItemDrops);

        foreach (var group in groups)
        {
            if (group.Count <= 1)
            {
                continue;
            }

            Vector3 centerPosition = CalculateCenterPosition(group);
            CombineItemsInGroup(group, centerPosition);
        }
    }

    private List<List<ItemDrop>> GroupItems(List<ItemDrop> items)
    {
        List<List<ItemDrop>> groups = new List<List<ItemDrop>>();
        HashSet<int> visited = new HashSet<int>();

        for (int i = 0; i < items.Count; i++)
        {
            if (visited.Contains(i) || processedItems.Contains(items[i]) || specialActionItems.Contains(items[i])) continue;

            // Skip items that are already at their maximum stack size or are used for special actions
            if (items[i].m_itemData.m_stack >= items[i].m_itemData.m_shared.m_maxStackSize)
            {
                visited.Add(i);
                continue;
            }

            List<ItemDrop> group = new List<ItemDrop> { items[i] };
            visited.Add(i);

            Jotunn.Logger.LogError(items[i].m_itemData.m_shared.m_name);

            for (int j = i + 1; j < items.Count; j++)
            {
                if (visited.Contains(j) || processedItems.Contains(items[j]) || specialActionItems.Contains(items[j])) continue;

                if (items[i].m_itemData.m_shared.m_name == items[j].m_itemData.m_shared.m_name &&
                    Vector3.Distance(items[i].transform.position, items[j].transform.position) <= itemRadius &&
                    items[j].m_itemData.m_stack < items[j].m_itemData.m_shared.m_maxStackSize &&
                    !buggyItems.Contains(items[i].m_itemData.m_shared.m_name))
                {
                    group.Add(items[j]);
                    visited.Add(j);
                }
            }

            groups.Add(group);
        }

        Debug.Log($"[CombineItemsMod] Grouping completed. Total groups: {groups.Count}");

        return groups;
    }

    //private bool IsSpecialActionItem(ItemDrop item)
    //{
    //    // Check if the item is used for special actions, like spawning bosses
    //    if (item.m_itemData.m_shared.m_name == "$item_trophy_deer" && item.m_itemData.m_stack >= 4)
    //    {
    //        specialActionItems.Add(item); // Mark this item as involved in a special action
    //        return true;
    //    }
    //    return false;
    //}

    private Vector3 CalculateCenterPosition(List<ItemDrop> group)
    {
        Vector3 sum = Vector3.zero;
        foreach (var item in group)
        {
            sum += item.transform.position;
        }
        return sum / group.Count;
    }

    private void CombineItemsInGroup(List<ItemDrop> group, Vector3 centerPosition)
    {
        // Calculate total stack size
        int totalStackSize = 0;
        int maxStackSize = group[0].m_itemData.m_shared.m_maxStackSize;
        ItemDrop baseItem = group[0];

        foreach (var item in group)
        {
            totalStackSize += item.m_itemData.m_stack;
            if (item != baseItem)
            {
                Debug.Log($"[CombineItemsMod] Destroying item: {item.m_itemData.m_shared.m_name}, Stack: {item.m_itemData.m_stack}");
                processedItems.Add(item); // Mark this item as processed
                item.m_nview.Destroy();
            }
        }

        // Adjust the stack size of the base item
        baseItem.m_itemData.m_stack = Mathf.Min(totalStackSize, maxStackSize);
        totalStackSize -= baseItem.m_itemData.m_stack;

        // Ensure the new item is placed on the ground
        Vector3 groundPosition = GetGroundPosition(centerPosition);
        baseItem.transform.position = groundPosition;
        baseItem.Save();
        Debug.Log($"[CombineItemsMod] Base item: {baseItem.m_itemData.m_shared.m_name}, New stack: {baseItem.m_itemData.m_stack}");
        processedItems.Add(baseItem); // Mark the base item as processed

        // Create new item drops with the remaining stack size, respecting the max stack size
        while (totalStackSize > 0)
        {
            int stackSize = Mathf.Min(totalStackSize, maxStackSize);
            totalStackSize -= stackSize;

            ItemDrop newItem = Object.Instantiate(baseItem, groundPosition, Quaternion.identity);
            newItem.m_itemData.m_stack = stackSize;
            newItem.Save();
            Debug.Log($"[CombineItemsMod] New item: {newItem.m_itemData.m_shared.m_name}, Stack: {newItem.m_itemData.m_stack}");
            processedItems.Add(newItem); // Mark new items as processed
        }
    }

    private Vector3 GetGroundPosition(Vector3 position)
    {
        RaycastHit hit;
        if (Physics.Raycast(position + Vector3.up * 10f, Vector3.down, out hit, 20f, LayerMask.GetMask("Default")))
        {
            position = hit.point;
        }

        position.y += 0.5f;
        return position;
    }
}

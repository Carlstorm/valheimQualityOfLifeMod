using System.Collections.Generic;

public class QuickStorageItem
{
    public ItemDrop.ItemData ItemData { get; set; }
    public int CombinedStackCount { get; set; }
    public List<ItemContainerPair> ItemContainerPairs { get; set; }
    public ItemCategory Category { get; set; }

    public QuickStorageItem(ItemDrop.ItemData itemData, int combinedStackCount, ItemCategory category)
    {
        ItemData = itemData;
        CombinedStackCount = combinedStackCount;
        Category = category;
        ItemContainerPairs = new List<ItemContainerPair>();
    }
}

public class ItemContainerPair
{
    public ItemDrop.ItemData Item { get; set; }
    public Container Container { get; set; }

    public ItemContainerPair(ItemDrop.ItemData item, Container container)
    {
        Item = item;
        Container = container;
    }
}
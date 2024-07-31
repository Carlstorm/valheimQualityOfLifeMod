using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class InventorySortItem
{
    public int Index { get; set; }
    public int GridWidth { get; set; }
    public Color Color { get; set; }
    public ItemCategory Category { get; set; }
    public ItemDrop.ItemData Item { get; set; }
    public InventorySortItem(int index, int gridWidth, Color color, ItemCategory category, ItemDrop.ItemData item)
    {
        Index = index;
        GridWidth = gridWidth;
        Color = color;
        Category = category;
        Item = item;
    }
}
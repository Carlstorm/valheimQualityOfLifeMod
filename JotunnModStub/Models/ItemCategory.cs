using System.Collections.Generic;
using UnityEngine;
using static ItemDrop.ItemData;

public class CategoryItem
{
    public ItemType ItemType { get; set; }
    public ItemCategory ItemCategory { get; set; }

    public CategoryItem(ItemType itemType, ItemCategory itemCategory)
    {
        ItemType = itemType;
        ItemCategory = itemCategory;
    }
}

public class ItemManager : MonoBehaviour
{
    private readonly List<CategoryItem> items;

    public Dictionary<ItemCategory, int> ItemCategorySortOrder { get; private set; }
    public Dictionary<ItemCategory, Color> ItemCategoryColors { get; private set; }

    public ItemManager()
    {
        items = new List<CategoryItem>
        {
            new CategoryItem(ItemType.None, ItemCategory.Material),
            new CategoryItem(ItemType.Material, ItemCategory.Material),
            new CategoryItem(ItemType.Consumable, ItemCategory.Consumable),
            new CategoryItem(ItemType.OneHandedWeapon, ItemCategory.Weapon),
            new CategoryItem(ItemType.Bow, ItemCategory.Weapon),
            new CategoryItem(ItemType.TwoHandedWeapon, ItemCategory.Weapon),
            new CategoryItem(ItemType.Attach_Atgeir, ItemCategory.Weapon),
            new CategoryItem(ItemType.TwoHandedWeaponLeft, ItemCategory.Weapon),
            new CategoryItem(ItemType.Shield, ItemCategory.Armor),
            new CategoryItem(ItemType.Helmet, ItemCategory.Armor),
            new CategoryItem(ItemType.Chest, ItemCategory.Armor),
            new CategoryItem(ItemType.Legs, ItemCategory.Armor),
            new CategoryItem(ItemType.Hands, ItemCategory.Armor),
            new CategoryItem(ItemType.Shoulder, ItemCategory.Armor),
            new CategoryItem(ItemType.Ammo, ItemCategory.Ammo),
            new CategoryItem(ItemType.AmmoNonEquipable, ItemCategory.Ammo),
            new CategoryItem(ItemType.Customization, ItemCategory.Misc),
            new CategoryItem(ItemType.Trophy, ItemCategory.Misc),
            new CategoryItem(ItemType.Misc, ItemCategory.Misc),
            new CategoryItem(ItemType.Fish, ItemCategory.Misc),
            new CategoryItem(ItemType.Torch, ItemCategory.Tool),
            new CategoryItem(ItemType.Tool, ItemCategory.Tool),
            new CategoryItem(ItemType.Utility, ItemCategory.Utility)
        };

        ItemCategorySortOrder = new Dictionary<ItemCategory, int>
        {
            { ItemCategory.Weapon, 0 },
            { ItemCategory.Tool, 1 },
            { ItemCategory.Armor, 2 },
            { ItemCategory.Consumable, 3 },
            { ItemCategory.Ammo, 4 },
            { ItemCategory.Utility, 5 },
            { ItemCategory.Misc, 6 },
            { ItemCategory.Material, 7 }
        };

        ItemCategoryColors = new Dictionary<ItemCategory, Color>
        {
            { ItemCategory.Material, ColorSettings.SortColorMaterial },
            { ItemCategory.Consumable, ColorSettings.SortColorConsumable },
            { ItemCategory.Weapon, ColorSettings.SortColorWeapon },
            { ItemCategory.Armor, ColorSettings.SortColorArmor },
            { ItemCategory.Ammo, ColorSettings.SortColorAmmo },
            { ItemCategory.Misc, ColorSettings.SortColorMisc },
            { ItemCategory.Tool, ColorSettings.SortColorTool },
            { ItemCategory.Utility, ColorSettings.SortColorUtility }
        };
    }

    public ItemCategory GetCategory(ItemType itemType)
    {
        var item = items.Find(i => i.ItemType == itemType);
        return item != null ? item.ItemCategory : ItemCategory.Misc;
    }

    public Color GetColor(ItemType itemType)
    {
        var item = items.Find(i => i.ItemType == itemType);
        if (item != null && ItemCategoryColors.TryGetValue(item.ItemCategory, out var color))
        {
            return color;
        }
        return Color.white;
    }

    public int GetCategorySortOrder(ItemCategory category)
    {
        return ItemCategorySortOrder.TryGetValue(category, out int order) ? order : int.MaxValue;
    }
}

public enum ItemCategory
{
    Misc = 0,
    Weapon = 1,
    Armor = 2,
    Tool = 3,
    Ammo = 4,
    Utility = 5,
    Consumable = 6,
    Material = 7,
}

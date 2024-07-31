using System;
using System.Collections.Generic;
using HarmonyLib;
using TMPro;
using UnityEngine;

public class CraftFromStorageMod : MonoBehaviour
{
    private static List<Container> _nearbyContainers = new List<Container>();

    public void UpdateOnContainerChange(List<Container> nearbyContainers)
    {
        _nearbyContainers.Clear();
        _nearbyContainers.AddRange(nearbyContainers);
    }

    [HarmonyPatch(typeof(InventoryGui), "SetupRequirement")]
    static class InventoryGui_SetupRequirement_Patch
    {
        static void Postfix(InventoryGui __instance, Transform elementRoot, Piece.Requirement req, Player player, bool craft, int quality)
        {
            if (req.m_resItem != null)
            {
                string itemName = req.m_resItem.m_itemData.m_shared.m_name;
                int invAmount = player.GetInventory().CountItems(itemName);
                int amount = req.GetAmount(quality);
                if (amount <= 0)
                {
                    return;
                }
                TMP_Text text = elementRoot.transform.Find("res_amount").GetComponent<TMP_Text>();
                if (invAmount < amount)
                {
                    foreach (Container c in _nearbyContainers)
                    {
                        invAmount += c.GetInventory().CountItems(itemName);
                        if (invAmount >= amount) break;
                    }

                    if (invAmount >= amount)
                    {
                        text.color = ((Mathf.Sin(Time.time * 10f) > 0f) ? Color.yellow : Color.white);
                    }
                }
                text.text = $"{amount}/{invAmount}";
            }
        }
    }

    [HarmonyPatch(typeof(Player), "HaveRequirementItems", new Type[] { typeof(Recipe), typeof(bool), typeof(int) })]
    static class HaveRequirementItems_Patch
    {
        static void Postfix(Player __instance, ref bool __result, Recipe piece, bool discover, int qualityLevel)
        {
            if (__result || discover)
                return;

            foreach (Piece.Requirement requirement in piece.m_resources)
            {
                if (requirement.m_resItem)
                {
                    string itemName = requirement.m_resItem.m_itemData.m_shared.m_name;
                    int amount = requirement.GetAmount(qualityLevel);
                    int invAmount = __instance.GetInventory().CountItems(itemName);
                    if (invAmount < amount)
                    {
                        foreach (Container c in _nearbyContainers)
                        {
                            invAmount += c.GetInventory().CountItems(itemName);
                            if (invAmount >= amount) break;
                        }
                        if (invAmount < amount)
                            return;
                    }
                }
            }
            __result = true;
        }
    }

    [HarmonyPatch(typeof(Player), "HaveRequirements", new Type[] { typeof(Piece), typeof(Player.RequirementMode) })]
    static class HaveRequirements_Patch
    {
        static void Postfix(Player __instance, ref bool __result, Piece piece, Player.RequirementMode mode)
        {
            if (__result || __instance?.transform?.position == null)
                return;

            if (piece.m_craftingStation)
            {
                if (mode == Player.RequirementMode.IsKnown || mode == Player.RequirementMode.CanAlmostBuild)
                {
                    if (!__instance.m_knownStations.ContainsKey(piece.m_craftingStation.m_name))
                    {
                        return;
                    }
                }
                else if (!CraftingStation.HaveBuildStationInRange(piece.m_craftingStation.m_name, __instance.transform.position))
                {
                    return;
                }
            }

            if (piece.m_dlc.Length > 0 && !DLCMan.instance.IsDLCInstalled(piece.m_dlc))
            {
                return;
            }

            foreach (Piece.Requirement requirement in piece.m_resources)
            {
                if (requirement.m_resItem && requirement.m_amount > 0)
                {
                    string itemName = requirement.m_resItem.m_itemData.m_shared.m_name;
                    if (mode == Player.RequirementMode.IsKnown)
                    {
                        if (!__instance.m_knownMaterial.Contains(itemName))
                        {
                            return;
                        }
                    }
                    else if (mode == Player.RequirementMode.CanAlmostBuild)
                    {
                        if (!__instance.GetInventory().HaveItem(itemName))
                        {
                            bool hasItem = false;
                            foreach (Container c in _nearbyContainers)
                            {
                                if (c.GetInventory().HaveItem(itemName))
                                {
                                    hasItem = true;
                                    break;
                                }
                            }
                            if (!hasItem)
                                return;
                        }
                    }
                    else if (mode == Player.RequirementMode.CanBuild)
                    {
                        int invAmount = __instance.GetInventory().CountItems(itemName);
                        if (invAmount < requirement.m_amount)
                        {
                            foreach (Container c in _nearbyContainers)
                            {
                                invAmount += c.GetInventory().CountItems(itemName);
                                if (invAmount >= requirement.m_amount)
                                {
                                    break;
                                }
                            }
                            if (invAmount < requirement.m_amount)
                                return;
                        }
                    }
                }
            }
            __result = true;
        }
    }

    [HarmonyPatch(typeof(Player), "ConsumeResources")]
    static class ConsumeResources_Patch
    {
        static bool Prefix(Player __instance, Piece.Requirement[] requirements, int qualityLevel)
        {
            Inventory pInventory = __instance.GetInventory();
            Dictionary<string, int> toConsume = new Dictionary<string, int>();

            foreach (Piece.Requirement requirement in requirements)
            {
                if (requirement.m_resItem)
                {
                    string reqName = requirement.m_resItem.m_itemData.m_shared.m_name;
                    int totalRequirement = requirement.GetAmount(qualityLevel);
                    if (totalRequirement <= 0) continue;

                    int totalAmount = pInventory.CountItems(reqName);
                    if (totalAmount < totalRequirement)
                    {
                        int remainingRequirement = totalRequirement - totalAmount;

                        foreach (Container c in _nearbyContainers)
                        {
                            int containerAmount = c.GetInventory().CountItems(reqName);
                            if (containerAmount >= remainingRequirement)
                            {
                                toConsume[reqName] = totalRequirement;
                                remainingRequirement = 0;
                                break;
                            }
                            else
                            {
                                remainingRequirement -= containerAmount;
                                toConsume[reqName] = totalAmount + containerAmount;
                            }
                        }

                        if (remainingRequirement > 0)
                        {
                            Debug.LogError("Not enough resources to complete the crafting.");
                            return false; // Prevent further processing if resources are insufficient
                        }
                    }
                    else
                    {
                        toConsume[reqName] = totalRequirement;
                    }
                }
            }

            // Consume resources from the player's inventory first
            foreach (var item in toConsume)
            {
                int remainingRequirement = item.Value;
                int playerInventoryAmount = pInventory.CountItems(item.Key);

                if (playerInventoryAmount >= remainingRequirement)
                {
                    pInventory.RemoveItem(item.Key, remainingRequirement);
                    remainingRequirement = 0;
                }
                else
                {
                    pInventory.RemoveItem(item.Key, playerInventoryAmount);
                    remainingRequirement -= playerInventoryAmount;
                }

                // Consume the remaining requirement from the nearby containers
                foreach (Container c in _nearbyContainers)
                {
                    if (remainingRequirement <= 0) break;

                    Inventory cInventory = c.GetInventory();
                    int containerAmount = cInventory.CountItems(item.Key);

                    if (containerAmount >= remainingRequirement)
                    {
                        cInventory.RemoveItem(item.Key, remainingRequirement);
                        remainingRequirement = 0;
                    }
                    else
                    {
                        cInventory.RemoveItem(item.Key, containerAmount);
                        remainingRequirement -= containerAmount;
                    }
                }
            }

            return false; // Prevent further processing since resources are consumed here
        }
    }

    [HarmonyPatch(typeof(InventoryGui), "OnCraftPressed")]
    static class DoCrafting_Patch
    {
        static bool Prefix(InventoryGui __instance, KeyValuePair<Recipe, ItemDrop.ItemData> ___m_selectedRecipe, ItemDrop.ItemData ___m_craftUpgradeItem)
        {
            if (___m_selectedRecipe.Key == null)
                return true;

            int qualityLevel = (___m_craftUpgradeItem != null) ? (___m_craftUpgradeItem.m_quality + 1) : 1;
            if (qualityLevel > ___m_selectedRecipe.Key.m_item.m_itemData.m_shared.m_maxQuality)
            {
                return true;
            }

            // Ensure this function only pulls resources without consuming them yet
            PullResources(Player.m_localPlayer, ___m_selectedRecipe.Key.m_resources, qualityLevel);
            return true;
        }
    }

    private static void PullResources(Player player, Piece.Requirement[] resources, int qualityLevel)
    {
        Inventory pInventory = player.GetInventory();
        foreach (Piece.Requirement requirement in resources)
        {
            if (requirement.m_resItem)
            {
                string reqName = requirement.m_resItem.m_itemData.m_shared.m_name;
                int totalRequirement = requirement.GetAmount(qualityLevel);
                if (totalRequirement <= 0) continue;

                int totalAmount = pInventory.CountItems(reqName);
                if (totalAmount < totalRequirement)
                {
                    int remainingRequirement = totalRequirement - totalAmount;

                    foreach (Container c in _nearbyContainers)
                    {
                        int containerAmount = c.GetInventory().CountItems(reqName);
                        if (containerAmount >= remainingRequirement)
                        {
                            remainingRequirement = 0;
                            break;
                        }
                        else
                        {
                            remainingRequirement -= containerAmount;
                        }
                    }

                    if (remainingRequirement > 0)
                    {
                        Debug.LogError("Not enough resources to complete the crafting.");
                    }
                }
            }
        }
    }
}

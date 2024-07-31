using HarmonyLib;
using Jotunn.Managers;
using UnityEngine;
using UnityEngine.UI;
using static InventoryGrid;
using static ItemDrop;

public class GuiPatches
{
    [HarmonyPatch(typeof(InventoryGui), "UpdateContainer")]
    public static class InventoryGui_UpdateContainer_Patch
    {
        private static bool Prefix(InventoryGui __instance, Player player)
        {
            if (__instance.m_currentContainer != null && MultipleStorageAccessGui.instance.IsVisible)
            {

                if ((bool)__instance.m_currentContainer && __instance.m_currentContainer.IsOwner())
                {
                    __instance.m_currentContainer.SetInUse(inUse: true);
                    __instance.m_container.gameObject.SetActive(value: false);
                    __instance.m_containerGrid.UpdateInventory(__instance.m_currentContainer.GetInventory(), null, __instance.m_dragItem);
                    __instance.m_containerName.text = Localization.instance.Localize(__instance.m_currentContainer.GetInventory().GetName());
                    if (__instance.m_firstContainerUpdate)
                    {
                        __instance.m_containerGrid.ResetView();
                        __instance.m_firstContainerUpdate = false;
                        __instance.m_containerHoldTime = 0f;
                        __instance.m_containerHoldState = 0;
                    }

                    if (ZInput.GetButton("Use") || ZInput.GetButton("JoyUse"))
                    {
                        __instance.m_containerHoldTime += Time.deltaTime;
                        if (__instance.m_containerHoldTime > __instance.m_containerHoldPlaceStackDelay && __instance.m_containerHoldState == 0)
                        {
                            __instance.m_currentContainer.StackAll();
                            __instance.m_containerHoldState = 1;
                        }
                        else if (__instance.m_containerHoldTime > __instance.m_containerHoldPlaceStackDelay + __instance.m_containerHoldExitDelay && __instance.m_containerHoldState == 1)
                        {
                            __instance.Hide();
                        }
                    }
                    else if (__instance.m_containerHoldState >= 0)
                    {
                        __instance.m_containerHoldState = -1;
                    }
                }
                else
                {
                    __instance.m_container.gameObject.SetActive(value: false);
                    if (__instance.m_dragInventory != null && __instance.m_dragInventory != Player.m_localPlayer.GetInventory())
                    {
                        __instance.SetupDragItem(null, null, 1);
                    }
                }
                return false;
            }
            return true;
        }

        private static void Postfix(InventoryGui __instance)
        {
            if (__instance.m_currentContainer != null && MultipleStorageAccessGui.instance.IsVisible)
            {
                ExtraInventoryButtonsGui.instance.ShowPlayerButtons();
            }
            else if (__instance.m_currentContainer != null)
            {
                ExtraInventoryButtonsGui.instance.ShowContainerButtons();
            }
            else
            {
                ExtraInventoryButtonsGui.instance.ShowPlayerButtons();
            }
        }
    }

    [HarmonyPatch(typeof(InventoryGui), "Hide")]
    public static class CloseContainerPatch
    {
        static void Postfix(InventoryGui __instance)
        {
            MultipleStorageAccessGui.instance.Hide();
        }
    }

    // TODO MOVE OWN FILE ALL BELOW
    [HarmonyPatch(typeof(Container), "OnContainerChanged")]
    public static class Container_OnContainerChanged_Patch
    {
        static void Postfix(Container __instance)
        {
            if (MultipleStorageAccessGui.instance.IsVisible)
            {
                Jotunn.Logger.LogWarning("DID CHANGE!");
                MultipleStorageAccess.instance.UpdatePanel();
            }
        }
    }

    [HarmonyPatch(typeof(InventoryGrid), "OnLeftClick")]
    public static class QuickStorageAccessPatch
    {
        private static bool Prefix(InventoryGrid __instance, UIInputHandler clickHandler)
        {
            if (MultipleStorageAccessGui.instance.IsVisible && __instance.m_inventory == Player.m_localPlayer.GetInventory())
            {
                if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
                {
                    GameObject go = clickHandler.gameObject;
                    Vector2i buttonPos = __instance.GetButtonPos(go);
                    ItemDrop.ItemData itemAt = __instance.m_inventory.GetItemAt(buttonPos.x, buttonPos.y);
                    if (itemAt != null)
                    {
                        MultipleStorageAccess.instance.TryPlaceStackThroughQuickAccess(itemAt, false);
                        return false;
                    }
                }
            }
            return true;
        }
    }


    [HarmonyPatch(typeof(InventoryGui), "OnSelectedItem")]
    public static class OnSelectedPatch
    {
        private static void Postfix(InventoryGui __instance, InventoryGrid grid, ItemDrop.ItemData item, Vector2i pos, InventoryGrid.Modifier mod)
        {
            if (!(bool)__instance.m_dragGo && MultipleStorageAccessGui.instance.IsVisible && __instance.m_currentContainer != null)
            {
                InventoryGui.instance.CloseContainer();
            }
        }
    }
}

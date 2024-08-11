//using HarmonyLib;
//using Jotunn.Configs;
//using Jotunn.Entities;
//using Jotunn.Managers;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using UnityEngine;
//using UnityEngine.EventSystems;
//using static Piece;
//using static Player;

//public class MovePiece : MonoBehaviour
//{
//    public Piece HoveringPiece { get; set; }

//    public Piece ToolPiece { get; set; }

//    public static MovePiece instance;

//    public bool IsPlacing = false;

//    private void Awake()
//    {
//        instance = this;
//        PrefabManager.OnVanillaPrefabsAvailable += Test;
//    }

//    public void Test()
//    {
//        PieceConfig rug = new PieceConfig();
//        rug.Name = "$move_piece_display_name";
//        rug.PieceTable = "Hammer";
//        rug.Category = "Misc";

//        PieceManager.Instance.AddPiece(new CustomPiece("move_piece", "piece_repair", rug));

//        // You want that to run only once, Jotunn has the piece cached for the game session
//        PrefabManager.OnVanillaPrefabsAvailable -= Test;
//    }

//    //[HarmonyPatch(typeof(Player), "SetSelectedPiece")]
//    //public static class Player_UseItem_Patchdawwad
//    //{
//    //    static bool Prefix(Player __instance, Piece p, ref bool __result)
//    //    {
//    //        Jotunn.Logger.LogError("DWADAW");
//    //        if (__instance.m_buildPieces.GetPieceIndex(p, out var index, out var category))
//    //        {
//    //            __instance.SetBuildCategory(category);
//    //            __instance.SetSelectedPiece(index);
//    //            __result = true;
//    //        }

//    //        __result = false;
//    //        return false;
//    //    }
//    //}

//    public void SelectPieceToMove(Player player)
//    {
//        MovePiece.instance.HoveringPiece = player.GetHoveringPiece();
//        if (!MovePiece.instance.HoveringPiece || !player.CheckCanRemovePiece(MovePiece.instance.HoveringPiece) || !PrivateArea.CheckAccess(MovePiece.instance.HoveringPiece.transform.position))
//        {
//            return;
//        }
//        player.m_noPlacementCost = true;
//        player.SetupPlacementGhost();
//    }

//    [HarmonyPatch(typeof(PieceTable), "GetSelectedPrefab")]
//    public static class PieceTable_UseItem_Patch
//    {
//        static bool Prefix(PieceTable __instance, ref GameObject __result)
//        {
//            // Check if the used item is our custom move hammer
//            if (instance.ToolPiece != null && instance.ToolPiece.name == "move_piece" && instance.HoveringPiece != null)
//            {
//                Requirement[] resources = new Requirement[0];
//                instance.HoveringPiece.m_resources = resources;
//                instance.HoveringPiece.m_craftingStation = null;
//                __result = instance.HoveringPiece.gameObject;
//                return false;
//            }

//            // Allow the default behavior for other items
//            return true;
//        }
//    }

//    [HarmonyPatch(typeof(Player), "UpdatePlacement")]
//    public static class PieceTable_UseItem_Patdwadch
//    {
//        static bool Prefix(Player __instance, bool takeInput, float dt)
//        {
//            __instance.UpdateWearNTearHover();
//            if (__instance.InPlaceMode() && !__instance.IsDead())
//            {

//                if (!takeInput)
//                {
//                    return false;
//                }

//                __instance.UpdateBuildGuiInput();
//                if (Hud.IsPieceSelectionVisible())
//                {
//                    return false;
//                }

//                ItemDrop.ItemData rightItem = __instance.GetRightItem();
//                if ((ZInput.GetButton("AltPlace") || ZInput.GetButton("JoyAltKeys")) && (ZInput.GetButtonDown("JoyLStick") || ZInput.GetButtonDown("JoyRStick") || ZInput.GetButtonDown("JoyButtonA") || ZInput.GetButtonDown("JoyButtonB") || ZInput.GetButtonDown("JoyButtonX") || ZInput.GetButtonDown("JoyButtonY") || ZInput.GetButtonDown("JoyDPadUp") || ZInput.GetButtonDown("JoyDPadDown") || ZInput.GetButtonDown("JoyDPadLeft") || ZInput.GetButtonDown("JoyDPadRight")))
//                {
//                    __instance.m_blockRemove = true;
//                }

//                if ((ZInput.GetButtonDown("Remove") || ZInput.GetButtonDown("JoyRemove")) && (ZInput.GetButton("AltPlace") || ZInput.GetButton("JoyAltKeys")) && (ZInput.InputLayout == InputLayout.Default || !ZInput.IsGamepadActive()))
//                {
//                    __instance.CopyPiece();
//                    __instance.m_blockRemove = true;
//                }
//                else if (!__instance.m_blockRemove && (ZInput.GetButtonUp("Remove") || ZInput.GetButtonUp("JoyRemove")))
//                {
//                    __instance.m_removePressedTime = Time.time;
//                }

//                if (!ZInput.GetButton("AltPlace") && !ZInput.GetButton("JoyAltKeys"))
//                {
//                    __instance.m_blockRemove = false;
//                }

//                if (Time.time - __instance.m_removePressedTime < 0.2f && rightItem.m_shared.m_buildPieces.m_canRemovePieces && Time.time - __instance.m_lastToolUseTime > __instance.m_removeDelay)
//                {
//                    __instance.m_removePressedTime = -9999f;
//                    if (__instance.HaveStamina(rightItem.m_shared.m_attack.m_attackStamina))
//                    {
//                        if (__instance.RemovePiece())
//                        {
//                            __instance.m_lastToolUseTime = Time.time;
//                            __instance.AddNoise(50f);
//                            __instance.UseStamina(rightItem.m_shared.m_attack.m_attackStamina, isHomeUsage: true);
//                            if (rightItem.m_shared.m_useDurability)
//                            {
//                                rightItem.m_durability -= rightItem.m_shared.m_useDurabilityDrain;
//                            }
//                        }
//                    }
//                    else
//                    {
//                        Hud.instance.StaminaBarEmptyFlash();
//                    }
//                }

//                if ((ZInput.GetButtonDown("Attack") || ZInput.GetButtonDown("JoyPlace")) && !Hud.InRadial())
//                {
//                    __instance.m_placePressedTime = Time.time;
//                }

//                if (Time.time - __instance.m_placePressedTime < 0.2f && Time.time - __instance.m_lastToolUseTime > __instance.m_placeDelay)
//                {
//                    Jotunn.Logger.LogError("DID START trying");
//                    __instance.m_placePressedTime = -9999f;
//                    if (ZInput.GetButton("JoyAltKeys"))
//                    {
//                        __instance.CopyPiece();
//                        __instance.m_blockRemove = true;
//                    }
//                    else
//                    {
//                        Jotunn.Logger.LogError("DID START trying dwadaw");
//                        Piece selectedPiece = __instance.m_buildPieces.GetSelectedPiece();
//                        if (selectedPiece != null)
//                        {
//                            if (__instance.HaveStamina(rightItem.m_shared.m_attack.m_attackStamina))
//                            {
            
//                                if (selectedPiece.m_repairPiece)
//                                {
//                                    instance.ToolPiece = selectedPiece;
//                                    if (instance.ToolPiece.name == "move_piece")
//                                    {
//                                        if (__instance.m_placementGhost != null)
//                                        {
//                                            //
//                                            Jotunn.Logger.LogError("dwadw dawdwa daw");
//                                            if (__instance.PlacePiece(MovePiece.instance.HoveringPiece))
//                                            {
//                                                __instance.m_noPlacementCost = false;

//                                                ZNetScene.instance.Destroy(MovePiece.instance.HoveringPiece.gameObject);
//                                                UnityEngine.Object.Destroy(__instance.m_placementGhost);

//                                                __instance.m_placementGhost = null;
//                                                MovePiece.instance.HoveringPiece = null;


//                                                //__instance.RemovePiece();
//                                            }

//                                            //
//                                        } else
//                                        {
//                                            Jotunn.Logger.LogError("DID START try move");
//                                            instance.SelectPieceToMove(__instance);
//                                            if (Input.GetAxis("Mouse ScrollWheel") != 0)
//                                            {
//                                                Input.ResetInputAxes();
//                                            }
//                                        }
//                                    } else
//                                    {
//                                        Jotunn.Logger.LogError("DID START try repair");
//                                        instance.ToolPiece = null;
//                                        __instance.Repair(rightItem, selectedPiece);
//                                    }
//                                }
//                                else if (__instance.m_placementGhost != null)
//                                {
//                                    if (__instance.m_noPlacementCost || __instance.HaveRequirements(selectedPiece, RequirementMode.CanBuild))
//                                    {
//                                        if (__instance.PlacePiece(selectedPiece))
//                                        {
//                                            __instance.m_lastToolUseTime = Time.time;
//                                            if (!ZoneSystem.instance.GetGlobalKey(GlobalKeys.NoBuildCost))
//                                            {
//                                                __instance.ConsumeResources(selectedPiece.m_resources, 0);
//                                            }

//                                            __instance.UseStamina(rightItem.m_shared.m_attack.m_attackStamina, isHomeUsage: true);
//                                            if (rightItem.m_shared.m_useDurability)
//                                            {
//                                                rightItem.m_durability -= rightItem.m_shared.m_useDurabilityDrain;
//                                            }
//                                        }
//                                    }
//                                    else
//                                    {
//                                        __instance.Message(MessageHud.MessageType.Center, "$msg_missingrequirement");
//                                    }
//                                }
//                            }
//                            else
//                            {
//                                Hud.instance.StaminaBarEmptyFlash();
//                            }
//                        }
//                    }
//                }

//                if ((bool)__instance.m_placementGhost)
//                {
//                    __instance.m_placementGhost.gameObject.GetComponent<IPieceMarker>()?.ShowBuildMarker();
//                }

//                Piece hoveringPiece = __instance.GetHoveringPiece();
//                if ((bool)hoveringPiece)
//                {
//                    hoveringPiece.gameObject.GetComponent<IPieceMarker>()?.ShowHoverMarker();
//                }

//                if ((bool)__instance.m_placementGhost)
//                {
//                    Piece component = __instance.m_placementGhost.GetComponent<Piece>();
//                    if ((object)component != null && component.m_canRotate && __instance.m_placementGhost.activeInHierarchy)
//                    {
//                        __instance.m_scrollCurrAmount += ZInput.GetMouseScrollWheel();
//                        if (__instance.m_scrollCurrAmount > __instance.m_scrollAmountThreshold)
//                        {
//                            __instance.m_scrollCurrAmount = 0f;
//                            __instance.m_placeRotation++;
//                        }

//                        if (__instance.m_scrollCurrAmount < 0f - __instance.m_scrollAmountThreshold)
//                        {
//                            __instance.m_scrollCurrAmount = 0f;
//                            __instance.m_placeRotation--;
//                        }
//                    }
//                }

//                float num = 0f;
//                bool flag = false;
//                if (ZInput.IsGamepadActive())
//                {
//                    switch (ZInput.InputLayout)
//                    {
//                        case InputLayout.Alternative1:
//                            {
//                                bool button2 = ZInput.GetButton("JoyRotate");
//                                bool button3 = ZInput.GetButton("JoyRotateRight");
//                                flag = button2 || button3;
//                                if (button2)
//                                {
//                                    num = 0.5f;
//                                }
//                                else if (button3)
//                                {
//                                    num = -0.5f;
//                                }

//                                break;
//                            }
//                        case InputLayout.Alternative2:
//                            {
//                                bool num2 = ZInput.GetButtonLastPressedTimer("JoyRotate") < 0.33f && ZInput.GetButtonUp("JoyRotate");
//                                bool button = ZInput.GetButton("JoyRotateRight");
//                                flag = num2 || button;
//                                if (num2)
//                                {
//                                    num = 0.5f;
//                                }
//                                else if (button)
//                                {
//                                    num = -0.5f;
//                                }

//                                break;
//                            }
//                        case InputLayout.Default:
//                            num = ZInput.GetJoyRightStickX();
//                            flag = ZInput.GetButton("JoyRotate") && Mathf.Abs(num) > 0.5f;
//                            break;
//                    }
//                }

//                if (flag)
//                {
//                    if (__instance.m_rotatePieceTimer == 0f)
//                    {
//                        if (num < 0f)
//                        {
//                            __instance.m_placeRotation++;
//                        }
//                        else
//                        {
//                            __instance.m_placeRotation--;
//                        }
//                    }
//                    else if (__instance.m_rotatePieceTimer > 0.25f)
//                    {
//                        if (num < 0f)
//                        {
//                            __instance.m_placeRotation++;
//                        }
//                        else
//                        {
//                            __instance.m_placeRotation--;
//                        }

//                        __instance.m_rotatePieceTimer = 0.17f;
//                    }

//                    __instance.m_rotatePieceTimer += dt;
//                }
//                else
//                {
//                    __instance.m_rotatePieceTimer = 0f;
//                }

//                {
//                    foreach (KeyValuePair<Material, float> item in __instance.m_ghostRippleDistance)
//                    {
//                        item.Key.SetFloat("_RippleDistance", ZInput.GetKey(KeyCode.LeftControl) ? item.Value : 0f);
//                    }

//                    return false;
//                }
//            }

//            if ((bool)__instance.m_placementGhost)
//            {
//                __instance.m_placementGhost.SetActive(value: false);
//            }
//            return false;
//        }
//    }
//    [HarmonyPatch(typeof(Player), "SetRightItem")]
//    public static class Player_SetRightItem_Patch
//    {
//        static void Postfix(Player __instance, ItemDrop.ItemData rightItem)
//        {
//            // Check if the tool is not our custom move hammer

//            Jotunn.Logger.LogError("DWADWDAW AWDDW ADWDWADW");
//            if (rightItem != null && rightItem.m_dropPrefab.name != "move_piece")
//            {
//                // Reset the state
//                if (MovePiece.instance.IsPlacing)
//                {
//                    UnityEngine.Object.Destroy(__instance.m_placementGhost);
//                    __instance.m_placementGhost = null;
//                    MovePiece.instance.IsPlacing = false;
//                    MovePiece.instance.HoveringPiece = null;

//                    Jotunn.Logger.LogError("Reset ghost and state due to tool change");
//                }
//            }
//        }
//    }
//}
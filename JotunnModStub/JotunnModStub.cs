using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using Jotunn.Entities;
using Jotunn.Managers;
using Jotunn.Utils;
using JotunnModStub.ConsoleCommands;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

namespace JotunnModStub
{
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    [BepInDependency(Jotunn.Main.ModGuid)]
    internal class JotunnModStub : BaseUnityPlugin
    {
        private List<Container> _nearbyContainers = new List<Container>();
        private List<ItemDrop> _nearbyItemDrops = new List<ItemDrop>();

        public const string PluginGUID = "com.jotunn.jotunnmodstub";
        public const string PluginName = "JotunnModStub";
        public const string PluginVersion = "0.0.1";

        private const float CheckInterval = 0.2f;
        private float nextCheckTime = 0f;

        private CombineItemsMod _combItems;
        private CraftFromStorageMod _craftFromStorage;
        private SortInventory _sortInventory;
        private QuickStack _quickStack;
        private ItemManager _itemManager;
        private MultipleStorageAccess _multipleStorageAccess;


        //private MovePiece _movePiece;

        //private QuickStorageAccessGui _quickStorageAccess;
        private MultipleStorageAccessGui _multipleStorageAccessGui;
        private ExtraInventoryButtonsGui _extraInventoryButtonsGui;


        private static string assetPath;
        private static string[] baseTemplate;
        private static Dictionary<ItemDrop.ItemData.ItemType, string[]> typeTemplates = new Dictionary<ItemDrop.ItemData.ItemType, string[]>();

        public static CustomLocalization Localization => LocalizationManager.Instance.GetLocalization();

        private void Awake()
        {
            InitializeLogger();
            RegisterCommands();
            InitializeComponents();
            GUIManager.OnCustomGUIAvailable += InitializeGui;

            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), null);
        }

        private void InitializeLogger()
        {
            Jotunn.Logger.LogInfo("ModStub has landed");
        }

        private void RegisterCommands()
        {
            CommandManager.Instance.AddConsoleCommand(new BetterSpawnCommand());
            CommandManager.Instance.AddConsoleCommand(new CheckForItems());
        }

        private void InitializeComponents()
        {
            //_movePiece = gameObject.AddComponent<MovePiece>();

            _combItems = gameObject.AddComponent<CombineItemsMod>();
            _craftFromStorage = gameObject.AddComponent<CraftFromStorageMod>();

            _sortInventory = gameObject.AddComponent<SortInventory>();

            _quickStack = gameObject.AddComponent<QuickStack>();

            _multipleStorageAccess = gameObject.AddComponent<MultipleStorageAccess>();


            _multipleStorageAccessGui = gameObject.AddComponent<MultipleStorageAccessGui>();
            _extraInventoryButtonsGui = gameObject.AddComponent<ExtraInventoryButtonsGui>();
        }

        private void InitializeGui()
        {
            if (GUIManager.Instance == null) { return; }
            if (InventoryGui.instance == null) { return; }

            _multipleStorageAccessGui.Init();
            _extraInventoryButtonsGui.Init();
        }

        private void Update()
        {
            if (Player.m_localPlayer == null)
            {
                return;
            }
            if (Player.m_localPlayer != null && Player.m_localPlayer.IsDead())
            {
                return;
            }

            ThrottleUpdate();
        }

        private void ThrottleUpdate()
        {
            if (Time.time >= nextCheckTime)
            {
                UpdateComponents();
                nextCheckTime = Time.time + CheckInterval;
            }
        }

        private void UpdateComponents()
        {
            Vector3 playerPos = Player.m_localPlayer.transform.position;

            List<Container> nearbyContainers = new List<Container>();
            List<ItemDrop> nearbyItemDrops = new List<ItemDrop>();
            Collider[] hitColliders = Physics.OverlapSphere(playerPos, 24f);

            foreach (var hitCollider in hitColliders)
            {
                Container container = hitCollider.GetComponentInParent<Container>();
                if (container != null && container.GetInventory() != null)
                {
                    // TODOD DO NOT USE OTHER THAN CHESTS
                    nearbyContainers.Add(container);
                }

                ItemDrop itemDrop = hitCollider.GetComponentInParent<ItemDrop>();
                if (itemDrop != null)
                {
                    nearbyItemDrops.Add(itemDrop);
                }
            }

            if (!IsListEqual(_nearbyItemDrops, nearbyItemDrops))
            {
                _nearbyItemDrops = nearbyItemDrops;

                _combItems.UpdateOnItemDropChange(_nearbyItemDrops);
            }
            if (!IsListEqual(_nearbyContainers, nearbyContainers))
            {

                Jotunn.Logger.LogError("Did RuN CONTAINERS");

                _nearbyContainers = nearbyContainers;

                if (!_nearbyContainers.Any())
                {
                    _multipleStorageAccessGui.Hide();
                }

                _craftFromStorage.UpdateOnContainerChange(_nearbyContainers);
                _quickStack.UpdateOnContainerChange(_nearbyContainers);
                _multipleStorageAccess.UpdateOnContainerChange(_nearbyContainers);
            }
        }

        private bool IsListEqual(List<ItemDrop> list1, List<ItemDrop> list2)
        {
            if (list1.Count != list2.Count)
            {
                return false;
            }

            for (int i = 0; i < list1.Count; i++)
            {
                if (!AreItemDropsEqual(list1[i], list2[i]))
                {
                    return false;
                }
            }

            return true;
        }

        private bool AreItemDropsEqual(ItemDrop item1, ItemDrop item2)
        {
            // Adjust the comparison based on relevant properties
            return item1.m_itemData.m_shared.m_name == item2.m_itemData.m_shared.m_name &&
                   item1.m_itemData.m_stack == item2.m_itemData.m_stack;
        }

        private bool IsListEqual(List<Container> list1, List<Container> list2)
        {
            if (list1.Count != list2.Count)
            {
                return false;
            }

            for (int i = 0; i < list1.Count; i++)
            {
                if (list1[i] != list2[i])
                {
                    return false;
                }
            }

            return true;
        }
    }
}

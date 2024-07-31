//using System.Collections.Generic;
//using UnityEngine;
//using UnityEngine.UI;
//using UnityEngine.EventSystems;
//using Jotunn.Entities;
//using Jotunn.Managers;
//using static SortInventory;
//using UnityEngine.UIElements;
//using HarmonyLib;
//using TMPro;
//using static ItemDrop;
//using System.Linq;
//using System;
//using static QuickStorageAccessGui;
//using System.Collections;

//public class CustomSelectable : Selectable, IPointerClickHandler
//{
//    public void OnPointerClick(PointerEventData eventData)
//    {
//        // Handle the click as usual
//        DoStateTransition(SelectionState.Normal, false);

//        // Deselect after click
//        EventSystem.current.SetSelectedGameObject(null);
//    }
//}

//public class ScrollPassThrough : MonoBehaviour, IScrollHandler
//{
//    public void OnScroll(PointerEventData eventData)
//    {
//        var scrollRect = GetComponentInParent<ScrollRect>();
//        if (scrollRect != null)
//        {
//            ExecuteEvents.Execute(scrollRect.gameObject, eventData, ExecuteEvents.scrollHandler);
//        }
//        else
//        {
//            Jotunn.Logger.LogWarning("no scrollRECT");
//        }
//    }
//}

//public class QuickStorageAccessGui : MonoBehaviour
//{
//    public static QuickStorageAccessGui Instance { get; private set; }

//    private GameObject QuickStorageAccessPanel;
//    private GameObject ScrollView;
//    private GameObject SearchInputObject;

//    private Color normalColor = new Color(0.132f, 0.132f, 0.132f, 0.502f);
//    private Color highlightColor = new Color(0.401f, 0.401f, 0.401f, 0.502f);

//    private string SearchString = string.Empty;
//    public bool QuickStorageOpen = false;

//    private float slotSize = 56f;
//    private Coroutine colorChangeCoroutine;

//    public void ToggleQuickStorageOpen()
//    {
//        QuickStorageOpen = !QuickStorageOpen;
//        if (QuickStorageAccessPanel == null)
//        {
//            Jotunn.Logger.LogWarning("QuickStorageAccessPanel is null");
//            return;
//        }
//        QuickStorageAccessPanel.SetActive(QuickStorageOpen);
//        if (QuickStorageOpen)
//        {
//            // UpdatePanel();
//        }
//        else
//        {
//            if (SearchInputObject == null || SearchInputObject.GetComponent<InputField>() == null)
//            {
//                Jotunn.Logger.LogWarning("SearchInputObject is null");
//                return;
//            }
//            SearchInputObject.GetComponent<InputField>().text = string.Empty;
//            SearchString = string.Empty;
//            OnSearchInputDefocused();

//            CanvasGroup containerCanvasGroup = InventoryGui.instance.m_container.GetComponent<CanvasGroup>();
//            if (containerCanvasGroup == null)
//            {
//                containerCanvasGroup = InventoryGui.instance.m_container.gameObject.AddComponent<CanvasGroup>();
//            }
//            containerCanvasGroup.alpha = 1;
//            containerCanvasGroup.blocksRaycasts = true;
//        }
//    }

//    public void CloseQuickStorageOpen()
//    {
//        if (QuickStorageAccessPanel == null || SearchInputObject == null || SearchInputObject.GetComponent<InputField>() == null)
//        {
//            Jotunn.Logger.LogWarning("SearchInputObject or QuickStorageAccessPanel is null");
//            return;
//        }
//        QuickStorageOpen = false;
//        QuickStorageAccessPanel.SetActive(QuickStorageOpen);
//        SearchInputObject.GetComponent<InputField>().text = string.Empty;
//        OnSearchInputDefocused();
//        SearchString = string.Empty;

//        CanvasGroup containerCanvasGroup = InventoryGui.instance.m_container.GetComponent<CanvasGroup>();
//        if (containerCanvasGroup == null)
//        {
//            containerCanvasGroup = InventoryGui.instance.m_container.gameObject.AddComponent<CanvasGroup>();
//        }
//        containerCanvasGroup.alpha = 1;
//        containerCanvasGroup.blocksRaycasts = true;
//    }

//    public void UpdateQuickStorageAccessVisibility()
//    {
//        var inventoryGui = InventoryGui.instance;
//        if (inventoryGui == null)
//        {
//            Jotunn.Logger.LogWarning("InventoryGui instance is not available.");
//            return;
//        }

//        bool containerOpen = inventoryGui.m_currentContainer != null;
//        QuickStorageAccessPanel.SetActive(!containerOpen);
//    }

//    public void MakePanels()
//    {
//        Instance = this;
//        var inventoryGui = InventoryGui.instance;
//        Transform PlayerTransform = inventoryGui.m_player.transform;

//        QuickStorageAccessPanel = GUIManager.Instance.CreateWoodpanel(
//            parent: PlayerTransform,
//            anchorMin: new Vector2(0f, 0f),
//            anchorMax: new Vector2(1f, 0f),
//            position: new Vector2(0f, -275f),
//            height: 450f,
//            draggable: false);
//        QuickStorageAccessPanel.SetActive(false);

//        SearchInputObject = GUIManager.Instance.CreateInputField(
//            parent: QuickStorageAccessPanel.transform,
//            anchorMin: new Vector2(0.5f, 0.5f),
//            anchorMax: new Vector2(0.5f, 0.5f),
//            position: new Vector2(0f, 185f),
//            contentType: InputField.ContentType.Standard,
//            placeholderText: "search...",
//            fontSize: 16,
//            width: 160f,
//            height: 30f
//        );

//        var searchInput = SearchInputObject.GetComponent<InputField>();
//        if (searchInput != null)
//        {
//            // Add event listener for text input
//            searchInput.onValueChanged.AddListener(OnSearchInputChanged);

//            // Add EventTrigger component
//            EventTrigger eventTrigger = SearchInputObject.AddComponent<EventTrigger>();

//            // Create entry for OnSelect event
//            EventTrigger.Entry onSelectEntry = new EventTrigger.Entry();
//            onSelectEntry.eventID = EventTriggerType.Select;
//            onSelectEntry.callback.AddListener((eventData) => { OnSearchInputFocused(); });
//            eventTrigger.triggers.Add(onSelectEntry);

//            // Create entry for OnDeselect event
//            EventTrigger.Entry onDeselectEntry = new EventTrigger.Entry();
//            onDeselectEntry.eventID = EventTriggerType.Deselect;
//            onDeselectEntry.callback.AddListener((eventData) => { OnSearchInputDefocused(); });
//            eventTrigger.triggers.Add(onDeselectEntry);
//        }
//        else
//        {
//            Debug.LogError("Failed to get InputField component from searchInputObject.");
//        }

//        ScrollView = GUIManager.Instance.CreateScrollView(
//            parent: QuickStorageAccessPanel.transform,
//            showHorizontalScrollbar: true,
//            showVerticalScrollbar: true,
//            handleSize: 5f,
//            handleDistanceToBorder: 0f,
//            handleColors: GUIManager.Instance.ValheimScrollbarHandleColorBlock,
//            slidingAreaBackgroundColor: GUIManager.Instance.ValheimBeige,
//            width: 500f,
//            height: 350f
//        );

//        var scrollViewRectTransform = ScrollView.GetComponent<RectTransform>();
//        scrollViewRectTransform.anchorMin = new Vector2(0f, 0f);
//        scrollViewRectTransform.anchorMax = new Vector2(1f, 1f);
//        scrollViewRectTransform.pivot = new Vector2(0.5f, 0.5f);
//        scrollViewRectTransform.offsetMin = new Vector2(0f, 0f); // Set bottom offset
//        scrollViewRectTransform.offsetMax = new Vector2(-35f, -70f);

//        int columns = 8;
//        float padding = 5f;

//        var gridObj = new GameObject("Grid");
//        gridObj.transform.SetParent(ScrollView.transform.Find("Scroll View/Viewport/Content"), true);
//        var gridLayoutGroup = gridObj.AddComponent<GridLayoutGroup>();
//        gridLayoutGroup.cellSize = new Vector2(slotSize, slotSize); // Set the size of each slot
//        gridLayoutGroup.spacing = new Vector2(padding, padding);
//        gridLayoutGroup.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
//        gridLayoutGroup.constraintCount = columns; // Number of columns

//        // DETAILS
//        var containerDetailsObj = new GameObject("ContainerDetails");
//        var containerDetailsRect = containerDetailsObj.AddComponent<RectTransform>();
//        containerDetailsObj.transform.SetParent(QuickStorageAccessPanel.transform, false);

//        containerDetailsRect.anchorMin = new Vector2(0, 1f);
//        containerDetailsRect.anchorMax = new Vector2(0, 1f);
//        containerDetailsRect.pivot = new Vector2(0, 1f);
//        containerDetailsRect.anchoredPosition = new Vector2(20, -12);

//        var layoutGroup = containerDetailsObj.AddComponent<VerticalLayoutGroup>();
//        layoutGroup.childAlignment = TextAnchor.UpperCenter;
//        layoutGroup.childForceExpandHeight = false;
//        layoutGroup.childForceExpandWidth = false;

//        var ContainerCount = new GameObject("ContainerCount");
//        var SlotCount = new GameObject("SlotCount");
//        var ItemCount = new GameObject("ItemCount");

//        SlotCount.transform.SetParent(containerDetailsObj.transform, false);
//        ContainerCount.transform.SetParent(containerDetailsObj.transform, false);
//        ItemCount.transform.SetParent(containerDetailsObj.transform, false);

//        var containerLayoutElement = ContainerCount.AddComponent<LayoutElement>();
//        containerLayoutElement.preferredWidth = 200; // Adjust as needed

//        var slotLayoutElement = SlotCount.AddComponent<LayoutElement>();
//        slotLayoutElement.preferredWidth = 200; // Adjust as needed

//        var itemLayoutElement = ItemCount.AddComponent<LayoutElement>();
//        itemLayoutElement.preferredWidth = 200; // Adjust as needed

//        var ContainerCountText = ContainerCount.AddComponent<Text>();
//        ContainerCountText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
//        ContainerCountText.alignment = TextAnchor.MiddleLeft;
//        ContainerCountText.fontSize = 14; // Adjust the font size as needed
//        ContainerCountText.fontStyle = FontStyle.Bold;
//        ContainerCountText.color = GUIManager.Instance.ValheimOrange; // Set the text color
//        ContainerCountText.raycastTarget = false; // Disable Raycast Target
//        ContainerCountText.text = "Containers: 0";

//        var SlotCountText = SlotCount.AddComponent<Text>();
//        SlotCountText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
//        SlotCountText.alignment = TextAnchor.MiddleLeft;
//        SlotCountText.fontSize = 14; // Adjust the font size as needed
//        SlotCountText.fontStyle = FontStyle.Bold;
//        SlotCountText.color = GUIManager.Instance.ValheimOrange; // Set the text color
//        SlotCountText.raycastTarget = false; // Disable Raycast Target
//        SlotCountText.text = "Slots: 0/0";

//        var ItemCountText = ItemCount.AddComponent<Text>();
//        ItemCountText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
//        ItemCountText.alignment = TextAnchor.MiddleLeft;
//        ItemCountText.fontSize = 14; // Adjust the font size as needed
//        ItemCountText.fontStyle = FontStyle.Bold;
//        ItemCountText.color = GUIManager.Instance.ValheimOrange; // Set the text color
//        ItemCountText.raycastTarget = false; // Disable Raycast Target
//        ItemCountText.text = "Items: 0";
//    }

//    private void OnSearchInputFocused()
//    {
//        // Block player and camera input
//        GUIManager.BlockInput(true);
//    }
//    private void OnSearchInputDefocused()
//    {
//        // Release player and camera input
//        GUIManager.BlockInput(false);
//    }

//    private void OnSearchInputChanged(string searchText)
//    {
//        SearchString = searchText;
//        UpdatePanel();
//        // Handle the search text change
//        Jotunn.Logger.LogWarning("Search input changed: " + SearchString);
//    }

//    private List<QuickStorageItem> SortItems(List<QuickStorageItem> items)
//    {
//        if (string.IsNullOrEmpty(SearchString))
//        {
//            // Sort alphabetically if searchString is empty
//            return items.OrderBy(item => item.ItemData.m_shared.m_name).ToList();
//        }
//        else
//        {
//            // Sort by relevance based on searchString
//            return items
//                .Where(item => item.ItemData.m_shared.m_name.IndexOf(SearchString, StringComparison.OrdinalIgnoreCase) >= 0)
//                .OrderBy(item => item.ItemData.m_shared.m_name)
//                .ToList();
//        }
//    }

//    public void UpdatePanel()
//    {
//        // LogHierarchy(QuickStorageAccessPanel);

//        var gridContentTransform = ScrollView.transform.Find("Scroll View/Viewport/Content/Grid");
//        var ContainerCount = QuickStorageAccessPanel.transform.Find("ContainerDetails/ContainerCount");
//        var SlotCount = QuickStorageAccessPanel.transform.Find("ContainerDetails/SlotCount");
//        var ItemCount = QuickStorageAccessPanel.transform.Find("ContainerDetails/ItemCount");

//        if (gridContentTransform == null || ContainerCount == null || SlotCount == null || ItemCount == null) { return; }

//        List<Transform> list = new List<Transform>();

//        var containers = FindNearbyContainers(Player.m_localPlayer.transform.position);
//        if (containers == null || !containers.Any())
//        {
//            return;
//        }

//        var quickStorageItems = GetNearbyItems(containers);

//        int totalItems = quickStorageItems.Count;
//        var SortedItems = SortItems(quickStorageItems);
//        int SortedCount = SortedItems.Count;

//        var ContainerCountText = ContainerCount.GetComponent<Text>();
//        ContainerCountText.text = $"Containers: {containers.Count}";

//        int totalFilledSlots = 0;
//        int totalEmptySlots = 0;

//        foreach (var container in containers)
//        {
//            var inventory = container.GetInventory();
//            int containerFilledSlots = 0;
//            int containerEmptySlots = 0;

//            foreach (var item in inventory.GetAllItems())
//            {
//                if (item != null && item.m_stack > 0)
//                {
//                    containerFilledSlots++;
//                }
//            }

//            int totalSlots = inventory.GetWidth() * inventory.GetHeight();
//            containerEmptySlots = totalSlots - containerFilledSlots;

//            totalFilledSlots += containerFilledSlots;
//            totalEmptySlots += containerEmptySlots;
//        }

//        var SlotCountText = SlotCount.GetComponent<Text>();
//        SlotCountText.text = $"Slots: {totalFilledSlots}/{totalEmptySlots + totalFilledSlots}";

//        int totalItemsNStacks = 0;
//        foreach (var item in quickStorageItems)
//        {
//            totalItemsNStacks += item.CombinedStackCount;
//        }

//        var ItemCountText = ItemCount.GetComponent<Text>();
//        ItemCountText.text = $"Items: {totalItemsNStacks}";

//        // Example: Using a built-in sprite from Valheim (adjust the sprite name/path as needed)
//        var roundedCornerSprite = GUIManager.Instance.GetSprite("item_background"); // Placeholder for actual sprite name

//        for (int i = SortedCount; i < gridContentTransform.transform.childCount; i++)
//        {
//            Destroy(gridContentTransform.GetChild(i).gameObject);
//        }

//        for (int i = 0; i < SortedCount; i++)
//        {
//            var quickStorageItem = SortedItems[i];

//            if (i < gridContentTransform.transform.childCount)
//            {
//                var child = gridContentTransform.GetChild(i);
//                var count = child.transform.Find("Count");
//                var text = count.GetComponent<Text>();
//                text.text = quickStorageItem.CombinedStackCount.ToString();

//                var icon = child.transform.Find("Icon");
//                var img = icon.GetComponent<UnityEngine.UI.Image>();
//                img.sprite = quickStorageItem.ItemData.GetIcon();

//                var eventTrigger = child.GetComponent<EventTrigger>();
//                ResetEventTriggerListeners(eventTrigger);
//                AddEventTriggerListener(eventTrigger, EventTriggerType.PointerClick, (eventData) => OnItemClick(quickStorageItem, eventData));
//            }
//            else
//            {
//                var slotObject = new GameObject("Slot");
//                slotObject.SetActive(false);

//                var backgroundImage = slotObject.AddComponent<UnityEngine.UI.Image>();
//                backgroundImage.sprite = roundedCornerSprite;

//                var selectable = slotObject.AddComponent<CustomSelectable>();

//                ColorBlock colorBlock = selectable.colors;
//                colorBlock.normalColor = normalColor; // Transparent
//                colorBlock.selectedColor = normalColor;
//                colorBlock.highlightedColor = highlightColor; // Slightly visible on hover
//                colorBlock.pressedColor = GUIManager.Instance.ValheimOrange;
//                selectable.colors = colorBlock;
//                selectable.targetGraphic = backgroundImage;

//                var iconObject = new GameObject("Icon");
//                iconObject.transform.SetParent(slotObject.transform, false);

//                var iconImage = iconObject.AddComponent<UnityEngine.UI.Image>();
//                iconImage.color = Color.white; // Ensure the icon is fully opaque
//                iconImage.rectTransform.sizeDelta = new Vector2(slotSize, slotSize); // Ensure icon size slightly smaller than slot
//                iconImage.sprite = quickStorageItem.ItemData.GetIcon();

//                // Create a Text object for the item count
//                var countObject = new GameObject("Count");
//                countObject.transform.SetParent(slotObject.transform, false);

//                var countText = countObject.AddComponent<Text>();
//                countText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
//                countText.alignment = TextAnchor.MiddleCenter;
//                countText.fontSize = 14; // Adjust the font size as needed
//                countText.fontStyle = FontStyle.Bold;
//                countText.color = Color.white; // Set the text color
//                countText.raycastTarget = false; // Disable Raycast Target
//                countText.text = quickStorageItem.CombinedStackCount.ToString();

//                // Position the count text within the button
//                var countRectTransform = countText.GetComponent<RectTransform>();
//                countRectTransform.anchoredPosition = new Vector2(0f, -20f); // Adjust the Y position as needed

//                slotObject.transform.SetParent(gridContentTransform.transform, false);

//                var eventTrigger = slotObject.AddComponent<EventTrigger>();
//                AddEventTriggerListener(eventTrigger, EventTriggerType.PointerClick, (eventData) => OnItemClick(quickStorageItem, eventData));

//                slotObject.AddComponent<ScrollPassThrough>();
//                slotObject.SetActive(true);
//            }
//        }
//    }

//    private void AddEventTriggerListener(EventTrigger trigger, EventTriggerType eventType, System.Action<BaseEventData> callback)
//    {
//        var entry = new EventTrigger.Entry { eventID = eventType };
//        entry.callback.AddListener(new UnityEngine.Events.UnityAction<BaseEventData>(callback));
//        trigger.triggers.Add(entry);
//    }

//    private void ResetEventTriggerListeners(EventTrigger eventTrigger)
//    {
//        eventTrigger.triggers.Clear();
//    }

//    private List<QuickStorageItem> GetNearbyItems(List<Container> containers)
//    {
//        Dictionary<string, QuickStorageItem> itemCounts = new Dictionary<string, QuickStorageItem>();

//        foreach (Container container in containers)
//        {
//            var inventory = container.GetInventory();
//            foreach (var item in inventory.GetAllItems())
//            {
//                if (itemCounts.ContainsKey(item.m_shared.m_name))
//                {
//                    itemCounts[item.m_shared.m_name].CombinedStackCount += item.m_stack;
//                    itemCounts[item.m_shared.m_name].ItemContainerPairs.Add(new ItemContainerPair(item, container));
//                }
//                else
//                {
//                    var quickStorageItem = new QuickStorageItem(item, item.m_stack);
//                    quickStorageItem.ItemContainerPairs.Add(new ItemContainerPair(item, container));
//                    itemCounts[item.m_shared.m_name] = quickStorageItem;
//                }
//            }
//        }

//        List<QuickStorageItem> combinedItemList = new List<QuickStorageItem>(itemCounts.Values);
//        return combinedItemList;
//    }

//    private static List<Container> FindNearbyContainers(Vector3 position)
//    {
//        List<Container> containers = new List<Container>();
//        Collider[] hitColliders = Physics.OverlapSphere(position, 24f);

//        foreach (var hitCollider in hitColliders)
//        {
//            Container container = hitCollider.GetComponentInParent<Container>();
//            if (container != null && container.GetInventory() != null)
//            {
//                containers.Add(container);
//            }
//        }

//        return containers;
//    }

//    private void OnItemClick(QuickStorageItem item, BaseEventData eventData)
//    {
//        if (InventoryGui.instance.m_dragItem != null)
//        {
//            if (InventoryGui.instance.m_dragInventory != Player.m_localPlayer.GetInventory())
//            {
//                InventoryGui.instance.SetupDragItem(null, null, 1);
//                return;
//            }
//            TryPlaceStackThroughQuickAccess(InventoryGui.instance.m_dragItem, true);
//            return;
//        }

//        var pointerEventData = eventData as PointerEventData;

//        // Logic to add item(s) to the player's inventory
//        Debug.Log("Item clicked: " + item.ItemData.m_shared.m_name);

//        // Assuming you have access to the player's inventory:
//        var playerInventory = Player.m_localPlayer.GetInventory();

//        if (playerInventory != null && item.CombinedStackCount > 0)
//        {
//            if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
//            {
//                // Control key is pressed, take the entire stack
//                AddStackToInventory(item, playerInventory);
//            }
//            else if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
//            {
//                int splitAmount = item.CombinedStackCount < item.ItemData.m_shared.m_maxStackSize ? item.CombinedStackCount : item.ItemData.m_shared.m_maxStackSize;
//                Container suitableContainer = FindContainerWithMaxStacks(item, splitAmount);

//                if (suitableContainer != null)
//                {
//                    suitableContainer = AggregateItemsToContainer(item, suitableContainer, splitAmount);

//                    InventoryGui.instance.m_currentContainer = suitableContainer;

//                    CanvasGroup containerCanvasGroup = InventoryGui.instance.m_container.GetComponent<CanvasGroup>();
//                    if (containerCanvasGroup == null)
//                    {
//                        containerCanvasGroup = InventoryGui.instance.m_container.gameObject.AddComponent<CanvasGroup>();
//                    }
//                    containerCanvasGroup.alpha = 0;
//                    containerCanvasGroup.blocksRaycasts = false;

//                    InventoryGui.instance.ShowSplitDialog(item.ItemData, suitableContainer.GetInventory());
//                }
//            }
//            else
//            {
//                int splitAmount = item.CombinedStackCount < item.ItemData.m_shared.m_maxStackSize ? item.CombinedStackCount : item.ItemData.m_shared.m_maxStackSize;
//                Container suitableContainer = FindContainerWithMaxStacks(item, splitAmount);

//                if (suitableContainer != null)
//                {
//                    suitableContainer = AggregateItemsToContainer(item, suitableContainer, splitAmount);

//                    InventoryGui.instance.m_currentContainer = suitableContainer;

//                    CanvasGroup containerCanvasGroup = InventoryGui.instance.m_container.GetComponent<CanvasGroup>();
//                    if (containerCanvasGroup == null)
//                    {
//                        containerCanvasGroup = InventoryGui.instance.m_container.gameObject.AddComponent<CanvasGroup>();
//                    }
//                    containerCanvasGroup.alpha = 0;
//                    containerCanvasGroup.blocksRaycasts = false;

//                    InventoryGui.instance.SetupDragItem(item.ItemData, suitableContainer.GetInventory(), item.ItemData.m_stack);
//                }
//            }
//        }
//    }

//    private Container FindContainerWithMaxStacks(QuickStorageItem quickStorageItem, int requiredAmount)
//    {
//        Container maxContainer = null;
//        int maxStack = 0;

//        foreach (var pair in quickStorageItem.ItemContainerPairs)
//        {
//            if (pair.Item.m_stack > maxStack)
//            {
//                maxStack = pair.Item.m_stack;
//                maxContainer = pair.Container;
//            }
//        }

//        return maxContainer;
//    }

//    private Container AggregateItemsToContainer(QuickStorageItem quickStorageItem, Container targetContainer, int requiredAmount)
//    {
//        int remainingAmount = requiredAmount - quickStorageItem.ItemContainerPairs
//            .FirstOrDefault(pair => pair.Container == targetContainer)?.Item.m_stack ?? 0;

//        foreach (var pair in quickStorageItem.ItemContainerPairs)
//        {
//            if (pair.Container != targetContainer && remainingAmount > 0)
//            {
//                int itemsToMove = Mathf.Min(pair.Item.m_stack, remainingAmount);
//                remainingAmount -= itemsToMove;

//                // Clone the item and adjust its stack size
//                var clonedItem = pair.Item.Clone();
//                clonedItem.m_stack = itemsToMove;

//                // Add cloned item to the target container
//                targetContainer.GetInventory().AddItem(clonedItem);
//                pair.Item.m_stack -= itemsToMove;
//                if (pair.Item.m_stack <= 0)
//                {
//                    pair.Container.GetInventory().RemoveItem(pair.Item);
//                }

//                // Save the source container state
//                pair.Container.Save();
//                pair.Container.GetInventory().Changed();
//            }

//            if (remainingAmount <= 0)
//            {
//                break;
//            }
//        }

//        // Save the target container state
//        targetContainer.Save();
//        targetContainer.GetInventory().Changed();

//        return targetContainer;
//    }

//    private void AddStackToInventory(QuickStorageItem item, Inventory playerInventory)
//    {
//        if (!playerInventory.CanAddItem(item.ItemData))
//        {
//            return;
//        }

//        int stackSize = item.ItemData.m_shared.m_maxStackSize;
//        int remainingStackSize = stackSize;

//        foreach (var pair in item.ItemContainerPairs)
//        {
//            if (remainingStackSize <= 0)
//                break;

//            var containerInventory = pair.Container.GetInventory();
//            var containerItem = containerInventory.GetItem(pair.Item.m_shared.m_name);

//            if (containerItem != null)
//            {
//                int itemsToTake = Mathf.Min(containerItem.m_stack, remainingStackSize);
//                remainingStackSize -= itemsToTake;
//                containerItem.m_stack -= itemsToTake;
//                item.CombinedStackCount -= itemsToTake;

//                if (containerItem.m_stack <= 0)
//                {
//                    containerInventory.RemoveItem(containerItem);
//                    pair.Item.m_stack = 0;  // Clear stack in pair
//                }
//            }
//        }

//        // Add the items to the player's inventory
//        var itemsToAdd = item.ItemData.Clone();
//        itemsToAdd.m_stack = stackSize - remainingStackSize;
//        playerInventory.AddItem(itemsToAdd);

//        // Update the UI to reflect the new count
//        Jotunn.Logger.LogWarning("Stack taken: " + itemsToAdd.m_shared.m_name + " (" + itemsToAdd.m_stack + ")");
//    }

//    private void IndicateFull()
//    {
//        var SlotCount = QuickStorageAccessPanel.transform.Find("ContainerDetails/SlotCount");
//        var SlotCountText = SlotCount.GetComponent<Text>();

//        if (colorChangeCoroutine != null)
//        {
//            StopCoroutine(colorChangeCoroutine);
//        }
//        colorChangeCoroutine = StartCoroutine(ChangeTextColorForASecond(SlotCountText, Color.red, 0.5f));
//    }

//    private IEnumerator ChangeTextColorForASecond(Text textElement, Color color, float duration)
//    {
//        textElement.color = color;
//        yield return new WaitForSeconds(duration);

//        // Check if the color is still red before resetting it to original
//        if (textElement.color == color)
//        {
//            textElement.color = GUIManager.Instance.ValheimOrange;
//        }

//        colorChangeCoroutine = null;
//    }

//    [HarmonyPatch(typeof(InventoryGrid), "OnLeftClick")]
//    public static class QuickStorageAccessPatch
//    {
//        private static bool Prefix(InventoryGrid __instance, UIInputHandler clickHandler)
//        {
//            Jotunn.Logger.LogWarning("was 1");
//            if (__instance.m_inventory != Player.m_localPlayer.GetInventory() || !Instance.QuickStorageOpen)
//            {
//                Jotunn.Logger.LogWarning("was 2");
//                return true;
//            }
//            // Check if control key is pressed and the left mouse button is clicked
//            if ((Input.GetKey(KeyCode.LeftControl) && Input.GetMouseButtonDown(0) || Input.GetKey(KeyCode.RightControl)) && Input.GetMouseButtonDown(0))
//            {
//                Jotunn.Logger.LogWarning("was 3");
//                GameObject go = clickHandler.gameObject;
//                Vector2i buttonPos = __instance.GetButtonPos(go);
//                ItemDrop.ItemData itemAt = __instance.m_inventory.GetItemAt(buttonPos.x, buttonPos.y);
//                if (itemAt != null)
//                {
//                    Instance.TryPlaceStackThroughQuickAccess(itemAt, false);
//                    return false;
//                }
//            }

//            return true; // Proceed with the original method
//        }
//    }

//    public void TryPlaceStackThroughQuickAccess(ItemDrop.ItemData itemData, bool fromDrag)
//    {
//        var playerInventory = Player.m_localPlayer.GetInventory();
//        var containers = FindNearbyContainers(Player.m_localPlayer.transform.position);
//        int remainingStack = itemData.m_stack;

//        // Try to add to existing stacks first
//        foreach (var container in containers)
//        {
//            var containerInventory = container.GetInventory();
//            foreach (var containerItem in containerInventory.GetAllItems())
//            {
//                if (containerItem.m_shared.m_name == itemData.m_shared.m_name && containerItem.m_stack < containerItem.m_shared.m_maxStackSize)
//                {
//                    int stackSpace = containerItem.m_shared.m_maxStackSize - containerItem.m_stack;
//                    int addAmount = Mathf.Min(stackSpace, remainingStack);
//                    containerItem.m_stack += addAmount;
//                    remainingStack -= addAmount;
//                    if (remainingStack <= 0)
//                        break;
//                }
//            }
//            if (remainingStack <= 0)
//                break;
//        }

//        // Add to empty slots in containers that already have the item
//        if (remainingStack > 0)
//        {
//            foreach (var container in containers)
//            {
//                var containerInventory = container.GetInventory();
//                foreach (var containerItem in containerInventory.GetAllItems())
//                {
//                    if (containerItem.m_shared.m_name == itemData.m_shared.m_name)
//                    {
//                        var itemClone = itemData.Clone();
//                        itemClone.m_stack = Mathf.Min(itemClone.m_stack, remainingStack);
//                        if (containerInventory.CanAddItem(itemClone))
//                        {
//                            containerInventory.AddItem(itemClone);
//                            remainingStack -= itemClone.m_stack;
//                        }
//                    }
//                    if (remainingStack <= 0)
//                        break;
//                }
//                if (remainingStack <= 0)
//                    break;
//            }
//        }

//        // Add to any empty slots in any container
//        if (remainingStack > 0)
//        {
//            foreach (var container in containers)
//            {
//                var containerInventory = container.GetInventory();
//                var itemClone = itemData.Clone();
//                itemClone.m_stack = remainingStack;
//                if (containerInventory.CanAddItem(itemClone))
//                {
//                    containerInventory.AddItem(itemClone);
//                    remainingStack = 0;
//                    break;
//                }
//            }
//        }

//        // Remove the item from the player's inventory
//        if (remainingStack < itemData.m_stack)
//        {
//            itemData.m_stack = remainingStack; // Update the stack size of the clicked item
//            if (itemData.m_stack <= 0)
//            {
//                playerInventory.RemoveItem(itemData);
//                if (fromDrag)
//                {
//                    InventoryGui.instance.SetupDragItem(null, null, 1);
//                }
//            }
//            else
//            {
//                QuickStorageAccessGui.Instance.IndicateFull();
//                if (fromDrag)
//                {
//                    InventoryGui.instance.m_dragAmount = itemData.m_stack;
//                }
//            }
//            Jotunn.Logger.LogWarning("Item moved to containers: " + itemData.m_shared.m_name);
//        }
//        else
//        {
//            Jotunn.Logger.LogWarning("No space available in nearby containers for item: " + itemData.m_shared.m_name);

//            QuickStorageAccessGui.Instance.IndicateFull();
//        }
//    }

//    public class QuickStorageItem
//    {
//        public ItemDrop.ItemData ItemData { get; set; }
//        public int CombinedStackCount { get; set; }
//        public List<ItemContainerPair> ItemContainerPairs { get; set; }

//        public QuickStorageItem(ItemDrop.ItemData itemData, int combinedStackCount)
//        {
//            ItemData = itemData;
//            CombinedStackCount = combinedStackCount;
//            ItemContainerPairs = new List<ItemContainerPair>();
//        }
//    }

//    public class ItemContainerPair
//    {
//        public ItemDrop.ItemData Item { get; set; }
//        public Container Container { get; set; }

//        public ItemContainerPair(ItemDrop.ItemData item, Container container)
//        {
//            Item = item;
//            Container = container;
//        }
//    }

//    [HarmonyPatch(typeof(Container), "OnContainerChanged")]
//    public static class Container_OnContainerChanged_Patch
//    {
//        static void Postfix(Container __instance)
//        {
//            if (InventoryGui.instance.m_currentContainer != null && Instance.QuickStorageOpen)
//            {
//                CanvasGroup containerCanvasGroup = InventoryGui.instance.m_container.GetComponent<CanvasGroup>();
//                if (containerCanvasGroup == null)
//                {
//                    containerCanvasGroup = InventoryGui.instance.m_container.gameObject.AddComponent<CanvasGroup>();
//                }
//                containerCanvasGroup.alpha = 1;
//                containerCanvasGroup.blocksRaycasts = true;
//                InventoryGui.instance.CloseContainer();
//            }

//            if (Instance != null)
//            {
//                Instance.UpdatePanel();
//            }
//        }
//    }

//    private void LogHierarchy(GameObject go, string indent = "")
//    {
//        Debug.Log($"{indent}{go.name}");
//        foreach (Transform child in go.transform)
//        {
//            LogHierarchy(child.gameObject, indent + "  ");
//        }
//    }
//}

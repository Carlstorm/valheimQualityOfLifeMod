using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Jotunn.Managers;
using System.Collections.Generic;
using System;
using System.Linq;
using static Switch;
using static UnityEngine.EventSystems.EventTrigger;

public class CustomInputFieldHandler : MonoBehaviour, ISelectHandler, IDeselectHandler
{
    private InputField inputField;

    void Start()
    {
        inputField = GetComponent<InputField>();

        var nav = inputField.navigation;
        nav.mode = Navigation.Mode.None;
        inputField.navigation = nav;
    }

    public void OnSelect(BaseEventData eventData)
    {
        GUIManager.BlockInput(true);
    }

    public void OnDeselect(BaseEventData eventData)
    {
        GUIManager.BlockInput(false);
    }

    void Update()
    {
        if (inputField.isFocused)
        {
            if (Input.GetKeyDown(KeyCode.Tab) || Input.GetKeyDown(KeyCode.Escape))
            {
                // Allow Tab key to close the interface
                GUIManager.BlockInput(false);
                EventSystem.current.SetSelectedGameObject(null);
                InventoryGui.instance.Hide(); // Custom method to close the interface
            }
        } else
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                // Allow Tab key to close the interface
                GUIManager.BlockInput(false);
                EventSystem.current.SetSelectedGameObject(null);
                InventoryGui.instance.Hide(); // Custom method to close the interface
            }
        }
    }
}

public class MultipleStorageAccessGui : MonoBehaviour
{
    private GameObject Grid;
    private GameObject QuickStorageAccessPanel;
    private GameObject SearchInputObject;
    private GameObject ScrollView;

    private Dropdown CatagoryDropdown;

    private CustomUITooltip TEstToll;

    private RectTransform ScrollViewRectTransform;

    private const float slotSize = 60f; // Define slot size if not defined elsewhere

    public bool IsVisible;

    public Text ContainerCountText;
    public Text SlotCountText;
    public Text ItemCountText;

    private Sprite SlotBg;

    public static MultipleStorageAccessGui instance { get; private set; }

    public class CustomSelectable : Selectable, IPointerClickHandler
    {
        public void OnPointerClick(PointerEventData eventData)
        {
            // Handle the click as usual
            DoStateTransition(SelectionState.Normal, false);

            // Deselect after click
            EventSystem.current.SetSelectedGameObject(null);
        }
    }

    public class ScrollPassThrough : MonoBehaviour, IScrollHandler
    {
        public void OnScroll(PointerEventData eventData)
        {
            var scrollRect = GetComponentInParent<ScrollRect>();
            if (scrollRect != null)
            {
                ExecuteEvents.Execute(scrollRect.gameObject, eventData, ExecuteEvents.scrollHandler);
            }
            else
            {
                Jotunn.Logger.LogWarning("no scrollRECT");
            }
        }
    }

    public void Awake()
    {
        instance = this;
    }

    public void Init()
    {
        SlotBg = GUIManager.Instance.GetSprite("item_background");
        var inventoryGui = InventoryGui.instance;
        Transform playerTransform = inventoryGui.m_player.transform;

        QuickStorageAccessPanel = CreatePanel(playerTransform);
        SearchInputObject = CreateSearchInputField();
        CatagoryDropdown = CreateSelectField();
        ScrollView = CreateScrollView();
        CreateResetButton();

        CreateGridLayout();
        CreateContainerDetails();
    }

    public void Toggle() {
        if (IsVisible) { 
            Hide();
        }
        else
        {
             Show();
        }
    }

    public void Show()
    {
        IsVisible = true;
        QuickStorageAccessPanel.SetActive(IsVisible);
        MultipleStorageAccess.instance.UpdatePanel();
    }
    public void Hide()
    {
        IsVisible = false;
        QuickStorageAccessPanel.SetActive(IsVisible);
        ResetFilters();
    }

    private GameObject CreatePanel(Transform parent)
    {
        var panel = GUIManager.Instance.CreateWoodpanel(
            parent: parent,
            anchorMin: new Vector2(0f, 0f),
            anchorMax: new Vector2(1f, 0f),
            position: new Vector2(0f, -265f),
            height: 450f,
            draggable: false
        );
        panel.SetActive(IsVisible);

        TEstToll = panel.AddComponent<CustomUITooltip>();
        TEstToll.m_tooltipPrefab = PrefabManager.Cache.GetPrefab<GameObject>("InventoryTooltip");
        TEstToll.Set("test", "mamam");

        return panel;
    }

    private Dropdown CreateSelectField()
    {
        var dropdownObject = GUIManager.Instance.CreateDropDown(
            parent: QuickStorageAccessPanel.transform,
            anchorMin: new Vector2(0.5f, 0.5f),
            anchorMax: new Vector2(0.5f, 0.5f),
            position: new Vector2(160f, 185f),
            fontSize: 14,
            width: 160f,
            height: 30f
        );

        var dropdown = dropdownObject.GetComponent<Dropdown>();
        List<string> options = System.Enum.GetNames(typeof(ItemCategory)).ToList();
        options.Insert(0, "All");

        if (dropdown != null)
        {
            dropdown.onValueChanged.AddListener(OnSelectChanged);
            dropdown.AddOptions(options);
        }

        return dropdown;
    }

    private GameObject CreateResetButton()
    {
        var ResetButtonObj = GUIManager.Instance.CreateButton(
            text: string.Empty,
            parent: QuickStorageAccessPanel.transform,
            anchorMin: new Vector2(0.5f, 0.5f),
            anchorMax: new Vector2(0.5f, 0.5f),
            position: new Vector2(255f, 185f),
            width: 20f,
            height: 20f
        );

        // Get the button component
        Button button = ResetButtonObj.GetComponent<Button>();

        // Add an Image component to the button if it doesn't already have one
        Image image = ResetButtonObj.GetComponent<Image>();
        if (image == null)
        {
            image = ResetButtonObj.AddComponent<Image>();
        }

        // Set the image sprite
        var sprite = GUIManager.Instance.GetSprite("refresh_icon");
        image.sprite = sprite;

        image.color = GUIManager.Instance.ValheimOrange; // Adjust the color as needed

        button.onClick.AddListener(ResetFilters);

        return ResetButtonObj;
    }

    private GameObject CreateSearchInputField()
    {
        var inputField = GUIManager.Instance.CreateInputField(
            parent: QuickStorageAccessPanel.transform,
            anchorMin: new Vector2(0.5f, 0.5f),
            anchorMax: new Vector2(0.5f, 0.5f),
            position: new Vector2(0f, 185f),
            contentType: InputField.ContentType.Standard,
            placeholderText: "search...",
            fontSize: 16,
            width: 150f,
            height: 30f
        );

        var searchInput = inputField.GetComponent<InputField>();
        if (searchInput != null)
        {
            inputField.gameObject.AddComponent<CustomInputFieldHandler>();
            //searchInput.gameObject.AddComponent<NonTabbableInputField>();
            searchInput.onValueChanged.AddListener(OnSearchInputChanged);
            //AddEventTrigger(inputField, EventTriggerType.Select, OnSearchInputFocused);
            //AddEventTrigger(inputField, EventTriggerType.Deselect, OnSearchInputDefocused);
        }
        else
        {
            Debug.LogError("Failed to get InputField component from searchInputObject.");
        }
        return inputField;
    }

    private void AddEventTrigger(GameObject target, EventTriggerType eventType, UnityEngine.Events.UnityAction<BaseEventData> action)
    {
        EventTrigger eventTrigger = target.GetComponent<EventTrigger>() ?? target.AddComponent<EventTrigger>();
        EventTrigger.Entry entry = new EventTrigger.Entry { eventID = eventType };
        entry.callback.AddListener(action);
        eventTrigger.triggers.Add(entry);
    }

    private GameObject CreateScrollView()
    {
        var scrollView = GUIManager.Instance.CreateScrollView(
            parent: QuickStorageAccessPanel.transform,
            showHorizontalScrollbar: true,
            showVerticalScrollbar: true,
            handleSize: 5f,
            handleDistanceToBorder: 0f,
            handleColors: GUIManager.Instance.ValheimScrollbarHandleColorBlock,
            slidingAreaBackgroundColor: GUIManager.Instance.ValheimBeige,
            width: 530f,
            height: 350f
        );

        var scrollRect = scrollView.GetComponent<RectTransform>();
        scrollRect.anchorMin = new Vector2(0f, 0f);
        scrollRect.anchorMax = new Vector2(1f, 1f);
        scrollRect.pivot = new Vector2(0.5f, 0.5f);
        scrollRect.offsetMin = new Vector2(0f, 0f); // Set bottom offset
        scrollRect.offsetMax = new Vector2(-20f, -70f);

        AddEventTrigger(scrollView, EventTriggerType.PointerClick, (eventtrigger) => MultipleStorageAccess.instance.OnContainerClick());
        return scrollView;
    }

    private void CreateGridLayout()
    {
        Grid = new GameObject("Grid");
        Grid.transform.SetParent(ScrollView.transform.Find("Scroll View/Viewport/Content"), false);
        var gridLayoutGroup = Grid.AddComponent<GridLayoutGroup>();
        gridLayoutGroup.cellSize = new Vector2(slotSize, slotSize); // Set the size of each slot
        gridLayoutGroup.spacing = new Vector2(5f, 5f);
        gridLayoutGroup.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        gridLayoutGroup.constraintCount = 8; // Number of columns

        ScrollViewRectTransform = gridLayoutGroup.GetComponent<RectTransform>();
    }

    private void CreateContainerDetails()
    {
        var containerDetailsObj = CreateDetailObject("ContainerDetails", new Vector2(20, -12));
        var layoutGroup = containerDetailsObj.AddComponent<VerticalLayoutGroup>();
        layoutGroup.childAlignment = TextAnchor.UpperCenter;
        layoutGroup.childForceExpandHeight = false;
        layoutGroup.childForceExpandWidth = false;

        var rectTransform = containerDetailsObj.GetComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(200, rectTransform.sizeDelta.y); // Set the desired width here, e.g., 200 units.

        ContainerCountText = CreateDetailText(containerDetailsObj.transform, "Containers: 0", "ContainerCount");
        SlotCountText = CreateDetailText(containerDetailsObj.transform, "Slots: 0 / 0", "SlotCount");
        ItemCountText = CreateDetailText(containerDetailsObj.transform, "Items: 0", "ItemCount");
    }

    private GameObject CreateDetailObject(string name, Vector2 position)
    {
        var obj = new GameObject(name);
        var rectTransform = obj.AddComponent<RectTransform>();
        obj.transform.SetParent(QuickStorageAccessPanel.transform, false);
        rectTransform.anchorMin = new Vector2(0, 1f);
        rectTransform.anchorMax = new Vector2(0, 1f);
        rectTransform.pivot = new Vector2(0, 1f);
        rectTransform.anchoredPosition = position;
        return obj;
    }

    private UnityEngine.UI.Text CreateDetailText(Transform parent, string text, string name)
    {
        var detailObj = new GameObject(name);
        detailObj.transform.SetParent(parent, false);

        var layoutElement = detailObj.AddComponent<LayoutElement>();
        layoutElement.preferredWidth = 200; // Adjust as needed

        var textComponent = detailObj.AddComponent<UnityEngine.UI.Text>();
        textComponent.font = GUIManager.Instance.AveriaSerif;
        textComponent.alignment = TextAnchor.MiddleLeft;
        textComponent.fontSize = 14; // Adjust the font size as needed
        textComponent.fontStyle = FontStyle.Bold;
        textComponent.color = GUIManager.Instance.ValheimOrange; // Set the text color
        textComponent.raycastTarget = false; // Disable Raycast Target
        textComponent.text = text;

        var detailObjtOutline = detailObj.AddComponent<UnityEngine.UI.Outline>();
        detailObjtOutline.effectColor = Color.black; // Set the outline color
        detailObjtOutline.effectDistance = new Vector2(1, -1); // Adjust the outline distance as needed

        return textComponent;
    }

    public void SetCategoryCounts(Dictionary<ItemCategory, int> categoryCounts)
    {
        int totalCount = categoryCounts.Values.Sum();

        // Update the dropdown options
        CatagoryDropdown.options[0].text = $"All ({totalCount})";

        for (int i = 1; i < CatagoryDropdown.options.Count; i++)
        {
            var category = (ItemCategory)(i - 1);
            int count = categoryCounts.ContainsKey(category) ? categoryCounts[category] : 0;
            CatagoryDropdown.options[i].text = $"{category} ({count})";
        }

        CatagoryDropdown.RefreshShownValue();
    }

    public void SetContainerDetialsCounts(int containerCount, int emptySlotCount, int filledSlotCount, int totalItemsCount)
    {
        var totalSlots = emptySlotCount + filledSlotCount;
        ContainerCountText.text = $"Containers: {containerCount}";
        SlotCountText.text = $"Slots: {filledSlotCount} / {totalSlots}";
        ItemCountText.text = $"Items: {totalItemsCount}";
    }

    public void SetItems(List<QuickStorageItem> items)
    {
        var itemCount = items.Count;
        var slotCount = Grid.transform.childCount;

        int maxCount = Math.Max(itemCount, slotCount);

        for (int i = 0; i < maxCount; i++)
        {
            if (i < itemCount)
            {
                var quickStorageItem = items[i];

                if (i < slotCount)
                {
                    var slot = Grid.transform.GetChild(i);

                    var count = slot.transform.Find("Count");
                    var text = count.GetComponent<UnityEngine.UI.Text>();
                    text.text = quickStorageItem.CombinedStackCount.ToString();

                    var icon = slot.transform.Find("Icon");
                    var img = icon.GetComponent<UnityEngine.UI.Image>();
                    img.sprite = quickStorageItem.ItemData.GetIcon();

                    var eventTrigger = slot.GetComponent<EventTrigger>();

                    AddEventTriggerListener2(quickStorageItem, slot.gameObject);

                    SetItemDetails(slot, quickStorageItem);

                    slot.gameObject.SetActive(true);
                } else
                {
                    var slotObject = new GameObject("Slot");
                    slotObject.SetActive(false);

                    var backgroundImage = slotObject.AddComponent<UnityEngine.UI.Image>();
                    // Assuming SlotBg is defined
                    backgroundImage.sprite = SlotBg;

                    var selectable = slotObject.AddComponent<CustomSelectable>();

                    ColorBlock colorBlock = selectable.colors;
                    colorBlock.normalColor = ColorSettings.ContainerSlotDefaultNormal;
                    colorBlock.selectedColor = ColorSettings.ContainerSlotDefaultNormal;
                    colorBlock.highlightedColor = ColorSettings.ContainerSlotHighligtNormal;
                    colorBlock.pressedColor = GUIManager.Instance.ValheimOrange;

                    selectable.colors = colorBlock;
                    selectable.targetGraphic = backgroundImage;

                    var iconObject = new GameObject("Icon");
                    iconObject.transform.SetParent(slotObject.transform, false);

                    var iconImage = iconObject.AddComponent<UnityEngine.UI.Image>();
                    iconImage.color = Color.white; // Ensure the icon is fully opaque
                    iconImage.rectTransform.sizeDelta = new Vector2(slotSize, slotSize); // Ensure icon size slightly smaller than slot
                                                                                         // Assuming quickStorageItem is defined and ItemData.GetIcon() returns a sprite
                    iconImage.sprite = quickStorageItem.ItemData.GetIcon();

                    var countObject = new GameObject("Count");
                    countObject.transform.SetParent(slotObject.transform, false);

                    var countText = countObject.AddComponent<UnityEngine.UI.Text>();
                    countText.font = GUIManager.Instance.AveriaSerifBold;
                    countText.alignment = TextAnchor.MiddleCenter;
                    countText.fontSize = 14; // Adjust the font size as needed
                    countText.color = Color.white; // Set the text color
                    countText.raycastTarget = false; // Disable Raycast Target
                    countText.text = quickStorageItem.CombinedStackCount.ToString();

                    var countOutline = countObject.AddComponent<UnityEngine.UI.Outline>();
                    countOutline.effectColor = Color.black; // Set the outline color
                    countOutline.effectDistance = new Vector2(1, -1); // Adjust the outline distance as needed

                    // Position the count text within the button
                    var countRectTransform = countText.GetComponent<RectTransform>();
                    countRectTransform.anchoredPosition = new Vector2(0f, -22f); // Adjust the Y position as needed

                    var QualityObj = new GameObject("Quality");
                    QualityObj.transform.SetParent(slotObject.transform, false);
                    var QualityText = QualityObj.AddComponent<UnityEngine.UI.Text>();
                    QualityText.font = GUIManager.Instance.AveriaSerifBold;
                    QualityText.alignment = TextAnchor.MiddleRight;
                    QualityText.fontSize = 14; // Adjust the font size as needed
                    QualityText.color = GUIManager.Instance.ValheimOrange; // Set the text color
                    QualityText.raycastTarget = false; // Disable Raycast Target

                    var QualityOutline = QualityObj.AddComponent<UnityEngine.UI.Outline>();
                    QualityOutline.effectColor = Color.black; // Set the outline color
                    QualityOutline.effectDistance = new Vector2(1, -1); // Adjust the outline distance as needed

                    // Position the count text within the button
                    var QualityRectTransform = QualityText.GetComponent<RectTransform>();
                    QualityRectTransform.anchoredPosition = new Vector2(-24f, 22f); // Adjust the Y position as needed

                    var DetailObj1 = new GameObject("Detail 1");
                    DetailObj1.transform.SetParent(slotObject.transform, false);
                    var DetailText1 = DetailObj1.AddComponent<UnityEngine.UI.Text>();
                    DetailText1.font = GUIManager.Instance.AveriaSerifBold;
                    DetailText1.alignment = TextAnchor.MiddleLeft;
                    DetailText1.fontSize = 12; // Adjust the font size as needed
                    DetailText1.color = GUIManager.Instance.ValheimOrange; // Set the text color
                    DetailText1.raycastTarget = false; // Disable Raycast Target
                    DetailText1.text = string.Empty;

                    var DetailOutline1 = DetailObj1.AddComponent<UnityEngine.UI.Outline>();
                    DetailOutline1.effectColor = Color.black; // Set the outline color
                    DetailOutline1.effectDistance = new Vector2(1, -1); // Adjust the outline distance as needed

                    // Position the count text within the button
                    var DetailRectTransform1 = DetailText1.GetComponent<RectTransform>();
                    DetailRectTransform1.anchoredPosition = new Vector2(24f, 22f); // Adjust the Y position as needed

                    var DetailObj2 = new GameObject("Detail 2");
                    DetailObj2.transform.SetParent(slotObject.transform, false);
                    var DetailText2 = DetailObj2.AddComponent<UnityEngine.UI.Text>();
                    DetailText2.font = GUIManager.Instance.AveriaSerifBold;
                    DetailText2.alignment = TextAnchor.MiddleLeft;
                    DetailText2.fontSize = 12; // Adjust the font size as needed
                    DetailText2.raycastTarget = false; // Disable Raycast Target
                    DetailText2.text = string.Empty;

                    var DetailOutline2 = DetailObj2.AddComponent<UnityEngine.UI.Outline>();
                    DetailOutline2.effectColor = Color.black; // Set the outline color
                    DetailOutline2.effectDistance = new Vector2(1, -1); // Adjust the outline distance as needed

                    // Position the count text within the button
                    var DetailRectTransform2 = DetailText2.GetComponent<RectTransform>();
                    DetailRectTransform2.anchoredPosition = new Vector2(24f, 10f); // Adjust the Y position as needed

                    var DetailObj3 = new GameObject("Detail 3");
                    DetailObj3.transform.SetParent(slotObject.transform, false);
                    var DetailText3 = DetailObj3.AddComponent<UnityEngine.UI.Text>();
                    DetailText3.font = GUIManager.Instance.AveriaSerifBold;
                    DetailText3.alignment = TextAnchor.MiddleLeft;
                    DetailText3.fontSize = 12; // Adjust the font size as needed
                    DetailText3.raycastTarget = false; // Disable Raycast Target
                    DetailText3.text = string.Empty;

                    var DetailOutline3 = DetailObj3.AddComponent<UnityEngine.UI.Outline>();
                    DetailOutline3.effectColor = Color.black; // Set the outline color
                    DetailOutline3.effectDistance = new Vector2(1, -1); // Adjust the outline distance as needed

                    // Position the count text within the button
                    var DetailRectTransform3 = DetailText3.GetComponent<RectTransform>();
                    DetailRectTransform3.anchoredPosition = new Vector2(24f, -2f); // Adjust the Y position as needed

                    slotObject.transform.SetParent(Grid.transform, true);

                    AddEventTriggerListener2(quickStorageItem, slotObject);

                    slotObject.AddComponent<ScrollPassThrough>();

                    SetItemDetails(slotObject.transform, quickStorageItem);

                    slotObject.SetActive(true); 
                }
            } else
            {
                Grid.transform.GetChild(i).gameObject.SetActive(false);
            }
        }
    }

    private void SetItemDetails(Transform slotObject, QuickStorageItem quickStorageItem)
    {
        var quality = slotObject.Find("Quality");
        quality.GetComponent<Text>().text = quickStorageItem.ItemData.m_shared.m_maxQuality > 1 ? quickStorageItem.ItemData.m_quality.ToString() : string.Empty;

        if (quickStorageItem.ItemData.m_shared.m_food+quickStorageItem.ItemData.m_shared.m_foodStamina+quickStorageItem.ItemData.m_shared.m_foodEitr > 3)
        {
            var detail1 = slotObject.Find("Detail 1");
            detail1.GetComponent<Text>().text = quickStorageItem.ItemData.m_shared.m_food.ToString();
            detail1.GetComponent<Text>().color = ColorSettings.HealthColor;

            var detail2 = slotObject.Find("Detail 2");
            detail2.GetComponent<Text>().text = quickStorageItem.ItemData.m_shared.m_foodStamina.ToString();
            detail2.GetComponent<Text>().color = ColorSettings.StaminaColor;

            var detail3 = slotObject.Find("Detail 3");
            detail3.GetComponent<Text>().text = quickStorageItem.ItemData.m_shared.m_foodEitr > 0 ? quickStorageItem.ItemData.m_shared.m_foodEitr.ToString() : string.Empty;
            detail3.GetComponent<Text>().color = ColorSettings.EitrColor;
        } else
        {
            var durabilityVal = quickStorageItem.ItemData.GetDurabilityPercentage();
            var detail1 = slotObject.Find("Detail 1");
            detail1.GetComponent<Text>().text = durabilityVal == 1 ? string.Empty : $"{Math.Floor(durabilityVal * 100)}%";
            detail1.GetComponent<Text>().color = ColorSettings.DurabilityColor;

            var detail2 = slotObject.Find("Detail 2");
            detail2.GetComponent<Text>().text = string.Empty;

            var detail3 = slotObject.Find("Detail 3");
            detail3.GetComponent<Text>().text = string.Empty;
        }
    }

    private void AddEventTriggerListener2(QuickStorageItem item, GameObject targetObj)
    {
        var eventTrigger = targetObj.GetComponent<EventTrigger>();
        if (eventTrigger == null)
        {
            eventTrigger = targetObj.AddComponent<EventTrigger>();
        } else
        {
            eventTrigger.triggers.Clear();
        }

        var entryClick = new EventTrigger.Entry { eventID = EventTriggerType.PointerClick };
        var entryEnter = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
        var entryExit = new EventTrigger.Entry { eventID = EventTriggerType.PointerExit };

        entryClick.callback.AddListener((eventData) => MultipleStorageAccess.instance.OnItemClick(item, eventData));
        entryEnter.callback.AddListener((eventData) => TEstToll.OnSlotHoverEnter(targetObj, item.ItemData.m_shared.m_name, item.ItemData.GetTooltip()));
        entryExit.callback.AddListener((eventData) => TEstToll.OnSlotHoverExit());

        eventTrigger.triggers.Add(entryClick);
        eventTrigger.triggers.Add(entryEnter);
        eventTrigger.triggers.Add(entryExit);
    }

    //private void AddEventTriggerListener(EventTrigger trigger, EventTriggerType eventType, System.Action<BaseEventData> callback)
    //{
    //    trigger.triggers.Clear();
    //    var entry = new EventTrigger.Entry { eventID = eventType };
    //    entry.callback.AddListener(new UnityEngine.Events.UnityAction<BaseEventData>(callback));

    //    trigger.triggers.Add(entry);
    //}

    public void ResetCountColor()
    {
        SlotCountText.color = GUIManager.Instance.ValheimOrange;
    }

    public void CountColorFull()
    {
        SlotCountText.color = Color.red;
    }

    private void ResetFilters()
    {
        MultipleStorageAccess.instance.category = null;
        MultipleStorageAccess.instance.searchTerm = string.Empty;
        var searchInput = SearchInputObject.GetComponent<InputField>();
        searchInput.text = string.Empty;
        CatagoryDropdown.value = 0;
    }

    private void OnSearchInputChanged(string searchText)
    {
        MultipleStorageAccess.instance.searchTerm = searchText;
        MultipleStorageAccess.instance.UpdatePanel();
    }

    private void OnSelectChanged(int selectIndex) {
        if (selectIndex == 0)
        {
            MultipleStorageAccess.instance.category = null;
        }
        else
        {
            ItemCategory selectedCategory = (ItemCategory)(selectIndex - 1);
            MultipleStorageAccess.instance.category = selectedCategory;
            Jotunn.Logger.LogError($"Selected: {selectedCategory}");
        }
        MultipleStorageAccess.instance.UpdatePanel();
    }

    //private void OnSearchInputFocused(BaseEventData eventData)
    //{
    //    //GUIManager.BlockInput(true);
    //}

    //private void OnSearchInputDefocused(BaseEventData eventData)
    //{
    //    //GUIManager.BlockInput(false);
    //}
}

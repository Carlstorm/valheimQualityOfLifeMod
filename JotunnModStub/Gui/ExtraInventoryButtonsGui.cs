using UnityEngine;
using UnityEngine.UI;
using Jotunn.Managers;

public class ExtraInventoryButtonsGui : MonoBehaviour
{
    private GameObject PlayerButtons;
    private GameObject ContainerButtons;

    private GameObject MultipleAccessButton;
    private GameObject QuickStackButton;

    public static ExtraInventoryButtonsGui instance { get; private set; }

    public void Awake()
    {
        instance = this;
    }

    public void Init()
    {
        var inventoryGui = InventoryGui.instance;
        if (inventoryGui == null)
        {
            Jotunn.Logger.LogWarning("InventoryGui instance is not available.");
            return;
        }

        Transform playerTransform = inventoryGui.m_player.transform;
        Transform containerTransform = inventoryGui.m_container.transform;

        if (playerTransform == null || containerTransform == null)
        {
            Jotunn.Logger.LogWarning("Could not find the specified parent transform in the InventoryGui.");
            return;
        }

        Jotunn.Logger.LogWarning("InventoryGui instance is not available. FAK");

        // Create parent objects for grouping buttons
        PlayerButtons = new GameObject("PlayerButtons");
        PlayerButtons.transform.SetParent(playerTransform, false);
        var playerLayout = PlayerButtons.AddComponent<HorizontalLayoutGroup>();
        playerLayout.childAlignment = TextAnchor.MiddleCenter;
        playerLayout.spacing = 5f;
        playerLayout.padding = new RectOffset(0, 10, 10, 10);
        playerLayout.childControlWidth = false;
        playerLayout.childControlHeight = false;
        playerLayout.childForceExpandWidth = false;
        playerLayout.childForceExpandHeight = false;

        // Set position right under the playerTransform
        RectTransform playerButtonsRect = PlayerButtons.GetComponent<RectTransform>();
        playerButtonsRect.anchorMin = new Vector2(0f, 0f);
        playerButtonsRect.anchorMax = new Vector2(0f, 0f);
        playerButtonsRect.pivot = new Vector2(0f, 0f);
        playerButtonsRect.anchoredPosition = new Vector2(-5f, -75f);

        // Create parent objects for grouping buttons
        ContainerButtons = new GameObject("ContainerButtons");
        ContainerButtons.transform.SetParent(containerTransform, false);
        var ContainerLayout = ContainerButtons.AddComponent<HorizontalLayoutGroup>();
        ContainerLayout.childAlignment = TextAnchor.MiddleCenter;
        ContainerLayout.spacing = 5f;
        ContainerLayout.padding = new RectOffset(0, 10, 10, 10);
        ContainerLayout.childControlWidth = false;
        ContainerLayout.childControlHeight = false;
        ContainerLayout.childForceExpandWidth = false;
        ContainerLayout.childForceExpandHeight = false;

        RectTransform containerButtonsRect = ContainerButtons.GetComponent<RectTransform>();
        containerButtonsRect.anchorMin = new Vector2(0f, 0f);
        containerButtonsRect.anchorMax = new Vector2(0f, 0f);
        containerButtonsRect.pivot = new Vector2(0f, 0f);
        containerButtonsRect.anchoredPosition = new Vector2(-10f, -75f);


        CreateButton("Sort", 70, PlayerButtons.transform, SortInventory.instance.SortPlayerInventory);
        QuickStackButton = CreateButton("Quick Stack", 135, PlayerButtons.transform, QuickStack.instance.DoStack);
        MultipleAccessButton = CreateButton("Containers (0)", 150, PlayerButtons.transform, MultipleStorageAccessGui.instance.Toggle);
        MultipleAccessButton.SetActive(false);
        QuickStackButton.SetActive(false);
        CreateButton("Sort", 60, ContainerButtons.transform, SortInventory.instance.SortContainerInventory);
    }

    public void HideMultipleAccessButton()
    {
        MultipleAccessButton.transform.gameObject.SetActive(false);
        QuickStackButton.transform.gameObject.SetActive(false);
    }

    public void ShowMultipleAccessButton(int containerCount)
    {
        Text buttonText = MultipleAccessButton.GetComponentInChildren<Text>();
        buttonText.text = $"Containers ({containerCount})";
        MultipleAccessButton.gameObject.SetActive(true);
        QuickStackButton.gameObject.SetActive(true);
    }

    private GameObject CreateButton(string text, float width, Transform parent, UnityEngine.Events.UnityAction action)
    {
        GameObject button = GUIManager.Instance.CreateButton(
            text: text,
            parent: parent,
            anchorMin: new Vector2(0f, 1f),
            anchorMax: new Vector2(0f, 1f),
            position: Vector2.zero,
            height: 30f,
            width: width
        );
        Button buttonComponent = button.GetComponent<Button>();
        buttonComponent.onClick.AddListener(action);
        return button;
    }

    public void ShowContainerButtons()
    {
        if (PlayerButtons != null) PlayerButtons.SetActive(false);
        if (ContainerButtons != null) ContainerButtons.SetActive(true);
    }

    public void ShowPlayerButtons()
    {
        if (PlayerButtons != null) PlayerButtons.SetActive(true);
        if (ContainerButtons != null) ContainerButtons.SetActive(false);
    }
}

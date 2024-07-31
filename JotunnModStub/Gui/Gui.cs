//using HarmonyLib;
//using Jotunn.Managers;
//using UnityEngine;
//using UnityEngine.UI;

//public class Gui : MonoBehaviour
//{
//    public static Gui Instance { get; private set; }

//    private SortInventory sortInventory;
//    private QuickStack quickStack;

//    public ExtraInventoryButtonsGui extraInventoryButtonsGui;
//    public MultipleStorageAccessGui multipleStorageAccessGui;

//    public Gui()
//    {
//        Instance = this;
//        multipleStorageAccessGui = new MultipleStorageAccessGui();
//        extraInventoryButtonsGui = new ExtraInventoryButtonsGui();
//    }

//    public void Init(SortInventory sortInventory, QuickStack quickStack, MultipleStorageAccess multipleStorageAccess)
//    {
//        this.sortInventory = sortInventory;
//        this.quickStack = quickStack;

//        extraInventoryButtonsGui.Init(sortInventory, quickStack);
//        multipleStorageAccessGui.Init();
//    }
//}

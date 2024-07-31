using Jotunn.Entities;
using Jotunn.Managers;
using System.Collections.Generic;
using UnityEngine;


namespace JotunnModStub.ConsoleCommands
{
    public class CheckForItems : ConsoleCommand
    {
        public override string Name => "stack_items";

        public override string Help => "Stacks items";

        public override void Run(string[] args)
        {
            var player = Player.m_localPlayer;
            if (player == null)
            {
                Console.instance.Print("Player not found");
                return;
            }

            float radius = 50f; // Radius to search for items
            Dictionary<string, ItemDrop> itemDictionary = new Dictionary<string, ItemDrop>();

            foreach (var item in UnityEngine.Object.FindObjectsOfType<ItemDrop>())
            {
                if (Vector3.Distance(player.transform.position, item.transform.position) <= radius)
                {
                    string itemName = item.m_itemData.m_shared.m_name;

                    if (itemDictionary.ContainsKey(itemName))
                    {
                        itemDictionary[itemName].m_itemData.m_stack += item.m_itemData.m_stack;
                        itemDictionary[itemName].Save();
                        Object.Destroy(item.gameObject);
                    }
                    else
                    {
                        itemDictionary[itemName] = item;
                    }
                }
            }

            Console.instance.Print($"Combined items within {radius} meters.");
            foreach (var kvp in itemDictionary)
            {
                Console.instance.Print($"{kvp.Key} now has a stack of {kvp.Value.m_itemData.m_stack}");
            }
        }

        public override List<string> CommandOptionList()
        {
            // No options needed for this command, so return an empty list
            return new List<string>();
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Object/Loot Table")]
public class LootTable : ScriptableObject
{
    public List<ItemRoll> items = new List<ItemRoll>();

    // Will roll the drops based on the items list and return a list of type Item with the rolled drops.
    public List<Item> RollDrops() {
        List<Item> drops = new List<Item>();
        foreach(ItemRoll item in items) { // Go through each ItemRoll in the items list
            if(Random.Range(1, 101) < item.weight) { // Calcualte from 1 to 100 the weight of the item. If you roll successfully, continue
                for(int i = 0; i < item.amount; i++) {
                    drops.Add(item.item); // Add the ItemRoll's item X times to the drops list, based on the ItemRoll's amount
                }
            }
        }
        if (drops.Count <= 0) return null; // If nothing was rolled, return null
        return drops; // Return the list of drops
    }
}

[System.Serializable]
public class ItemRoll
{
    public Item item;
    public int amount = 1;
    [Range(0, 100)] public int weight = 100;

    public ItemRoll(Item _item, int _amount, int _weight) {
        this.item = _item;
        this.amount = _amount;
        this.weight = _weight;
    }
}


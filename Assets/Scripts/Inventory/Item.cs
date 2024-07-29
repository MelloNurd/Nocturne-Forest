using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Object/Item")]
public class Item : ScriptableObject
{
    public Sprite image; 
    public ItemAction usage = ItemAction.Ingredient;
    //public Vector2Int range = new Vector2Int(5, 4);
    public int maxStackSize = 1;
    public int marketPrice = 10;
    public int useAmount; // This is a generic int that we can use for things like healing amounts
    public bool deleteOnUse = true;
}

public enum ItemAction
{
    Ingredient,
    Heal,
    Key
}
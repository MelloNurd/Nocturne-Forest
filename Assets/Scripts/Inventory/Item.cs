using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(menuName = "Scriptable Object/Item")]
public class Item : ScriptableObject
{
    public Sprite image; 
    public ItemType type;
    //public Vector2Int range = new Vector2Int(5, 4);
    public int maxStackSize = 1;
    public int marketPrice;
   
}

public enum ItemType
{
    Ingredient,
    Utility
}

// This will be better implemented as an interface
//public enum ActionType
//{
//    Dig,
//    Grab
//}
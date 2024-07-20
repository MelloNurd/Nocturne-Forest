using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(menuName = "Scriptable Object/Item")]
public class Item : ScriptableObject
{
    [Header("Only gameplay")]
    public TileBase Tile;
    public ItemType type;
    public ActionType actionType;
    //public Vector2Int range = new Vector2Int(5, 4);
    //public int maxStackSize = 1;

    [Header("Only UI")]
    public bool stackable = true;

    [Header("Both")]
    public Sprite image;
}

public enum ItemType
{
    Ingredient,
    Utility
}

public enum ActionType
{
    Dig,
    Grab
}
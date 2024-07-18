using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(menuName = "Scriptable Object/Item")]
public class Item : ScriptableObject
{
    public TileBase Tile;
    public Sprite image;
    public ItemType type;
    public ActionType actionType;
    public Vector2Int range = new Vector2Int(5, 4);

    //[Header("Only gameplay")]

    //[Header("Only UI")]

    //[Header("Both")]

}

public enum ItemType
{
    Ingredient,
    Tool
}

public enum ActionType
{
    Dig,
    Mine
}
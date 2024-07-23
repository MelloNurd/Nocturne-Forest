using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Object/Recipe")]
public class Recipe : ScriptableObject
{
    public Item craftedItem = null;
    public Workstation craftingWorkstation = Workstation.Cauldron;
    public List<Item> craftingIngredients = new List<Item>();
}

public enum Workstation {
    Cauldron
}
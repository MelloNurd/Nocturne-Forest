using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Object/Lore")]
public class Lore : ScriptableObject
{
    public string loreText;
    public bool showLoreInBook;
}

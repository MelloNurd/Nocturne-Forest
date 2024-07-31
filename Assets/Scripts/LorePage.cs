using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LorePage : Interactable {

    public Lore lorePage;

    protected override void Awake() {
        base.Awake();
    }

    // Start is called before the first frame update
    protected override void Start() {
        base.Start();

        if (lorePage == null) Debug.LogError("No lore page assigned to page: " + gameObject.name);
    }

    // Update is called once per frame
    protected override void Update() {
        base.Update();
    }

    public override void Interact() {
        if (!canInteract) return;
        PlayerPrefs.SetInt("LorePage_" + lorePage.name, 1);
        //InventoryManager.currentInstance.globalCraftingRecipes[index].showRecipeInBook = true;
        gameObject.SetActive(false);
    }
}

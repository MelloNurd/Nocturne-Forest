using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShopChest : Interactable
{
    protected override void Awake() 
    {
        base.Awake();
    }

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
    }

    public override void Interact() {
        if (!canInteract) return;

        InventoryManager.currentInstance.ToggleInventory(InventoryManager.InventoryOpening.ShopInventory, gameObject);
    }
}

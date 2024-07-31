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

    protected override void OnDisable() {
        base.OnDisable();
    }

    protected override void OnDestroy() {
        base.OnDestroy();
    }

    public override void Interact() {
        if (!canInteract) return;

        if (interactSound != null) player.PlaySound(interactSound, 1, Random.Range(0.85f, 1.15f));
        InventoryManager.currentInstance.ToggleInventory(InventoryManager.InventoryOpening.ShopInventory, gameObject);
    }
}

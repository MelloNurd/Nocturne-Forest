using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Activator : Interactable
{
    [Header("Activator")]
    public Item keyItem = null; // This will be the item needed to "activate" this obj. If it is null, it will "activate" with any item.

    public UnityEvent onSuccessfulInteract;

    protected override void Awake() {
        base.Awake();
    }

    // Start is called before the first frame update
    protected override void Start() {
        base.Start();
    }

    // Update is called once per frame
    protected override void Update() {
        base.Update();
    }

    public override void Interact() {
        if (keyItem != null) {
            Item heldItem = InventoryManager.currentInstance.GetSelectedItem(false);
            if (heldItem == null || heldItem != keyItem) return;
            if(heldItem.deleteOnUse) InventoryManager.currentInstance.GetSelectedItem(true);
        }

        Debug.Log("Successfully activated " + gameObject.name);
        onSuccessfulInteract?.Invoke();
    }
}

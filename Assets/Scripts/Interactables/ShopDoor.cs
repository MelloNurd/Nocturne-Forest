using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShopDoor : Interactable
{
    [SerializeField] Shop shop;

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

    protected override void OnDisable() {
        base.OnDisable();
    }

    protected override void OnDestroy() {
        base.OnDestroy();
    }

    public override void Interact() {
        if (!canInteract) return;

        InventoryManager.currentInstance.ToggleInventory(InventoryManager.InventoryOpening.DoorMenu, gameObject);
    }

    public void OpenCloseShop() {
        if (shop.isShopOpen) CloseShop();
        else OpenShop();
    }

    public void OpenShop() {
        InventoryManager.currentInstance.openCloseShopText.text = "Close  Shop";
        shop.isShopOpen = true;
    }

    public void CloseShop() {
        InventoryManager.currentInstance.openCloseShopText.text = "Open  Shop";
        shop.isShopOpen = false;
    }
}

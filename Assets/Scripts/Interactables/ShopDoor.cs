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

    public override void Interact() {
        InventoryManager.currentInstance.ToggleInventory(InventoryManager.InventoryOpening.DoorMenu, gameObject);
    }

    public void OpenCloseShop() {
        if (shop.isShopOpen) CloseShop();
        else OpenShop();
    }

    void OpenShop() {
        shop.isShopOpen = true;
    }

    void CloseShop() {
        shop.isShopOpen = false;
    }
}

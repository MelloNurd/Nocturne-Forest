using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShopCheckout : Interactable
{
    public Customer customer;

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
        customer.OnCheckout();
        PlayerPrefs.SetInt("total_items_sold", PlayerPrefs.GetInt("total_items_sold", 0) + 1);
    }
}

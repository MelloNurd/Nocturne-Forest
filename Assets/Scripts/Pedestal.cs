using System.Collections;
using System.Collections.Generic;
using System.Threading;
using TMPro;
using UnityEngine;

public class Pedestal : MonoBehaviour
{
    [SerializeField] GameObject pedestalUI;
    GameObject player;
    SpriteRenderer childSpriteRenderer;

    public static Pedestal openedPedestal;

    InventorySlot pedestalUISlot;
    TMP_InputField pedestalPriceField;

    public Item sellItem;
    public int sellPrice;
    public int count;

    public void OpenPedestal() {
        if (openedPedestal == this) { // When you try to open the pedestal you already have opened
            ClosePedestal();
            return;
        }

        pedestalUI.SetActive(true);

        openedPedestal = this;
    }

    public void ClosePedestal() {
        pedestalUI.SetActive(false);
        openedPedestal = null;
    }

    public void UpdatePedestalItem() {
        StartCoroutine(SetItem()); // I don't know why, but this is running too early for the item to be updated in the hiearchy, so we are waiting 0.1 seconds.
    }

    IEnumerator SetItem() {
        yield return new WaitForSeconds(0.1f);
        InventoryItem item = pedestalUISlot.GetItemInSlot();
        if (item == null) {
            Debug.LogError("Unable to set item for pedestal: " + gameObject.name);
        }
        else {
            sellItem = item.item;
            count = item.count;
            childSpriteRenderer.sprite = item.item.image;
        }
    }

    public void UpdatePedestalPrice() {
        sellPrice = int.Parse(pedestalPriceField.text);
    }

    private void Awake() {
        childSpriteRenderer = transform.Find("Item").GetComponent<SpriteRenderer>();

        player = GameObject.Find("Player");

        pedestalUISlot = GameObject.Find("Pedestal Item Slot").GetComponent<InventorySlot>();
        pedestalPriceField = GameObject.Find("Price Input Field").GetComponent<TMP_InputField>();
    }

    private void Start() {
        pedestalUI.SetActive(false);
    }

    public void Update() {
        if (openedPedestal == this && Vector2.Distance(player.transform.position, transform.position) > 3)
        {
            ClosePedestal();
        }
    }
}

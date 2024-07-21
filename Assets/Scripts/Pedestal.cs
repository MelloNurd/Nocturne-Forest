using System.Collections;
using System.Collections.Generic;
using System.Threading;
using TMPro;
using Unity.VisualScripting;
using UnityEditor.U2D.Aseprite;
using UnityEngine;
using static UnityEditor.Progress;

public class Pedestal : MonoBehaviour
{
    GameObject player;
    SpriteRenderer childSpriteRenderer;

    public static Pedestal openedPedestal;

    [SerializeField] GameObject pedestalUI;
    [SerializeField] GameObject inventorySlotPrefab;
    GameObject pedestalSlotObj;
    InventorySlot pedestalUISlot;
    TMP_InputField pedestalPriceField;
    Vector3 pedestalSlotPos;

    public Item sellItem;
    public int sellPrice;
    public int count;

    public void OpenPedestal() {
        if (openedPedestal == this) { // When you try to open the pedestal you already have opened
            ClosePedestal();
            return;
        }
        else if(openedPedestal != null) {
            openedPedestal.ClosePedestal();
        }

        pedestalUI.SetActive(true);
        pedestalSlotObj.SetActive(true);
        UpdatePedestalUI();
        openedPedestal = this;
    }

    public void ClosePedestal() {
        pedestalUI.SetActive(false);
        pedestalSlotObj.SetActive(false);
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
            childSpriteRenderer.enabled = true;
        }
    }

    public void UpdatePedestalPrice() {
        sellPrice = int.Parse(pedestalPriceField.text);
    }

    public void ClearItem() {
        sellItem = null;
        count = 0;
        childSpriteRenderer.sprite = null;
        childSpriteRenderer.enabled = false;
    }

    void UpdatePedestalUI() {
        pedestalPriceField.text = sellPrice.ToString();
    }

    private void Awake() {
        childSpriteRenderer = transform.Find("Item").GetComponent<SpriteRenderer>();
        if (sellItem == null) childSpriteRenderer.enabled = false;

        player = GameObject.Find("Player");

        // This is really ugly
        pedestalSlotObj = Instantiate(inventorySlotPrefab, pedestalUI.transform.GetChild(0).Find("SlotPosition").position, Quaternion.identity);
        pedestalSlotObj.name = gameObject.name + " Slot";
        pedestalSlotObj.transform.SetParent(pedestalUI.transform.GetChild(0));
        pedestalSlotObj.GetComponent<RectTransform>().sizeDelta = new Vector2(60, 60);
        pedestalSlotObj.SetActive(false);

        pedestalPriceField = pedestalUI.transform.GetChild(0).GetChild(2).GetComponent<TMP_InputField>();

        pedestalUISlot = pedestalSlotObj.GetComponent<InventorySlot>();
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

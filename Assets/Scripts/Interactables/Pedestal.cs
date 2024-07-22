using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using TMPro;
using Unity.VisualScripting;
using UnityEditor.U2D.Aseprite;
using UnityEngine;
using static UnityEditor.Progress;

public class Pedestal : Interactable
{
    SpriteRenderer childSpriteRenderer;

    public static Pedestal openedPedestal;

    Shop shop;

    [SerializeField] GameObject pedestalUI;
    [SerializeField] GameObject inventorySlotPrefab;
    GameObject pedestalSlotObj;
    InventorySlot pedestalUISlot;
    TMP_InputField pedestalPriceField;
    Vector3 pedestalSlotPos;

    public Item sellItem;
    public int sellPrice = 1;
    public int count;

    public override void Interact() {
        if (!canInteract) return;
        OpenPedestal();
    }

    public void OpenPedestal() {
        if (openedPedestal == this) { // When you try to open the pedestal you already have opened
            ClosePedestal();
            return;
        }
        else if(openedPedestal != null) {
            openedPedestal.ClosePedestal();
        }

        openedPedestal = this;
        pedestalUI.SetActive(true);
        pedestalSlotObj.SetActive(true);
        UpdatePedestalUI();
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

        if (sellItem != null) shop.RemoveItem(this); // This happens when swapping the item
        
        InventoryItem item = pedestalUISlot.GetItemInSlot();
        if (item == null) {
            Debug.LogError("Unable to set item for pedestal: " + gameObject.name);
        }
        else {
            sellItem = item.item;
            count = item.count;
            childSpriteRenderer.sprite = item.item.image;
            shop.AddItem(this);
        }
    }

    public void UpdatePedestalPrice() {
        openedPedestal.sellPrice = int.Parse(pedestalPriceField.text);
        shop.UpdateItem(this);
    }

    public void ClearItem() {
        shop.RemoveItem(this);
        sellItem = null;
        count = 0;
        childSpriteRenderer.sprite = null;
    }

    void UpdatePedestalUI() {
        pedestalPriceField.text = sellPrice.ToString();
    }

    protected override void Awake() {
        base.Awake();

        childSpriteRenderer = transform.Find("Item").GetComponent<SpriteRenderer>();

        childSpriteRenderer.transform.DOLocalMove(Vector2.up, 0.8f).SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo).Goto(Random.Range(0, 0.8f), true);

        if (sellItem == null) childSpriteRenderer.sprite = null;

        // This is really ugly
        pedestalSlotObj = Instantiate(inventorySlotPrefab, pedestalUI.transform.GetChild(0).Find("SlotPosition").position, Quaternion.identity);
        pedestalSlotObj.name = gameObject.name + " Slot";
        pedestalSlotObj.transform.SetParent(pedestalUI.transform.GetChild(0));
        pedestalSlotObj.GetComponent<RectTransform>().sizeDelta = new Vector2(60, 60);

        pedestalUISlot = pedestalSlotObj.GetComponent<InventorySlot>();
        pedestalUISlot.onDropCall.AddListener(UpdatePedestalItem);
        pedestalUISlot.onItemLeave.AddListener(ClearItem);

        pedestalSlotObj.SetActive(false);

        pedestalPriceField = pedestalUI.transform.GetChild(0).GetChild(2).GetComponent<TMP_InputField>();

        shop = GameObject.FindGameObjectWithTag("Shop").GetComponent<Shop>();
    }

    protected override void Start() {
        base.Start();
        pedestalUI.SetActive(false);
    }

    protected override void Update() {
        base.Update();
        if (openedPedestal == this && Vector2.Distance(player.transform.position, transform.position) > 3)
        {
            ClosePedestal();
        }
    }
}

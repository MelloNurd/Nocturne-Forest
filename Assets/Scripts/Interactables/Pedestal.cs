using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Pedestal : Interactable
{
    SpriteRenderer childSpriteRenderer;

    Shop shop;

    [Header("Pedestal")]
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
        if (!canInteract || (shop.IsCustomerShopping() && shop.ShopHasItem(this))) return;
        OpenPedestal();
    }

    protected override void OnDisable() {
        base.OnDisable();
        SaveInventory();
    }

    protected override void OnDestroy() {
        base.OnDestroy();
    }

    private void OnEnable() {
        StartCoroutine(LoadInventory());
    }

    void SaveInventory() {
        string saveValue = (sellItem == null ? "" : sellItem.name) + ';' + count + ';' + sellPrice;
        PlayerPrefs.SetString("PEDESTALS_" + gameObject.name, saveValue);
    }

    IEnumerator LoadInventory() {
        yield return new WaitForSeconds(0.1f); // Have to wait for InventoryManager
        string data = PlayerPrefs.GetString("PEDESTALS_" + gameObject.name, "");
        if (!string.IsNullOrEmpty(data)) {
            if (pedestalUISlot.GetItemInSlot() != null) {
                Debug.LogWarning("Unable to load for pedestal: " + gameObject.name + ". Item already detected in slot!");
            }
            else {
                sellPrice = int.Parse(data.Split(';')[2]);
                count = int.Parse(data.Split(';')[1]);

                if (InventoryManager.itemLookup.TryGetValue(data.Split(';')[0], out Item _item)) {
                    InventoryItem newItem = InventoryManager.currentInstance.SpawnNewItem(_item, pedestalUISlot);
                    newItem.count = count;
                    UpdatePedestalItem();
                }
            }
        }
    }

    public void OpenPedestal() {
        InventoryManager.currentInstance.ToggleInventory(InventoryManager.InventoryOpening.ItemPedestal, gameObject);
        pedestalSlotObj.SetActive(true);
        UpdatePedestalUI();
    }

    public void ClosePedestal() {
        InventoryManager.currentInstance.ToggleInventory(InventoryManager.InventoryOpening.Closing, gameObject);
        pedestalSlotObj.SetActive(false);
    }

    public void UpdatePedestalItem() {
        if (sellItem != null) shop.RemoveItem(this); // This happens when swapping the item

        InventoryItem item = pedestalUISlot.GetItemInSlot();
        if (item == null || item.item == null) return;
        else {
            sellItem = item.item;
            shop.AddItemToShop(this);
        }

        count = item.count;
        childSpriteRenderer.sprite = item.item.image;
        //StartCoroutine(SetItem()); // I don't know why, but this is running too early for the item to be updated in the hiearchy, so we are waiting 0.1 seconds.
    }

    IEnumerator SetItem() {
        yield return new WaitForSeconds(0.1f);

        if (sellItem != null) shop.RemoveItem(this); // This happens when swapping the item
        
        InventoryItem item = pedestalUISlot.GetItemInSlot();
        if (item == null || item.item == null) {
            Debug.LogError("Unable to set item for pedestal: " + gameObject.name);
        }
        else {
            Debug.Log(item.item);
            sellItem = item.item;
            shop.AddItemToShop(this);
        }
        count = item.count;
        childSpriteRenderer.sprite = item.item.image;
    }

    public void UpdatePedestalPrice() {
        if (!player.itemOpened.TryGetComponent(out Pedestal _pedestal)) return;
        int.TryParse(pedestalPriceField.text, out _pedestal.sellPrice);
        if(_pedestal.sellItem != null) shop.UpdateItem(this);
    }

    public void ClearItem() {
        ClearItem(false);
    }
    public void ClearItem(bool clearItem) {
        Debug.Log("clear test");
        if (clearItem && pedestalUISlot.transform.childCount > 0) Destroy(pedestalUISlot.transform.GetChild(0).gameObject);
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

        childSpriteRenderer.transform.DOLocalMove(new Vector2(childSpriteRenderer.transform.localPosition.x, 0.8f), 0.8f).SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo).Goto(Random.Range(0, 0.8f), true);

        if (sellItem == null) childSpriteRenderer.sprite = null;

        // This is really ugly
        pedestalSlotObj = Instantiate(inventorySlotPrefab, pedestalUI.transform.Find("SlotPosition").position, Quaternion.identity);
        pedestalSlotObj.name = gameObject.name + " Slot";
        pedestalSlotObj.transform.SetParent(pedestalUI.transform);
        pedestalSlotObj.GetComponent<RectTransform>().sizeDelta = new Vector2(60, 60);
        pedestalSlotObj.GetComponent<Image>().color = Color.clear;

        pedestalUISlot = pedestalSlotObj.GetComponent<InventorySlot>();
        pedestalUISlot.onDisableCall.AddListener(UpdatePedestalItem);
        pedestalUISlot.onItemLeave.AddListener(ClearItem);

        pedestalSlotObj.SetActive(false);

        pedestalPriceField = pedestalUI.transform.GetChild(2).GetComponent<TMP_InputField>();

        shop = GameObject.FindGameObjectWithTag("Shop").GetComponent<Shop>();
    }

    protected override void Start() {
        base.Start();
        pedestalUI.SetActive(false);
    }

    protected override void Update() {
        base.Update();

        if (pedestalSlotObj.activeSelf && shop.ShopHasItem(this) && shop.IsCustomerShopping())
        {
            ClosePedestal();
        }
        //if (player.itemOpened == gameObject && Vector2.Distance(player.transform.position, transform.position) > 3)
        //{
        //    ClosePedestal();
        //}
    }
}

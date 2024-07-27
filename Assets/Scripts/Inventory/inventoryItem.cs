using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.Events;
using DG.Tweening;

public class InventoryItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
{
    [Header("UI")]
    public Image image;
    public TMP_Text countText;

    public Item item;
    public int count = 1;
    [HideInInspector] public Transform parentAfterDrag;

    [HideInInspector] public InventorySlot slot;

    Color slotColor;

    Player player;

    bool canDrag;

    TMP_Text itemTitle;

    private void Awake() {
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
        countText.raycastTarget = false;
        itemTitle = InventoryManager.currentInstance.itemTitleObj.GetComponent<TMP_Text>();
        itemTitle.raycastTarget = false;
        slotColor = Color.white;
    }

    private void Update() {
        canDrag = (InventoryManager.currentInstance.draggingItem == null | InventoryManager.currentInstance.draggingItem == this);
    }

    //set's item's details
    public void InitializeItem(Item newItem)
    {
        item = newItem;
        image.sprite = newItem.image;
        RefreshCount();
    }

    //changes the count number on item stack
    public void RefreshCount()
    {
        //changes count value to text
        countText.text = count.ToString();
        //if more than one item in stack displays text
        countText.gameObject.SetActive(count > 1);
    }

    public void SetCount(int _count) {
        count = _count;
        RefreshCount();
    }

    //picks up item's icon and sets an origin for if dropped in invalid spot
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (eventData.button != 0 || !canDrag) return; // We only care about left click

        image.raycastTarget = false;

        slot = transform.parent.GetComponent<InventorySlot>();
        if (slot != null) {
            slotColor = slot.GetComponent<Image>().color;
            slot.OnItemLeave();
            slot.GetComponent<Image>().color = new Color(slotColor.r, slotColor.b, slotColor.g, slotColor.a * 0.5f);
        }

        parentAfterDrag = transform.parent;
        transform.SetParent(transform.root);

        InventoryManager.currentInstance.draggingItem = this;
    }

    //moves item's icon when being dragged in inventory
    public void OnDrag(PointerEventData eventData)
    {
        if (eventData.button != 0 || !canDrag) return; // We only care about left click

        transform.position = Input.mousePosition + Vector3.forward;
        /*if (Input.GetMouseButtonDown(1)) // Right-click
        {
            Debug.Log("Right-click detected during drag");
            DropOneItem();
        }*/
    }

    //drops item's icon and either snaps into a slot or goes back to original slot
    public void OnEndDrag(PointerEventData eventData)
    {
        if (eventData.button != 0 || !canDrag) return; // We only care about left click
        
        //Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        //RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction, Mathf.Infinity);
        //if (hit.collider != null)
        //    Debug.Log(hit.collider.gameObject.name);

        if (slot != null) {
            slot.GetComponent<Image>().color = slotColor;
        }
        transform.SetParent(parentAfterDrag);

        InventoryManager.currentInstance.draggingItem = null;

        image.raycastTarget = true;
    }

    public void OnPointerClick(PointerEventData eventData) {
        if (eventData.button != 0) return;
        InventoryManager.currentInstance.KillHideItemTitle();
        InventoryManager.currentInstance.itemTitleObj.GetComponent<RectTransform>().anchoredPosition = GetComponent<RectTransform>().position + Vector3.up * 70;
        itemTitle.text = item.name;
        InventoryManager.currentInstance.HideItemTitle();
    }
}

using DG.Tweening;
using DG.Tweening.Core;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DropArea : MonoBehaviour, IDropHandler
{
    InventoryManager inventoryManager;
    public bool trash;

    PointerEventData eventData;
    List<RaycastResult> raysastResults = new List<RaycastResult>();

    public bool highlighted;

    public Image areaImage;
    public Image iconImage;

    Color opaqueColor;
    Color translucentColor;
    Color clearColor;

    float fadeSpeed = 0.1f;

    private void Start() {
        inventoryManager = InventoryManager.currentInstance;
        eventData = new PointerEventData(EventSystem.current);

        areaImage = GetComponent<Image>();
        iconImage = transform.GetChild(0).GetComponent<Image>();

        opaqueColor = Color.white;
        translucentColor = new Color(0, 0, 0, 0.5f);
        clearColor = Color.clear;

        areaImage.color = clearColor;
        iconImage.color = clearColor;
    }

    private void Update() {
        if(InventoryManager.currentInstance.draggingItem && (!highlighted && IsMouseOver() && InventoryManager.currentInstance.draggingItem)) {
            highlighted = true;
            ShowArea();
        }
        else if (!InventoryManager.currentInstance.draggingItem || (highlighted && !IsMouseOver())) {
            highlighted = false;
            HideArea();
        }
    }

    void ShowArea() { ShowArea(fadeSpeed); }
    void ShowArea(float _fadeSpeed) {
        areaImage.DOColor(translucentColor, _fadeSpeed);
        iconImage.DOColor(opaqueColor, _fadeSpeed);
    }

    void HideArea() { HideArea(fadeSpeed); }
    void HideArea(float _fadeSpeed) {
        areaImage.DOColor(clearColor, _fadeSpeed);
        iconImage.DOColor(clearColor, _fadeSpeed);
    }

    private bool IsMouseOver() {
        eventData.position = Input.mousePosition;
        EventSystem.current.RaycastAll(eventData, raysastResults);
        return raysastResults.Any(x => x.gameObject == gameObject);
    }

    public void OnDrop(PointerEventData eventData)
    {
        InventoryItem drugItem = eventData.pointerDrag.GetComponent<InventoryItem>();
        if (drugItem.slot != null) {
            drugItem.slot.GetComponent<Image>().color = Color.white;
        }
        if (!trash)
        {
            inventoryManager.DropItem(drugItem.item, drugItem.count);
        }
        Destroy(drugItem.gameObject);
    }
}


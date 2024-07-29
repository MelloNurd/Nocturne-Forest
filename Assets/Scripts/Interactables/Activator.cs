using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class Activator : Interactable
{
    [Header("Activator")]
    public Item keyItem = null; // This will be the item needed to "activate" this obj. If it is null, it will "activate" with any item.

    public string hintText;
    TMP_Text hintTextObj;

    Tween hintTween;

    public bool shakeOnFail = true;

    public UnityEvent onSuccessfulInteract;

    protected override void Awake() {
        base.Awake();
        hintTextObj = GetComponentInChildren<TMP_Text>();
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
            if (heldItem == null || heldItem != keyItem) {
                hintTextObj.text = hintText;
                hintTween.Kill();
                if(shakeOnFail) transform.DOShakePosition(0.15f, 0.2f, 50);
                hintTween = hintTextObj.DOFade(0, 0.5f).SetDelay(1f);
            }
            if(heldItem.deleteOnUse) InventoryManager.currentInstance.GetSelectedItem(true);
        }

        Debug.Log("Successfully activated " + gameObject.name);
        onSuccessfulInteract?.Invoke();
    }

    private void OnDisable() {
        hintTween.Kill();
    }
}

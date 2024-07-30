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

    public bool saveState = true; // This will make it so interacting once will keep the changes when relaoding scene
    public bool resetSaveOnStart = false; // Checking this will refresh the saved data for the object when starting the game

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

        if (resetSaveOnStart) PlayerPrefs.SetInt("Activator_" + gameObject.scene + "_" + gameObject.name, 0);

        if (PlayerPrefs.GetInt("Activator_" + gameObject.scene + "_" + gameObject.name, 0) != 0)
            onSuccessfulInteract?.Invoke();
    }

    // Update is called once per frame
    protected override void Update() {
        base.Update();
    }

    public override void Interact() {
        if (keyItem != null) {
            Item heldItem = InventoryManager.currentInstance.GetSelectedItem(false);
            Debug.Log(heldItem);
            if (heldItem == null || heldItem != keyItem) {
                hintTextObj.text = hintText;
                hintTextObj.color = Color.white;
                hintTween.Kill();
                if (shakeOnFail) transform.DOShakePosition(0.15f, 0.2f, 50);
                hintTween = hintTextObj.DOFade(0, 0.5f).SetDelay(1f);
                return;
            }
            else {
                if(heldItem.deleteOnUse) InventoryManager.currentInstance.GetSelectedItem(true);
            }
        }
        PlayerPrefs.SetInt("Activator_" + gameObject.scene + "_" + gameObject.name, 1);
        onSuccessfulInteract?.Invoke();
    }

    private void OnDisable() {
        hintTween.Kill();
    }
}

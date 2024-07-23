using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEditorInternal.Profiling.Memory.Experimental;
using UnityEngine;

public class Pickupable : MonoBehaviour
{
    public Item item;

    public bool canPickup = true;

    public int stackSize = 1; // not yet implemented

    GameObject player;
    float playerDist;
    float playerPickupRange;

    InventoryManager playerInventory;

    bool isInRange;

    SpriteRenderer childSpriteRenderer;

    [SerializeField] Material normalMat;
    [SerializeField] Material outlineMat;

    private void Awake() {
        if(item == null) {
            Debug.LogError("Interactable with no item assigned found: " + gameObject.name);
            canPickup = false;
        }

        player = GameObject.FindGameObjectWithTag("Player");
        playerPickupRange = player.GetComponent<Player>().interactionRange;

        childSpriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    private void Start() {
        DisableOutline();
        playerInventory = InventoryManager.currentInstance;
    }

    public void OnDrop(Item _item) {
        UpdatePickupableObj(_item);
        canPickup = false;

        Vector3 originalSize = transform.localScale;
        transform.localScale = originalSize * 0.5f;

        InventoryManager invManager = InventoryManager.currentInstance;

        // Using DOTween package. Jump to a random position within dropRange. After animation, run the pickup's OnItemSpawn script.
        transform.DOJump(transform.position + (Vector3)UnityEngine.Random.insideUnitCircle * invManager.dropRange, invManager.dropStrength, 1, invManager.dropDuration).onComplete = OnItemSpawn;
        transform.DOScale(originalSize, invManager.dropDuration * 0.8f); // Scales the object up smoothly in dropDuration length * 0.8f (when it's 80% done)
    }

    // Update is called once per frame
    void Update()
    {
        playerDist = Vector2.Distance(transform.position, player.transform.position);
        isInRange = playerDist <= playerPickupRange;
        if(canPickup) {
            if (isInRange && !IsOutlined()) {
                EnableOutline();
            }
            else if (!isInRange && IsOutlined()) {
                DisableOutline();
            }
        }
    }

    void EnableOutline() {
        childSpriteRenderer.sharedMaterial = outlineMat;
    }

    void DisableOutline() {
        childSpriteRenderer.sharedMaterial = normalMat;
    }

    bool IsOutlined() {
        return childSpriteRenderer.sharedMaterial == outlineMat;
    }

    public void Pickup() {
        if (!canPickup || !isInRange) return;

        playerInventory.AddItem(item);
        ObjectPoolManager.ReturnObjectToPool(gameObject);
    }

    public void UpdatePickupableObj(Item newItem) {
        childSpriteRenderer.sprite = newItem.image;
        item = newItem;
    }

    public void OnItemSpawn() { // This is called at the end of the "jump" animation when an item drops
        canPickup = true;
        //transform.Find("Sprite").DOLocalMove(Vector2.up * 0.4f, 0.8f).SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo); // Plays the "bouncing" animation. 
        // To give the animation an offset, use .Goto(Random.Range(0, 0.8f), true) to the line above. This can make it look strange though.
    }
}

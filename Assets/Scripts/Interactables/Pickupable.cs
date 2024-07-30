using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEngine;

public class Pickupable : MonoBehaviour
{
    [Header("Respawning")]
    [Tooltip("This only applies to pickupables that were not dropped by an enemy/player.")] public RespawnType respawningType = RespawnType.Persistent;
    [Tooltip("This is only affective when using the \"Random\" respawn type.")][Range(0, 100)] public int respawnChance = 50;
    [Tooltip("If this is checked, the enemy will respawn even if it has been killed in OneTime respawning.")] public bool overrideOneTime = false;

    public Item item;
    public bool overrideSprite = false; // Set this to true if you want the pickupable in the world to have a different sprite than the Item's sprite

    public bool canPickup = true;

    public bool playerDropped = false;
    public bool droppedItem = false;

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
        if(!droppedItem) {
            switch (respawningType) {
                case RespawnType.Persistent:
                    PlayerPrefs.SetInt(gameObject.scene + "_" + gameObject.name, 0); // Resets the value keeping track of if it died, for OneTime
                    break; // Basically for persistent, do nothing to the gameobject
                case RespawnType.Random:
                    PlayerPrefs.SetInt(gameObject.scene + "_" + gameObject.name, 0); // Resets the value keeping track of if it died, for OneTime
                    if (Random.Range(1, 101) >= respawnChance) gameObject.SetActive(false);
                    break;
                case RespawnType.OneTime:
                    int hasBeenGrabbed = PlayerPrefs.GetInt(gameObject.scene + "_" + gameObject.name, 0);
                    if (hasBeenGrabbed != 0 && !overrideOneTime) gameObject.SetActive(false);
                    break;
                default: break;
            }
        }
    }

    private void OnEnable() {
        if(!overrideSprite) UpdateSprite();
    }

    public void UpdateSprite() {
        if (item != null) childSpriteRenderer.sprite = item.image;
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

        if(!playerDropped) PlayerPrefs.SetInt("total_items_collected", PlayerPrefs.GetInt("total_items_collected", 0) + 1);

        if (item.name.ToLower() == "rock" && Random.Range(0, 20) == 0) item = InventoryManager.itemLookup.GetValueOrDefault("Diamond");

        playerInventory.AddItem(item);
        if (!ObjectPoolManager.ReturnObjectToPool(gameObject)) { // This returns false if it was not in an object pool (meaning it was placed manually)
            Debug.Log("test2");
            if (respawningType == RespawnType.OneTime) PlayerPrefs.SetInt(gameObject.scene + "_" + gameObject.name, 1);
            gameObject.SetActive(false);
        }
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

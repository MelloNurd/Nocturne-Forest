using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Interactable : MonoBehaviour
{
    public static Interactable nextInteract; // This will be the object that the player has in range to interact. Only one Interactable can be interacted with at a time.

    [Header("Interactable")]
    public bool canInteract = true;
    protected bool isInRange;

    [SerializeField] Material normalMat;
    [SerializeField] Material outlineMat;

    SpriteRenderer spriteRenderer;

    protected GameObject playerObj;
    protected Player player;
    float playerDist;

    public abstract void Interact();
    
    // Start is called before the first frame update
    protected virtual void Awake()
    {
        gameObject.layer = LayerMask.NameToLayer("Interactable");
    }

    protected virtual void Start() {
        spriteRenderer = GetComponent<SpriteRenderer>();
        playerObj = GameObject.FindGameObjectWithTag("Player");
        player = playerObj.GetComponent<Player>();
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        playerDist = Vector2.Distance(transform.position, playerObj.transform.position);
        if (nextInteract != null && playerDist < nextInteract.playerDist && canInteract) nextInteract = this;
        else if (isInRange && nextInteract == null && canInteract) nextInteract = this;
        isInRange = playerDist <= player.interactionRange;
        if (canInteract) {
            if (!IsOutlined() && nextInteract == this && isInRange) {
                EnableOutline();
            }
            else if (IsOutlined() && (nextInteract != this || !isInRange)) {
                DisableOutline();
                nextInteract = null;
            }
        }

        if (player.itemOpened == gameObject && Vector2.Distance(playerObj.transform.position, transform.position) > 3) {
            InventoryManager.currentInstance.ToggleInventory(InventoryManager.InventoryOpening.Closing, gameObject);
        }
    }

    protected bool IsOutlined() {
        return spriteRenderer.sharedMaterial == outlineMat;
    }

    public void EnableOutline() {
        spriteRenderer.sharedMaterial = outlineMat;
    }

    public void DisableOutline() {
        spriteRenderer.sharedMaterial = normalMat;
    }
}

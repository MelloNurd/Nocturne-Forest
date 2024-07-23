using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Interactable : MonoBehaviour
{
    public static Interactable nextInteract; // This will be the object that the player has in range to interact. Only one Interactable can be interacted with at a time.

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
        if (nextInteract != null && playerDist < nextInteract.playerDist) nextInteract = this;
        else if (isInRange && nextInteract == null) nextInteract = this;
        isInRange = playerDist <= player.interactionRange;
        if (canInteract) {
            if (!IsOutlined() && nextInteract == this && isInRange) {
                EnableOutline();
            }
            else if (IsOutlined() && (nextInteract != this || !isInRange)) {
                DisableOutline();
            }
        }

        if (player.itemOpened == gameObject && Vector2.Distance(playerObj.transform.position, transform.position) > 3) {
            InventoryManager.currentInstance.ToggleInventory(InventoryManager.InventoryOpening.Closing, gameObject);
        }
    }

    protected bool IsOutlined() {
        return spriteRenderer.sharedMaterial == outlineMat;
    }

    void EnableOutline() {
        spriteRenderer.sharedMaterial = outlineMat;
    }

    void DisableOutline() {
        spriteRenderer.sharedMaterial = normalMat;
    }
}

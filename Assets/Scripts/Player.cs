using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{

    [Serializable]
    public enum PlayerStates {
        Static, // The player cannot move
        Dynamic, // Basically, if the player is capable of moving and performing actions.
        Rolling,
        Attacking
    }

    [SerializeField] Animator animator;

    [Header("General")]
    public bool canPickup = true;
    public bool canOpenInventory = true;
    public bool enableUpgradeChanges = true;
    public GameObject itemOpened = null;
    public PlayerStates currentState = PlayerStates.Dynamic;

    [Header("Combat")]
    GameObject healthBar;
    Tween healthTween;
    [SerializeField] float maxHealth = 20f;
    [SerializeField] float maxHealthUpgradeModifier = 5f;
    [HideInInspector] public float currentHealth = 20f;
    public float attackDamage = 5f;
    [SerializeField] float attackDamageUpgradeModifier = 1.2f;
    [Tooltip("Knockback multiplier when hit")] public float baseKnockback; // This is the knockback multiplier the player receives on hit
    [Tooltip("Knockback multiplier when attacking")] public float attackKnockback; // This is the knockback multiplier the player gives on attacl


    private GameObject attackArea;
    private InventoryManager inventoryManager;
    Rigidbody2D rb;

    // Input stuff
    [HideInInspector] public PlayerInputActions playerControls;
    private InputAction move;
    private InputAction attack;
    private InputAction roll;
    private InputAction interact;
    private InputAction useItem;
    private InputAction openInv;

    Vector2 moveDirection = Vector2.zero; // Vector which will be based on the movement inputs

    public float interactionRange = 2f;
    LayerMask interactionMask;

    [Header("Movement")]
    [SerializeField] float moveSpeed = 2f; // The base speed of the player
    [SerializeField] float moveSpeedUpgradeModifier = 0.5f;

    [Header("Attacking")]
    [SerializeField] public bool canAttack = true;
    [SerializeField] float attackDelaySeconds = 0.2f; // The amount of time from attacking (clicking) until the attack hitbox activates
    [SerializeField] float attackLengthSeconds = 0.2f; // The length of time that the attack hitbox is active
    [SerializeField] float attackSpeedModifier = 0.2f; // Multiplicative modifier of moveSpeed while attacking

    [Header("Rolling")]
    [SerializeField] public bool canRoll = true;
    [SerializeField] float rollLengthSeconds = 0.15f; // The length of time that the roll occurs
    [SerializeField] float rollCooldownSeconds = 1f; // The length of time until the player can roll again
    [SerializeField] float rollSpeedModifier = 3.5f; // Multiplicative modifier of moveSpeed while rolling

    [Header("Debug")]
    [Tooltip("This will only show in Scene view.")][SerializeField] bool showRadiusSizes = true; // Shows the radiuses for various variables within the scene view

    private void OnEnable()
    { // This is where we are initialzing all of the input stuff
        move = playerControls.Player.Move;
        move.Enable();

        attack = playerControls.Player.Attack;
        attack.Enable();
        attack.performed += OnAttack;

        roll = playerControls.Player.Roll;
        roll.performed += OnRoll;
        roll.Enable();

        interact = playerControls.Player.Interact;
        interact.performed += OnInteract;
        interact.Enable();

        useItem = playerControls.Player.Use;
        useItem.performed += OnItemUse;
        useItem.Enable();

        openInv = playerControls.Player.OpenInventory;
        openInv.performed += OnInventory;
        openInv.Enable();
    }

    private void OnDisable()
    {
        // Disabling all the input stuff when the script is disabled
        move.Disable();
        attack.Disable();
        roll.Disable();
        interact.Disable();
        useItem.Disable();
        openInv.Disable();
    }

    private void Awake() 
    {
        playerControls = new PlayerInputActions(); // Creates an input object

        attackArea = transform.Find("AttackArea").gameObject; // Initializes object to the "AttackArea" child
        DisableAttackArea(); // Disables the attack area on start, just in case

        interactionMask = LayerMask.GetMask("Interactable", "Pickupable"); // Sets the interactionMask to only interact with the layer "Interactable". This is used in the OverlapCircle function.

        rb = GetComponent<Rigidbody2D>();

        healthBar = transform.Find("HealthBarOBJ").Find("HealthBarParent").gameObject;
        healthBar.transform.parent.localScale = Vector2.right;

        //Time.timeScale = Time.timeScale / 2;
    }

    private void Start() {
        Vector2 facingDir = GetDirectionFacing();
        attackArea.transform.right = facingDir;
        attackArea.transform.localPosition = facingDir; // Do this once so the attackArea is going to be not centered on the player at the start

        inventoryManager = InventoryManager.currentInstance;

        if(enableUpgradeChanges) {
            int upgradeLevel = PlayerPrefs.GetInt("player_stats_health", 1)-1;
            maxHealth = maxHealth + (maxHealthUpgradeModifier * upgradeLevel);

            upgradeLevel = PlayerPrefs.GetInt("player_stats_speed", 1) - 1;
            moveSpeed = moveSpeed + (moveSpeedUpgradeModifier * upgradeLevel);

            upgradeLevel = PlayerPrefs.GetInt("player_stats_atkdmg", 1) - 1;
            attackDamage = attackDamage + (attackDamageUpgradeModifier * upgradeLevel);
        }

        currentHealth = maxHealth;
    }

    // Update is called once per frame
    void Update()
    {
        if (currentState != PlayerStates.Rolling) { // Updates moveDirection as long as the player is not rolling (moveDirection is locked when rolling)
            moveDirection = move.ReadValue<Vector2>();
            if(IsMoving() && itemOpened == null && currentState != PlayerStates.Attacking)
            {
                animator.SetFloat("XInput", moveDirection.x);
                animator.SetFloat("YInput", moveDirection.y);
                animator.SetBool("Walking", true);
            }
            else
            {
                animator.SetBool("Walking", false);
            }
        }
    }

    private void FixedUpdate() {
        TryMove();
    }

    void TryMove() {
        if (itemOpened != null) { // Don't let the player move if they have an opened inventory
            rb.velocity = Vector2.zero;
            return;
        }

        // These are the different movement codes. Different states will move at different speeds.
        switch (currentState) {
            case PlayerStates.Dynamic: // Code for when player is in "Dynamic" state
                rb.velocity = (Vector3)moveDirection * moveSpeed; // Updates position based on movement
                break;
            case PlayerStates.Rolling: // Code for when the player is in Rolling state
                rb.velocity = (Vector3)moveDirection * moveSpeed * rollSpeedModifier; // Modifies speed by rollSpeedModifier
                break;
            case PlayerStates.Attacking: // Code for when the player is in Attacking state
                rb.velocity = (Vector3)moveDirection * moveSpeed * attackSpeedModifier; // Modifies speed by rollSpeedModifier
                break;
            default:
                break;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision) {
        if(currentState != PlayerStates.Rolling && collision.CompareTag("EnemyAttack")) { // Code for when the player is hit by Enemy attack area
            // Player was hit
            float knockbackMultipler = 1f;

            Vector3 knockbackDir = (transform.position - collision.transform.position).normalized;

            float damage = 0f;

            if (collision.transform.parent.TryGetComponent(out EnemyBase enemy)) {
                knockbackMultipler = enemy.attackKnockback;
                damage = enemy.attackDamage;
            }
            else if (collision.TryGetComponent(out Projectile projectile)) {
                knockbackMultipler = projectile.attackKnockback;
                knockbackDir = projectile.travelDir;
                damage = projectile.attackDamage;
            }

            Knockback(knockbackDir, knockbackMultipler);
            TakeDamage(damage);
        }
    }

    void TakeDamage(float damage) {
        // The order of stuff is a little confusing here because we need to subtract damage, clamp to zero if below, set the health bar, THEN check if its below zero to die...
        currentHealth -= damage;
        if(currentHealth <= 0) currentHealth = 0;

        UpdateHealthBar();

        if (currentHealth <= 0)  Die(); 
    }

    void Die() {
        healthTween.Kill();
        currentHealth = 0;
        Debug.Log("holy moly the player died!");
    }

    public void UpdateHealthBar() {
        if (!gameObject || !gameObject.activeSelf) return;

        healthBar.transform.localScale = new Vector2((currentHealth / maxHealth) * 0.54f, 1);
        healthTween.Kill();
        healthBar.transform.parent.localScale = Vector2.one;
        healthTween = healthBar.transform.parent.DOScale(Vector2.right, 0.2f).SetDelay(3f);
    }

    public void Knockback(Vector3 dir, float knockbackPower = 1f) {
        currentState = PlayerStates.Static;
        float distance = baseKnockback * knockbackPower * 1.6f;
        Vector3 knockedBackPos = transform.position + dir * distance;
        RaycastHit2D hit = Physics2D.Raycast(transform.position, dir, Vector2.Distance(transform.position, knockedBackPos), LayerMask.GetMask("Terrain", "Interactable"));
        if (hit.collider != null) knockedBackPos = hit.point;
        transform.DOJump(knockedBackPos, distance * 0.16f, 1, distance * 0.16f).SetEase(Ease.Linear).onComplete = () => { currentState = PlayerStates.Dynamic; };
    }

    #region InputCalls

    private void OnAttack(InputAction.CallbackContext context) { // Function is called when the attack input button is pressed
        if (currentState != PlayerStates.Dynamic || !canAttack || itemOpened != null) return;

        Vector2 facingDir = GetDirectionFacing();
        attackArea.transform.right = facingDir;

        Vector2 posOffset = facingDir * 0.85f;
        if (facingDir == Vector2.right || facingDir == Vector2.left) posOffset += new Vector2(0, 0.2f);
        attackArea.transform.localPosition = posOffset;

        StartCoroutine(Attack());
    }

    private void OnRoll(InputAction.CallbackContext context) { // Function is called when the roll input button is pressed
        if (currentState != PlayerStates.Dynamic || !canRoll) return;

        StartCoroutine(Roll());
    }

    private void OnInteract(InputAction.CallbackContext context) { // Function is called when the interact input button is pressed
        if (!canPickup) return;
        if(itemOpened != null) { // We close inventory if any inventories are open. This way, pressing the Interact button will close the inventory too.
            inventoryManager.ToggleInventory(InventoryManager.InventoryOpening.Closing, gameObject);
            return;
        }

        // Gets all interactable/pickupables in range and orders it by closest to player
        Collider2D[] interacted = Physics2D.OverlapCircleAll(transform.position, interactionRange, interactionMask).OrderBy(x => Vector2.Distance(transform.position, x.transform.position)).ToArray();

        Debug.Log(Interactable.nextInteract);
        if(Interactable.nextInteract != null) { // If there is a highlighted interactable, interact with it and ignore the pickupables
            Interactable.nextInteract.Interact();
            return;
        }

        // If there have not been any Interactables, then we look for Pickupables.
        foreach (Collider2D objCol in interacted) { // Performs actions on each collider in range
            if (objCol.TryGetComponent(out Pickupable pickupable)) pickupable.Pickup(); // Attempts to pickup the item. There are checks inside of the function that determine if it can be picked up.
        }
    }

    private void OnItemUse(InputAction.CallbackContext context) {
        Item usedItem = inventoryManager.GetSelectedItem(false);
        if (usedItem == null) return;

        Debug.Log(usedItem.usage);
        switch(usedItem.usage) {
            case ItemAction.Heal:
                currentHealth += usedItem.useAmount;
                if (currentHealth > maxHealth) currentHealth = maxHealth;
                else if (currentHealth <= 0) {
                    currentHealth = 0;
                    UpdateHealthBar();
                    Die();
                }

                UpdateHealthBar();
                if (usedItem.deleteOnUse) inventoryManager.GetSelectedItem(true);
                break;
            case ItemAction.Key:
                break;
            case ItemAction.Ingredient:
                break;
            default:
                break;
        }
    }

    private void OnInventory(InputAction.CallbackContext context) {
        if (!canOpenInventory) return;

        if(itemOpened == null) inventoryManager.ToggleInventory(InventoryManager.InventoryOpening.PlayerInventory, gameObject);
        else inventoryManager.ToggleInventory(InventoryManager.InventoryOpening.Closing, gameObject);
    }

    #endregion

    public bool IsMoving() { // Returns true or false based on whether the player is inputting movement buttons (keys, controller, etc) OR the player is in static state
        return moveDirection != Vector2.zero || currentState == PlayerStates.Static;
    }

    private Vector2 GetDirectionFacing() {
        // This is ugly, and I'd have done a switch statement instead, but this is just because we're going to have many different animations for each state, so we're just checking the names for key terms...
        if(animator.GetCurrentAnimatorClipInfo(0)[0].clip.name.Contains("Up")) {
            return Vector2.up;
        }
        else if (animator.GetCurrentAnimatorClipInfo(0)[0].clip.name.Contains("Right")) {
            return Vector2.right;
        }
        else if (animator.GetCurrentAnimatorClipInfo(0)[0].clip.name.Contains("Down")) {
            return Vector2.down;
        }
        else if (animator.GetCurrentAnimatorClipInfo(0)[0].clip.name.Contains("Left")) {
            return Vector2.left;
        }
        return Vector2.zero;
    }

    IEnumerator Attack() {
        currentState = PlayerStates.Attacking; // Changes state to atacking and disables further attacking
        canAttack = false;

        animator.SetBool("Attack", true);
        yield return new WaitForSeconds(attackDelaySeconds); // Waits for attackDelaySeconds
        EnableAttackArea(); // Enables the attack area
        yield return new WaitForSeconds(attackLengthSeconds); // Waits for attackLengthSeconds
        DisableAttackArea(); // Disables the attack area
        animator.SetBool("Attack", false);

        currentState = PlayerStates.Dynamic; // Changes state back to Dynamic
        canAttack = true; // Re-enables attacking
    }

    IEnumerator Roll() {
        currentState = PlayerStates.Rolling; // Changes state to rolling and disables further rolling
        canRoll = false;

        animator.SetBool("Roll", true);
        yield return new WaitForSeconds(rollLengthSeconds); // Waits for rollLengthSeconds

        currentState = PlayerStates.Dynamic; // Changes state back to Dynamic
        animator.SetBool("Roll", false);
        yield return new WaitForSeconds(rollCooldownSeconds); // Waits for rollCooldownSeconds
        

        canRoll = true; // Re-enables rolling
    }

    private void EnableAttackArea() { // Enabling the attackArea gameobject, which has the collider for attacking
        attackArea.SetActive(true);
        attackArea.GetComponent<BoxCollider2D>().enabled = true;
    }

    private void DisableAttackArea() { // Disabling the attackArea gameobject, which has the collider for attacking
        attackArea.SetActive(false);
        attackArea.GetComponent<BoxCollider2D>().enabled = false;
    }

    private void OnDrawGizmos() { // Function that draws the debug circles in the scene view
        if (showRadiusSizes) {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, interactionRange);
        }
    }
}

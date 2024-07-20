using System;
using System.Collections;
using System.Collections.Generic;
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
    [SerializeField] float maxHealth = 20f;
    public float currentHealth = 20f;
    public PlayerStates currentState = PlayerStates.Dynamic;

    private GameObject attackArea;

    // Input stuff
    [HideInInspector] public PlayerInputActions playerControls;
    private InputAction move;
    private InputAction attack;
    private InputAction roll;
    private InputAction interact;
    
    Vector2 moveDirection = Vector2.zero; // Vector which will be based on the movement inputs

    public float interactionRange = 2f;
    LayerMask interactionMask;

    [Header("Movement")]
    [SerializeField] float moveSpeed = 0.25f; // The base speed of the player

    [Header("Attacking")]
    [SerializeField] bool canAttack = true;
    [SerializeField] float attackDelaySeconds = 0.2f; // The amount of time from attacking (clicking) until the attack hitbox activates
    [SerializeField] float attackLengthSeconds = 0.2f; // The length of time that the attack hitbox is active
    [SerializeField] float attackSpeedModifier = 0.2f; // Multiplicative modifier of moveSpeed while attacking

    [Header("Rolling")]
    [SerializeField] bool canRoll = true;
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
    }

    private void OnDisable()
    {
        // Disabling all the input stuff when the script is disabled
        move.Disable();
        attack.Disable();
        roll.Disable();
        interact.Disable();
    }

    private void Awake() 
    {
        playerControls = new PlayerInputActions(); // Creates an input object

        attackArea = transform.Find("AttackArea").gameObject; // Initializes object to the "AttackArea" child
        DisableAttackArea(); // Disables the attack area on start, just in case

        interactionMask = LayerMask.GetMask("Interactable"); // Sets the interactionMask to only interact with the layer "Interactable". This is used in the OverlapCircle function.
    }

    // Update is called once per frame
    void Update()
    {
        if (currentState != PlayerStates.Rolling) { // Updates moveDirection as long as the player is not rolling (moveDirection is locked when rolling)
            moveDirection = move.ReadValue<Vector2>();
            if(moveDirection != Vector2.zero)
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

        if (IsMoving()) { // If the player is moving, update the AttackArea position and rotating around the player
            attackArea.transform.right = moveDirection;
            attackArea.transform.localPosition = moveDirection;
        }
    }

    private void FixedUpdate() {
        // These are the different movement codes. Different states will move at different speeds.
        if (currentState == PlayerStates.Dynamic) // Code for when player is in "Dynamic" state
        {
            transform.position += (Vector3)moveDirection * moveSpeed; // Updates position based on movement
        }
        else if (currentState == PlayerStates.Rolling) { // Code for when the player is in Rolling state
            transform.position += (Vector3)moveDirection * moveSpeed * rollSpeedModifier; // Modifies speed by rollSpeedModifier
        }
        else if (currentState == PlayerStates.Attacking) { // Code for when the player is in Attacking state
            transform.position += (Vector3)moveDirection * moveSpeed * attackSpeedModifier; // Modifies speed by rollSpeedModifier
        }
    }

    private void OnTriggerEnter2D(Collider2D collision) {
        if(currentState != PlayerStates.Rolling && collision.CompareTag("EnemyAttack")) { // Code for when the player is hit by Enemy attack area
            // Player was hit
        }
    }

    private void OnAttack(InputAction.CallbackContext context) { // Function is called when the attack input button is pressed
        if (currentState != PlayerStates.Dynamic || !canAttack) return;

        StartCoroutine(Attack());
    }

    private void OnRoll(InputAction.CallbackContext context) { // Function is called when the roll input button is pressed
        if (currentState != PlayerStates.Dynamic || !canRoll) return;

        StartCoroutine(Roll());
    }

    private void OnInteract(InputAction.CallbackContext context) { // Function is called when the interact input button is pressed
        Collider2D[] interacted = Physics2D.OverlapCircleAll(transform.position, interactionRange, interactionMask); // Gets all interactable objects within the interactionRange of the player into a collider array
        foreach (Collider2D objCol in interacted) { // Performs actions on each collider in range
            if(objCol.TryGetComponent(out Pickupable pickupable)) pickupable.Pickup(); // Attempts to pickup the item. There are checks inside of the function that determine if it can be picked up.
        }
        // Probably do another pass to check for interactables, things like doors and whatnot. Would be prioritized by which comes first in the list.
    }

    public bool IsMoving() { // Returns true or false based on whether the player is inputting movement buttons (keys, controller, etc) OR the player is in static state
        return moveDirection != Vector2.zero || currentState == PlayerStates.Static;
    }

    IEnumerator Attack() {
        currentState = PlayerStates.Attacking; // Changes state to atacking and disables further attacking
        canAttack = false;

        yield return new WaitForSeconds(attackDelaySeconds); // Waits for attackDelaySeconds
        EnableAttackArea(); // Enables the attack area
        yield return new WaitForSeconds(attackLengthSeconds); // Waits for attackLengthSeconds
        DisableAttackArea(); // Disables the attack area

        currentState = PlayerStates.Dynamic; // Changes state back to Dynamic
        canAttack = true; // Re-enables attacking
    }

    IEnumerator Roll() {
        currentState = PlayerStates.Rolling; // Changes state to rolling and disables further rolling
        canRoll = false;

        yield return new WaitForSeconds(rollLengthSeconds); // Waits for rollLengthSeconds

        currentState = PlayerStates.Dynamic; // Changes state back to Dynamic

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

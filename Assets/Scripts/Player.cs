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
        // Running,
        Rolling,
        Attacking
    }

    public PlayerStates currentState = PlayerStates.Dynamic;

    private GameObject attackArea;

    [HideInInspector] public PlayerInputActions playerControls;

    private InputAction move;
    private InputAction attack;
    private InputAction roll;
    private InputAction interact;
    
    Vector2 moveDirection = Vector2.zero;

    [Header("Moving")]
    [SerializeField] float moveSpeed = 0.25f;

    [Header("Attacking")]
    [SerializeField] bool canAttack = true;
    [SerializeField] float attackDelaySeconds = 0.2f; // The amount of time from attacking (clicking) until the attack hitbox activates
    [SerializeField] float attackLengthSeconds = 0.2f; // The length of time that the attack hitbox is active
    [SerializeField] float attackSpeedModifier = 0.2f;

    [Header("Rolling")]
    [SerializeField] bool canRoll = true;
    [SerializeField] float rollLengthSeconds = 0.1f; // The length of time that the roll occurs
    [SerializeField] float rollCooldownSeconds = 1f; // The length of time until the player can roll again
    [SerializeField] float rollSpeedModifier = 5f;

    private void OnEnable()
    {
        move = playerControls.Player.Move;
        move.performed += OnMove;
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
        move.Disable();
        attack.Disable();
        roll.Disable();
        interact.Disable();
    }

    private void Awake() 
    {
        playerControls = new PlayerInputActions();

        attackArea = transform.Find("AttackArea").gameObject;
        DisableAttackArea();
    }

    // Start is called before the first frame update
    void Start()
    {
                
    }

    // Update is called once per frame
    void Update()
    {
        if (currentState != PlayerStates.Rolling) {
            moveDirection = move.ReadValue<Vector2>();
        }

        if (IsMoving()) {
            attackArea.transform.right = moveDirection;
            attackArea.transform.localPosition = moveDirection;
        }
    }

    private void FixedUpdate() {
        if (currentState == PlayerStates.Dynamic) 
        {
            transform.position += (Vector3)moveDirection * moveSpeed;
        }
        else if (currentState == PlayerStates.Rolling) {
            transform.position += (Vector3)moveDirection * moveSpeed * rollSpeedModifier;
        }
        else if (currentState == PlayerStates.Attacking) {
            transform.position += (Vector3)moveDirection * moveSpeed * attackSpeedModifier;
        }
    }

    private void OnMove(InputAction.CallbackContext context) {
        // wowee
    }

    private void OnAttack(InputAction.CallbackContext context) {
        if (currentState != PlayerStates.Dynamic || !canAttack) return;

        StartCoroutine(Attack());
    }

    private void OnRoll(InputAction.CallbackContext context) {
        if (currentState != PlayerStates.Dynamic || !canRoll) return;

        StartCoroutine(Roll());
    }

    private void OnInteract(InputAction.CallbackContext context) {
        Debug.Log("Player Interacted");
    }

    public bool IsMoving() {
        return moveDirection != Vector2.zero || currentState == PlayerStates.Static;
    }

    IEnumerator Attack() {
        currentState = PlayerStates.Attacking;
        canAttack = false;

        yield return new WaitForSeconds(attackDelaySeconds);
        EnableAttackArea();
        yield return new WaitForSeconds(attackLengthSeconds);
        DisableAttackArea();

        currentState = PlayerStates.Dynamic;
        canAttack = true;
    }

    IEnumerator Roll() {
        currentState = PlayerStates.Rolling;
        canRoll = false;

        yield return new WaitForSeconds(rollLengthSeconds);

        currentState = PlayerStates.Dynamic;

        yield return new WaitForSeconds(rollCooldownSeconds);

        canRoll = true;
    }

    private void EnableAttackArea() { // This is not to enable the canAttack bool, this is for setting the attackArea to enabled
        attackArea.SetActive(true);
        attackArea.GetComponent<BoxCollider2D>().enabled = true;
    }

    private void DisableAttackArea() { // This is not to disable the canAttack bool, this is for setting the attackArea to disabled
        attackArea.SetActive(false);
        attackArea.GetComponent<BoxCollider2D>().enabled = false;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{

    private GameObject attackArea;

    [HideInInspector] public PlayerInputActions playerControls;

    private InputAction move;
    private InputAction attack;
    private InputAction roll;
    private InputAction interact;
    
    Vector2 moveDirection = Vector2.zero;

    [HideInInspector] public bool canMove = true;
    private bool lockMovement = false; // This bool will lock movement to the whatever it currently is and prevent it from changing (utilized in rolling)

    [Range(0.01f, 5f)]
    [SerializeField] private float moveSpeed = 0.5f;

    [HideInInspector] public bool canAttack = true;
    private float attackDelaySeconds = 0.2f; // The amount of time from attacking (clicking) until the attack hitbox activates
    private float attackLengthSeconds = 0.2f; // The length of time that the attack hitbox is active
    private float attackSpeedModifier = 0.2f;

    [HideInInspector] public bool canRoll = true;
    private float rollDelaySeconds = 0f; // The amount of time from rolling (pressing the button) until the roll starts
    private float rollLengthSeconds = 0.1f; // The length of time that the roll occurs
    private float rollCooldownLength = 3f; // not yet implemented
    private float rollSpeedModifier = 5f;

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
        if (!lockMovement && canMove) {
            moveDirection = move.ReadValue<Vector2>();
        }

        if (IsMoving()) {
            attackArea.transform.right = moveDirection;// = Quaternion.LookRotation(moveDirection); //RotateAround(transform.position, Vector3.forward, angle);
            attackArea.transform.localPosition = moveDirection;
        }
    }

    private void FixedUpdate() {
        if (canMove) 
        {
            transform.position += (Vector3)moveDirection * moveSpeed;
        }
    }

    private void OnMove(InputAction.CallbackContext context) {
        // wowee
    }

    private void OnAttack(InputAction.CallbackContext context) {
        if (!canAttack) return;

        StartCoroutine(Attack());
    }

    private void OnRoll(InputAction.CallbackContext context) {
        if (!IsMoving() || !canRoll) return;

        StartCoroutine(Roll());
    }

    private void OnInteract(InputAction.CallbackContext context) {
        Debug.Log("Player Interacted");
    }

    public bool IsMoving() {
        return moveDirection != Vector2.zero;
    }

    IEnumerator Attack() {

        canAttack = false;
        canRoll = false;
        float tempMoveSpeed = moveSpeed;
        moveSpeed *= attackSpeedModifier;

        yield return new WaitForSeconds(attackDelaySeconds);
        EnableAttackArea();
        yield return new WaitForSeconds(attackLengthSeconds);
        DisableAttackArea();
        
        canAttack = true;
        canRoll = true;
        moveSpeed = tempMoveSpeed;
    }

    IEnumerator Roll() {
        canRoll = false;
        canAttack = false;

        yield return new WaitForSeconds(rollDelaySeconds);
        lockMovement = true; // This will not disable movement, but disable the changing of the movement. In other words, it locks moveDirection.
        float tempMoveSpeed = moveSpeed;
        moveSpeed *= rollSpeedModifier;
        
        yield return new WaitForSeconds(rollLengthSeconds);
        
        moveSpeed = tempMoveSpeed;
        lockMovement = false;
        canAttack = true;
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

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

    [Range(0.01f, 5f)]
    [SerializeField] private float moveSpeed = 0.5f;

    [HideInInspector] public bool canAttack = true;
    private float attackDelaySeconds = 0.2f; // The amount of time from attacking (clicking) until the attack hitbox activates
    private float attackLengthSeconds = 0.2f; // The amount of time that the attack hitbox is active

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
        if (canMove) {
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
        Debug.Log("Player Rolled");
    }

    private void OnInteract(InputAction.CallbackContext context) {
        Debug.Log("Player Interacted");
    }

    public bool IsMoving() {
        return moveDirection != Vector2.zero;
    }

    IEnumerator Attack() {
        canAttack = false;
        float tempMoveSpeed = moveSpeed;
        moveSpeed *= 0.2f;

        yield return new WaitForSeconds(attackDelaySeconds);
        EnableAttackArea();
        yield return new WaitForSeconds(attackLengthSeconds);
        DisableAttackArea();
        
        canAttack = true;
        moveSpeed = tempMoveSpeed;
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

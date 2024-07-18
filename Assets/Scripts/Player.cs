using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{

    [HideInInspector] public PlayerInputActions playerControls;

    private InputAction move;
    private InputAction fire;
    
    Vector2 moveDirection = Vector2.zero;


    [Range(0.01f, 5f)]
    [SerializeField] private float moveSpeed = 0.5f;

    private bool canAttack = true;

    private void OnEnable()
    {
        move = playerControls.Player.Move;
        move.Enable();

        fire = playerControls.Player.Fire;
        fire.Enable();
        fire.performed += Fire;
    }

    private void OnDisable()
    {
        move.Disable();
        fire.Disable();
    }

    private void Awake() 
    {
        playerControls = new PlayerInputActions();        
    }

    // Start is called before the first frame update
    void Start()
    {
                
    }

    // Update is called once per frame
    void Update()
    {
        moveDirection = move.ReadValue<Vector2>();
    }

    private void FixedUpdate() {
        transform.position += (Vector3)moveDirection * moveSpeed;
    }

    private void Fire(InputAction.CallbackContext context) {
        if (!canAttack) return;

        Debug.Log("Player Attacked");
    }
}

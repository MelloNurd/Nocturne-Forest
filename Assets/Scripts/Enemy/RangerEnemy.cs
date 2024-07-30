using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class RangerEnemy : EnemyBase
{
    [Serializable]
    public enum EnemyStates {
        Static, // The enemy basically has no AI
        Roaming,
        Patrolling,
        Pursuing,
        Attacking
    }

    [Serializable] enum PatrolStyles {
        Random,
        RoundRobin
    }

    [Header("General")]
    public EnemyStates currentState = EnemyStates.Roaming;

    GameObject playerObj;
    Vector2 playerDir;
    float playerDist;

    [SerializeField] Animator animator;

    [Header("Movement")]
    [SerializeField] float moveSpeed = 2f; // The base speed of the enemy

    [SerializeField] float playerDistanceCutoff = 1.25f; // The distance from the player the enemy will stop moving towards the player. This is so it doesn't go inside the player.

    [Header("Roaming")]
    [SerializeField] bool canRoam = true;
    [SerializeField] float roamRadius = 5f; // Maximum distance the enemy may pick to roam to, in a circle
    [SerializeField] float timeBetweenRoam = 3f; // The amount of time the enemy pauses before moving to a new random point
    [SerializeField] float roamSpeedModifier = 1f; // Multiplicative modifier of moveSpeed while roaming
    Vector2 activeRoamPoint = Vector2.zero;

    [Header("Patrolling")]
    [SerializeField] bool canPatrol = true;
    [SerializeField] List<Vector2> patrolPoints = new List<Vector2>(); // A list of Vector2's that the enemy will patrol amongst
    [SerializeField] PatrolStyles patrolStyle = PatrolStyles.RoundRobin; // Round-Robin: picks the next patrol point in the list; Random: Picks a random patrol point from the list
    [SerializeField] float timeBetweenPatrol = 3f; // The amount of time the enemy pauses before moving to a new patrol point
    [SerializeField] float patrolSpeedModifier = 1f; // Multiplicative modifier of moveSpeed while patrolling
    Vector2 activePatrolPoint;

    [Header("Pursuing")]
    [SerializeField] bool canPursue = true;
    [SerializeField] float pursuitDistance = 5f; // Distance the player has to be from the enemy for the enemy to start pursuing
    [SerializeField] float pursuitLossDistance = 15f; // How far away an active pursuit has to be to stop pursuiing
    [SerializeField] float pursueSpeedModifier = 1f; // Multiplicative modifier of moveSpeed while pursuing

    [Header("Attacking")]
    [SerializeField] bool canAttack = true;
    [SerializeField] float attackRange = 2f; // Distance the player has to be from the enemy for the enemy to attack
    [SerializeField] float attackDelaySeconds = 0.2f; // The amount of time from attacking (clicking) until the attack hitbox activates
    [SerializeField] float attackLengthSeconds = 0.2f; // The length of time that the attack hitbox is active
    [SerializeField] float attackCooldownSeconds = 2f; // The length of time until the player can roll again
    [SerializeField] float attackSpeedModifier = 0.2f; // Multiplicative modifier of moveSpeed while attacking

    [Header("Projectiles")]
    [SerializeField] GameObject projectilePrefab;
    [SerializeField] int projectileCount = 1; // The number of projectiles shot each time the ranger attacks.
    [SerializeField] int projectileSpread = 30; // The spread of the shot projectiles. This only applies when there are more than 1 projectiles shot.

    [Header("Debug")]
    [Tooltip("This will only show in Scene view.")][SerializeField] bool showRadiusSizes = true; // Shows the radiuses for various variables within the scene view

    protected override void Awake() {
        base.Awake();
    }
    protected override void Start() {
        base.Start();
        playerObj = player.gameObject;
        activeRoamPoint = GetNewRoamPoint();
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();

        if (rb.velocity != Vector2.zero)
        {
            animator.SetFloat("XInput", rb.velocity.x);
            animator.SetFloat("YInput", rb.velocity.y);
            animator.SetBool("Walking", true);
        }
        else
        {
            animator.SetBool("Walking", false);
        }

        if (currentState != EnemyStates.Static) { // If the enemy is not static or charging, update the relative player's stats
            playerDir = (playerObj.transform.position - transform.position).normalized; // Direction the player is from the enemy
            playerDist = Vector2.Distance(transform.position, playerObj.transform.position); // Distance the player is from the enemy
        }
    }

    private void FixedUpdate() {
        // These are the different movement codes. Different states will move at different speeds.
        switch (currentState) {
            case EnemyStates.Roaming: // Code for when enemy is in roaming state
                if (!canRoam) return;

                if (playerDist <= pursuitDistance) { // If enemy is close enough to player, start pursuing
                    currentState = EnemyStates.Pursuing;
                    return;
                }

                rb.velocity = ((Vector3)activeRoamPoint - transform.position).normalized * moveSpeed * roamSpeedModifier; // Moving in direction of activeRoamPoint

                if (Vector2.Distance(transform.position, activeRoamPoint) < 0.2f) {
                    if(gameObject.name == "Medusa") Debug.Log("test for raom");
                    StartCoroutine(TryRoam()); // If has reached roam point, try to get a new roam point
                }
                break;
            case EnemyStates.Patrolling: // Code for when enemy is in patrolling state
                if (!canPatrol) return;

                if (playerDist <= pursuitDistance) { // If enemy is close enough to player, start pursuing
                    currentState = EnemyStates.Pursuing;
                    return;
                }

                rb.velocity = ((Vector3)activePatrolPoint - transform.position).normalized * moveSpeed * patrolSpeedModifier; // Moving in direction of actingPatrolPoint

                if (Vector2.Distance(transform.position, activePatrolPoint) < 0.2f) {
                    StartCoroutine(TryPatrol()); // If has reached patrol point, try to get a new patrol point
                }
                break;
            case EnemyStates.Pursuing: // Code for when enemy is in pursuing state
                if (!canPursue) return;

                if (playerDist >= pursuitLossDistance) { // If player gets too far away, stop pursuing
                    ResetMovementState();
                    return;
                }

                if (playerDist > playerDistanceCutoff) rb.velocity = (Vector3)playerDir * moveSpeed * pursueSpeedModifier; // Moves in direction of player. Only does this when far enough away, to prevent enemy from walking into player.
                else rb.velocity = Vector2.zero;

                TryAttack(); // Checks if attacking conditions are met. If so, attacks
                break;
            case EnemyStates.Attacking: // Code for when enemy is in attacking state
                if (playerDist > playerDistanceCutoff) rb.velocity = (Vector3)playerDir * moveSpeed * attackSpeedModifier;
                else rb.velocity = Vector2.zero;
                break;
            default:
                break;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision) {
        if (collision.CompareTag("PlayerAttack")) { // When getting hit by the player's attack area
            if (currentState == EnemyStates.Static) return;

            if (TakeDamage(player.attackDamage)) Knockback(player.attackKnockback);
        }
    }
    public void Knockback(float knockbackPower = 1f) {
        currentState = EnemyStates.Static;
        Vector3 playerDir = (transform.position - player.transform.position).normalized;
        float distance = baseKnockback * knockbackPower*1.6f;
        Vector3 knockedBackPos = transform.position + playerDir * distance; // Get the point we are being knocked back to
        RaycastHit2D hit = Physics2D.Raycast(transform.position, playerDir, Vector2.Distance(transform.position, knockedBackPos), LayerMask.GetMask("Terrain", "Interactable"));
        if (hit.collider != null) knockedBackPos = hit.point; // If the knocked back to point is blocked by an object, set it to where it was blocked
        transform.DOJump(knockedBackPos, distance * 0.16f, 1, distance * 0.16f).SetEase(Ease.Linear).onComplete = ResetMovementState;
    }

    void ResetMovementState() { // Using this because we might not know which "passive" state the enemy should switch to when done pursuing
        if (patrolPoints.Count <= 0) currentState = EnemyStates.Roaming; // If there are no patrol points in the patrolPoints list, switch to roaming
        else currentState = EnemyStates.Patrolling; // Otherwise, switch to patrolling
    }

    void TryAttack() {
        if (!canAttack) return;

        if(projectilePrefab == null) {
            Debug.LogError("No projectile prefab assigned on: " + gameObject.name);
            return;
        }

        if (playerDist <= attackRange) { // If the player is within the attack range and the enemy can attack
            StartCoroutine(Attack()); // Otherwise, do a normal attack
        }
    }

    IEnumerator Attack() {
        animator.SetFloat("XInput", playerDir.x);
        animator.SetFloat("YInput", playerDir.y);
        currentState = EnemyStates.Attacking; // Sets state to attack and disables further attacking
        canAttack = false;
        animator.SetBool("Attacking", true);
        yield return new WaitForSeconds(attackDelaySeconds); // Waits for attackDelaySeconds
        ShootProjectiles(); // Enables attack area
        yield return new WaitForSeconds(attackLengthSeconds); // Waits for attackLengthSeconds
        animator.SetBool("Attacking", false);
        ResetMovementState(); // Changes state back to "passive"

        yield return new WaitForSeconds(attackCooldownSeconds); // Waits for attackCooldownSeconds

        canAttack = true; // Re-enables attacking
    }

    private void ShootProjectiles() { // Enabling the attackArea gameobject, which has the collider for attacking
        GameObject bullet;
        
        float angle = Mathf.Atan2(playerDir.x, playerDir.y) * Mathf.Rad2Deg; // Calculates the angle (int) from the normalized player direction (Vector2)
        
        angle -= (projectileSpread * 0.5f) * (projectileCount - 1); // Subtracts from the angle the amount calculated based on bullet count + spread, to get proper bullet distribution
        for (int i = 0; i < projectileCount; i++) { // Spawns a bullet for each count
            bullet = ObjectPoolManager.SpawnObject(projectilePrefab, transform.position, Quaternion.identity, ObjectPoolManager.PoolType.Projectiles);
            Vector2 travelDir = new Vector2(Mathf.Sin(angle * Mathf.Deg2Rad), Mathf.Cos(angle * Mathf.Deg2Rad)).normalized; // Converts the angle (int) to a normalized direction (Vector2)
            bullet.GetComponent<Projectile>().travelDir = travelDir;
            bullet.transform.right = travelDir;
            angle += projectileSpread; // Adds the spread for the next bullet to use
        }
    }

    Vector3 GetNewRoamPoint() {
        Vector3 roamPoint;
        int threshold = 0;
        LayerMask mask = LayerMask.GetMask("Terrain", "Interactable");

        do {
            roamPoint = (Vector2)transform.position + (UnityEngine.Random.insideUnitCircle * roamRadius);
            threshold++;
        } while (Physics2D.OverlapCircleAll(roamPoint, 0f, mask).Length > 0 && threshold < 50); // Continue to get a new point until it's not in a collider

        RaycastHit2D hit = Physics2D.Raycast(transform.position, (roamPoint - transform.position).normalized, Vector2.Distance(transform.position, roamPoint), LayerMask.GetMask("Terrain", "Interactable"));
        if (hit.collider != null) roamPoint = hit.point;

        return roamPoint; // Get a random point in a circle around the enemy, by roamRadius size
    }

    Vector2 GetNewPatrolPoint() {
        if(patrolPoints.Count <= 0) { // If there are no patrol points in the list
            //Debug.LogError("No patrol points loaded!");
            return Vector2.zero;
        }

        if(patrolStyle == PatrolStyles.Random) { // When patrolling by Random
            int index;
            do {
                index = UnityEngine.Random.Range(0, patrolPoints.Count);
            } while (index == patrolPoints.IndexOf(activePatrolPoint)); // Basically ensuring we pick a random patrol point from the list that isn't the one the enemy is already at. This has potential to break, like for instance if there is only one point in the list.
            return patrolPoints[index]; // Returns the new patrol point
        }
        else if (patrolStyle == PatrolStyles.RoundRobin) { // When patrolling by Round-Robin
            int index = patrolPoints.IndexOf(activePatrolPoint);

            index =  index < 0 ? 0 : (index + 1) % patrolPoints.Count; // If index is less than 0, set it to zero. Otherwise, increment it / loop around to zero if larger than patrolPoints.Count

            return patrolPoints[index]; // Returns the new patrol point
        }
        else { // Should not ever run, but needed it for the function to not error
            Debug.LogError("Invalid Patrol Style selected!");
            return Vector2.zero;
        }
    }

    IEnumerator TryRoam() { // Attempts to get a new roaming point
        canRoam = false;
        rb.velocity = Vector2.zero;
        yield return new WaitForSeconds(timeBetweenRoam); // Waits for timeBetweenRoam
        activeRoamPoint = GetNewRoamPoint(); // Assigns a new random roaming point
        canRoam = true;
    }

    IEnumerator TryPatrol() { // Attempts to get a new patrolling point
        canPatrol = false;
        rb.velocity = Vector2.zero;
        yield return new WaitForSeconds(timeBetweenPatrol); // Waits for timeBetweenPatrol
        activePatrolPoint = GetNewPatrolPoint(); // Assigns a new patrol point
        canPatrol = true;
    }

    private void OnDrawGizmos() { // Function that draws the debug circles in the scene view
        if (showRadiusSizes) {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, pursuitDistance);

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, pursuitLossDistance);

            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(transform.position, attackRange);
        }
    }
}

using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using static Player;

public class RangerEnemy : MonoBehaviour
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
    [SerializeField] float maxHealth = 20f;
    public float currentHealth = 20f;
    public EnemyStates currentState = EnemyStates.Roaming;

    [Header("Item Dropping")]
    public List<Item> drops = new List<Item>();
    [SerializeField] float dropRange = 2f; // Maximum distance a dropped item will drop from the player
    [SerializeField] float dropStrength = 1.5f; // How high the "jump" arc is when dropping an item
    [SerializeField] float dropDuration = 0.6f; // How long it takes for the item to reach it's dropped position
    [SerializeField] GameObject pickupablePrefab;

    GameObject player;
    Vector2 playerDir;
    float playerDist;

    [Header("Movement")]
    [SerializeField] float moveSpeed = 0.25f; // The base speed of the enemy

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

    // Start is called before the first frame update
    void Awake()
    {
        player = GameObject.Find("Player");
    }

    // Update is called once per frame
    void Update()
    {
        if (currentState != EnemyStates.Static) { // If the enemy is not static or charging, update the relative player's stats
            playerDir = (player.transform.position - transform.position).normalized; // Direction the player is from the enemy
            playerDist = Vector2.Distance(transform.position, player.transform.position); // Distance the player is from the enemy
        }
    }

    private void FixedUpdate() {
        // These are the different movement codes. Different states will move at different speeds.
        if (currentState == EnemyStates.Roaming) { // Code for when enemy is in roaming state
            if (!canRoam) return;

            if (playerDist <= pursuitDistance) { // If enemy is close enough to player, start pursuing
                currentState = EnemyStates.Pursuing;
                return;
            }

            transform.position += ((Vector3)activeRoamPoint - transform.position).normalized * moveSpeed * roamSpeedModifier; // Moving in direction of activeRoamPoint

            if (Vector2.Distance(transform.position, activeRoamPoint) < 0.2f) {
                StartCoroutine(TryRoam()); // If has reached roam point, try to get a new roam point
            }
        }
        else if (currentState == EnemyStates.Patrolling) { // Code for when enemy is in patrolling state
            if (!canPatrol) return;

            if (playerDist <= pursuitDistance) { // If enemy is close enough to player, start pursuing
                currentState = EnemyStates.Pursuing;
                return;
            }

            transform.position += ((Vector3)activePatrolPoint - transform.position).normalized * moveSpeed * patrolSpeedModifier; // Moving in direction of actingPatrolPoint

            if (Vector2.Distance(transform.position, activePatrolPoint) < 0.2f) {
                StartCoroutine(TryPatrol()); // If has reached patrol point, try to get a new patrol point
            }
        }
        else if (currentState == EnemyStates.Pursuing) { // Code for when enemy is in pursuing state
            if (!canPursue) return;

            if (playerDist >= pursuitLossDistance) { // If player gets too far away, stop pursuing
                ResetMovementState();
                return;
            }

            if (playerDist > playerDistanceCutoff) transform.position += (Vector3)playerDir * moveSpeed * pursueSpeedModifier; // Moves in direction of player. Only does this when far enough away, to prevent enemy from walking into player.

            TryAttack(); // Checks if attacking conditions are met. If so, attacks
        }
        else if (currentState == EnemyStates.Attacking) { // Code for when enemy is in attacking state
            if (playerDist > playerDistanceCutoff) transform.position += (Vector3)playerDir * moveSpeed * attackSpeedModifier;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision) {
        if (collision.CompareTag("PlayerAttack")) { // When getting hit by the player's attack area
            Die();
        }
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
        currentState = EnemyStates.Attacking; // Sets state to attack and disables further attacking
        canAttack = false;

        yield return new WaitForSeconds(attackDelaySeconds); // Waits for attackDelaySeconds
        ShootProjectiles(); // Enables attack area
        yield return new WaitForSeconds(attackLengthSeconds); // Waits for attackLengthSeconds

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
            bullet.GetComponent<Projectile>().travelDir = new Vector2(Mathf.Sin(angle * Mathf.Deg2Rad), Mathf.Cos(angle * Mathf.Deg2Rad)).normalized; // Converts the angle (int) to a normalized direction (Vector2)
            angle += projectileSpread; // Adds the spread for the next bullet to use
        }
    }

    Vector3 GetNewRoamPoint() {
        return (Vector2)transform.position + (UnityEngine.Random.insideUnitCircle * roamRadius); // Get a random point in a circle around the enemy, by roamRadius size
    }

    Vector2 GetNewPatrolPoint() {
        if(patrolPoints.Count <= 0) { // If there are no patrol points in the list
            Debug.LogError("No patrol points loaded!");
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
        yield return new WaitForSeconds(timeBetweenRoam); // Waits for timeBetweenRoam
        activeRoamPoint = GetNewRoamPoint(); // Assigns a new random roaming point
        canRoam = true;
    }

    IEnumerator TryPatrol() { // Attempts to get a new patrolling point
        canPatrol = false;
        yield return new WaitForSeconds(timeBetweenPatrol); // Waits for timeBetweenPatrol
        activePatrolPoint = GetNewPatrolPoint(); // Assigns a new patrol point
        canPatrol = true;
    }
    public void DropItem(Item item) {
        GameObject droppedItem = ObjectPoolManager.SpawnObject(pickupablePrefab, transform.position, Quaternion.identity, ObjectPoolManager.PoolType.Pickupables);
        Pickupable pickupScript = droppedItem.GetComponent<Pickupable>();
        pickupScript.UpdatePickupableObj(item);
        pickupScript.canPickup = false;

        Vector3 originalSize = droppedItem.transform.localScale;
        droppedItem.transform.localScale = originalSize * 0.5f;

        // Using DOTween package. Jump to a random position within dropRange. After animation, run the pickup's OnItemSpawn script.
        droppedItem.transform.DOJump(transform.position + (Vector3)UnityEngine.Random.insideUnitCircle * dropRange, dropStrength, 1, dropDuration).onComplete = pickupScript.OnItemSpawn;
        droppedItem.transform.DOScale(originalSize, dropDuration * 0.8f); // Scales the object up smoothly in dropDuration length * 0.8f (when it's 80% done)
    }

    public void Die() {
        foreach(Item item in drops) {
            DropItem(item);
        }
        Destroy(gameObject);
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

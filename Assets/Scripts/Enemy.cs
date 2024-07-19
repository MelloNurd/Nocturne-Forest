using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using static Player;

public class Enemy : MonoBehaviour
{
    [Serializable]
    public enum EnemyStates {
        Static, // The enemy basically has no AI
        Roaming,
        Patrolling,
        Pursuing,
        Attacking,
        Charging
    }

    [Serializable] enum PatrolStyles {
        Random,
        RoundRobin
    }

    public EnemyStates currentState = EnemyStates.Roaming;

    private GameObject attackArea;

    GameObject player;
    Vector2 playerDir;
    float playerDist;


    [SerializeField] float moveSpeed = 0.25f;

    [Header("Roaming")]
    [SerializeField] bool canRoam = true;
    [SerializeField] float roamRadius = 5f;
    [SerializeField] float timeBetweenRoam = 3f;
    [SerializeField] float roamSpeedModifier = 1f;
    Vector2 activeRoamPoint = Vector2.zero;

    [Header("Patrolling")]
    [SerializeField] bool canPatrol = true;
    [SerializeField] List<Vector2> patrolPoints = new List<Vector2>();
    [SerializeField] PatrolStyles patrolStyle = PatrolStyles.RoundRobin;
    [SerializeField] float timeBetweenPatrol = 3f;
    [SerializeField] float patrolSpeedModifier = 1f;
    Vector2 activePatrolPoint;

    [Header("Pursuing")]
    [SerializeField] bool canPursue = true;
    [SerializeField] float pursuitDistance = 5f;
    [SerializeField] float pursuitLossDistance = 15f; // How far away an active pursuit has to be to stop pursuiting
    [SerializeField] float pursueSpeedModifier = 1f; // This controls the behaviour of the player's moveSpeed while attacking

    [Header("Attacking")]
    [SerializeField] bool canAttack = true;
    [SerializeField] float attackDelaySeconds = 0.2f; // The amount of time from attacking (clicking) until the attack hitbox activates
    [SerializeField] float attackLengthSeconds = 0.2f; // The length of time that the attack hitbox is active
    [SerializeField] float attackSpeedModifier = 0.2f; // This controls the behaviour of the player's moveSpeed while attacking

    [Header("Charging")]
    [SerializeField] bool canCharge = true;
    [SerializeField] float chargeDelaySeconds = 0.2f; // The length of time that the roll occurs
    [SerializeField] float chargeLengthSeconds = 0.1f; // The length of time that the roll occurs
    [SerializeField] float chargeCooldownSeconds = 1f; // The length of time until the player can roll again
    [SerializeField] float chargeSpeedModifier = 5f; // This controls the behaviour of the player's moveSpeed while rolling

    [Header("Debug")]
    [Tooltip("This will only show in Scene view.")][SerializeField] bool showRadiusSizes;

    // Start is called before the first frame update
    void Awake()
    {
        attackArea = transform.Find("AttackArea").gameObject;
        DisableAttackArea();

        player = GameObject.Find("Player");

        activePatrolPoint = GetNewPatrolPoint();
        activeRoamPoint = GetNewRoamPoint();
    }

    // Update is called once per frame
    void Update()
    {
        if (currentState != EnemyStates.Static || currentState != EnemyStates.Charging) {
            playerDir = (player.transform.position - transform.position).normalized;
            playerDist = Vector2.Distance(transform.position, player.transform.position);
        }
    }

    private void OnDrawGizmos() {
        if (showRadiusSizes) {
            Gizmos.color = new Color(0,255, 0, 0.5f);
            Gizmos.DrawWireSphere(transform.position, pursuitDistance);

            Gizmos.color = new Color(255, 0, 0, 0.5f);
            Gizmos.DrawWireSphere(transform.position, pursuitLossDistance);
        }
    }

    private void FixedUpdate() {
        // These are the different movement codes. Different states will move at different speeds.
        if (currentState == EnemyStates.Roaming) {
            if (canRoam) {
                transform.position += ((Vector3)activeRoamPoint - transform.position).normalized * moveSpeed * roamSpeedModifier;

                if (Vector2.Distance(transform.position, activeRoamPoint) < 0.2f) {
                    StartCoroutine(TryRoam());
                }
            }
            if (canPursue && playerDist <= pursuitDistance) {
                currentState = EnemyStates.Pursuing;
            }
        }
        else if (currentState == EnemyStates.Patrolling) {
            if(canPatrol) {
                transform.position += ((Vector3)activePatrolPoint - transform.position).normalized * moveSpeed * roamSpeedModifier;

                if (Vector2.Distance(transform.position, activePatrolPoint) < 0.2f) {
                    StartCoroutine(TryPatrol());
                }
            }
            if (canPursue && playerDist <= pursuitDistance) {
                currentState = EnemyStates.Pursuing;
            }
        }
        else if (currentState == EnemyStates.Pursuing) {
            transform.position += (Vector3)playerDir * moveSpeed * pursueSpeedModifier;

            if(playerDist >= pursuitLossDistance) {
                if (patrolPoints.Count <= 0) currentState = EnemyStates.Roaming;
                else currentState = EnemyStates.Patrolling;
            }
        }
        else if (currentState == EnemyStates.Attacking) {
            transform.position += (Vector3)playerDir * moveSpeed * attackSpeedModifier;
        }
        else if (currentState == EnemyStates.Charging) {
            transform.position += (Vector3)playerDir * moveSpeed * chargeSpeedModifier;
        }
    }

    private void EnableAttackArea() { // This is not to enable the canAttack bool, this is for setting the attackArea to enabled
        attackArea.SetActive(true);
        attackArea.GetComponent<BoxCollider2D>().enabled = true;
    }

    private void DisableAttackArea() { // This is not to disable the canAttack bool, this is for setting the attackArea to disabled
        attackArea.SetActive(false);
        attackArea.GetComponent<BoxCollider2D>().enabled = false;
    }

    IEnumerator Charge() {
        currentState = EnemyStates.Static;
        canCharge = false;

        yield return new WaitForSeconds(chargeDelaySeconds);

        currentState = EnemyStates.Charging;

        yield return new WaitForSeconds(chargeLengthSeconds);

        currentState = EnemyStates.Roaming;

        yield return new WaitForSeconds(chargeCooldownSeconds);

        canCharge = true;
    }

    Vector3 GetNewRoamPoint() {
        return (Vector2)transform.position + (UnityEngine.Random.insideUnitCircle * roamRadius);
    }

    IEnumerator TryRoam() {
        canRoam = false;
        yield return new WaitForSeconds(timeBetweenRoam);
        activeRoamPoint = GetNewRoamPoint();
        canRoam = true;
    }

    Vector2 GetNewPatrolPoint() {
        if(patrolPoints.Count <= 0) {
            Debug.LogError("No patrol points loaded!");
            return Vector2.zero;
        }

        if(patrolStyle == PatrolStyles.Random) {
            int index;
            do {
                index = UnityEngine.Random.Range(0, patrolPoints.Count);
            } while (index == patrolPoints.IndexOf(activePatrolPoint));
            return patrolPoints[index];
        }
        else if (patrolStyle == PatrolStyles.RoundRobin) {
            int index = patrolPoints.IndexOf(activePatrolPoint);

            index =  index < 0 ? 0 : (index + 1) % patrolPoints.Count; // If index is less than 0, set it to zero. Otherwise, increment it / loop around to zero if larger than patrolPoints.Count

            return patrolPoints[index];
        }
        else {
            Debug.LogError("Invalid Patrol Style selected!");
            return Vector2.zero;
        }
    }

    IEnumerator TryPatrol() {
        canPatrol = false;
        yield return new WaitForSeconds(timeBetweenPatrol);
        activePatrolPoint = GetNewPatrolPoint();
        canPatrol = true;
    }
}

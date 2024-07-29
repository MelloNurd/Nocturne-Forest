using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    [HideInInspector] public Vector2 travelDir = Vector2.zero;

    public float attackKnockback = 0.5f;

    public float attackDamage = 5f;

    [SerializeField] float travelSpeed = 1f; // The speed the projectiles travel at

    [SerializeField] float lifetimeLimit = 10f; // Maximum number of seconds the projectile can exist before being deleted

    private void OnEnable() {
        StartCoroutine(TimedDeath());
        //transform.right = travelDir;
    }

    private void OnDisable() {
        StopCoroutine(TimedDeath());
    }

    private void FixedUpdate() {
        transform.position += (Vector3)travelDir * travelSpeed;
    }

    IEnumerator TimedDeath() {
        yield return new WaitForSeconds(lifetimeLimit);
        ObjectPoolManager.ReturnObjectToPool(gameObject);
    }
}

using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class EnemyBase : MonoBehaviour
{
    [Header("Combat")]
    public float maxHealth = 20f;
    public float currentHealth;
    public float attackDamage = 5f;
    public float baseKnockback = 1f;
    public float attackKnockback = 1f;

    [Header("Item Dropping")]
    [SerializeField] LootTable itemDrops;
    GameObject pickupablePrefab;

    protected Player player;
    protected Rigidbody2D rb;

    protected virtual void Awake() {
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
        rb = GetComponent<Rigidbody2D>();
        currentHealth = maxHealth;
    }

    protected virtual void Start() {

    }

    protected virtual void Update() {

    }

    protected bool TakeDamage(float damage) {
        if ((currentHealth -= damage) <= 0) {
            Die();
            return false;
        }
        return true;
    }

    public virtual void Die() {
        foreach (Item item in itemDrops.RollDrops()) {
            DropItem(item);
        }
        Destroy(gameObject);
    }

    public void DropItem(Item item) {
        if(pickupablePrefab == null) pickupablePrefab = InventoryManager.currentInstance.pickupablePrefab; // Unfortunately have to do this here, as Awake is too early to set this and there is no Start for this abstract class
        GameObject droppedItem = ObjectPoolManager.SpawnObject(pickupablePrefab, transform.position, Quaternion.identity, ObjectPoolManager.PoolType.Pickupables);
        droppedItem.GetComponent<Pickupable>().OnDrop(item);
    }
}

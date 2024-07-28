using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EnemyTypes {
    Golbin,
    Slime
}

public abstract class EnemyBase : MonoBehaviour
{
    public EnemyTypes enemyType;

    [Header("Combat")]
    public float maxHealth = 20f;
    public float currentHealth;
    GameObject healthBar;
    Tween healthTween;
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
        healthBar = transform.Find("HealthBarOBJ").Find("HealthBarParent").gameObject;
        healthBar.transform.parent.localScale = Vector2.right;
    }

    protected virtual void Start() {

    }

    protected virtual void Update() {

    }

    protected bool TakeDamage(float damage) {
        if ((currentHealth -= damage) <= 0) {
            UpdateHealthBar();
            Die();
            return false;
        }
        UpdateHealthBar();
        return true;
    }

    public void UpdateHealthBar() {
        if (!gameObject || !gameObject.activeSelf) return;
        healthBar.transform.localScale = new Vector2((currentHealth / maxHealth) * 0.54f, 1);
        healthTween.Kill();
        healthBar.transform.parent.localScale = Vector2.one;
        healthTween = healthBar.transform.parent.DOScale(Vector2.right, 0.5f).SetDelay(3f);
    }

    public virtual void Die() {
        healthTween.Kill();
        string playerPrefsKey = enemyType.ToString() + "_killed";
        PlayerPrefs.SetInt(playerPrefsKey, PlayerPrefs.GetInt(playerPrefsKey, 0) + 1);
        PlayerPrefs.SetInt("total_enemies_killed", PlayerPrefs.GetInt("total_enemies_killed", 0) + 1);
        if (itemDrops != null) {
            List<Item> drops = itemDrops.RollDrops();
            foreach (Item item in itemDrops.RollDrops()) {
                DropItem(item);
            }
        }
        Destroy(gameObject);
    }

    public void DropItem(Item item) {
        if(pickupablePrefab == null) pickupablePrefab = InventoryManager.currentInstance.pickupablePrefab; // Unfortunately have to do this here, as Awake is too early to set this and there is no Start for this abstract class
        GameObject droppedItem = ObjectPoolManager.SpawnObject(pickupablePrefab, transform.position, Quaternion.identity, ObjectPoolManager.PoolType.Pickupables);
        droppedItem.GetComponent<Pickupable>().OnDrop(item);
    }
}

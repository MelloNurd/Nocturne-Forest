using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EnemyTypes {
    Golbin,
    Slime,
    Bat,
    Dryad,
    Gnome,
    Lizardfolk,
    Medusa,
    Ogre,
    Red_Slime
}

public enum RespawnType {
    Persistent,
    Random,
    OneTime
}

public abstract class EnemyBase : MonoBehaviour
{
    public EnemyTypes enemyType;

    [Header("Respawning")]
    public RespawnType respawningType = RespawnType.Persistent;
    [Tooltip("This is only affective when using the \"Random\" respawn type.")] [Range(0, 100)] public int respawnChance = 50;
    [Tooltip("If this is checked, the enemy will respawn even if it has been killed in OneTime respawning.")] public bool overrideOneTime = false;

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
        switch (respawningType) {
            case RespawnType.Persistent:
                PlayerPrefs.SetInt(gameObject.scene + "_" + gameObject.name, 0); // Resets the value keeping track of if it died, for OneTime
                break; // Basically for persistent, do nothing to the gameobject
            case RespawnType.Random:
                PlayerPrefs.SetInt(gameObject.scene + "_" + gameObject.name, 0); // Resets the value keeping track of if it died, for OneTime
                if (Random.Range(1, 101) >= respawnChance) gameObject.SetActive(false);
                break;
            case RespawnType.OneTime:
                int hasDied = PlayerPrefs.GetInt(gameObject.scene + "_" + gameObject.name, 0);
                if(hasDied != 0 && !overrideOneTime) gameObject.SetActive(false);
                break;
            default: break;
        }
    }

    protected virtual void Update() {
        // For now this does nothing, but still need the function as it is virtual    
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
        // Kill healthTween in case it's running. This prevents errors.
        healthTween.Kill();

        // Storing if it has been killed or not. Note: Each enemy has to have a different name within the scene for this to work!
        if(respawningType == RespawnType.OneTime) PlayerPrefs.SetInt(gameObject.scene + "_" + gameObject.name, 1);
        
        // Incrementing the enemy's slain numbers
        string playerPrefsKey = enemyType.ToString() + "_killed";
        PlayerPrefs.SetInt(playerPrefsKey, PlayerPrefs.GetInt(playerPrefsKey, 0) + 1);
        PlayerPrefs.SetInt("total_enemies_killed", PlayerPrefs.GetInt("total_enemies_killed", 0) + 1);

        // Run through item drops
        if (itemDrops != null) {
            List<Item> drops = itemDrops.RollDrops();
            foreach (Item item in itemDrops.RollDrops()) {
                DropItem(item);
            }
        }
        // Destroy the gameObject
        Destroy(gameObject);
    }

    public void DropItem(Item item) {
        if(pickupablePrefab == null) pickupablePrefab = InventoryManager.currentInstance.pickupablePrefab; // Unfortunately have to do this here, as Awake is too early to set this and there is no Start for this abstract class
        GameObject droppedItem = ObjectPoolManager.SpawnObject(pickupablePrefab, transform.position, Quaternion.identity, ObjectPoolManager.PoolType.Pickupables);
        droppedItem.GetComponent<Pickupable>().OnDrop(item);
    }
}

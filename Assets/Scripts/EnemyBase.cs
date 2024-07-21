using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class EnemyBase : MonoBehaviour
{
    [Header("Item Dropping")]
    [SerializeField] LootTable itemDrops;
    float dropRange = 2f; // Maximum distance a dropped item will drop from the player
    float dropStrength = 1.5f; // How high the "jump" arc is when dropping an item
    float dropDuration = 0.6f; // How long it takes for the item to reach it's dropped position
    GameObject pickupablePrefab;

    protected Player player;

    private void Awake() {
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
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
        Pickupable pickupScript = droppedItem.GetComponent<Pickupable>();
        pickupScript.UpdatePickupableObj(item);
        pickupScript.canPickup = false;

        Vector3 originalSize = droppedItem.transform.localScale;
        droppedItem.transform.localScale = originalSize * 0.5f;

        // Using DOTween package. Jump to a random position within dropRange. After animation, run the pickup's OnItemSpawn script.
        droppedItem.transform.DOJump(transform.position + (Vector3)UnityEngine.Random.insideUnitCircle * dropRange, dropStrength, 1, dropDuration).onComplete = pickupScript.OnItemSpawn;
        droppedItem.transform.DOScale(originalSize, dropDuration * 0.8f); // Scales the object up smoothly in dropDuration length * 0.8f (when it's 80% done)
    }
}

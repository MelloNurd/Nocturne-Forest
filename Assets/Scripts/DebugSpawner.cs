using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugSpawner : MonoBehaviour
{
    [SerializeField] int maxColCount = 7;

    Vector3 offset = Vector2.zero;

    // Start is called before the first frame update
    void Start()
    {
        GameObject pickupPrefab = InventoryManager.currentInstance.pickupablePrefab;

        int colIndex = 0;
        foreach(Item item in InventoryManager.currentInstance.globalItemList) {
            if (colIndex >= maxColCount) {
                colIndex = 0;
                offset = new Vector2(0, offset.y - 2.2f);
            }
            Pickupable pickup = Instantiate(pickupPrefab, transform.position + offset, Quaternion.identity).GetComponent<Pickupable>();
            pickup.item = item;
            pickup.UpdateSprite();
            colIndex++;
            offset = new Vector2(offset.x + 2.2f, offset.y);
        }
    }
}

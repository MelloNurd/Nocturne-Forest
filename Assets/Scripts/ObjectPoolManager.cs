using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class ObjectPoolManager : MonoBehaviour
{
    public static List<PooledObjectInfo> ObjectPools = new List<PooledObjectInfo>();

    private GameObject objectPoolsObj;

    private static GameObject projectilesPool;
    private static GameObject pickupablesPool;

    public enum PoolType {
        Projectiles,
        Pickupables,
        None
    }

    public static PoolType PoolingType;

    private void Awake() {
        objectPoolsObj = new GameObject("Object Pools");

        projectilesPool = new GameObject("Projectiles Pool");
        projectilesPool.transform.SetParent(objectPoolsObj.transform);

        pickupablesPool = new GameObject("Pickupables Pool");
        pickupablesPool.transform.SetParent(objectPoolsObj.transform);
    }

    public static GameObject SpawnObject(GameObject objectToSpawn, Vector3 spawnPosition, Quaternion spawnRotation, PoolType poolType) {
        PooledObjectInfo pool = ObjectPools.Find(p => p.LookupString == objectToSpawn.name);

        if (pool == null) // If pool DNE, create one
        {
            pool = new PooledObjectInfo() { LookupString = objectToSpawn.name };
            ObjectPools.Add(pool);
        }

        // Check if any inactive objects in pool
        GameObject spawnableObj = pool.InactiveObjects.FirstOrDefault();

        if (spawnableObj == null) {
            GameObject parentObj = SetParentObject(poolType);

            spawnableObj = Instantiate(objectToSpawn, spawnPosition, spawnRotation);

            if(parentObj != null) spawnableObj.transform.SetParent(parentObj.transform);
        }
        else {
            spawnableObj.transform.position = spawnPosition;
            spawnableObj.transform.rotation = spawnRotation;
            pool.InactiveObjects.Remove(spawnableObj);
            spawnableObj.SetActive(true);
        }

        return spawnableObj;
    }

    private static GameObject SetParentObject(PoolType type) {
        switch(type) {
            case PoolType.Projectiles:
                return projectilesPool;
            case PoolType.Pickupables:
                return pickupablesPool;
            case PoolType.None:
                return null;
            default:
                return null;
        }
    }
    
    public static void ReturnObjectToPool(GameObject obj) {
        string name = obj.name.Substring(0, obj.name.Length - 7); // Removes "(Clone") from the end of the object's name in the tree
        PooledObjectInfo pool = ObjectPools.Find(p => p.LookupString == name);

        if (pool == null) Debug.LogWarning("Trying to return a non-pooled object to a pool: " + obj.name);
        else {
            obj.SetActive(false);
            pool.InactiveObjects.Add(obj);
        }
    }
}

public class PooledObjectInfo {
    public string LookupString;
    public List<GameObject> InactiveObjects = new List<GameObject>();
}
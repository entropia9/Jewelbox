using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPooler : MonoBehaviour
{
    public GameObject pooledObject;
    public int pooledAmount;
    List<GameObject> pooledObjects;

    // create list of objects
    void Awake()
    {
        pooledObjects = new List<GameObject>();
        for (int i = 0; i < pooledAmount; i++)
        {
            GameObject obj = (GameObject)Instantiate(pooledObject);
            obj.transform.SetParent(this.transform, false);
  //          obj.transform.parent = this.transform;
            pooledObjects.Add(obj);
            obj.SetActive(false);


        }
    }

    public GameObject GetPooledObject()
    {
        for (int i = 0; i < pooledObjects.Count; i++)
        {
            if (!pooledObjects[i].activeInHierarchy)
            {
                return pooledObjects[i];
            }

        }

        GameObject obj = (GameObject)Instantiate(pooledObject);
        obj.transform.parent = this.transform;
        pooledObjects.Add(obj);
        obj.SetActive(false);
   
        return obj;
    }

}

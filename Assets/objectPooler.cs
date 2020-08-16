using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class objectPooler : MonoBehaviour
{
    public GameObject[] pooledObject;
    public int[] pooledAmount;
    public bool willGrow = true;

    List<GameObject>[] pool;

    // Start is called before the first frame update
    void Start()
    {
        pool = new List<GameObject>[pooledObject.Length];

        for (int i = 0; i < pool.Length; i++)
        {
            pool[i] = new List<GameObject>();

            for (int x = 0; x < pooledAmount[i]; x++)
            {
                GameObject obj = Instantiate(pooledObject[i]);
                obj.transform.parent = this.transform;
                obj.SetActive(false);
                pool[i].Add(obj);
            }
        }
    }

    public GameObject GetPooledObject(GameObject wantedPrefab = null)
    {
        int idx = 0;
        if(wantedPrefab != null)
        {
            idx = System.Array.IndexOf(pooledObject, wantedPrefab);

            if (idx == -1) return null;
        }


        for (int i = 0; i < pool[idx].Count; i++)
        {
            if (!pool[idx][i].activeInHierarchy)
            {
                return pool[idx][i];
            }
        }

        if (willGrow)
        {
            GameObject obj = Instantiate(pooledObject[idx]);
            obj.transform.parent = this.transform;
            pool[idx].Add(obj);

            return obj;
        }

        return null;
    }
}

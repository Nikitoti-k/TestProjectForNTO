using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{
    private GameObject prefab;
    private List<GameObject> pool = new List<GameObject>();
    private Transform parent;

    public void Initialize(GameObject prefab, int initialSize, Transform parent = null)
    {
        this.prefab = prefab;
        this.parent = parent;
        for (int i = 0; i < initialSize; i++)
        {
            GameObject obj = Instantiate(prefab, parent);
            obj.SetActive(false);
            pool.Add(obj);
        }
    }

    public GameObject Get()
    {
        foreach (GameObject obj in pool)
        {
            if (!obj.activeInHierarchy)
            {
                obj.SetActive(true);
                return obj;
            }
        }

        GameObject newObj = Instantiate(prefab, parent);
        pool.Add(newObj);
        return newObj;
    }

    public void Return(GameObject obj)
    {
        obj.SetActive(false);
    }

    public void ReturnAll()
    {
        foreach (GameObject obj in pool)
        {
            obj.SetActive(false);
        }
    }
}
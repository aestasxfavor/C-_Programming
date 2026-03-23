using System.Collections.Generic;
using UnityEngine;

public class MolePool : MonoBehaviour
{
    [Header("Pool Setting")]
    [SerializeField] private Mole molePrefab;
    [SerializeField] private int poolSize = 9;

    private List<Mole> pool = new List<Mole>();

    private void Awake()
    {
        CreatePool();
    }

    private void CreatePool()
    {
        for (int i = 0; i < poolSize; i++)
        {
            Mole mole = Instantiate(molePrefab, transform);
            mole.gameObject.SetActive(false);
            pool.Add(mole);
        }
    }

    public Mole GetMole()
    {
        for (int i = 0; i < pool.Count; i++)
        {
            if (!pool[i].IsActive)
            {
                return pool[i];
            }
        }

        return null;
    }

    public int GetPoolCount()
    {
        return pool.Count;
    }

    public void ForceHideAll()
    {
        for (int i = 0; i < pool.Count; i++)
        {
            if (pool[i].gameObject.activeSelf)
            {
                pool[i].ForceHide();
            }
        }
    }
}
using System.Collections.Generic;
using UnityEngine;

public class MolePool : MonoBehaviour
{
    [Header("Pool Setting")]
    [SerializeField] private Mole molePrefab;
    [SerializeField] private int poolSize = 9;

    private List<Mole> molePool = new List<Mole>();

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
            molePool.Add(mole);
        }
    }

    public Mole GetMole()
    {
        for (int i = 0; i < molePool.Count; i++)
        {
            if (!molePool[i].IsActive)
            {
                return molePool[i];
            }
        }

        return null;
    }

    public int GetPoolCount()
    {
        return molePool.Count;
    }

    public void HideAllMoles()
    {
        for (int i = 0; i < molePool.Count; i++)
        {
            if (molePool[i].gameObject.activeSelf)
            {
                molePool[i].HideImmediately();
            }
        }
    }
}
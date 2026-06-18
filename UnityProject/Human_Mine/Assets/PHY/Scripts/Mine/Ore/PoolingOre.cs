using UnityEngine;

public class PoolingOre : MonoBehaviour
{
    private OreSpawner spawner;
    private Transform currentSpawnPoint;

    public Transform CurrentSpawnPoint => currentSpawnPoint;

    public void Init(OreSpawner owner)
    {
        spawner = owner;
    }

    public void SetSpawnPoint(Transform spawnPoint)
    {
        currentSpawnPoint = spawnPoint;

        transform.position = spawnPoint.position;
        transform.rotation = spawnPoint.rotation;
    }

    public void Despawn()
    {
        if (spawner != null)
        {
            spawner.ReleaseOre(this);
            return;
        }

        gameObject.SetActive(false);
    }
}

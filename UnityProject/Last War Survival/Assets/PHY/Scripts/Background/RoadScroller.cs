using UnityEngine;

public class RoadScroller : MonoBehaviour
{
    [Header("Road Chunks")]
    [SerializeField] private Transform[] roadChunks;

    [Header("Scroll Settings")]
    [SerializeField] private float scrollSpeed = 8f;
    [SerializeField] private float chunkLength = 15f;
    [SerializeField] private float recycleZ = -15f;

    [Header("State")]
    [SerializeField] private bool canScroll = true;

    private void Update()
    {
        if (!canScroll)
        {
            return;
        }

        if (!HasRoadChunks())
        {
            return;
        }

        MoveRoadChunks();
        RepositionPassedChunks();
    }

    private void MoveRoadChunks()
    {
        for (int i = 0; i < roadChunks.Length; i++)
        {
            if (roadChunks[i] == null)
            {
                continue;
            }

            roadChunks[i].position += Vector3.back * scrollSpeed * Time.deltaTime;
        }
    }

    private void RepositionPassedChunks()
    {
        for (int i = 0; i < roadChunks.Length; i++)
        {
            if (roadChunks[i] == null)
            {
                continue;
            }

            if (roadChunks[i].position.z <= recycleZ)
            {
                MoveChunkToFront(roadChunks[i]);
            }
        }
    }

    private void MoveChunkToFront(Transform chunk)
    {
        float frontZ = GetFrontChunkZ();

        Vector3 nextPosition = chunk.position;
        nextPosition.z = frontZ + chunkLength;

        chunk.position = nextPosition;
    }

    private float GetFrontChunkZ()
    {
        float frontZ = roadChunks[0].position.z;

        for (int i = 1; i < roadChunks.Length; i++)
        {
            if (roadChunks[i] == null)
            {
                continue;
            }

            if (roadChunks[i].position.z > frontZ)
            {
                frontZ = roadChunks[i].position.z;
            }
        }

        return frontZ;
    }

    private bool HasRoadChunks()
    {
        return roadChunks != null && roadChunks.Length > 0 && roadChunks[0] != null;
    }

    public void StopScroll()
    {
        canScroll = false;
    }

    public void ResumeScroll()
    {
        canScroll = true;
    }

    public void SetScrollSpeed(float speed)
    {
        scrollSpeed = speed;
    }
}
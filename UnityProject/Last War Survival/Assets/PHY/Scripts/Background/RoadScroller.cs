using UnityEngine;

public class RoadScroller : MonoBehaviour
{
    [Header("Road Chunks")]
    [SerializeField] private Transform[] roadChunks;

    [Header("Scroll Settings")]
    [SerializeField] private float scrollSpeed = 8f;
    [SerializeField] private float chunkLength = 15f;
    [SerializeField] private float resetZ = -15f;

    [Header("State")]
    [SerializeField] private bool canScroll = true;

    private void Update()
    {
        if (!canScroll)
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
            roadChunks[i].position += Vector3.back * scrollSpeed * Time.deltaTime;
        }
    }

    private void RepositionPassedChunks()
    {
        for (int i = 0; i < roadChunks.Length; i++)
        {
            if (roadChunks[i].position.z <= resetZ)
            {
                MoveChunkToFront(roadChunks[i]);
            }
        }
    }

    private void MoveChunkToFront(Transform chunk)
    {
        float frontZ = GetFrontChunkZ();

        Vector3 newPosition = chunk.position;
        newPosition.z = frontZ + chunkLength;
        chunk.position = newPosition;
    }

    private float GetFrontChunkZ()
    {
        float maxZ = roadChunks[0].position.z;

        for (int i = 1; i < roadChunks.Length; i++)
        {
            if (roadChunks[i].position.z > maxZ)
            {
                maxZ = roadChunks[i].position.z;
            }
        }

        return maxZ;
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

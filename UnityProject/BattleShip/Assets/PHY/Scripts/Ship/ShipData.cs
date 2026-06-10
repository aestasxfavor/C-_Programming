using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ShipData
{
    public int shipId;
    public int size;
    public List<Vector2Int> positions = new List<Vector2Int>();
    public bool isPlaced;

    public ShipData(int _shipId, int _size)
    {
        shipId = _shipId;
        size = _size;
        isPlaced = false;
    }

}

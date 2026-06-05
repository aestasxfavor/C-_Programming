using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ShipData
{
    public int shipID;
    public int size;
    public List<Vector2Int> positions = new List<Vector2Int>();
    public bool isPlaced;

    public ShipData(int _shipID, int _size)
    {
        shipID = _shipID;
        size = _size;
        isPlaced = false;
    }

}

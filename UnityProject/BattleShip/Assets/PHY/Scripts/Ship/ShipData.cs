using System.Collections.Generic;
using UnityEngine;

// 함선 ID, 크기, 배치 위치, 배치 완료 여부를 저장하는 런타임 함선 데이터
[System.Serializable]
public class ShipData
{
    // TODO: 함선 ID, 크기, 표시용 Sprite는 추후 ShipDefinitionSO로 분리 가능
    // 런타임 중 변하는 배치 위치와 배치 여부는 SO가 아닌 ShipData 인스턴스에서 관리
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

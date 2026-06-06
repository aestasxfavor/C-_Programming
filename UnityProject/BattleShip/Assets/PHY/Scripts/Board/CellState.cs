using UnityEngine;

public enum CellState
{
    Empty,      // 물
    Ship,       // 배
    Blocked,    // 배 주변 배치 금지(8칸)
    Land,       // 육지
    Hit,        // 명중   x
    Miss        // 빗나감 ㅇ
}

public enum AttackResult
{
    Invalid,
    Hit,
    Miss,
    Sunk,
    GameOver
}
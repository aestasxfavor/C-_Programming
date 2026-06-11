using UnityEngine;

// 보드 한 칸의 현재 상태를 나타내는 enum
public enum CellState
{
    Empty,      // 물
    Ship,       // 배
    Blocked,    // 배 주변 배치 금지(8칸)
    Land,       // 육지
    Hit,        // 명중   x
    Miss,        // 빗나감 ㅇ
    SunkShip    // 침볼한 배
}

// 공격 판정 결과를 나타내는 enum
public enum AttackResult
{
    Invalid,
    Hit,
    Miss,
    Sunk,
    GameOver
}
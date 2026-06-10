using System;
using System.Collections.Generic;
using UnityEngine;

public class BattleAttackController
{
    private readonly int boardSize;
    private readonly CellState[,] boardStates;
    private readonly int[,] shipIDByCell;
    private readonly ShipData[] ships;
    private readonly Action<int, int> refreshCell;

    private string lastSunkAroundPositionsText = "";
    private string lastSunkShipId = "";

    public BattleAttackController(
        int _boardSize,
        CellState[,] _boardStates,
        int[,] _shipIDByCell,
        ShipData[] _ships,
        Action<int, int> _refreshCell
    )
    {
        boardSize = _boardSize;
        boardStates = _boardStates;
        shipIDByCell = _shipIDByCell;
        ships = _ships;
        refreshCell = _refreshCell;
    }

    public AttackResult ReceiveAttack(int x, int y)
    {
        ClearLastSunkResult();

        if (!CanAttackCell(x, y))
        {
            return AttackResult.Invalid;
        }

        CellState state = boardStates[x, y];

        if (state == CellState.Ship)
        {
            return HandleHit(x, y);
        }

        if (state == CellState.Empty || state == CellState.Blocked)
        {
            return HandleMiss(x, y);
        }

        return AttackResult.Invalid;
    }

    public void ApplyAttackResult(int x, int y, string resultText, string aroundPositionsText)
    {
        if (!IsInsideBoard(x, y))
        {
            Debug.LogWarning($"[Result] 결과 적용 실패: 보드 밖 좌표 X={x}, Y={y}");
            return;
        }

        switch (resultText)
        {
            case "HIT":
                ApplyHitResult(x, y);
                break;

            case "MISS":
                ApplyMissResult(x, y);
                break;

            case "SUNK":
                ApplyHitResult(x, y);
                ApplyAroundMissPositions(aroundPositionsText);
                break;

            case "GAME_OVER":
                ApplyHitResult(x, y);
                ApplyAroundMissPositions(aroundPositionsText);
                break;

            default:
                Debug.LogWarning($"[Result] 알 수 없는 결과 타입: {resultText}");
                break;
        }
    }

    public bool CanRequestAttack(int x, int y)
    {
        return CanAttackCell(x, y);
    }

    public string GetLastSunkAroundPositionsText()
    {
        return lastSunkAroundPositionsText;
    }

    public string GetLastSunkShipId()
    {
        return lastSunkShipId;
    }

    public void ClearLastSunkResult()
    {
        lastSunkShipId = "";
        lastSunkAroundPositionsText = "";
    }

    private AttackResult HandleHit(int x, int y)
    {
        int shipID = shipIDByCell[x, y];

        boardStates[x, y] = CellState.Hit;
        RefreshCell(x, y);

        Debug.Log($"[Battle] Hit X={x}, Y={y}, ShipID={shipID}");

        if (IsShipSunk(shipID))
        {
            lastSunkShipId = GetShipStatusId(shipID);

            MarkMissAroundSunkShip(shipID);

            if (IsAllShipsSunk())
            {
                Debug.Log($"[Battle] 모든 함선 침몰, LastSunkShip={lastSunkShipId}");
                return AttackResult.GameOver;
            }

            Debug.Log($"[Battle] Sunk ShipID={shipID}, LastSunkShip={lastSunkShipId}");
            return AttackResult.Sunk;
        }

        return AttackResult.Hit;
    }

    private AttackResult HandleMiss(int x, int y)
    {
        boardStates[x, y] = CellState.Miss;
        RefreshCell(x, y);

        Debug.Log($"[Battle] Miss X={x}, Y={y}");
        return AttackResult.Miss;
    }

    private void ApplyHitResult(int x, int y)
    {
        boardStates[x, y] = CellState.Hit;
        RefreshCell(x, y);

        Debug.Log($"[EnemyBoard] Hit 표시 X={x}, Y={y}");
    }

    private void ApplyMissResult(int x, int y)
    {
        boardStates[x, y] = CellState.Miss;
        RefreshCell(x, y);

        Debug.Log($"[EnemyBoard] Miss 표시 X={x}, Y={y}");
    }

    private void ApplyAroundMissPositions(string aroundPositionsText)
    {
        if (string.IsNullOrEmpty(aroundPositionsText))
        {
            return;
        }

        string[] positions = aroundPositionsText.Split(';');

        for (int i = 0; i < positions.Length; i++)
        {
            string positionText = positions[i];

            if (string.IsNullOrEmpty(positionText))
            {
                continue;
            }

            string[] xy = positionText.Split(',');

            if (xy.Length < 2)
            {
                continue;
            }

            if (!int.TryParse(xy[0], out int x) || !int.TryParse(xy[1], out int y))
            {
                continue;
            }

            if (!IsInsideBoard(x, y))
            {
                continue;
            }

            CellState state = boardStates[x, y];

            if (state == CellState.Hit || state == CellState.Miss || state == CellState.Land)
            {
                continue;
            }

            boardStates[x, y] = CellState.Miss;
            RefreshCell(x, y);

            Debug.Log($"[EnemyBoard] 침몰 주변 Miss 표시 X={x}, Y={y}");
        }
    }

    private bool CanAttackCell(int x, int y)
    {
        if (!IsInsideBoard(x, y))
        {
            return false;
        }

        CellState state = boardStates[x, y];

        if (state == CellState.Hit || state == CellState.Miss)
        {
            return false;
        }

        if (state == CellState.Land)
        {
            return false;
        }

        return true;
    }

    private bool IsShipSunk(int shipID)
    {
        if (shipID < 0 || shipID >= ships.Length)
        {
            return false;
        }

        ShipData ship = ships[shipID];

        for (int i = 0; i < ship.positions.Count; i++)
        {
            Vector2Int position = ship.positions[i];

            if (!IsInsideBoard(position.x, position.y))
            {
                continue;
            }

            if (boardStates[position.x, position.y] != CellState.Hit)
            {
                return false;
            }
        }

        return true;
    }

    private void MarkMissAroundSunkShip(int shipID)
    {
        lastSunkAroundPositionsText = "";

        if (shipID < 0 || shipID >= ships.Length)
        {
            return;
        }

        ShipData ship = ships[shipID];

        HashSet<Vector2Int> aroundMissPositions = new HashSet<Vector2Int>();

        for (int i = 0; i < ship.positions.Count; i++)
        {
            Vector2Int shipPosition = ship.positions[i];

            for (int y = -1; y <= 1; y++)
            {
                for (int x = -1; x <= 1; x++)
                {
                    if (x == 0 && y == 0)
                    {
                        continue;
                    }

                    int checkX = shipPosition.x + x;
                    int checkY = shipPosition.y + y;

                    if (!IsInsideBoard(checkX, checkY))
                    {
                        continue;
                    }

                    CellState aroundState = boardStates[checkX, checkY];

                    if (aroundState == CellState.Hit || aroundState == CellState.Miss)
                    {
                        continue;
                    }

                    if (aroundState == CellState.Ship)
                    {
                        continue;
                    }

                    if (aroundState == CellState.Land)
                    {
                        continue;
                    }

                    if (aroundState == CellState.Empty || aroundState == CellState.Blocked)
                    {
                        boardStates[checkX, checkY] = CellState.Miss;
                        RefreshCell(checkX, checkY);

                        aroundMissPositions.Add(new Vector2Int(checkX, checkY));
                    }
                }
            }
        }

        lastSunkAroundPositionsText = ConvertPositionsToText(aroundMissPositions);
    }

    private string ConvertPositionsToText(HashSet<Vector2Int> positions)
    {
        if (positions == null || positions.Count == 0)
        {
            return "";
        }

        List<string> positionTexts = new List<string>();

        foreach (Vector2Int position in positions)
        {
            positionTexts.Add($"{position.x},{position.y}");
        }

        return string.Join(";", positionTexts);
    }

    private bool IsAllShipsSunk()
    {
        for (int i = 0; i < ships.Length; i++)
        {
            if (!ships[i].isPlaced)
            {
                return false;
            }

            if (!IsShipSunk(ships[i].shipID))
            {
                return false;
            }
        }

        return true;
    }

    private string GetShipStatusId(int shipID)
    {
        switch (shipID)
        {
            case 0:
                return "Ship2";

            case 1:
                return "Ship3A";

            case 2:
                return "Ship3B";

            case 3:
                return "Ship4";

            case 4:
                return "Ship5";

            default:
                return "";
        }
    }

    private void RefreshCell(int x, int y)
    {
        refreshCell?.Invoke(x, y);
    }

    private bool IsInsideBoard(int x, int y)
    {
        return x >= 0 && x < boardSize && y >= 0 && y < boardSize;
    }
}
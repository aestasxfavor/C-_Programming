using System;
using System.Collections.Generic;
using UnityEngine;

public class BattleAttackController
{
    private readonly int boardSize;
    private readonly CellState[,] boardStates;
    private readonly int[,] shipIdByCell;
    private readonly ShipData[] ships;
    private readonly Action<int, int> refreshCell;
    private readonly Action<List<Vector2Int>, string> showSunkShipVisual;

    private string lastSunkAroundPositionsText = "";
    private string lastSunkShipPositionsText = "";
    private string lastSunkShipId = "";

    public BattleAttackController(
        int boardSize,
        CellState[,] boardStates,
        int[,] shipIdByCell,
        ShipData[] ships,
        Action<int, int> refreshCell,
        Action<List<Vector2Int>, string> showSunkShipVisual = null
    )
    {
        this.boardSize = boardSize;
        this.boardStates = boardStates;
        this.shipIdByCell = shipIdByCell;
        this.ships = ships;
        this.refreshCell = refreshCell;
        this.showSunkShipVisual = showSunkShipVisual;
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

    public void ApplyAttackResult(
        int x,
        int y,
        string resultText,
        string sunkShipId,
        string aroundPositionsText,
        string sunkShipPositionsText
    )
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
                ApplySunkShipPositions(sunkShipPositionsText, sunkShipId);
                ApplyAroundMissPositions(aroundPositionsText);
                break;

            case "GAME_OVER":
                ApplyHitResult(x, y);
                ApplySunkShipPositions(sunkShipPositionsText, sunkShipId);
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

    public string GetLastSunkShipPositionsText()
    {
        return lastSunkShipPositionsText;
    }

    public string GetLastSunkShipId()
    {
        return lastSunkShipId;
    }

    public void ClearLastSunkResult()
    {
        lastSunkShipId = "";
        lastSunkAroundPositionsText = "";
        lastSunkShipPositionsText = "";
    }

    private AttackResult HandleHit(int x, int y)
    {
        int shipId = shipIdByCell[x, y];

        boardStates[x, y] = CellState.Hit;
        RefreshCell(x, y);

        Debug.Log($"[Battle] Hit X={x}, Y={y}, ShipId={shipId}");

        if (IsShipSunk(shipId))
        {
            lastSunkShipId = GetShipStatusId(shipId);
            lastSunkShipPositionsText = ConvertShipPositionsToText(shipId);

            MarkMissAroundSunkShip(shipId);

            if (IsAllShipsSunk())
            {
                Debug.Log($"[Battle] 모든 함선 침몰, LastSunkShip={lastSunkShipId}");
                return AttackResult.GameOver;
            }

            Debug.Log($"[Battle] Sunk ShipId={shipId}, LastSunkShip={lastSunkShipId}");
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

    private void ApplySunkShipPositions(string sunkShipPositionsText, string sunkShipId)
    {
        if (string.IsNullOrEmpty(sunkShipPositionsText))
        {
            return;
        }

        List<Vector2Int> sunkShipPositions = ParsePositions(sunkShipPositionsText);

        if (sunkShipPositions.Count == 0)
        {
            return;
        }

        for (int i = 0; i < sunkShipPositions.Count; i++)
        {
            Vector2Int position = sunkShipPositions[i];

            if (!IsInsideBoard(position.x, position.y))
            {
                continue;
            }

            if (boardStates[position.x, position.y] == CellState.Land)
            {
                continue;
            }

            boardStates[position.x, position.y] = CellState.Hit;
            RefreshCell(position.x, position.y);

            Debug.Log($"[EnemyBoard] 침몰 배 위치 Hit 표시 X={position.x}, Y={position.y}");
        }

        showSunkShipVisual?.Invoke(sunkShipPositions, sunkShipId);
    }

    private void ApplyAroundMissPositions(string aroundPositionsText)
    {
        if (string.IsNullOrEmpty(aroundPositionsText))
        {
            return;
        }

        List<Vector2Int> aroundPositions = ParsePositions(aroundPositionsText);

        for (int i = 0; i < aroundPositions.Count; i++)
        {
            Vector2Int position = aroundPositions[i];

            if (!IsInsideBoard(position.x, position.y))
            {
                continue;
            }

            CellState state = boardStates[position.x, position.y];

            if (state == CellState.Hit || state == CellState.Miss || state == CellState.Land || state == CellState.SunkShip)
            {
                continue;
            }

            boardStates[position.x, position.y] = CellState.Miss;
            RefreshCell(position.x, position.y);

            Debug.Log($"[EnemyBoard] 침몰 주변 Miss 표시 X={position.x}, Y={position.y}");
        }
    }

    private bool CanAttackCell(int x, int y)
    {
        if (!IsInsideBoard(x, y))
        {
            return false;
        }

        CellState state = boardStates[x, y];

        if (state == CellState.Hit || state == CellState.Miss || state == CellState.SunkShip)
        {
            return false;
        }

        if (state == CellState.Land)
        {
            return false;
        }

        return true;
    }

    private bool IsShipSunk(int shipId)
    {
        if (shipId < 0 || shipId >= ships.Length)
        {
            return false;
        }

        ShipData ship = ships[shipId];

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

    private void MarkMissAroundSunkShip(int shipId)
    {
        lastSunkAroundPositionsText = "";

        if (shipId < 0 || shipId >= ships.Length)
        {
            return;
        }

        ShipData ship = ships[shipId];

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

                    if (aroundState == CellState.Hit || aroundState == CellState.Miss || aroundState == CellState.SunkShip)
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

    private string ConvertShipPositionsToText(int shipId)
    {
        if (shipId < 0 || shipId >= ships.Length)
        {
            return "";
        }

        ShipData ship = ships[shipId];

        if (ship == null || ship.positions == null || ship.positions.Count == 0)
        {
            return "";
        }

        return ConvertPositionsToText(ship.positions);
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

    private string ConvertPositionsToText(List<Vector2Int> positions)
    {
        if (positions == null || positions.Count == 0)
        {
            return "";
        }

        List<string> positionTexts = new List<string>();

        for (int i = 0; i < positions.Count; i++)
        {
            Vector2Int position = positions[i];

            if (!IsInsideBoard(position.x, position.y))
            {
                continue;
            }

            positionTexts.Add($"{position.x},{position.y}");
        }

        return string.Join(";", positionTexts);
    }

    private List<Vector2Int> ParsePositions(string positionsText)
    {
        List<Vector2Int> positions = new List<Vector2Int>();

        if (string.IsNullOrEmpty(positionsText))
        {
            return positions;
        }

        string[] splitPositions = positionsText.Split(';');

        for (int i = 0; i < splitPositions.Length; i++)
        {
            string positionText = splitPositions[i];

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

            positions.Add(new Vector2Int(x, y));
        }

        return positions;
    }

    private bool IsAllShipsSunk()
    {
        for (int i = 0; i < ships.Length; i++)
        {
            if (!ships[i].isPlaced)
            {
                return false;
            }

            if (!IsShipSunk(ships[i].shipId))
            {
                return false;
            }
        }

        return true;
    }

    private string GetShipStatusId(int shipId)
    {
        switch (shipId)
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
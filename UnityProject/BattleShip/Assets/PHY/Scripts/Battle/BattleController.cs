using System;
using UnityEngine;

// 공격 요청, 공격 결과 처리, 턴 전환, 턴 타이머, 승패 판정을 담당하는 전투 컨트롤러
public class BattleController : MonoBehaviour
{
    #region 인스펙터 필드

    [Header("보드 연결")]
    [SerializeField] private BoardView myBoardView;
    [SerializeField] private BoardView enemyBoardView;

    [Header("UI 컨트롤러")]
    [SerializeField] private BattleUIController battleUIController;

    // TODO: 턴 제한 시간은 추후 BattleShipRuleConfigSO로 분리 가능
    [Header("턴 시간")]
    [SerializeField] private float turnTimeLimit = 15f;
    [SerializeField] private float turnTimer;
    [SerializeField] private bool isTurnTimerRunning;

    [Header("전투 상태")]
    [SerializeField] private bool isMyTurn;
    [SerializeField] private bool isWaitingResult;
    [SerializeField] private bool isGameOver;

    #endregion

    #region 외부 콜백 / 상태 프로퍼티

    private Func<bool> checkDisconnected;
    private Func<bool> checkRestarting;
    private Func<bool> checkLeaving;
    private Func<GameState> getGameState;
    private Action<GameState> setGameState;
    private Func<string, bool> packetSender;
    private Action updateStatusText;

    public bool IsMyTurn => isMyTurn;
    public bool IsWaitingResult => isWaitingResult;
    public bool IsGameOver => isGameOver;

    #endregion

    #region 초기화 / 전투 상태 제어

    public void Setup(
        Func<bool> _disconnectedCheck,
        Func<bool> _restartingCheck,
        Func<bool> _leavingCheck,
        Func<GameState> _gameStateGetter,
        Action<GameState> _gameStateSetter,
        Func<string, bool> _packetSender,
        Action _statusTextUpdater)
    {
        checkDisconnected = _disconnectedCheck;
        checkRestarting = _restartingCheck;
        checkLeaving = _leavingCheck;
        getGameState = _gameStateGetter;
        setGameState = _gameStateSetter;
        packetSender = _packetSender;
        updateStatusText = _statusTextUpdater;
    }

    public void ResetBattle()
    {
        isMyTurn = false;
        isWaitingResult = false;
        isGameOver = false;
        isTurnTimerRunning = false;
        turnTimer = 0f;

        ClearTurnTimeText();
    }

    public void StartBattle(bool startWithMyTurn)
    {
        isGameOver = false;
        isWaitingResult = false;
        isMyTurn = startWithMyTurn;

        updateStatusText?.Invoke();

        if (isMyTurn)
        {
            Debug.Log("[Turn] 내 턴 시작");
            StartTurnTimer();
        }
        else
        {
            Debug.Log("[Turn] 상대 턴 대기");
            StopTurnTimer();
        }
    }

    public void UpdateBattle()
    {
        UpdateTurnTimer();
    }

    public void StopBattle()
    {
        isMyTurn = false;
        isWaitingResult = false;
        isGameOver = true;

        StopTurnTimer();
        updateStatusText?.Invoke();
    }

    #endregion

    #region 공격 요청 / 패킷 수신 처리

    // 내 턴에 상대 보드 공격을 요청하고 ATTACK 패킷 전송
    public void TryAttackEnemyBoard(int x, int y)
    {
        if (IsDisconnected())
        {
            Debug.Log("[Network] 연결 끊김 상태라 공격 불가");
            return;
        }

        if (IsRestarting() || IsLeaving())
        {
            Debug.Log("[Battle] 씬 전환 중이라 공격 불가");
            return;
        }

        if (getGameState == null || getGameState() != GameState.Battle)
        {
            Debug.Log("[Battle] 전투 단계가 아니라 공격 불가");
            return;
        }

        if (isGameOver)
        {
            Debug.Log("[Battle] 이미 게임 종료 상태");
            return;
        }

        if (enemyBoardView == null)
        {
            Debug.LogError("[Battle] enemyBoardView 연결 필요");
            return;
        }

        if (!isMyTurn)
        {
            Debug.Log("[Turn] 내 턴이 아니라 공격 불가");
            return;
        }

        if (isWaitingResult)
        {
            Debug.Log("[Turn] 이전 공격 결과 대기 중");
            return;
        }

        if (!enemyBoardView.CanRequestAttack(x, y))
        {
            Debug.Log($"[Battle] 공격 불가 칸 X={x}, Y={y}");
            return;
        }

        string packet = $"{PacketProtocol.ATTACK}|{x}|{y}";

        if (!SendPacket(packet))
        {
            return;
        }

        isWaitingResult = true;

        updateStatusText?.Invoke();
        StopTurnTimer();

        Debug.Log($"[Attack] 공격 패킷 전송 X={x}, Y={y}");
    }

    // 상대가 보낸 ATTACK 패킷을 내 보드에 판정하고 RESULT 패킷으로 응답
    public void ReceiveAttackPacket(string[] packetParts)
    {
        if (packetParts.Length < 3)
        {
            Debug.LogWarning("[Attack] ATTACK 패킷 형식 오류");
            return;
        }

        if (getGameState == null || getGameState() != GameState.Battle)
        {
            Debug.Log("[Attack] Battle 단계가 아니라 ATTACK 무시");
            return;
        }

        if (isGameOver)
        {
            Debug.Log("[Attack] 이미 게임 종료 상태라 ATTACK 무시");
            return;
        }

        if (IsRestarting() || IsLeaving())
        {
            Debug.Log("[Attack] 씬 전환 중이라 ATTACK 무시");
            return;
        }

        if (myBoardView == null)
        {
            Debug.LogError("[Attack] myBoardView 연결 필요");
            return;
        }

        if (!int.TryParse(packetParts[1], out int x) || !int.TryParse(packetParts[2], out int y))
        {
            Debug.LogWarning("[Attack] 좌표 파싱 실패");
            return;
        }

        Debug.Log($"[Attack] 공격 패킷 수신 X={x}, Y={y}");

        AttackResult result = myBoardView.ReceiveAttack(x, y);
        string resultText = ConvertAttackResultToPacketText(result);

        string sunkShipId = "-";
        string aroundPositionsText = "-";
        string sunkShipPositionsText = "-";

        if (result == AttackResult.Sunk || result == AttackResult.GameOver)
        {
            sunkShipId = myBoardView.GetLastSunkShipId();
            aroundPositionsText = myBoardView.GetLastSunkAroundPositionsText();
            sunkShipPositionsText = myBoardView.GetLastSunkShipPositionsText();

            if (string.IsNullOrEmpty(sunkShipId))
            {
                sunkShipId = "-";
            }

            if (string.IsNullOrEmpty(aroundPositionsText))
            {
                aroundPositionsText = "-";
            }

            if (string.IsNullOrEmpty(sunkShipPositionsText))
            {
                sunkShipPositionsText = "-";
            }

            MarkMyShipSunk(sunkShipId);
        }

        string resultPacket = $"{PacketProtocol.RESULT}|{x}|{y}|{resultText}|{sunkShipId}|{aroundPositionsText}|{sunkShipPositionsText}";

        SendPacket(resultPacket);

        Debug.Log($"[Result] 결과 패킷 전송 {resultText}, X={x}, Y={y}, Ship={sunkShipId}");

        if (result == AttackResult.Invalid)
        {
            Debug.Log("[Attack] 유효하지 않은 공격이라 턴 변경 없음");
            return;
        }

        if (result == AttackResult.GameOver)
        {
            SetGameOver(false);
            return;
        }

        isWaitingResult = false;

        if (result == AttackResult.Miss)
        {
            isMyTurn = true;

            Debug.Log("[Turn] 상대 공격이 빗나감, 내 턴 시작");

            updateStatusText?.Invoke();
            StartTurnTimer();
        }
        else if (result == AttackResult.Hit || result == AttackResult.Sunk)
        {
            isMyTurn = false;

            Debug.Log("[Turn] 상대 공격이 명중함, 상대 턴 유지");

            updateStatusText?.Invoke();
            StopTurnTimer();
        }
    }

    // 상대가 보낸 RESULT 패킷을 상대 보드 표시와 턴 상태에 반영
    public void ReceiveResultPacket(string[] packetParts)
    {
        if (packetParts.Length < 6)
        {
            Debug.LogWarning("[Result] RESULT 패킷 형식 오류");
            return;
        }

        if (!int.TryParse(packetParts[1], out int x) || !int.TryParse(packetParts[2], out int y))
        {
            Debug.LogWarning("[Result] 좌표 파싱 실패");
            return;
        }

        string resultText = packetParts[3];
        string sunkShipId = packetParts[4];
        string aroundPositionsText = packetParts[5];
        string sunkShipPositionsText = "";

        if (packetParts.Length >= 7)
        {
            sunkShipPositionsText = packetParts[6];
        }

        if (sunkShipId == "-")
        {
            sunkShipId = "";
        }

        if (aroundPositionsText == "-")
        {
            aroundPositionsText = "";
        }

        if (sunkShipPositionsText == "-")
        {
            sunkShipPositionsText = "";
        }

        Debug.Log($"[Result] 결과 수신 {resultText}, X={x}, Y={y}, Ship={sunkShipId}");

        isWaitingResult = false;

        if (resultText == "INVALID")
        {
            isMyTurn = true;

            Debug.Log("[Result] INVALID 수신, 내 턴 유지");

            updateStatusText?.Invoke();
            StartTurnTimer();
            return;
        }

        if (enemyBoardView == null)
        {
            Debug.LogError("[Result] enemyBoardView 연결 필요");
            return;
        }

        enemyBoardView.ApplyAttackResult(
            x,
            y,
            resultText,
            sunkShipId,
            aroundPositionsText,
            sunkShipPositionsText
        );

        if (!string.IsNullOrEmpty(sunkShipId))
        {
            MarkEnemyShipSunk(sunkShipId);
        }

        if (resultText == "GAME_OVER")
        {
            SetGameOver(true);
            return;
        }

        if (resultText == "MISS")
        {
            isMyTurn = false;

            Debug.Log("[Turn] 공격 실패, 상대 턴 대기");

            updateStatusText?.Invoke();
            StopTurnTimer();
        }
        else if (resultText == "HIT" || resultText == "SUNK")
        {
            isMyTurn = true;

            Debug.Log("[Turn] 공격 성공, 내 턴 유지");

            updateStatusText?.Invoke();
            StartTurnTimer();
        }
    }

    public void ReceiveGameOverPacket()
    {
        if (isGameOver)
        {
            return;
        }

        SetGameOver(true);
    }

    public void ReceiveTurnTimeoutPacket()
    {
        if (getGameState == null || getGameState() != GameState.Battle)
        {
            return;
        }

        if (isGameOver)
        {
            return;
        }

        if (IsRestarting() || IsLeaving())
        {
            return;
        }

        isMyTurn = true;
        isWaitingResult = false;

        Debug.Log("[TurnTimer] 상대 시간 초과, 내 턴 시작");

        updateStatusText?.Invoke();
        StartTurnTimer();
    }

    #endregion

    #region 게임 종료 처리

    private void SetGameOver(bool isWin)
    {
        if (IsDisconnected())
        {
            return;
        }

        if (IsRestarting() || IsLeaving())
        {
            return;
        }

        isGameOver = true;
        isMyTurn = false;
        isWaitingResult = false;

        StopTurnTimer();

        setGameState?.Invoke(GameState.GameOver);

        ShowGameOverUI(isWin);
        updateStatusText?.Invoke();

        if (isWin)
        {
            Debug.Log("[GameOver] 승리");
        }
        else
        {
            Debug.Log("[GameOver] 패배");
        }
    }

    #endregion

    #region 턴 타이머

    private void StartTurnTimer()
    {
        if (IsDisconnected())
        {
            return;
        }

        if (IsRestarting() || IsLeaving())
        {
            return;
        }

        if (getGameState == null || getGameState() != GameState.Battle)
        {
            return;
        }

        if (isGameOver)
        {
            return;
        }

        if (!isMyTurn)
        {
            return;
        }

        turnTimer = turnTimeLimit;
        isTurnTimerRunning = true;

        updateStatusText?.Invoke();
        UpdateTurnTimerUI();

        Debug.Log($"[TurnTimer] 턴 타이머 시작: {turnTimeLimit}초");
    }

    public void StopTurnTimer()
    {
        isTurnTimerRunning = false;
        UpdateTurnTimerUI();
    }

    private void UpdateTurnTimer()
    {
        if (IsDisconnected())
        {
            StopTurnTimer();
            return;
        }

        if (IsRestarting() || IsLeaving())
        {
            StopTurnTimer();
            return;
        }

        if (!isTurnTimerRunning)
        {
            UpdateTurnTimerUI();
            return;
        }

        if (getGameState == null || getGameState() != GameState.Battle)
        {
            StopTurnTimer();
            return;
        }

        if (isGameOver)
        {
            StopTurnTimer();
            return;
        }

        if (!isMyTurn)
        {
            StopTurnTimer();
            return;
        }

        if (isWaitingResult)
        {
            UpdateTurnTimerUI();
            return;
        }

        turnTimer -= Time.deltaTime;
        UpdateTurnTimerUI();

        if (turnTimer <= 0f)
        {
            OnTurnTimeout();
        }
    }

    private void UpdateTurnTimerUI()
    {
        if (battleUIController == null)
        {
            return;
        }

        if (IsDisconnected())
        {
            ClearTurnTimeText();
            return;
        }

        if (IsRestarting())
        {
            ClearTurnTimeText();
            return;
        }

        if (IsLeaving())
        {
            ClearTurnTimeText();
            return;
        }

        if (getGameState == null || getGameState() != GameState.Battle || isGameOver)
        {
            ClearTurnTimeText();
            return;
        }

        if (!isMyTurn)
        {
            ClearTurnTimeText();
            return;
        }

        if (isWaitingResult)
        {
            ClearTurnTimeText();
            return;
        }

        if (!isTurnTimerRunning)
        {
            ClearTurnTimeText();
            return;
        }

        int displayTime = Mathf.CeilToInt(turnTimer);
        battleUIController.SetTurnTimeText(displayTime);
    }

    private void ClearTurnTimeText()
    {
        if (battleUIController == null)
        {
            return;
        }

        battleUIController.ClearTurnTimeText();
    }

    // 턴 제한 시간이 끝났을 때 턴을 상대에게 넘기는 처리
    private void OnTurnTimeout()
    {
        if (IsDisconnected())
        {
            return;
        }

        if (IsRestarting() || IsLeaving())
        {
            return;
        }

        if (!isMyTurn)
        {
            return;
        }

        if (isWaitingResult)
        {
            return;
        }

        Debug.Log("[TurnTimer] 제한 시간 초과, 턴 넘김");

        isMyTurn = false;
        isWaitingResult = false;

        updateStatusText?.Invoke();
        StopTurnTimer();

        SendPacket(PacketProtocol.TURN_TIMEOUT);

        Debug.Log("[Turn] 시간 초과로 상대 턴 대기");
    }

    #endregion

    #region 내부 유틸

    private bool SendPacket(string packet)
    {
        if (packetSender == null)
        {
            return false;
        }

        return packetSender(packet);
    }

    private bool IsDisconnected()
    {
        return checkDisconnected != null && checkDisconnected();
    }

    private bool IsRestarting()
    {
        return checkRestarting != null && checkRestarting();
    }

    private bool IsLeaving()
    {
        return checkLeaving != null && checkLeaving();
    }

    private void ShowGameOverUI(bool isWin)
    {
        if (battleUIController != null)
        {
            battleUIController.ShowGameOverUI(isWin);
        }
    }

    private void MarkMyShipSunk(string shipId)
    {
        if (battleUIController != null)
        {
            battleUIController.MarkMyShipSunk(shipId);
        }
    }

    private void MarkEnemyShipSunk(string shipId)
    {
        if (battleUIController != null)
        {
            battleUIController.MarkEnemyShipSunk(shipId);
        }
    }

    private string ConvertAttackResultToPacketText(AttackResult result)
    {
        switch (result)
        {
            case AttackResult.Hit:
                return "HIT";

            case AttackResult.Miss:
                return "MISS";

            case AttackResult.Sunk:
                return "SUNK";

            case AttackResult.GameOver:
                return "GAME_OVER";

            default:
                return "INVALID";
        }
    }

    #endregion
}
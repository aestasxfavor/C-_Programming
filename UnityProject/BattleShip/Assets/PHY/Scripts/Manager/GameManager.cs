using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("게임 상태")]
    [SerializeField] private GameState gameState = GameState.None;

    [Header("Ready 상태")]
    [SerializeField] private bool isMyReady;
    [SerializeField] private bool isOpponentReady;
    [SerializeField] private bool isPlacementLocked;

    [Header("보드 연결")]
    [SerializeField] private BoardView myBoardView;
    [SerializeField] private BoardView enemyBoardView;

    [Header("UI 전환")]
    [SerializeField] private GameObject shipCanvas;
    [SerializeField] private GameObject enemyBoardPanel;

    [Header("로컬 테스트")]
    // TCP 없이 Editor 단독으로 전투 판정 테스트할 때만 사용
    // 체크 ON  : Ready 클릭 시 상대도 Ready 한 것으로 처리하고 바로 Battle 진입
    // 체크 OFF : 기존 TCP READY 패킷 송수신 흐름 사용
    [SerializeField] private bool useLocalBattleTest;

    [Header("전투 턴 상태")]
    [SerializeField] private bool isMyTurn;
    [SerializeField] private bool isWaitingResult;
    [SerializeField] private bool isGameOver;

    public bool IsPlacementLocked => isPlacementLocked;
    public bool IsBattle => gameState == GameState.Battle;
    public GameState CurrentState => gameState;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        gameState = GameState.Placement;

        ShowPlacementUI();

        Debug.Log("[GameManager] Placement 단계 시작");
    }

    public void OnClickReadyButton()
    {
        if (isMyReady)
        {
            Debug.Log("[Ready] 이미 Ready 상태");
            return;
        }

        if (!IsAllShipsPlaced())
        {
            Debug.LogWarning("[Ready] 배 5척을 모두 배치해야 Ready 가능");
            return;
        }

        isMyReady = true;
        isPlacementLocked = true;

        Debug.Log("[Ready] 내 Ready 완료");
        Debug.Log("[Placement] 배치 수정 잠금");

        if (useLocalBattleTest)
        {
            Debug.Log("[Ready] 로컬 전투 테스트 모드라 바로 Battle 진입");

            isOpponentReady = true;
            TryStartBattle();

            return;
        }

        gameState = GameState.WaitingReady;

        TrySendPacket(PacketProtocol.READY);

        TryStartBattle();
    }

    private bool IsAllShipsPlaced()
    {
        if (myBoardView == null)
        {
            Debug.LogError("[Ready] myBoardView 연결 필요");
            return false;
        }

        return myBoardView.IsAllShipsPlaced();
    }

    public void OnReceivePacket(string packet)
    {
        if (string.IsNullOrEmpty(packet))
        {
            return;
        }

        Debug.Log($"[Packet Received] {packet}");

        string[] split = packet.Split('|');

        switch (split[0])
        {
            case PacketProtocol.READY:
                OnOpponentReady();
                break;

            case PacketProtocol.ATTACK:
                OnReceiveAttackPacket(split);
                break;

            case PacketProtocol.RESULT:
                OnReceiveResultPacket(split);
                break;

            case PacketProtocol.GAME_OVER:
                OnReceiveGameOverPacket();
                break;

            default:
                Debug.LogWarning($"[Packet] 알 수 없는 패킷: {packet}");
                break;
        }
    }

    private void OnOpponentReady()
    {
        if (isOpponentReady)
        {
            return;
        }

        isOpponentReady = true;

        Debug.Log("[Ready] 상대 Ready 수신");

        TryStartBattle();
    }

    private void TryStartBattle()
    {
        if (!isMyReady)
        {
            Debug.Log("[Ready] 내 Ready 대기 중");
            return;
        }

        if (!isOpponentReady)
        {
            Debug.Log("[Ready] 상대 Ready 대기 중");
            return;
        }

        if (gameState == GameState.Battle)
        {
            return;
        }

        gameState = GameState.Battle;

        isGameOver = false;
        isWaitingResult = false;

        if (useLocalBattleTest)
        {
            isMyTurn = true;
        }
        else
        {
            isMyTurn = TCPManager.Instance != null && TCPManager.Instance.IsHost;
        }

        ShowBattleUI();

        Debug.Log("[Battle] 양쪽 Ready 완료, 전투 단계 진입");

        if (isMyTurn)
        {
            Debug.Log("[Turn] 내 턴 시작");
        }
        else
        {
            Debug.Log("[Turn] 상대 턴 대기");
        }
    }

    public void TryAttackEnemyBoard(int x, int y)
    {
        if (gameState != GameState.Battle)
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

        if (!enemyBoardView.CanRequestAttack(x, y))
        {
            Debug.Log($"[Battle] 공격 불가 칸 X={x}, Y={y}");
            return;
        }

        if (useLocalBattleTest)
        {
            AttackResult localResult = enemyBoardView.ReceiveAttack(x, y);

            Debug.Log($"[Battle] 로컬 공격 결과: {localResult}, X={x}, Y={y}");

            if (localResult == AttackResult.GameOver)
            {
                SetGameOver(true);
            }

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

        string packet = $"{PacketProtocol.ATTACK}|{x}|{y}";

        if (!TrySendPacket(packet))
        {
            return;
        }

        isWaitingResult = true;

        Debug.Log($"[Attack] 공격 패킷 전송 X={x}, Y={y}");
    }

    private void OnReceiveAttackPacket(string[] split)
    {
        if (split.Length < 3)
        {
            Debug.LogWarning("[Attack] ATTACK 패킷 형식 오류");
            return;
        }

        if (gameState != GameState.Battle)
        {
            Debug.Log("[Attack] Battle 단계가 아니라 ATTACK 무시");
            return;
        }

        if (isGameOver)
        {
            Debug.Log("[Attack] 이미 게임 종료 상태라 ATTACK 무시");
            return;
        }

        if (myBoardView == null)
        {
            Debug.LogError("[Attack] myBoardView 연결 필요");
            return;
        }

        if (!int.TryParse(split[1], out int x) || !int.TryParse(split[2], out int y))
        {
            Debug.LogWarning("[Attack] 좌표 파싱 실패");
            return;
        }

        Debug.Log($"[Attack] 공격 패킷 수신 X={x}, Y={y}");

        AttackResult result = myBoardView.ReceiveAttack(x, y);
        string resultText = ConvertAttackResultToPacketText(result);

        string aroundPositionsText = "";

        if (result == AttackResult.Sunk || result == AttackResult.GameOver)
        {
            aroundPositionsText = myBoardView.GetLastSunkAroundPositionsText();
        }

        string resultPacket = $"{PacketProtocol.RESULT}|{x}|{y}|{resultText}";

        if (!string.IsNullOrEmpty(aroundPositionsText))
        {
            resultPacket += $"|{aroundPositionsText}";
        }

        TrySendPacket(resultPacket);

        Debug.Log($"[Result] 결과 패킷 전송 {resultText}, X={x}, Y={y}");

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

        isMyTurn = true;
        isWaitingResult = false;

        Debug.Log("[Turn] 내 턴 시작");
    }

    private void OnReceiveResultPacket(string[] split)
    {
        if (split.Length < 4)
        {
            Debug.LogWarning("[Result] RESULT 패킷 형식 오류");
            return;
        }

        if (!int.TryParse(split[1], out int x) || !int.TryParse(split[2], out int y))
        {
            Debug.LogWarning("[Result] 좌표 파싱 실패");
            return;
        }

        string resultText = split[3];
        string aroundPositionsText = split.Length >= 5 ? split[4] : "";

        Debug.Log($"[Result] 결과 수신 {resultText}, X={x}, Y={y}");

        isWaitingResult = false;

        if (resultText == "INVALID")
        {
            isMyTurn = true;
            Debug.Log("[Result] INVALID 수신, 내 턴 유지");
            return;
        }

        if (enemyBoardView == null)
        {
            Debug.LogError("[Result] enemyBoardView 연결 필요");
            return;
        }

        enemyBoardView.ApplyAttackResult(x, y, resultText, aroundPositionsText);

        if (resultText == "GAME_OVER")
        {
            SetGameOver(true);
            return;
        }

        isMyTurn = false;

        Debug.Log("[Turn] 상대 턴 대기");
    }

    private void OnReceiveGameOverPacket()
    {
        if (isGameOver)
        {
            return;
        }

        SetGameOver(true);
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

    private bool TrySendPacket(string packet)
    {
        if (useLocalBattleTest)
        {
            Debug.Log($"[Packet Send] 로컬 테스트 모드라 패킷 전송 생략: {packet}");
            return false;
        }

        if (TCPManager.Instance == null)
        {
            Debug.LogWarning($"[Packet Send] TCPManager.Instance 없음: {packet}");
            return false;
        }

        if (!TCPManager.Instance.IsConnected)
        {
            Debug.LogWarning($"[Packet Send] TCP 연결 안 됨: {packet}");
            return false;
        }

        TCPManager.Instance.Send(packet);
        return true;
    }

    private void SetGameOver(bool isWin)
    {
        isGameOver = true;
        isMyTurn = false;
        isWaitingResult = false;

        gameState = GameState.GameOver;

        if (isWin)
        {
            Debug.Log("[GameOver] 승리");
        }
        else
        {
            Debug.Log("[GameOver] 패배");
        }
    }

    private void ShowPlacementUI()
    {
        if (shipCanvas != null)
        {
            shipCanvas.SetActive(true);
        }

        if (enemyBoardPanel != null)
        {
            enemyBoardPanel.SetActive(false);
        }
    }

    private void ShowBattleUI()
    {
        if (shipCanvas != null)
        {
            shipCanvas.SetActive(false);
        }

        if (enemyBoardPanel != null)
        {
            enemyBoardPanel.SetActive(true);
        }
    }

    [ContextMenu("Debug Start Battle")]
    private void DebugStartBattle()
    {
        isMyReady = true;
        isOpponentReady = true;
        isPlacementLocked = true;
        gameState = GameState.Battle;

        isGameOver = false;
        isWaitingResult = false;
        isMyTurn = true;

        ShowBattleUI();

        Debug.Log("[Debug] 강제 Battle 단계 진입");
    }
}
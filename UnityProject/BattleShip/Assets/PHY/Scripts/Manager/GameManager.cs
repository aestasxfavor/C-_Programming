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

        // 로컬 전투 테스트 전용 분기
        // TCP 연결 없이 전투 판정, UI 전환, Hit/Miss/Sunk/GameOver를 확인하기 위한 임시 흐름
        // 실제 TCP 전투 테스트 시 useLocalBattleTest 체크 해제
        if (useLocalBattleTest)
        {
            Debug.Log("[Ready] 로컬 전투 테스트 모드라 바로 Battle 진입");

            isOpponentReady = true;
            TryStartBattle();

            return;
        }

        gameState = GameState.WaitingReady;

        if (TCPManager.Instance != null)
        {
            TCPManager.Instance.Send(PacketProtocol.READY);
        }
        else
        {
            Debug.LogWarning("[Ready] TCPManager.Instance가 없어 READY 패킷 전송 생략");
        }

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

                // TCP 전투 연결 시 추가 위치
                // case PacketProtocol.ATTACK:
                //     OnReceiveAttackPacket(split);
                //     break;
                //
                // case PacketProtocol.RESULT:
                //     OnReceiveResultPacket(split);
                //     break;
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

        gameState = GameState.Battle;

        ShowBattleUI();

        Debug.Log("[Battle] 양쪽 Ready 완료, 전투 단계 진입");
    }

    public void TryAttackEnemyBoard(int x, int y)
    {
        if (gameState != GameState.Battle)
        {
            Debug.Log("[Battle] 전투 단계가 아니라 공격 불가");
            return;
        }

        if (enemyBoardView == null)
        {
            Debug.LogError("[Battle] enemyBoardView 연결 필요");
            return;
        }

        // 현재는 로컬 전투 테스트용으로 enemyBoardView에서 직접 판정
        // TCP 연결 후에는 ATTACK 패킷 전송 → 상대 myBoardView에서 ReceiveAttack 판정 → RESULT 수신 흐름으로 변경 예정
        AttackResult result = enemyBoardView.ReceiveAttack(x, y);

        if (result == AttackResult.Invalid)
        {
            Debug.Log($"[Battle] 공격 불가 칸 X={x}, Y={y}");
            return;
        }

        Debug.Log($"[Battle] 공격 결과: {result}, X={x}, Y={y}");

        if (result == AttackResult.GameOver)
        {
            SetGameOver(true);
        }
    }

    private void SetGameOver(bool isWin)
    {
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

        ShowBattleUI();

        Debug.Log("[Debug] 강제 Battle 단계 진입");
    }
}
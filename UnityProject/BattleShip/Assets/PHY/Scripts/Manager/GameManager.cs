using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] private GameState gameState = GameState.None;

    [SerializeField] private bool isMyReady;
    [SerializeField] private bool isOpponentReady;
    [SerializeField] private bool isPlacementLocked;

    [SerializeField] private BoardView boardView;

    public bool IsPlacementLocked => isPlacementLocked;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        gameState = GameState.Placement;
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
        gameState = GameState.WaitingReady;

        Debug.Log("[Ready] 내 Ready 완료");
        Debug.Log("[Placement] 배치 수정 잠금");

        TCPManager.Instance.Send(PacketProtocol.READY);

        TryStartBattle();
    }

    private bool IsAllShipsPlaced()
    {
        // TODO: 나중에 실제 배치된 배 개수 체크로 교체
        // 예: return shipPlacementManager.PlacedShipCount >= 5;

        if (boardView == null) return false;

        return true;
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

        Debug.Log("[Battle] 양쪽 Ready 완료, 전투 단계 진입");
    }
}
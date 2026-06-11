using System;
using UnityEngine;

// 양쪽 Ready 상태 동기화, 배치 잠금, Battle 진입 조건 확인을 담당하는 컨트롤러
public class ReadyController : MonoBehaviour
{
    [Header("Ready 상태")]
    [SerializeField] private bool isMyReady;
    [SerializeField] private bool isOpponentReady;
    [SerializeField] private bool isPlacementLocked;

    [Header("Ready 전송 상태")]
    [SerializeField] private bool isWaitingReadySend;

    private Func<bool> checkAllShipsPlaced;
    private Func<bool> checkTcpConnected;
    private Func<string, bool> packetSender;

    private Action showWaitingReadyState;
    private Action tryStartBattle;
    private Action updateStatusText;

    public bool IsMyReady => isMyReady;
    public bool IsOpponentReady => isOpponentReady;
    public bool IsPlacementLocked => isPlacementLocked;

    public void Setup(
        Func<bool> _allShipsPlacedChecker,
        Func<bool> _tcpConnectedChecker,
        Func<string, bool> _packetSender,
        Action _waitingReadySetter,
        Action _battleStarter,
        Action _statusTextUpdater)
    {
        checkAllShipsPlaced = _allShipsPlacedChecker;
        checkTcpConnected = _tcpConnectedChecker;
        packetSender = _packetSender;

        showWaitingReadyState = _waitingReadySetter;
        tryStartBattle = _battleStarter;
        updateStatusText = _statusTextUpdater;
    }

    public void ResetReadyState()
    {
        isMyReady = false;
        isOpponentReady = false;
        isPlacementLocked = false;
        isWaitingReadySend = false;
    }

    // 내 함선 배치 완료 여부를 확인하고 READY 패킷 전송
    public void ClickReady()
    {
        if (isMyReady)
        {
            Debug.Log("[Ready] 이미 Ready 상태");
            return;
        }

        if (checkAllShipsPlaced == null || !checkAllShipsPlaced())
        {
            Debug.LogWarning("[Ready] 배 5척을 모두 배치해야 Ready 가능");
            return;
        }

        isMyReady = true;
        isPlacementLocked = true;

        Debug.Log("[Ready] 내 Ready 완료");
        Debug.Log("[Placement] 배치 수정 잠금");

        showWaitingReadyState?.Invoke();
        updateStatusText?.Invoke();

        if (checkTcpConnected != null && checkTcpConnected())
        {
            packetSender?.Invoke(PacketProtocol.READY);
            isWaitingReadySend = false;

            Debug.Log("[Ready] READY 패킷 즉시 전송");
        }
        else
        {
            isWaitingReadySend = true;

            Debug.Log("[Ready] TCP 연결 전이라 READY 패킷 전송 예약");
        }

        tryStartBattle?.Invoke();
    }

    public void ReceiveOpponentReady()
    {
        if (isOpponentReady)
        {
            return;
        }

        isOpponentReady = true;

        updateStatusText?.Invoke();

        Debug.Log("[Ready] 상대 Ready 수신");

        tryStartBattle?.Invoke();
    }

    public void TrySendWaitingReady(bool canSend)
    {
        if (!canSend)
        {
            return;
        }

        if (!isWaitingReadySend)
        {
            return;
        }

        if (!isMyReady)
        {
            return;
        }

        if (checkTcpConnected == null || !checkTcpConnected())
        {
            return;
        }

        packetSender?.Invoke(PacketProtocol.READY);
        isWaitingReadySend = false;

        Debug.Log("[Ready] 예약된 READY 패킷 전송 완료");

        tryStartBattle?.Invoke();
    }

    public void ClearWaitingReady()
    {
        isWaitingReadySend = false;
    }

    // Ready 이후 함선 배치 수정 방지
    public void LockPlacement()
    {
        isPlacementLocked = true;
    }
}
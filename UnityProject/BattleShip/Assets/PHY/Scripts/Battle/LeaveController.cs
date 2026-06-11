using System;
using System.Collections;
using UnityEngine;

// 매치 나가기, 상대 나가기 수신, 연결 끊김 후 타이틀 복귀 흐름을 처리하는 컨트롤러
public class LeaveController : MonoBehaviour
{
    private MatchController matchController;
    private Func<string, bool> packetSender;
    private Action stopBattle;
    private Action lockPlacement;
    private Action updateStatusText;
    private Action hideGameOverUI;
    private Action showDisconnectUI;

    private Coroutine leaveRoutine;

    public void Setup(
        MatchController _match,
        Func<string, bool> _packetSender,
        Action _battleStopper,
        Action _placementLocker,
        Action _statusUpdater,
        Action _gameOverUIHider,
        Action _disconnectUIShower)
    {
        matchController = _match;
        packetSender = _packetSender;
        stopBattle = _battleStopper;
        lockPlacement = _placementLocker;
        updateStatusText = _statusUpdater;
        hideGameOverUI = _gameOverUIHider;
        showDisconnectUI = _disconnectUIShower;
    }

    // 나가기 버튼 입력 시 LEAVE 패킷 전송 후 타이틀 복귀 처리
    public void ClickExit()
    {
        if (matchController == null)
        {
            Debug.LogError("[Leave] MatchController 연결 필요");
            return;
        }

        if (leaveRoutine != null)
        {
            Debug.Log("[Leave] 이미 나가기 처리 중");
            return;
        }

        if (!matchController.TryLeave())
        {
            return;
        }

        stopBattle?.Invoke();
        lockPlacement?.Invoke();

        updateStatusText?.Invoke();
        hideGameOverUI?.Invoke();

        if (TCPManager.Instance != null && TCPManager.Instance.IsConnected)
        {
            leaveRoutine = StartCoroutine(SendLeaveThenGoTitle());
            return;
        }

        matchController.GoTitleNow();
    }

    private IEnumerator SendLeaveThenGoTitle()
    {
        Debug.Log("[Leave] LEAVE 패킷 전송 시작");

        SendPacket(PacketProtocol.LEAVE);

        yield return new WaitForSeconds(0.15f);

        if (TCPManager.Instance != null && TCPManager.Instance.IsConnected)
        {
            SendPacket(PacketProtocol.LEAVE);
            Debug.Log("[Leave] LEAVE 패킷 재전송");
        }

        yield return new WaitForSeconds(0.15f);

        leaveRoutine = null;

        matchController.GoTitleAfterSendLeave();
    }

    // 상대의 LEAVE 패킷 수신 시 연결 끊김 UI와 타이틀 복귀 처리
    public void ReceiveLeave()
    {
        if (matchController == null)
        {
            Debug.LogError("[Leave] MatchController 연결 필요");
            return;
        }

        if (!matchController.ReceiveLeave())
        {
            return;
        }

        if (leaveRoutine != null)
        {
            StopCoroutine(leaveRoutine);
            leaveRoutine = null;
        }

        stopBattle?.Invoke();
        lockPlacement?.Invoke();

        updateStatusText?.Invoke();

        hideGameOverUI?.Invoke();
        showDisconnectUI?.Invoke();

        matchController.GoTitleAfterLeave();
    }

    public void Disconnect()
    {
        if (matchController == null)
        {
            Debug.LogError("[Leave] MatchController 연결 필요");
            return;
        }

        if (!matchController.Disconnect())
        {
            return;
        }

        if (leaveRoutine != null)
        {
            StopCoroutine(leaveRoutine);
            leaveRoutine = null;
        }

        stopBattle?.Invoke();
        lockPlacement?.Invoke();

        updateStatusText?.Invoke();

        hideGameOverUI?.Invoke();
        showDisconnectUI?.Invoke();

        matchController.GoTitleAfterDisconnect();
    }

    private bool SendPacket(string packet)
    {
        if (packetSender == null)
        {
            Debug.LogWarning("[Leave] packetSender 연결 안 됨");
            return false;
        }

        return packetSender(packet);
    }
}
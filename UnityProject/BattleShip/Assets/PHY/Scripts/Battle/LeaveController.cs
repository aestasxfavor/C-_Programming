using System;
using System.Collections;
using UnityEngine;

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
        MatchController match,
        Func<string, bool> _packetSender,
        Action battleStopper,
        Action placementLocker,
        Action statusUpdater,
        Action gameOverUIHider,
        Action disconnectUIShower)
    {
        matchController = match;
        packetSender = _packetSender;
        stopBattle = battleStopper;
        lockPlacement = placementLocker;
        updateStatusText = statusUpdater;
        hideGameOverUI = gameOverUIHider;
        showDisconnectUI = disconnectUIShower;
    }

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
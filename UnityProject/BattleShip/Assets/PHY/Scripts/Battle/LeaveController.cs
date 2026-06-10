using System;
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
            SendPacket(PacketProtocol.LEAVE);
            matchController.GoTitleAfterSendLeave();
            return;
        }

        matchController.GoTitleNow();
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
            return false;
        }

        return packetSender(packet);
    }
}
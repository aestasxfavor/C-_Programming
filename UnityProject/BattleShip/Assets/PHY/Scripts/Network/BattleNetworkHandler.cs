using System;
using UnityEngine;

public class BattleNetworkHandler : MonoBehaviour
{
    private Func<bool> isDisconnected;

    private Action receiveReady;
    private Action<string[]> receiveAttack;
    private Action<string[]> receiveResult;
    private Action receiveGameOver;
    private Action receiveTurnTimeout;
    private Action receiveReplayReady;
    private Action receiveReplayStart;
    private Action receiveLeave;

    public void Setup(
        Func<bool> disconnectedCheck,
        Action readyHandler,
        Action<string[]> attackHandler,
        Action<string[]> resultHandler,
        Action gameOverHandler,
        Action turnTimeoutHandler,
        Action replayReadyHandler,
        Action replayStartHandler,
        Action leaveHandler)
    {
        isDisconnected = disconnectedCheck;

        receiveReady = readyHandler;
        receiveAttack = attackHandler;
        receiveResult = resultHandler;
        receiveGameOver = gameOverHandler;
        receiveTurnTimeout = turnTimeoutHandler;
        receiveReplayReady = replayReadyHandler;
        receiveReplayStart = replayStartHandler;
        receiveLeave = leaveHandler;
    }

    public void ReceivePacket(string packet)
    {
        if (string.IsNullOrEmpty(packet))
        {
            return;
        }

        if (IsDisconnected())
        {
            Debug.Log($"[Packet] 연결 끊김 상태라 패킷 무시: {packet}");
            return;
        }

        Debug.Log($"[Packet Received] {packet}");

        string[] split = packet.Split('|');

        if (split.Length == 0)
        {
            return;
        }

        switch (split[0])
        {
            case PacketProtocol.READY:
                receiveReady?.Invoke();
                break;

            case PacketProtocol.ATTACK:
                receiveAttack?.Invoke(split);
                break;

            case PacketProtocol.RESULT:
                receiveResult?.Invoke(split);
                break;

            case PacketProtocol.GAME_OVER:
                receiveGameOver?.Invoke();
                break;

            case PacketProtocol.TURN_TIMEOUT:
                receiveTurnTimeout?.Invoke();
                break;

            case PacketProtocol.REPLAY_READY:
                receiveReplayReady?.Invoke();
                break;

            case PacketProtocol.REPLAY_START:
                receiveReplayStart?.Invoke();
                break;

            case PacketProtocol.LEAVE:
                receiveLeave?.Invoke();
                break;

            default:
                Debug.LogWarning($"[Packet] 알 수 없는 패킷: {packet}");
                break;
        }
    }

    private bool IsDisconnected()
    {
        return isDisconnected != null && isDisconnected();
    }
}
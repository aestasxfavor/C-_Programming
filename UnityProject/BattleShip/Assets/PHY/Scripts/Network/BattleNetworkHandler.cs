using System;
using UnityEngine;

// Ready, 전투, 채팅, 리플레이, 나가기 패킷을 각 컨트롤러로 분배하는 패킷 처리 스크립트
public class BattleNetworkHandler : MonoBehaviour
{
    private Func<bool> checkDisconnected;

    private Action receiveReady;
    private Action<string[]> receiveAttack;
    private Action<string[]> receiveResult;
    private Action receiveGameOver;
    private Action receiveTurnTimeout;
    private Action receiveReplayReady;
    private Action receiveReplayStart;
    private Action receiveLeave;
    private Action<string[]> receiveChat;

    public void Setup(
        Func<bool> disconnectedCheck,
        Action readyHandler,
        Action<string[]> attackHandler,
        Action<string[]> resultHandler,
        Action gameOverHandler,
        Action turnTimeoutHandler,
        Action replayReadyHandler,
        Action replayStartHandler,
        Action leaveHandler,
        Action<string[]> chatHandler)
    {
        checkDisconnected = disconnectedCheck;

        receiveReady = readyHandler;
        receiveAttack = attackHandler;
        receiveResult = resultHandler;
        receiveGameOver = gameOverHandler;
        receiveTurnTimeout = turnTimeoutHandler;
        receiveReplayReady = replayReadyHandler;
        receiveReplayStart = replayStartHandler;
        receiveLeave = leaveHandler;
        receiveChat = chatHandler;
    }

    // 수신된 패킷 타입을 확인하고 알맞은 컨트롤러 콜백으로 전달
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

        string[] packetParts = packet.Split('|');

        if (packetParts.Length == 0)
        {
            return;
        }

        switch (packetParts[0])
        {
            case PacketProtocol.READY:
                receiveReady?.Invoke();
                break;

            case PacketProtocol.ATTACK:
                receiveAttack?.Invoke(packetParts);
                break;

            case PacketProtocol.RESULT:
                receiveResult?.Invoke(packetParts);
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

            case PacketProtocol.CHAT:
                receiveChat?.Invoke(packetParts);
                break;

            default:
                Debug.LogWarning($"[Packet] 알 수 없는 패킷: {packet}");
                break;
        }
    }

    // TCPManager를 통해 상대에게 패킷 전송
    public bool SendPacket(string packet)
    {
        if (IsDisconnected())
        {
            Debug.LogWarning($"[Packet Send] 연결 끊김 상태라 패킷 전송 생략: {packet}");
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

    private bool IsDisconnected()
    {
        return checkDisconnected != null && checkDisconnected();
    }
}
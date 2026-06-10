using System;
using UnityEngine;

public class ReplayController : MonoBehaviour
{
    [Header("Replay 상태")]
    [SerializeField] private bool isMyReplayReady;
    [SerializeField] private bool isOpponentReplayReady;

    private MatchController matchController;
    private Func<GameState> getGameState;
    private Func<string, bool> packetSender;
    private Action stopBattle;

    public void Setup(
        MatchController match,
        Func<GameState> gameStateGetter,
        Func<string, bool> packetSender,
        Action battleStopper)
    {
        matchController = match;
        getGameState = gameStateGetter;
        this.packetSender = packetSender;
        stopBattle = battleStopper;
    }

    public void ResetState()
    {
        isMyReplayReady = false;
        isOpponentReplayReady = false;
    }

    public void ClickReplay()
    {
        if (matchController == null)
        {
            Debug.LogError("[Replay] MatchController 연결 필요");
            return;
        }

        if (matchController.IsDisconnected)
        {
            Debug.Log("[Network] 연결 끊김 상태라 Replay 불가");
            return;
        }

        if (matchController.IsLeaving)
        {
            Debug.Log("[Replay] 타이틀 복귀 중이라 Replay 불가");
            return;
        }

        if (matchController.IsRestarting)
        {
            Debug.Log("[Replay] 이미 재시작 진행 중");
            return;
        }

        if (getGameState == null || getGameState() != GameState.GameOver)
        {
            Debug.Log("[Replay] 게임 종료 상태가 아니라 재시작 불가");
            return;
        }

        if (isMyReplayReady)
        {
            Debug.Log("[Replay] 이미 재시작 Ready 상태");
            return;
        }

        isMyReplayReady = true;

        Debug.Log("[Replay] 내 재시작 Ready");

        SendPacket(PacketProtocol.REPLAY_READY);

        TryRestartGame();
    }

    public void ReceiveReplayReady()
    {
        if (getGameState == null || getGameState() != GameState.GameOver)
        {
            Debug.Log("[Replay] 게임 종료 상태가 아니라 REPLAY_READY 무시");
            return;
        }

        if (isOpponentReplayReady)
        {
            return;
        }

        isOpponentReplayReady = true;

        Debug.Log("[Replay] 상대 재시작 Ready 수신");

        TryRestartGame();
    }

    public void ReceiveReplayStart()
    {
        if (matchController == null)
        {
            return;
        }

        if (matchController.IsRestarting)
        {
            return;
        }

        Debug.Log("[Replay] 재시작 시작 패킷 수신");

        matchController.StartReplay();

        stopBattle?.Invoke();

        matchController.RestartGameScene();
    }

    private void TryRestartGame()
    {
        if (matchController == null)
        {
            return;
        }

        if (matchController.IsDisconnected)
        {
            return;
        }

        if (matchController.IsLeaving)
        {
            return;
        }

        if (!isMyReplayReady)
        {
            Debug.Log("[Replay] 내 재시작 Ready 대기");
            return;
        }

        if (!isOpponentReplayReady)
        {
            Debug.Log("[Replay] 상대 재시작 Ready 대기");
            return;
        }

        if (matchController.IsRestarting)
        {
            return;
        }

        Debug.Log("[Replay] 양쪽 재시작 Ready 완료, BattleScene 재시작 준비");

        matchController.StartReplay();

        SendPacket(PacketProtocol.REPLAY_START);

        stopBattle?.Invoke();

        matchController.RestartGameScene();
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
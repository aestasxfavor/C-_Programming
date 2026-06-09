using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MatchController : MonoBehaviour
{
    private static bool isReplayLoading;

    [Header("씬 이름")]
    [SerializeField] private string titleSceneName = "Title";
    [SerializeField] private string gameSceneName = "Game";

    public bool IsRestarting { get; private set; }
    public bool IsLeaving { get; private set; }
    public bool IsDisconnected { get; private set; }

    private bool isClearingReplay;
    private Action refreshStatus;

    public void Setup(Action refreshStatusAction)
    {
        refreshStatus = refreshStatusAction;
    }

    public void ResetState()
    {
        IsDisconnected = false;
        IsRestarting = isReplayLoading;
        IsLeaving = false;
        isClearingReplay = false;

        if (IsRestarting)
        {
            Debug.Log("[Replay] 씬 재시작 상태 유지 중");
        }

        refreshStatus?.Invoke();
    }

    public void CheckReplayReconnect()
    {
        if (!isReplayLoading)
        {
            return;
        }

        if (isClearingReplay)
        {
            return;
        }

        if (TCPManager.Instance == null)
        {
            return;
        }

        if (!TCPManager.Instance.IsConnected)
        {
            return;
        }

        StartCoroutine(ClearReplayAfterReconnect());
    }

    public void StartReplay()
    {
        isReplayLoading = true;
        IsRestarting = true;
        isClearingReplay = false;

        refreshStatus?.Invoke();

        Debug.Log("[Replay] 재시작 플래그 설정");
    }

    public void ClearBattleLock()
    {
        IsRestarting = false;
        IsLeaving = false;

        refreshStatus?.Invoke();
    }

    public void ResetDebugState()
    {
        isReplayLoading = false;
        IsRestarting = false;
        IsLeaving = false;
        IsDisconnected = false;
        isClearingReplay = false;

        refreshStatus?.Invoke();
    }

    public bool TryLeave()
    {
        if (IsDisconnected)
        {
            Debug.Log("[Network] 연결 끊김 상태라 Exit 무시");
            return false;
        }

        if (IsLeaving)
        {
            return false;
        }

        IsLeaving = true;

        refreshStatus?.Invoke();

        Debug.Log("[UI] Exit 버튼 클릭");

        return true;
    }

    public bool ReceiveLeave()
    {
        if (IsLeaving)
        {
            return false;
        }

        IsLeaving = true;
        IsDisconnected = true;

        refreshStatus?.Invoke();

        Debug.Log("[Network] 상대가 매치에서 나감");

        return true;
    }

    public bool Disconnect(bool isLocalTest)
    {
        if (isReplayLoading || IsRestarting)
        {
            Debug.Log("[Network] Replay 중이라 연결 끊김 무시");
            return false;
        }

        if (IsLeaving)
        {
            Debug.Log("[Network] Leave 처리 중이라 연결 끊김 무시");
            return false;
        }

        if (IsDisconnected)
        {
            return false;
        }

        if (isLocalTest)
        {
            return false;
        }

        IsDisconnected = true;

        refreshStatus?.Invoke();

        Debug.Log("[Network] 상대와의 연결이 끊김");

        return true;
    }

    public void GoTitleNow()
    {
        SceneManager.LoadScene(titleSceneName);
    }

    public void GoTitleAfterLeave()
    {
        StartCoroutine(GoTitleAfterDelay(2f));
    }

    public void GoTitleAfterDisconnect()
    {
        StartCoroutine(GoTitleAfterDelay(2f));
    }

    public void GoTitleAfterSendLeave()
    {
        StartCoroutine(GoTitleAfterDelay(0.25f));
    }

    public void RestartGameScene()
    {
        StartCoroutine(RestartGameSceneAfterDelay());
    }

    public void ClearReplayAfterSceneLoad(GameState currentState)
    {
        if (!IsRestarting)
        {
            return;
        }

        StartCoroutine(ClearReplayAfterSceneLoadRoutine(currentState));
    }

    private IEnumerator ClearReplayAfterSceneLoadRoutine(GameState currentState)
    {
        yield return new WaitForSecondsRealtime(0.7f);

        if (currentState != GameState.Placement)
        {
            yield break;
        }

        isReplayLoading = false;
        IsRestarting = false;
        isClearingReplay = false;

        refreshStatus?.Invoke();

        Debug.Log("[Replay] 씬 재시작 후 Placement 상태 전환 완료");
    }

    private IEnumerator ClearReplayAfterReconnect()
    {
        isClearingReplay = true;

        yield return new WaitForSecondsRealtime(0.7f);

        if (TCPManager.Instance != null && TCPManager.Instance.IsConnected)
        {
            isReplayLoading = false;
            IsRestarting = false;
            isClearingReplay = false;

            refreshStatus?.Invoke();

            Debug.Log("[Replay] TCP 재연결 확인, 재시작 플래그 해제");
            yield break;
        }

        isClearingReplay = false;
    }

    private IEnumerator GoTitleAfterDelay(float delay)
    {
        yield return new WaitForSecondsRealtime(delay);

        SceneManager.LoadScene(titleSceneName);
    }

    private IEnumerator RestartGameSceneAfterDelay()
    {
        yield return new WaitForSecondsRealtime(0.5f);

        SceneManager.LoadScene(gameSceneName);
    }
}
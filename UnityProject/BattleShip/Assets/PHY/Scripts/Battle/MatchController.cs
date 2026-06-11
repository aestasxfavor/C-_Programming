using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

// 매치 재시작, 나가기, 연결 끊김 상태와 씬 이동 흐름을 관리하는 컨트롤러
public class MatchController : MonoBehaviour
{
    private static bool isReplayLoading;

    // TODO: SO로 분리 가능
    [Header("씬 이름")]
    [SerializeField] private string titleSceneName = "Title";
    [SerializeField] private string gameSceneName = "Game";

    [Header("매치 상태")]
    [SerializeField] private bool isRestarting;
    [SerializeField] private bool isLeaving;
    [SerializeField] private bool isDisconnected;

    private bool isClearingReplay;

    private Action updateStatusText;

    public bool IsRestarting => isRestarting;
    public bool IsLeaving => isLeaving;
    public bool IsDisconnected => isDisconnected;

    public void Setup(Action _statusUpdater)
    {
        updateStatusText = _statusUpdater;
    }

    // 새 매치 시작 시 재시작 / 나가기 / 연결 끊김 상태 초기화
    public void ResetState()
    {
        isDisconnected = false;
        isRestarting = isReplayLoading;
        isLeaving = false;
        isClearingReplay = false;

        if (isRestarting)
        {
            Debug.Log("[Replay] 씬 재시작 상태 유지 중");
        }

        updateStatusText?.Invoke();
    }

    public void ClearMatchLock()
    {
        isRestarting = false;
        isLeaving = false;

        updateStatusText?.Invoke();
    }

    public void StartReplay()
    {
        isReplayLoading = true;
        isRestarting = true;
        isClearingReplay = false;

        updateStatusText?.Invoke();

        Debug.Log("[Replay] 재시작 플래그 설정");
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

    // Replay로 Game 씬을 다시 로드한 뒤 매치 잠금 상태 해제
    public void ClearReplayAfterSceneLoad(GameState currentState)
    {
        if (!isRestarting)
        {
            return;
        }

        StartCoroutine(ClearReplayAfterSceneLoadRoutine(currentState));
    }

    // 중복 나가기 입력을 막고 타이틀 복귀 상태로 전환
    public bool TryLeave()
    {
        if (isDisconnected)
        {
            Debug.Log("[Network] 연결 끊김 상태라 Exit 무시");
            return false;
        }

        if (isLeaving)
        {
            return false;
        }

        isLeaving = true;

        updateStatusText?.Invoke();

        Debug.Log("[UI] Exit 버튼 클릭");

        return true;
    }

    public bool ReceiveLeave()
    {
        if (isLeaving)
        {
            return false;
        }

        isLeaving = true;
        isDisconnected = true;

        updateStatusText?.Invoke();

        Debug.Log("[Network] 상대가 매치에서 나감");

        return true;
    }

    public bool Disconnect()
    {
        if (isReplayLoading || isRestarting)
        {
            Debug.Log("[Network] Replay 중이라 연결 끊김 무시");
            return false;
        }

        if (isLeaving)
        {
            Debug.Log("[Network] Leave 처리 중이라 연결 끊김 무시");
            return false;
        }

        if (isDisconnected)
        {
            return false;
        }

        isDisconnected = true;

        updateStatusText?.Invoke();

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

    private IEnumerator ClearReplayAfterSceneLoadRoutine(GameState currentState)
    {
        yield return new WaitForSecondsRealtime(0.7f);

        if (currentState != GameState.Placement)
        {
            yield break;
        }

        isReplayLoading = false;
        isRestarting = false;
        isClearingReplay = false;

        updateStatusText?.Invoke();

        Debug.Log("[Replay] 씬 재시작 후 Placement 상태 전환 완료");
    }

    private IEnumerator ClearReplayAfterReconnect()
    {
        isClearingReplay = true;

        yield return new WaitForSecondsRealtime(0.7f);

        if (TCPManager.Instance != null && TCPManager.Instance.IsConnected)
        {
            isReplayLoading = false;
            isRestarting = false;
            isClearingReplay = false;

            updateStatusText?.Invoke();

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
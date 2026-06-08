using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using UnityEngine;

public class TCPManager : MonoBehaviour
{
    public static TCPManager Instance { get; private set; }

    [Header("TCP 프로토콜")]
    [SerializeField] private int port = 7777;
    [SerializeField] private string hostIP = "127.0.0.1";

    [Header("자동 연결 테스트")]
    [SerializeField] private bool autoConnectOnStart = true;
    [SerializeField] private bool editorIsHost = true;

    private TcpListener listener;
    private TcpClient client;
    private StreamReader reader;
    private StreamWriter writer;

    private bool isConnected;
    public bool IsConnected => isConnected;

    public bool IsHost { get; private set; }

    private readonly ConcurrentQueue<string> receiveQueue = new ConcurrentQueue<string>();

    private bool disconnectPending;
    private bool disconnectNotified;
    private bool isClosingByOwner;


    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        Application.runInBackground = true;

        if (!autoConnectOnStart)
        {
            return;
        }

#if UNITY_EDITOR
        if (editorIsHost)
        {
            StartHost();
        }
        else
        {
            StartClient();
        }
#else
        if (editorIsHost)
        {
            StartClient();
        }
        else
        {
            StartHost();
        }
#endif
    }

    private void Update()
    {
        while (receiveQueue.TryDequeue(out string message))
        {
            Debug.Log($"[TCP] 처리할 패킷: {message}");

            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnReceivePacket(message);
            }
        }

        if (disconnectPending)
        {
            disconnectPending = false;

            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnNetworkDisconnected();
            }
        }
    }

    public async void StartHost()
    {
        if (isConnected || listener != null)
        {
            Debug.LogWarning("[TCP] 이미 Host 실행 중");
            return;
        }

        ResetDisconnectState();

        IsHost = true;

        Debug.Log("[TCP] Host 시작");

        try
        {
            listener = new TcpListener(IPAddress.Any, port);
            listener.Start();

            Debug.Log("[TCP] Client 접속 대기 중");

            client = await listener.AcceptTcpClientAsync();

            if (isClosingByOwner)
            {
                return;
            }

            Debug.Log("[TCP] Client 접속 완료");

            SetupStream();
            _ = ReceiveLoop();
        }
        catch (Exception e)
        {
            if (!isClosingByOwner)
            {
                Debug.LogWarning($"[TCP] Host 시작 또는 접속 대기 중 오류: {e.Message}");
                MarkDisconnected();
            }
        }
    }

    public async void StartClient()
    {
        if (isConnected || client != null)
        {
            Debug.LogWarning("[TCP] 이미 Client 실행 중");
            return;
        }

        ResetDisconnectState();

        IsHost = false;

        Debug.Log("[TCP] Client 접속 시도");

        while (!isConnected && !isClosingByOwner)
        {
            try
            {
                client = new TcpClient();
                await client.ConnectAsync(hostIP, port);

                if (isClosingByOwner)
                {
                    return;
                }

                Debug.Log("[TCP] Host 접속 완료");

                SetupStream();
                _ = ReceiveLoop();

                return;
            }
            catch (Exception e)
            {
                if (isClosingByOwner)
                {
                    return;
                }

                Debug.LogWarning($"[TCP] Client 접속 실패, 재시도 예정: {e.Message}");

                client?.Close();
                client = null;

                await Task.Delay(1000);
            }
        }
    }

    private void SetupStream()
    {
        NetworkStream stream = client.GetStream();

        reader = new StreamReader(stream);
        writer = new StreamWriter(stream);
        writer.AutoFlush = true;

        isConnected = true;

        Debug.Log("[TCP] Stream 설정 완료");
    }

    private async Task ReceiveLoop()
    {
        try
        {
            while (isConnected && client != null)
            {
                string message = await reader.ReadLineAsync();

                if (message == null)
                {
                    Debug.Log("[TCP] 연결 종료 감지");
                    MarkDisconnected();
                    break;
                }

                if (message.Length == 0)
                {
                    continue;
                }

                Debug.Log($"[TCP] 수신: {message}");

                EnqueueReceivedMessage(message);
            }
        }
        catch (Exception e)
        {
            if (!isClosingByOwner)
            {
                Debug.LogWarning($"[TCP] 수신 중 연결 끊김 감지: {e.Message}");
                MarkDisconnected();
            }
        }
    }

    public void EnqueueReceivedMessage(string message)
    {
        receiveQueue.Enqueue(message);
    }

    public void Send(string message)
    {
        if (!isConnected || writer == null)
        {
            Debug.LogWarning("[TCP] 연결되지 않아 송신 실패");
            return;
        }

        Debug.Log($"[TCP] 송신: {message}");

        try
        {
            writer.WriteLine(message);
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[TCP] 송신 중 연결 끊김 감지: {e.Message}");
            MarkDisconnected();
        }
    }

    private void MarkDisconnected()
    {
        if (isClosingByOwner)
        {
            return;
        }

        if (disconnectNotified)
        {
            return;
        }

        disconnectNotified = true;
        disconnectPending = true;
        isConnected = false;

        CloseConnection();
    }

    private void ResetDisconnectState()
    {
        isClosingByOwner = false;
        disconnectPending = false;
        disconnectNotified = false;
        isConnected = false;
    }

    private void CloseConnection()
    {
        try
        {
            writer?.Close();
        }
        catch { }

        try
        {
            reader?.Close();
        }
        catch { }

        try
        {
            client?.Close();
        }
        catch { }

        try
        {
            listener?.Stop();
        }
        catch { }

        writer = null;
        reader = null;
        client = null;
        listener = null;
    }

    private void OnDestroy()
    {
        isClosingByOwner = true;
        isConnected = false;

        CloseConnection();

        if (Instance == this)
        {
            Instance = null;
        }
    }
}
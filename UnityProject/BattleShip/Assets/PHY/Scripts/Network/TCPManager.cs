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
    }

    public async void StartHost()
    {
        if (isConnected || listener != null)
        {
            Debug.LogWarning("[TCP] 이미 Host 실행 중");
            return;
        }

        IsHost = true;

        Debug.Log("[TCP] Host 시작");

        listener = new TcpListener(IPAddress.Any, port);
        listener.Start();

        Debug.Log("[TCP] Client 접속 대기 중");

        client = await listener.AcceptTcpClientAsync();

        Debug.Log("[TCP] Client 접속 완료");

        SetupStream();
        _ = ReceiveLoop();
    }

    public async void StartClient()
    {
        IsHost = false;

        Debug.Log("[TCP] Client 접속 시도");

        while (!isConnected)
        {
            try
            {
                client = new TcpClient();
                await client.ConnectAsync(hostIP, port);

                Debug.Log("[TCP] Host 접속 완료");

                SetupStream();
                _ = ReceiveLoop();

                return;
            }
            catch (Exception e)
            {
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
        while (isConnected && client != null && client.Connected)
        {
            string message = await reader.ReadLineAsync();

            // Todo: 연결 종료 시 ReadLineAsync가 null을 반환할 수 있음
            // 현재는 빈 문자열과 null을 같이 무시하지만,
            // 추후 TCP 안정화 단계에서 null 수신 시 연결 종료 처리로 분리할 것
            if (string.IsNullOrEmpty(message))
            {
                continue;
            }

            Debug.Log($"[TCP] 수신: {message}");

            EnqueueReceivedMessage(message);
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

        writer.WriteLine(message);
    }

    private void OnDestroy()
    {
        isConnected = false;

        writer?.Close();
        reader?.Close();
        client?.Close();
        listener?.Stop();
    }
}
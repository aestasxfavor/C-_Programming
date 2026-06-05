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

    private TcpListener listener;
    private TcpClient client;
    private StreamReader reader;
    private StreamWriter writer;

    private bool isConnected;

    private readonly ConcurrentQueue<string> receiveQueue = new ConcurrentQueue<string>();

    public bool IsConnected => isConnected;

    private void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        while(receiveQueue.TryDequeue(out string message))
        {
            Debug.Log($"[TCP] 처리할 패킷: {message}");
         
            if(GameManager.Instance != null)
            {
                GameManager.Instance.OnReceivePacket(message);
            }
        }
    }

    public async void StartHost()
    {
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
        Debug.Log("[TCP] Client 접속 시도");

        client = new TcpClient();
        await client.ConnectAsync(hostIP, port);

        Debug.Log("[TCP] Host 접속 완료");

        SetupStream();
        _ = ReceiveLoop();
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
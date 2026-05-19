using UnityEngine;
using UnityEngine.UI;
using TMPro;

// 밑의 3가지가 있어야 네트워크 연결이 가능함
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

using System;
using System.Text;
using System.IO;


[Serializable]
public class DataClass
{
    public string name;
    public string message;
}

public class TCPChatManager : MonoBehaviour
{
    [SerializeField] private Button btnServer;
    [SerializeField] private Button btnClient;
    [SerializeField] private Button btnSendMsg;
    [SerializeField] private TMP_InputField inputFieldMsg;
    [SerializeField] private TextMeshProUGUI messeageText;

    [SerializeField] private string IpAddress = "127.0.0.1"; // 나 자신의 ip 주소
    [SerializeField] private int portNumber = 7777;         // 테스트를 하기위한 고유 포트 번호
    
    // 서버를 열고 클라가 접속하고 연결하는 통로
    private TcpClient client;
    private TcpListener listener;
    private NetworkStream stream;

    // 쓰고 읽는 기능을 처리하기 위해
    private StreamReader reader;
    private StreamWriter writer;

    // 연결에 성공했을 때 true로 처리
    private bool isConnect = false;
    private bool isRunning = false;

    private void Awake()
    {
        btnServer.onClick.AddListener(() => { _ = StartServerAsync(); });
        btnClient.onClick.AddListener(() => { _ = ConnectClientAsync(); });
        
    }

    // 서버를 열어서 대기함
    private async Task StartServerAsync()
    {
       
        if (isRunning) return;
        isRunning = true;

        try
        {
            // 소켓 생성에서 바인딩까지
            listener = new TcpListener(IPAddress.Parse(IpAddress), portNumber);
            listener.Start();
            AppendMessage("서버 시작, 접속 대기중입니다");

            // 연결 대기 
            client = await listener.AcceptTcpClientAsync(); // 메인이 아닌 서버에서 대기한다.
            
            // 연결 완료하고 연결통로 받기
            stream = client.GetStream();    // 연결을 받아온다.
            AppendMessage("유저와 연결 완료 되었습니다.");
            
            // 데이터를 읽고 쓰기 위해, 데이터를 송수신하기위해 미리 세팅함
            reader = new StreamReader(stream, Encoding.UTF8);
            writer = new StreamWriter(stream, Encoding.UTF8);

            // 상대가 보내는 데이터를 받기 위해 비동기 함수로 호출
            await RecieveDataAsync();
        }
        catch (Exception e)
        {
            AppendMessage(e);
        }

    }

    // 서버에 접속하는 비동기 함수
    private async Task ConnectClientAsync()
    {

    }

    // 데이터를 수신하는 비동기 함수
    private async Task RecieveDataAsync()
    {

    }

    // 데이터를 송신하는 비동기 함수
    private async Task SendDataAsync(DataClass data)
    {

    }

    private void AppendMessage(string s)
    {
        Debug.Log(s);
    }

    private void AppendMessage(Exception e)
    {
        Debug.LogException(e);
    }



    /*
     * 1. 마스터 서버 열기
     * 2. 클라이언트가 서버에 접속하기
     * 3. 데이터 수신 비동기 함수로 대기 => 서브스레드가  Task async await
     * 4. 데이터 송신
     */

    // 코루틴도 메인 쓰레드에서 활동한다.
}

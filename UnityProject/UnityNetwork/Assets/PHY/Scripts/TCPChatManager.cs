using UnityEngine;
using UnityEngine.UI;
using TMPro;

// 밑의 3가지가 있어야 네트워크 연결이 가능함
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System;
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

    [SerializeField] private string IPAddress = "127.0.0.1"; // 나 자신의 ip 주소
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

    private void Awake()
    {
        btnServer.onClick.AddListener(AppendMessage);
        // 람다식 말고 표현할 방법 있을 거 같은데
        // 그건 ㅇㅈ 람다를 안쓸 순 없음
        inputFieldMsg.onSubmit.AddListener((s) => { AppendMessage(s);});
    }

    private void AppendMessage()
    {
        Debug.Log("Test");
    }

    private void AppendMessage(string s)
    {
        Debug.Log($"Text: {s}");
    }
}

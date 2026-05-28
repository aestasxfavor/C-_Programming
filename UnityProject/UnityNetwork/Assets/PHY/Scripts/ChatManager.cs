using UnityEngine;
using UnityEngine.UI;
using TMPro;

using System.Threading.Tasks;

using System;
using System.Net;
using System.Net.Sockets;
using System.IO;



public class PacketData
{
    public string Name;
    public string Contents;
}

public class ChatData
{
    public string Name;
    public string Msg;

    public ChatData(string _name, string _msg)
    {
        Name = _name;
        Msg = _msg;
    }
    public ChatData() { }
}

public class GomokuData
{
    public int X;
    public int Y;

    public GomokuData(int _x, int _y)
    {
        X = _x;
        Y = _y;
    }
    public GomokuData() { }
}


public class ChatManager : MonoBehaviour
{
    private static ChatManager instance;
    public static ChatManager Instance { get { return instance; } }


    public const string ChatDataStr = "ChatData";
    public const string GomokuDataStr = "GomokuData";

    [SerializeField]
    private TextMeshProUGUI LogText;
    [SerializeField]
    private TMP_InputField ipInputField;
    [SerializeField]
    private TMP_InputField textInputField;
    [SerializeField]
    private Button btnMsgSend;


    [SerializeField]
    private string ipAddress = "127.0.0.1";
    [SerializeField]
    private int port = 7777;

    private TcpListener listener;
    private TcpClient client;
    private NetworkStream stream;

    private StreamWriter writer;
    private StreamReader reader;


    private bool isRunning = false;
    [SerializeField]
    private string myName;


    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(this.gameObject);
    }
    private void OnEnable()
    {
        textInputField.onSubmit.AddListener((msg) => { SendChatDataEvent(); });
        btnMsgSend.onClick.AddListener(SendChatDataEvent);
    }
    private void OnDisable()
    {
        textInputField.onSubmit.RemoveAllListeners();
        btnMsgSend.onClick.RemoveAllListeners();
    }
    public async Task StartServerAsync()
    {
        if (isRunning)
            return;
        isRunning = true;

        try
        {
            myName = "서버";

            string str = ipAddress;
            if (ipInputField.text != "")
                str = ipInputField.text;
            listener = new TcpListener(IPAddress.Parse(str), port);
            listener.Start();

            AppendLog("상대와의 연결을 대기중입니다.");
            client = await listener.AcceptTcpClientAsync();
            stream = client.GetStream();
            AppendLog("상대방이 연결되었습니다.");

            writer = new StreamWriter(stream, System.Text.Encoding.UTF8);
            reader = new StreamReader(stream, System.Text.Encoding.UTF8);
            writer.AutoFlush = true;

            GomokuManager.Instance.GameMode = GameModeType.multi;
            GomokuManager.Instance.IsRunning = true;
            GomokuManager.Instance.MyStone = MyStoneType.black;

            await RecieveDataAsync();
        }
        catch (Exception e)
        {
            AppendLog(e);
        }
    }
    public async Task ConnectClientAsync()
    {
        if (isRunning)
            return;
        isRunning = true;

        try
        {
            myName = "클라";
            client = new TcpClient();

            await client.ConnectAsync(ipAddress, port);
            AppendLog("접속이 되었습니다.");

            stream = client.GetStream();
            writer = new StreamWriter(stream, System.Text.Encoding.UTF8);
            reader = new StreamReader(stream, System.Text.Encoding.UTF8);
            writer.AutoFlush = true;

            GomokuManager.Instance.GameMode = GameModeType.multi;
            GomokuManager.Instance.IsRunning = true;
            GomokuManager.Instance.MyStone = MyStoneType.white;

            await RecieveDataAsync();
        }
        catch (Exception e)
        {
            AppendLog(e);
        }
    }
    private async Task RecieveDataAsync()
    {
        while (reader != null)
        {
            string json = await reader.ReadLineAsync();
            PacketData data = JsonUtility.FromJson<PacketData>(json);

            switch (data.Name)
            {
                case ChatDataStr:
                    ChatData chatData = JsonUtility.FromJson<ChatData>(data.Contents);
                    LogText.text += $" {chatData.Name} : {chatData.Msg}\n";
                    break;
                case GomokuDataStr:
                    GomokuData gomokuData = JsonUtility.FromJson<GomokuData>(data.Contents);
                    GomokuManager.Instance.PutStone(gomokuData.X, gomokuData.Y);
                    break;
            }
        }
    }
    private void SendChatDataEvent()
    {
        PacketData data = new PacketData();

        ChatData chatData = new ChatData();
        chatData.Name = myName;
        chatData.Msg = textInputField.text;
        string json = JsonUtility.ToJson(chatData);

        data.Name = ChatDataStr;
        data.Contents = json;

        textInputField.text = "";
        LogText.text += $" {chatData.Name} : {chatData.Msg}\n";
        //Task.Run(async () => { await SendDataAsync(data); });
        _ = SendDataAsync(data);
    }

    public void SendGomokuDataEvent(int _x, int _y)
    {
        PacketData data = new PacketData();

        GomokuData gomokuData = new GomokuData();
        gomokuData.X = _x;
        gomokuData.Y = _y;
        string json = JsonUtility.ToJson(gomokuData);

        data.Name = GomokuDataStr;
        data.Contents = json;

        _ = SendDataAsync(data);
    }

    private async Task SendDataAsync(PacketData _data)
    {
        string json = JsonUtility.ToJson(_data);
        await writer.WriteLineAsync(json);
    }
    private void AppendLog(string _str)
    {
        LogText.text += " " + _str + "\n";
        Debug.Log(_str);
    }
    private void AppendLog(Exception _e)
    {
        LogText.text += " " + _e.ToString() + "\n";
        Debug.Log(_e);
    }


}
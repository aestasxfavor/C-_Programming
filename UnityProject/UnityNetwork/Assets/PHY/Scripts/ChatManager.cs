using UnityEngine;

using System.Net;
using System.Net.Sockets;
using System.IO;

public class ChatManager : MonoBehaviour
{
    private static ChatManager instance;
    public static ChatManager Instance { get { return instance; } }
    private TcpListener listener;
    private TcpClient client;
    private NetworkStream stream;

    private StreamReader reader;
    private StreamWriter writer;

    private int portNumber = 7777;
    [SerializeField] private string ipAdress = "127.0.0.1";

    private bool isRunning= false;
    [SerializeField] private string myName;

    private void Awake()
    {
        if(instance == null) instance = this;
        else
        {
            
        }
    }

}

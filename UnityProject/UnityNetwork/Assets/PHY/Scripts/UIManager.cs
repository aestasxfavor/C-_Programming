using UnityEngine;
using UnityEngine.UI;
using TMPro;

using System.Threading.Tasks;

public class UIManager : MonoBehaviour
{
    public GameObject TitleParent;
    public GameObject ModeParent;
    public GameObject MultiParent;
    public GameObject ChatParent;

    [SerializeField]
    private Button btnGameStart;
    [SerializeField]
    private Button btnSingle;
    [SerializeField]
    private Button btnMulti;

    [SerializeField]
    private Button btnServer;
    [SerializeField]
    private Button btnClient;

    private void OnEnable()
    {
        btnGameStart.onClick.AddListener(GameStartBtn);
        btnSingle.onClick.AddListener(SingleModeBtn);
        btnMulti.onClick.AddListener(MulitModeBtn);
        btnServer.onClick.AddListener(ServerBtn);
        btnClient.onClick.AddListener(ClientBtn);
    }
    private void OnDisable()
    {
        btnGameStart.onClick.RemoveAllListeners();
        btnSingle.onClick.RemoveAllListeners();
        btnMulti.onClick.RemoveAllListeners();
        btnServer.onClick.RemoveAllListeners();
        btnClient.onClick.RemoveAllListeners();
    }

    private void GameStartBtn()
    {
        TitleParent.SetActive(false);
        ModeParent.SetActive(true);
    }

    private void SingleModeBtn()
    {
        ModeParent.SetActive(false);
        GomokuManager.Instance.GameMode = GameModeType.single;
        GomokuManager.Instance.IsRunning = true;
    }

    private void MulitModeBtn()
    {
        ModeParent.SetActive(false);
        MultiParent.SetActive(true);
    }


    private void ServerBtn()
    {
        //Task.Run(async () => { await ChatManager.Instance.StartServerAsync(); });
        _ = ChatManager.Instance.StartServerAsync();
        MultiParent.SetActive(false);
        ChatParent.SetActive(true);
    }

    private void ClientBtn()
    {
        //Task.Run(ChatManager.Instance.ConnectClientAsync);
        _ = ChatManager.Instance.ConnectClientAsync();
        MultiParent.SetActive(false);
        ChatParent.SetActive(true);
    }




}
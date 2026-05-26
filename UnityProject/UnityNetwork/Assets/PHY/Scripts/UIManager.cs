using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public GameObject TitleParent;
    public GameObject ModeParent;
    public GameObject MultiParent;

    [SerializeField] private Button btnGameStart;
    [SerializeField] private Button btnSingle;
    [SerializeField] private Button btnMulti;

    [SerializeField] private Button btnServer;
    [SerializeField] private Button btnClient;

    private void OnEnable()
    {
        btnGameStart.onClick.AddListener(GameStartBtn);
        btnSingle.onClick.AddListener(SingleModeBtn);
        btnMulti.onClick.AddListener(MultiModeBtn);
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

    public void GameStartBtn()
    {
        TitleParent.SetActive(false);
        ModeParent.SetActive(true);
    }

    public void SingleModeBtn()
    {
        ModeParent.SetActive(false);
    }

    public void MultiModeBtn()
    {
        ModeParent.SetActive(false);
        MultiParent.SetActive(true);
    }

    public void ServerBtn()
    {

    }

    public void ClientBtn() 
    {

    }


}

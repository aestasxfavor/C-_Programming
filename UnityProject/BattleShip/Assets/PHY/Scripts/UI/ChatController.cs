using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChattingController : MonoBehaviour
{
    [Header("채팅 UI")]
    [SerializeField] private GameObject chatPanel;
    [SerializeField] private TMP_InputField chatInputField;
    [SerializeField] private Button sendButton;
    [SerializeField] private TextMeshProUGUI chatLogText;
    [SerializeField] private ScrollRect chatScrollRect;

    [Header("채팅 설정")]
    [SerializeField] private int maxMessageCount = 30;

    private readonly List<string> chatMessages = new List<string>();

    private Func<string, bool> packetSender;
    private Func<bool> checkDisconnected;
    private Func<bool> checkLeaving;
    private Func<bool> checkRestarting;

    public void Setup(
        Func<string, bool> packetSender,
        Func<bool> disconnectedCheck,
        Func<bool> leavingCheck,
        Func<bool> restartingCheck)
    {
        this.packetSender = packetSender;
        checkDisconnected = disconnectedCheck;
        checkLeaving = leavingCheck;
        checkRestarting = restartingCheck;
    }

    private void Awake()
    {
        if (sendButton != null)
        {
            sendButton.onClick.AddListener(OnClickSendButton);
        }

        if (chatInputField != null)
        {
            chatInputField.onSubmit.AddListener(OnSubmitChat);
        }
    }

    private void OnDestroy()
    {
        if (sendButton != null)
        {
            sendButton.onClick.RemoveListener(OnClickSendButton);
        }

        if (chatInputField != null)
        {
            chatInputField.onSubmit.RemoveListener(OnSubmitChat);
        }
    }

    public void OnClickSendButton()
    {
        TrySendChatMessage();
    }

    public void ReceiveChatPacket(string[] packetParts)
    {
        if (packetParts == null || packetParts.Length < 2)
        {
            return;
        }

        string message = GetMessageFromPacket(packetParts);

        if (string.IsNullOrEmpty(message))
        {
            return;
        }

        AddChatMessage("상대", message);

        Debug.Log($"[Chat] 상대 메시지 수신: {message}");
    }

    public void ClearChat()
    {
        chatMessages.Clear();

        if (chatLogText != null)
        {
            chatLogText.text = "";
        }
    }

    private void OnSubmitChat(string inputText)
    {
        TrySendChatMessage();
    }

    private void TrySendChatMessage()
    {
        if (chatInputField == null)
        {
            return;
        }

        string message = chatInputField.text.Trim();

        if (string.IsNullOrEmpty(message))
        {
            FocusInputField();
            return;
        }

        if (!CanSendChat())
        {
            Debug.Log("[Chat] 현재 상태에서는 채팅 전송 불가");
            FocusInputField();
            return;
        }

        string safeMessage = MakeSafeMessage(message);
        string packet = $"{PacketProtocol.CHAT}|{safeMessage}";

        if (!SendPacket(packet))
        {
            FocusInputField();
            return;
        }

        AddChatMessage("나", safeMessage);

        chatInputField.text = "";
        FocusInputField();

        Debug.Log($"[Chat] 내 메시지 전송: {safeMessage}");
    }

    private bool CanSendChat()
    {
        if (IsDisconnected())
        {
            return false;
        }

        if (IsLeaving())
        {
            return false;
        }

        if (IsRestarting())
        {
            return false;
        }

        return true;
    }

    private bool SendPacket(string packet)
    {
        if (packetSender == null)
        {
            return false;
        }

        return packetSender(packet);
    }

    private void AddChatMessage(string senderName, string message)
    {
        string line = $"{senderName}: {message}";

        chatMessages.Add(line);

        while (chatMessages.Count > maxMessageCount)
        {
            chatMessages.RemoveAt(0);
        }

        RefreshChatLog();
    }

    private void RefreshChatLog()
    {
        if (chatLogText == null)
        {
            return;
        }

        chatLogText.text = string.Join("\n", chatMessages);

        ScrollToBottom();
    }

    private void ScrollToBottom()
    {
        if (chatScrollRect == null)
        {
            return;
        }

        Canvas.ForceUpdateCanvases();
        chatScrollRect.verticalNormalizedPosition = 0f;
    }

    private string MakeSafeMessage(string message)
    {
        if (string.IsNullOrEmpty(message))
        {
            return "";
        }

        return message
            .Replace("|", " ")
            .Replace("\r", " ")
            .Replace("\n", " ");
    }

    private string GetMessageFromPacket(string[] packetParts)
    {
        if (packetParts.Length < 2)
        {
            return "";
        }

        return string.Join("|", packetParts, 1, packetParts.Length - 1);
    }

    private void FocusInputField()
    {
        if (chatInputField == null)
        {
            return;
        }

        chatInputField.ActivateInputField();
    }

    private bool IsDisconnected()
    {
        return checkDisconnected != null && checkDisconnected();
    }

    private bool IsLeaving()
    {
        return checkLeaving != null && checkLeaving();
    }

    private bool IsRestarting()
    {
        return checkRestarting != null && checkRestarting();
    }
}
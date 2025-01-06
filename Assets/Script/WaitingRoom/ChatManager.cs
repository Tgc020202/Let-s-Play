using UnityEngine;
using Photon.Chat;
using Photon.Pun;
using ExitGames.Client.Photon;
using UnityEngine.UI;

public class ChatManager : MonoBehaviour, IChatClientListener
{
    // UI Components
    public Transform chatContent;
    public InputField chatInputField;
    public Button sendButton;
    public Button openButton;
    public Button closeButton;

    // Scripts
    private ChatClient chatClient;

    // GameObjects
    public GameObject chatBox;
    public GameObject chatMessagePrefab;

    // Defines    
    private bool isChatBoxActive = false;
    private const byte ChatMessageEventCode = 1;
    private string chatChannel = "GlobalChat";

    void Start()
    {
        chatClient = new ChatClient(this);
        chatClient.Connect(PhotonNetwork.PhotonServerSettings.AppSettings.AppIdChat, PhotonNetwork.AppVersion, new AuthenticationValues(PhotonNetwork.NickName));

        chatBox.SetActive(false);
        sendButton.onClick.AddListener(SendMessage);
        openButton.onClick.AddListener(ToggleChatBox);
        closeButton.onClick.AddListener(ToggleChatBox);
        openButton.interactable = true;
        closeButton.interactable = false;
    }

    void Update()
    {
        if (chatClient != null)
        {
            chatClient.Service();
        }

        if (Input.GetKey(KeyCode.Return))
        {
            SendMessage();
        }
    }

    public void ToggleChatBox()
    {
        isChatBoxActive = !isChatBoxActive;
        chatBox.SetActive(isChatBoxActive);
        openButton.interactable = !isChatBoxActive;
        closeButton.interactable = isChatBoxActive;
    }

    public void OnConnected()
    {
        Debug.Log("Connected to Photon Chat!");
        chatClient.Subscribe(chatChannel);
    }

    public void OnDisconnected()
    {
        Debug.Log("Disconnected from Photon Chat!");
    }

    public void OnChatStateChange(ChatState state)
    {
        Debug.Log($"Chat state changed: {state}");
    }

    public void OnSubscribed(string[] channels, bool[] results)
    {
        Debug.Log($"Subscribed to channel: {channels[0]}");
        AddMessageToChat("System", "You joined the global chat!");
    }

    public void OnUnsubscribed(string[] channels)
    {
        Debug.Log($"Unsubscribed from channel: {channels[0]}");
    }

    public void OnGetMessages(string channelName, string[] senders, object[] messages)
    {
        for (int i = 0; i < senders.Length; i++)
        {
            AddMessageToChat(senders[i], messages[i].ToString());
        }
    }

    public void OnPrivateMessage(string sender, object message, string channelName)
    {
        Debug.Log($"Private message from {sender}: {message}");
    }

    public void OnErrorInfo(string channelName, string error, object data)
    {
        Debug.LogError($"Error in channel {channelName}: {error}");
    }

    public void DebugReturn(DebugLevel level, string message)
    {
        // This is required by IChatClientListener
        Debug.Log($"DebugReturn [{level}]: {message}");
    }

    public void OnStatusUpdate(string user, int status, bool gotMessage, object message)
    {
        Debug.Log($"Status update for user {user}: status={status}, gotMessage={gotMessage}, message={message}");
    }

    public void OnUserSubscribed(string channel, string user)
    {
        Debug.Log($"User {user} subscribed to channel {channel}");
        AddMessageToChat("System", $"{user} joined the channel.");
    }

    public void OnUserUnsubscribed(string channel, string user)
    {
        Debug.Log($"User {user} unsubscribed from channel {channel}");
        AddMessageToChat("System", $"{user} left the channel.");
    }

    public void SendMessage()
    {
        string message = chatInputField.text.Trim();
        if (!string.IsNullOrEmpty(message))
        {
            chatClient.PublishMessage(chatChannel, message);
            chatInputField.text = "";
        }
    }

    private void AddMessageToChat(string sender, string message)
    {
        GameObject newMessage = Instantiate(chatMessagePrefab, chatContent);
        Text messageText = newMessage.GetComponent<Text>();

        if (sender == SessionManager.Instance.username)
        {
            sender = "Me";
            messageText.color = Color.white;
        }
        else if (sender == "System")
        {
            messageText.color = Color.green;
        }
        else
        {
            messageText.color = Color.gray;
        }

        messageText.text = $"<b>{sender}:</b> {message}";
    }

    void OnDestroy()
    {
        if (chatClient != null)
        {
            chatClient.Disconnect();
        }
    }
}

using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class ChatManager : NetworkBehaviour
{
    public static ChatManager Singleton;
    public static bool IsTyping { get; private set; }  

    public ChatMessage chatMessagePrefab;
    public CanvasGroup CanvasGroup;
    public TMP_InputField chatInput;

    public string playerName;

    int lastSubmitFrame = -1;

    void Awake()
    {
        Singleton = this;
    }

    void OnEnable()
    {
        chatInput.onSubmit.AddListener(OnSubmit);
        chatInput.onSelect.AddListener(OnChatSelected);
        chatInput.onDeselect.AddListener(OnChatDeselected);
    }

    void OnDisable()
    {
        chatInput.onSubmit.RemoveListener(OnSubmit);
        chatInput.onSelect.RemoveListener(OnChatSelected);
        chatInput.onDeselect.RemoveListener(OnChatDeselected);
        IsTyping = false;
    }

    void OnChatSelected(string _) => IsTyping = true;
    void OnChatDeselected(string _) => IsTyping = false;

    void Update()
    {
        if (Keyboard.current == null) return;

        if (Keyboard.current.enterKey.wasPressedThisFrame
            && !chatInput.isFocused
            && Time.frameCount != lastSubmitFrame)
        {
            chatInput.ActivateInputField();
        }

        if (chatInput.isFocused && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            chatInput.text = "";
            EventSystem.current.SetSelectedGameObject(null);
        }

        if (chatInput.isFocused
            && Mouse.current != null
            && Mouse.current.leftButton.wasPressedThisFrame
            && !EventSystem.current.IsPointerOverGameObject())
        {
            EventSystem.current.SetSelectedGameObject(null);
        }
    }

    void OnSubmit(string text)
    {
        lastSubmitFrame = Time.frameCount;
        SendChatMessage(text, playerName);
        chatInput.text = "";
        EventSystem.current.SetSelectedGameObject(null);
    }

    public void SendChatMessage(string message, string sender = null)
    {
        if (string.IsNullOrWhiteSpace(message)) return;

        string formatted = (string.IsNullOrEmpty(sender) ? "Player" : sender) + " > " + message;
        SendChatMessageServerRpc(formatted);
    }

    void AddMessage(string msg)
    {
        ChatMessage CM = Instantiate(chatMessagePrefab, CanvasGroup.transform);
        CM.SetMessage(msg);
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    void SendChatMessageServerRpc(string message)
    {
        ReceiveChatMessageClientRpc(message);
    }

    [Rpc(SendTo.ClientsAndHost)]
    void ReceiveChatMessageClientRpc(string message)
    {
        AddMessage(message);
    }
}

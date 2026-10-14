using TMPro;
using UnityEngine;

public class ChatMessage : MonoBehaviour
{
    public TMP_Text messageText;

    public void SetMessage(string message)
    {
        messageText.text = message;
    }
}

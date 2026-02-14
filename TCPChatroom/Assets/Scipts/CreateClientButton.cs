using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class CreateClientButton : MonoBehaviour
{
    public GameObject clientPrefab;
    public GameObject disconnectButtonPrefab;
    public GameObject chatPrefab;

    public TMP_InputField usernameInputField;

    public Canvas canvas;
    public GameObject RightVerticalBox;

    private GameObject client;
    private Button clientDisconnectButton;
    private GameObject chatbox;
    private string clientUsername;
    private TextMeshProUGUI sendButtonText;

    public void OnButtonClick()
    {
        if (usernameInputField.text != null)
        {
            
            clientUsername = usernameInputField.text;
            client = Instantiate(clientPrefab);

            client.GetComponent<ClientScript>().setUsername(clientUsername);

            clientDisconnectButton = Instantiate(disconnectButtonPrefab).GetComponent<Button>();
            clientDisconnectButton.transform.SetParent(RightVerticalBox.transform, false);
            clientDisconnectButton.GetComponentInChildren<TextMeshProUGUI>().text = "Disconnect " + clientUsername;

            chatbox = Instantiate(chatPrefab);
            chatbox.transform.SetParent(canvas.transform, false);

            clientDisconnectButton.GetComponent<DisconnectClientButton>().ChatBox = chatbox;

            Transform[] placeHolderTextObject = chatbox.GetComponentsInChildren<Transform>();

            foreach (Transform placeHolder in placeHolderTextObject)
            {
                GameObject childGameObject = placeHolder.gameObject;
                if (childGameObject.name == "SendMessageButtonText")
                {
                    sendButtonText = childGameObject.GetComponent<TextMeshProUGUI>();
                    sendButtonText.text += clientUsername;
                }
                else if (childGameObject.name == "Content")
                {
                    client.GetComponent<ClientScript>().chatBoxMessageZone = childGameObject;
                }
                else if (childGameObject.name == "MessageInputField")
                {
                    client.GetComponent<ClientScript>().messageInputField = childGameObject.GetComponent<TMP_InputField>();
                }
            }
        }
        else
        {
            Debug.Log("Username cannot be empty");
        }
    }
}

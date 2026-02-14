using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class CreateClientButton : MonoBehaviour
{
    public GameObject clientPrefab;
    public GameObject disconnectButtonPrefab;
    public GameObject chatPrefab;
    public GameObject chatMessagePrefab;

    public TMP_InputField usernameInputField;
    public GameObject MessageBoxPlaceHolderText;

    public Canvas canvas;
    public GameObject RightVerticalBox;

    private GameObject client;
    private Button clientDisconnectButton;
    private GameObject chatbox;
    private GameObject messageBox;
    private Button disconnectButton;
    private string clientUsername;

    public void OnButtonClick()
    {
        if (usernameInputField.text != null)
        {
            
            clientUsername = usernameInputField.text;
            client = Instantiate(clientPrefab);

            client.GetComponent<ClientScript>().setUsername(usernameInputField.text);

            clientDisconnectButton = Instantiate(disconnectButtonPrefab).GetComponent<Button>();
            clientDisconnectButton.transform.SetParent(RightVerticalBox.transform, false);
            clientDisconnectButton.GetComponentInChildren<TextMeshProUGUI>().text = "Disconnect " + clientUsername;

            chatbox = Instantiate(chatPrefab);
            chatbox.transform.SetParent(canvas.transform, false);

            messageBox = Instantiate(chatMessagePrefab);
            messageBox.transform.SetParent(chatbox.transform, false);
            // need to make it so chat boxes also get destroyed when the client disconnects and make the place holder text display the clients username so you can tell which is which
            //messageBox.
        }
        else
        {
            Debug.Log("Username cannot be empty");
        }
    }
}

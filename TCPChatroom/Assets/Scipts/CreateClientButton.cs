using UnityEngine;
using TMPro;

public class CreateClientButton : MonoBehaviour
{
    public GameObject clientPrefab;

    public TMP_InputField usernameInputField;

    private GameObject client;

    public string GetInputFieldText()
    {
        string clientUsername;

        return clientUsername = usernameInputField.text;
    }

    public void OnButtonClick()
    {
        if (usernameInputField.text != null)
        {
            client = Instantiate(clientPrefab);

            client.GetComponent<ClientBehavior>().setUsername(usernameInputField.text);
        }
        else
        {
            Debug.Log("Username cannot be empty");
        }
    }
}

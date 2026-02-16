using UnityEngine;
using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using TMPro;

public class ClientScript : MonoBehaviour
{
    public GameObject helperPrefab;
    public GameObject chatMessagePrefab;
    private Helper helper;
    private string messageToSend;

    private string serverIP; // Set this to your server's IP address.
    private int serverPort;             // Set this to your server's port.

    private TcpClient client;
    private NetworkStream stream;
    private Thread clientReceiveThread;

    private string m_Username;
    [HideInInspector]
    public GameObject chatBoxMessageZone;
    public TMP_InputField messageInputField;

    public void setUsername(string username)
    {
        m_Username = username;
    }

    public string getUsername()
    {
        return m_Username;
    }

    public void OnSendMessageButtonClicked()
    {
        if (string.IsNullOrEmpty(messageToSend))
        {
            Debug.Log("Message is empty!");
        }
        else
        {
            SendMessageToServer(m_Username + ": " + messageToSend);
            messageInputField.text = "";
        }
    }

    void Start()
    {
        helper = helperPrefab.GetComponent<Helper>();

        serverIP = helper.ip;
        serverPort = helper.port;
        ConnectToServer();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            if (string.IsNullOrEmpty(messageInputField.text))
            {
                Debug.Log("Message is empty!");
            }
            else
            {
                messageToSend = messageInputField.text;
                SendMessageToServer(m_Username + ": " + messageToSend);
                messageInputField.text = "";
            }
        }

        if (Input.GetKeyDown(KeyCode.J))
        {
            DisconnectClient();
        }
    }

    void ConnectToServer()
    {
        try
        {
            client = new TcpClient(serverIP, serverPort);
            stream = client.GetStream();
            Debug.Log("Connected to server.");

            clientReceiveThread = new Thread(new ThreadStart(ListenForData));
            clientReceiveThread.IsBackground = true;
            clientReceiveThread.Start();
            SendMessageToServer(m_Username + ": has joined the chat");
            Debug.Log("Instance ID: " + GetInstanceID());
        }
        catch (SocketException e)
        {
            Debug.LogError("SocketException: " + e.ToString());
        }
    }

    private void ListenForData()
    {
        try
        {
            byte[] bytes = new byte[1024];
            while (true)
            {
                // Check if there's any data available on the network stream
                if (stream.DataAvailable)
                {
                    int length;
                    // Read incoming stream into byte array.
                    while ((length = stream.Read(bytes, 0, bytes.Length)) != 0)
                    {
                        var incomingData = new byte[length];
                        Array.Copy(bytes, 0, incomingData, 0, length);
                        // Convert byte array to string message.
                        string serverMessage = Encoding.UTF8.GetString(incomingData);
                        Debug.Log("Server message received: " + serverMessage);

                        UnityMainThreadDispatcher.Enqueue(() =>
                        {
                            GameObject message = Instantiate(chatMessagePrefab);
                            message.transform.SetParent(chatBoxMessageZone.transform, false);
                            message.GetComponent<TextMeshProUGUI>().text = serverMessage;
                        });
                    }
                }
            }
        }
        catch (SocketException socketException)
        {
            Debug.Log("Socket exception: " + socketException);
        }
    }

    public void SendMessageToServer(string message)
    {
        if (client == null)
        {
            Debug.LogError("Client is null.");
            return;
        }
        else if (!client.Connected)
        {
            Debug.LogError("Client is not connected to server.");
            return;
        }

        byte[] data = Encoding.UTF8.GetBytes(message);
        stream.Write(data, 0, data.Length);
        Debug.Log("Sent message to server: " + message);
    }

    public void DisconnectClient()
    {
        SendMessageToServer(m_Username + ": has left the chat");

        Debug.Log("Instance ID: " + GetInstanceID());

        if (stream != null)
            stream?.Close();
        if (client != null)
            client?.Close();
        if (clientReceiveThread != null)
            clientReceiveThread?.Abort();

        Destroy(gameObject);
    }

    void OnApplicationQuit()
    {
        DisconnectClient();
    }
}
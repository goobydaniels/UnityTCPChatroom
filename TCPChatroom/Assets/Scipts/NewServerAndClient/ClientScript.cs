using UnityEngine;
using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;

public class ClientScript : MonoBehaviour
{
    public GameObject helperPrefab;
    private Helper helper;

    private string serverIP; // Set this to your server's IP address.
    private int serverPort;             // Set this to your server's port.
    private string messageToSend = "Hello Server!"; // The message to send.

    private TcpClient client;
    private NetworkStream stream;
    private Thread clientReceiveThread;

    private string m_Username;

    public void setUsername(string username)
    {
        m_Username = username;
    }

    public string getUsername()
    {
        return m_Username;
    }

    public void OnDisconnectButtonClicked()
    {
        Debug.Log("I want to disconnect");

        SendMessageToServer("I want to disconnect");

        DisconnectClient();

        Destroy(gameObject);
    }

    void Start()
    {
        helper = helperPrefab.GetComponent<Helper>();

        serverIP = helper.ip;
        serverPort = helper.port;
        ConnectToServer();
        SendMessageToServer(m_Username);
    }

    void Update()
    {
        //disable this if you are sending from another script or a button
        //if (Input.GetKeyDown(KeyCode.Return))
        //{
        //    SendMessageToServer(messageToSend);
        //}
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
        if (client == null || !client.Connected)
        {
            Debug.LogError("Client not connected to server.");
            return;
        }

        byte[] data = Encoding.UTF8.GetBytes(message);
        stream.Write(data, 0, data.Length);
        Debug.Log("Sent message to server: " + message);
    }

    public void DisconnectClient()
    {
        if (stream != null)
            stream?.Close();
        if (client != null)
            client?.Close();
        if (clientReceiveThread != null)
            clientReceiveThread?.Abort();
    }

    void OnApplicationQuit()
    {
        DisconnectClient();
    }
}
using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;
using UnityEngine.InputSystem;

public class ServerScript : MonoBehaviour
{
    TcpListener server = null;
    TcpClient client = null;
    NetworkStream stream = null;
    Thread thread;

    public GameObject helperPrefab;
    private Helper helper;

    // Initalizes a new thread that runs SetupServer, this creates a thread seperate from Unity's main thread
    private void Start()
    {
        helper = helperPrefab.GetComponent<Helper>();
        thread = new Thread(new ThreadStart(SetupServer));
        thread.Start();
    }

    // The 
    private void Update()
    {
        //if (Input.GetKeyDown(KeyCode.Space))
        //{
        //    SendMessageToClient("Hello");
        //}
    }

    private void SetupServer()
    {
        try
        {
            IPAddress localAddr = IPAddress.Parse(helper.ip);
            server = new TcpListener(localAddr, helper.port);
            server.Start();

            byte[] buffer = new byte[1024];
            string data = null;

            while (true)
            {
                Debug.Log("Waiting for connection...");
                client = server.AcceptTcpClient();
                Debug.Log("Connected!");

                data = null;
                stream = client.GetStream();

                int i;

                while ((i = stream.Read(buffer, 0, buffer.Length)) != 0)
                {
                    data = Encoding.UTF8.GetString(buffer, 0, i);
                    Debug.Log("Received: " + data);

                    string response = "Server response: " + data.ToString();
                    SendMessageToClient(message: response);
                }
                client.Close();
            }
        }
        catch (SocketException e)
        {
            Debug.Log("SocketException: " + e);
        }
        finally
        {
            server.Stop();
        }
    }

    private void OnApplicationQuit()
    {
        stream.Close();
        client.Close();
        server.Stop();
        thread.Abort();
    }

    public void SendMessageToClient(string message)
    {
        byte[] msg = Encoding.UTF8.GetBytes(message);
        stream.Write(msg, 0, msg.Length);
        Debug.Log("Sent: " + message);
    }
}
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
            // Parse ip as IpAddress
            IPAddress localAddr = IPAddress.Parse(helper.ip);
            // TCPListener created on ip address and port
            server = new TcpListener(localAddr, helper.port);
            // Start the server
            server.Start();

            // byte array declared to store data from the client
            byte[] buffer = new byte[1024];
            string data = null;

            // Listen and accept new clients
            while (true)
            {
                Debug.Log("Waiting for connection...");
                // Waits and accepts an incoming connection
                client = server.AcceptTcpClient();
                Debug.Log("Connected!");

                data = null;
                // Gets netowrk stream assoicated with client
                stream = client.GetStream();

                int i;

                // Used to read the data sent by client, reads into buffer and continues until there is no more data
                while ((i = stream.Read(buffer, 0, buffer.Length)) != 0)
                {
                    // Converts byte array from buffer into a string using utf8, this is the data recieved from the client
                    data = Encoding.UTF8.GetString(buffer, 0, i);
                    Debug.Log("Received: " + data);

                    // Makes responce string
                    string response = "Server response: " + data.ToString();
                    SendMessageToClient(message: response);
                }
                // Closes connection with client
                //client.Close();
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
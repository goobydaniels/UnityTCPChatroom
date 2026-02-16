using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
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

    private List<TcpClient> clients = new List<TcpClient>();

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

            while (true)
            {
                Debug.Log("Waiting for connection...");
                TcpClient newClient = server.AcceptTcpClient();
                Debug.Log("Client connected!");

                lock (clients)
                {
                    clients.Add(newClient);
                }

                Thread clientThread = new Thread(() => HandleClient(newClient));
                clientThread.Start();
            }
        }
        catch (SocketException e)
        {
            Debug.Log("SocketException: " + e);
        }
    }

    private void HandleClient(TcpClient client)
    {
        NetworkStream stream = client.GetStream();
        byte[] buffer = new byte[1024];

        try
        {
            int bytesRead;

            while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) != 0)
            {
                string data = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                Debug.Log("Received: " + data);

                BroadcastMessage(data);
            }
        }
        catch (Exception e)
        {
            Debug.Log("Client disconnected: " + e.Message);
        }
        finally
        {
            lock (clients)
            {
                clients.Remove(client);
            }

            stream.Close();
            client.Close();
        }
    }

    private void BroadcastMessage(string message)
    {
        byte[] msg = Encoding.UTF8.GetBytes(message);

        lock (clients)
        {
            foreach (TcpClient client in clients)
            {
                try
                {
                    NetworkStream stream = client.GetStream();
                    stream.Write(msg, 0, msg.Length);
                }
                catch
                {
                    // Ignore broken clients
                }
            }
        }

        Debug.Log("Broadcasted: " + message);
    }

    private void OnApplicationQuit()
    {
        lock (clients)
        {
            foreach (TcpClient client in clients)
            {
                client.Close();
            }
        }

        server?.Stop();
        thread?.Abort();
    }

    public void SendMessageToClient(string message)
    {
        byte[] msg = Encoding.UTF8.GetBytes(message);
        stream.Write(msg, 0, msg.Length);
        Debug.Log("Sent: " + message);
    }
}
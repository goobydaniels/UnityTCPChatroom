using TMPro;
using Unity.Collections;
using Unity.Networking.Transport;
using UnityEngine;

public class ServerBehavior : MonoBehaviour
{
    // This is the primary API that interacts with the transport
    NetworkDriver m_Driver;
    // This holds all connections to the server
    NativeList<NetworkConnection> m_Connections;
    // Define TCP pipeline before connections are made
    NetworkPipeline TCPPipeline;

    private string clientJoinedMessage = ": has joined the chat";
    private string clientLeftMessage = ": has left the chat";
    //private string messageBreaker = ": ";

    void Start()
    {
        // Creates TCP network driver
        m_Driver = NetworkDriver.Create(new WebSocketNetworkInterface());
        // Populates connections list, setting the intial capasity to 16 clients, and marks the allocators as persistent
        m_Connections = new NativeList<NetworkConnection>(16, Allocator.Persistent);

        // Define TCP pipeline before connections are made
        TCPPipeline = m_Driver.CreatePipeline(typeof(ReliableSequencedPipelineStage));

        // Define endpoint to port 7777
        var endpoint = NetworkEndpoint.AnyIpv4.WithPort(7777);

        // Attempt to bind the driver to endpoint, if the binding is successful the listen method is called
        if (m_Driver.Bind(endpoint) != 0)
        {
            Debug.LogError("failed to bind to port 7777");
            return;
        }

        // Listen to all IP addresses on computer
        m_Driver.Listen();
    }

    void Update()
    {
        // This forces a synchronization on main thread in order to update and handle data later
        m_Driver.ScheduleUpdate().Complete();

        // Clean up old connections
        for (int i = 0; i < m_Connections.Length; i++)
        {
            if (!m_Connections[i].IsCreated)
            {
                m_Connections.RemoveAtSwapBack(i);
                i--;
            }
        }

        // Accepts new connections
        NetworkConnection c;
        while ((c = m_Driver.Accept())!= default)
        {
            m_Connections.Add(c);
            Debug.Log("Accpeted a connection.");
        }

        // Call pop event for connection while there are still events that need to be processed
        for (int i = 0;i < m_Connections.Length; i++)
        {
            // The DataStreamReader returned here will be used to read the data messages
            DataStreamReader stream;
            NetworkEvent.Type cmd;

            while ((cmd = m_Driver.PopEventForConnection(m_Connections[i], out stream)) != NetworkEvent.Type.Empty)
            {
                // If the NetworkEvent is connect
                if (cmd == NetworkEvent.Type.Connect)
                {
                    // Reads a string from the data stream
                    FixedString4096Bytes usernameJoined = stream.ReadFixedString4096();
                    usernameJoined += clientJoinedMessage;
                    Debug.Log($"Got {usernameJoined} from a client");

                    //usernameJoined += clientJoinedMessage;

                    Debug.Log(usernameJoined);

                    // To send data back a DataStreamWriter is used, a writer is created when BeginSend is used
                    // NetowrkPipeline.Null is the unreliable pipeline (udp) will need to use the reliable pipeline documented here: https://docs.unity3d.com/Packages/com.unity.transport@2.0/manual/pipelines-usage.html
                    m_Driver.BeginSend(TCPPipeline, m_Connections[i], out var writer);
                    writer.WriteFixedString4096(usernameJoined);
                    m_Driver.EndSend(writer);
                }
                else if (cmd == NetworkEvent.Type.Data)
                {
                    /*
                     * This is where the messages need to be sent to each client
                     */

                    // Reads a string from the data stream
                    FixedString4096Bytes message = stream.ReadFixedString4096();
                    Debug.Log($"Got {message} from a client");

                    // Message needs to have user name attached to the front

                    // Send message back to clients
                    m_Driver.BeginSend(TCPPipeline, m_Connections[i], out var writer);
                    writer.WriteFixedString4096(message);
                    m_Driver.EndSend(writer);
                }
                // Handle disconnect
                else if (cmd == NetworkEvent.Type.Disconnect)
                {
                    // Reads a string from the data stream
                    FixedString4096Bytes usernameLeft = stream.ReadFixedString4096();
                    Debug.Log($"Got {usernameLeft} from a client");

                    usernameLeft += clientLeftMessage;

                    // Add message to chat text box

                    Debug.Log("Client disconnected from the server.");

                    m_Connections[i] = default;
                    break;
                }
            }
        }
    }

    private void OnDestroy()
    {
        // Dispose of memory if needed
        if (m_Driver.IsCreated)
        {
            m_Driver.Dispose();
            m_Connections.Dispose();
        }
    }
}

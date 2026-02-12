using UnityEngine;
using Unity.Networking.Transport;

public class ClientBehavior : MonoBehaviour
{
    NetworkDriver m_Driver;
    NetworkConnection m_Connection;
    NetworkPipeline TCPPipeline;

    void Start()
    {
        // Create TCP driver and connect it to server address
        m_Driver = NetworkDriver.Create(new WebSocketNetworkInterface());

        // Define TCP pipeline before connections are made
        TCPPipeline = m_Driver.CreatePipeline(typeof(ReliableSequencedPipelineStage));

        var endpoint = NetworkEndpoint.LoopbackIpv4.WithPort(7777);
        m_Connection = m_Driver.Connect(endpoint);
    }

    void Update()
    {
        m_Driver.ScheduleUpdate().Complete();

        if (!m_Connection.IsCreated)
        {
            return;
        }

        //  Same functionality as the server but for one connection
        Unity.Collections.DataStreamReader stream;
        NetworkEvent.Type cmd;

        while ((cmd = m_Connection.PopEvent(m_Driver, out stream)) != NetworkEvent.Type.Empty)
        {
            // NetworkEvent.Type.Connect is called when the connect call has succeeded
            if (cmd == NetworkEvent.Type.Connect)
            {
                Debug.Log("We are now connected to the server.");

                // When a connection is established between the client and the server, the number is sent, BeginSend/EndSend pattern together with the DataStreamWriter
                uint value = 1;
                m_Driver.BeginSend(TCPPipeline, m_Connection, out var writer);
                writer.WriteUInt(value);
                m_Driver.EndSend(writer);
            }
            // When the NetworkEvent type is Data,read the value back from the server and then call the Disconnect method
            else if (cmd == NetworkEvent.Type.Data)
            {
                uint value = stream.ReadUInt();
                Debug.Log($"Got the value {value} back from the server.");

                m_Connection.Disconnect(m_Driver);
                m_Connection = default;
            }
            // Handle disconnects
            else if (cmd == NetworkEvent.Type.Disconnect)
            {
                Debug.Log("Client got disconnected from server.");
                m_Connection = default;
            }
        }
    }

    // Dispose driver
    private void OnDestroy()
    {
        m_Driver.Dispose();
    }
}

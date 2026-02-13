using Unity.Collections;
using Unity.Networking.Transport;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public class ClientBehavior : MonoBehaviour
{
    NetworkDriver m_Driver;
    NetworkConnection m_Connection;
    NetworkPipeline TCPPipeline;

    private string m_Username;

    public void setUsername(string username)
    {
        m_Username = username;
    }

    public string getUsername()
    {
        return m_Username;
    }

    private string m_Message;

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
                /*
                 * Here is where the client username needs to be sent to the server
                 */

                Debug.Log("Client is now connected to the server.");

                // When a connection is established between the client and the server, the client username is sent, BeginSend/EndSend pattern together with the DataStreamWriter
                m_Driver.BeginSend(TCPPipeline, m_Connection, out var writer);
                writer.WriteFixedString4096(m_Username);
                m_Driver.EndSend(writer);
            }
            // When the NetworkEvent type is Data, read the value back from the server and then call the Disconnect method
            else if (cmd == NetworkEvent.Type.Data)
            {
                FixedString4096Bytes message = stream.ReadFixedString4096();
                Debug.Log($"Got the value {message} back from the server.");
            }
            // Handle disconnects
            else if (cmd == NetworkEvent.Type.Disconnect)
            {
                /*
                 * Here is where the username left message needs to be sent to the server
                 */

                m_Driver.BeginSend(TCPPipeline, m_Connection, out var writer);
                writer.WriteFixedString4096(m_Username);
                m_Driver.EndSend(writer);

                DisconnectClient();
            }
        }
    }

    private void DisconnectClient()
    {
        m_Connection.Disconnect(m_Driver);
        Debug.Log("Client got disconnected from server.");
        m_Connection = default;
    }

    public void OnDisconnectButtonClick()
    {
        DisconnectClient();
    }

    // Dispose driver
    private void OnDestroy()
    {
        m_Driver.Dispose();
        Debug.Log("On destroy");
    }
}

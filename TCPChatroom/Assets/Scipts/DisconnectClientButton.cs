using UnityEditor.PackageManager;
using UnityEngine;

public class DisconnectClientButton : MonoBehaviour
{
    [HideInInspector]
    public GameObject ChatBox;
    [HideInInspector]
    public ClientScript client;

    public void OnSelfClick()
    {
        if (client != null)
        {
            client.DisconnectClient();
            Destroy(client);
        }

        if (ChatBox != null)
        {
            Destroy(ChatBox);
        }

        Destroy(gameObject);
    }

    //private void Update()
    //{
    //    if (Input.GetKeyDown(KeyCode.J))
    //    {
    //        Destroy(ChatBox);
    //        Destroy(gameObject);
    //    }
    //}
}

using UnityEngine;

public class SendMessageButton : MonoBehaviour
{
    [HideInInspector]
    public GameObject Client;
    private ClientScript clientScript;

    void Start()
    {
        clientScript = GetComponent<ClientScript>();
    }
}

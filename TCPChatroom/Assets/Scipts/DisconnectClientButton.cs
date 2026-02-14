using UnityEditor.PackageManager;
using UnityEngine;

public class DisconnectClientButton : MonoBehaviour
{
    [HideInInspector]
    public GameObject ChatBox;

    public void OnSelfClick()
    {
        Destroy(ChatBox);
        Destroy(gameObject);
    }
}

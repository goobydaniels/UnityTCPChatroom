using UnityEditor.PackageManager;
using UnityEngine;

public class DisconnectClientButton : MonoBehaviour
{
    public void OnSelfClick()
    {
        Destroy(gameObject);
    }
}

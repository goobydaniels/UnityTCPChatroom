using TMPro;
using UnityEngine;

public class PlaceholderTextHolder : MonoBehaviour
{
    public GameObject PlaceHolderObject;

    [HideInInspector]
    public TextMeshProUGUI placeHolderText;

    void Start()
    {
        placeHolderText = PlaceHolderObject.GetComponent<TextMeshProUGUI>();
    }
}

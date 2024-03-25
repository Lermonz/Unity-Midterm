using UnityEngine;
using TMPro;

public class DeathCount : MonoBehaviour
{
    int _deaths = 0;
    public int Deaths {
        get => _deaths;
        set {
            _deaths = value;
            TextBox2.text = Deaths.ToString()+" Deaths";
        }
    }
    public TextMeshProUGUI TextBox2;
}
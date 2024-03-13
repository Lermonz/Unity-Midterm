using UnityEngine;
using TMPro;

public class CheesesGot : MonoBehaviour
{
    int _score = 0;
    public int Score {
        get => _score;
        set {
            _score = value;
            TextBox.text = Score.ToString()+" Cheeses";
        }
    }
    public TextMeshProUGUI TextBox;
}

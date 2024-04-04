using UnityEngine;
using TMPro;

public class FinalScore : MonoBehaviour
{
    int _score = 0;
    public int TheScore {
        get => _score;
        set {
            _score = value;
            TextBox.text = "Congratulations!\nFinal Score: "+TheScore.ToString();
        }
    }
    public TextMeshProUGUI TextBox;
    void Update()
    {
        if(GameBehaviour.Instance.State == GameBehaviour.GameState.Finale) {
            TextBox.enabled = true;
        }
        else {
            TextBox.enabled = false;
        }
    }
}

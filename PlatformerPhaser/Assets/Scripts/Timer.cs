using UnityEngine;
using TMPro;

public class Timer : MonoBehaviour
{
    float _totalFrames = 0;
    public int ms = 0;
    public int s = 0;
    public int m = 0;
    string _timeDisplay;
    public TMP_Text _timer;
    void Update()
    {
        _totalFrames+=Time.deltaTime;
        m = Mathf.FloorToInt(_totalFrames/60);
        s = Mathf.FloorToInt(_totalFrames%60);
        ms = Mathf.FloorToInt(_totalFrames%1*1000);
        _timer.text = string.Format("{0:00}:{1:00}:{2:000}", m, s, ms);
    }
}

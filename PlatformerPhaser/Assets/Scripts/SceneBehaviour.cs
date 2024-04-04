using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class SceneBehaviour : MonoBehaviour
{
    GameObject _titleObject;
    GameObject _subtitleObject;
    GameObject _otherTextObject;
    TextMeshProUGUI _titleText;
    TextMeshProUGUI _subtitleText;
    TextMeshProUGUI _otherText;
    bool _moveCanvas;
    float alpha = 1;
    public static bool _isTitle = true;
    void Start() {
        _titleObject = GameObject.Find("Title");
        _subtitleObject = GameObject.Find("Subtitle");
        _otherTextObject = GameObject.Find("PressStartToPlay");
        _titleText = _titleObject.GetComponent<TextMeshProUGUI>();
        _subtitleText = _subtitleObject.GetComponent<TextMeshProUGUI>();
        _otherText = _otherTextObject.GetComponent<TextMeshProUGUI>();
    }
    void Update()
    {
        if(Input.anyKeyDown){
            _isTitle = false;
            StartCoroutine(ToGameplay());
        }
        if(_moveCanvas) {
            _titleObject.transform.position -= Vector3.up*0.025f;
            _subtitleObject.transform.position -= Vector3.up*0.025f;
            _otherTextObject.transform.position -= Vector3.up*0.02f;
            _titleObject.transform.localEulerAngles += Vector3.up*0.1f;
            _subtitleObject.transform.localEulerAngles += Vector3.up*0.1f;
            _otherTextObject.transform.localEulerAngles += Vector3.up*0.1f;
            alpha-=0.005f;
            _titleText.color = new Color(1f,1f,1f, alpha);
            _subtitleText.color = new Color(1f,1f,1f, alpha);
            _otherText.color = new Color(1f,1f,1f, alpha);
        }
    }
    IEnumerator ToGameplay(){
        _moveCanvas = true;
        yield return new WaitForSeconds(1f);
        SceneManager.LoadScene("Gameplay");
    }
}

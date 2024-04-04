using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneBehaviour : MonoBehaviour
{
    GameObject _canvas;
    bool _moveCanvas;
    void Start() {
        _canvas = GameObject.Find("Canvas");
    }
    void Update()
    {
        if(Input.anyKeyDown){
            StartCoroutine(ToGameplay());
        }
        if(_moveCanvas) {
            _canvas.transform.position -= Vector3.up;
        }
    }
    IEnumerator ToGameplay(){
        TextFlicker._flickerInterval -= 0.5f;
        yield return new WaitForSeconds(0.8f);
        _moveCanvas = true;
        yield return new WaitForSeconds(2.5f);
        SceneManager.LoadScene("Gameplay");
    }
}

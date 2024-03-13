using UnityEngine;
using TMPro;
using System.Collections;

public class TextFlicker : MonoBehaviour
{
    TextMeshProUGUI _textBox;
    [Range(0.5f,2.0f)] //property attribute
    [SerializeField] float _flickerInterval;
    bool _onCoroutine;
    void Start()
    {
        _textBox = GetComponent<TextMeshProUGUI>();
    }
    // Update is called once per frame
    void Update()
    {
        if(!_onCoroutine) {
            StartCoroutine(Flicker());
        }
    }
    IEnumerator Flicker() {
        _onCoroutine = true;
        _textBox.enabled = !_textBox.enabled;
        yield return new WaitForSeconds(_flickerInterval);
        _onCoroutine = false;
    }
}

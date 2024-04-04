using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicChange : MonoBehaviour
{
    [SerializeField] AudioClip _titleMusic;
    [SerializeField] AudioClip _gameMusic;
    AudioSource _audioSource;
    DynamicVolume _joinLater;
    bool _didCoroutine;
    // Start is called before the first frame update
    void Start()
    {
        _audioSource = GetComponent<AudioSource>();   
    }

    void Update()
    {
        if(SceneBehaviour._isTitle) {
            _audioSource.clip = _titleMusic;
        }
        else if(_joinLater == null){
            _audioSource.volume -= Time.deltaTime*0.65f;
            Debug.Log(_audioSource.volume);
        }
    }
}

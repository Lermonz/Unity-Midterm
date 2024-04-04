using UnityEngine;

public class DynamicVolume : MonoBehaviour
{
    AudioSource _audioSource;
    void Awake() {
        _audioSource = GetComponent<AudioSource>();
    }
    void Update()
    {
        if(GameBehaviour.Instance.State == GameBehaviour.GameState.Pause) {
            _audioSource.volume = 0.6f;
        }
        else {
            _audioSource.volume = 1f;
        }
    }
}

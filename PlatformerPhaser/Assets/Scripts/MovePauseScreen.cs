using UnityEngine;

public class MovePauseScreen : MonoBehaviour
{
    Camera _camera;
    void Start()
    {
        _camera = GameObject.Find("Main Camera").GetComponent<Camera>();
    }
    void Update()
    {
        if(GameBehaviour.Instance.State == GameBehaviour.GameState.Pause) {
            transform.position = new Vector3(0,_camera.transform.position.y,96);
        }
        else {
            transform.position = new Vector3(0,-50,96);
        }
    }
}
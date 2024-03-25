using UnityEngine;

public class Rotate : MonoBehaviour
{
    [Range(1,360)][SerializeField] float speed;
    [SerializeField] bool clockwise;
    void Update()
    {
        int dir = clockwise? -1 : 1;
        transform.Rotate(0,0,dir*speed*Time.deltaTime);
    }
}

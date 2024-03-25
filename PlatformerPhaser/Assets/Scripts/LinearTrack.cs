using UnityEngine;

public class LinearTrack : MonoBehaviour
{
    Vector3 _startPos;
    Vector3 _endPos;
    [SerializeField] bool _isVertical;
    [SerializeField] [Range(-20,20)] float _distance;
    [SerializeField] [Range(1,10)] float _speed;
    float _realSpeed;
    float elapsedFrames;
    float elapsedFramesReverse;
    bool directionNormal = true;
    void Start()
    {
        _startPos = new Vector3(transform.position.x, transform.position.y,0);
        _endPos = !_isVertical ? _startPos + Vector3.right*_distance : _startPos + Vector3.up*_distance;
        _realSpeed = 1000 / _speed;
    }
    void Update()
    {
        if(directionNormal){
            if(elapsedFrames < _realSpeed) {
                elapsedFrames+=Time.deltaTime*100;
                float interpRatio = elapsedFrames/_realSpeed;
                transform.position = Vector3.Lerp(_startPos,_endPos,interpRatio);
            }
            else {
                directionNormal = false;
                elapsedFramesReverse = (int)_realSpeed;
            }
        }
        else{
            if(elapsedFramesReverse > 0) {
                elapsedFramesReverse-=Time.deltaTime*100;
                float interpRatio = elapsedFramesReverse/_realSpeed;
                transform.position = Vector3.Lerp(_startPos,_endPos,interpRatio);
            }
            else {
                directionNormal = true;
                elapsedFrames = 0;
            }
        }
    }
    void OnCollisionEnter2D(Collision2D other) {
        other.transform.SetParent(transform);
    }
    void OnCollisionExit2D(Collision2D other) {
        other.transform.SetParent(null);
    }
}

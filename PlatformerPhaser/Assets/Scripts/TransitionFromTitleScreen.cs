using System.Collections;
using UnityEngine;

public class TransitionFromTitleScreen : MonoBehaviour
{
    int interpolationFrameCount = 60;
    float elapsedFrames;
    Vector3 newCameraPos = new Vector3(0,0,-10);
    void Update()
    {  
        if(elapsedFrames < interpolationFrameCount) {
            elapsedFrames+=(Time.deltaTime*4f);
            float interpolateRatio = (float)elapsedFrames/interpolationFrameCount;
            Vector3 interpolatedPos = Vector3.Lerp(transform.position, newCameraPos, interpolateRatio);
            transform.position = interpolatedPos;
            //FIX IT
        }
        StartCoroutine(MoveUp());
        
    }
    IEnumerator MoveUp() {
        yield return new WaitForSeconds(6f);
        this.enabled = false;
    }
}

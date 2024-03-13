using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeScreen : MonoBehaviour
{
    int interpolationFrameCount = 60;
    int elapsedFrames;
    Vector3 newCameraPos;
    bool hasRun = false;
    void Update()
    {
        if(GameBehaviour.Instance.State == GameBehaviour.GameState.ScreenMoveUp) {
            if(!hasRun) {
                newCameraPos = new Vector3(transform.position.x, transform.position.y + 20f, transform.position.z);
                hasRun = true;
            }
            if(elapsedFrames < interpolationFrameCount) {
                elapsedFrames++;
                float interpolateRatio = (float)elapsedFrames/interpolationFrameCount;
                Vector3 interpolatedPos = Vector3.Lerp(transform.position, newCameraPos, interpolateRatio);
                transform.position = interpolatedPos;
                //FIX IT
            }
            StartCoroutine(MoveUp());
        }
        else {
            elapsedFrames = 0;
            hasRun = false;
        }
    }
    IEnumerator MoveUp() {
        yield return new WaitForSeconds(0.3f);
        PlayerMovement._isLevelTransition = false;
    }
}

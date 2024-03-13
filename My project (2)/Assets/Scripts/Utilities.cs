using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Utilities : MonoBehaviour
{
    public static float SetYLimit(Camera cam, SpriteRenderer renderer) {
        Vector3 screenTop = new Vector3(0, cam.pixelHeight,0);
        float heightInUnits = cam.ScreenToWorldPoint(screenTop).y;

        float spriteHeight = renderer.size.y;
        return heightInUnits - spriteHeight * 0.5f;
    }

    public static float SetXLimit(Camera cam, SpriteRenderer renderer) {
        Vector3 screenSide = new Vector3(cam.pixelWidth,0,0);
        float widthInUnits = cam.ScreenToWorldPoint(screenSide).x;

        float spriteWidth = renderer.size.x;
        return widthInUnits - spriteWidth * 0.5f;
    }
}

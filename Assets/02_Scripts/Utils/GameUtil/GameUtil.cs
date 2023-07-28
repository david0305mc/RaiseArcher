using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameUtil 
{
    public static Vector2 WorldPositionToScreenSpaceCameraPosition(Camera worldCamera, Canvas canvas, Vector3 position)
    {
        Vector2 viewport = worldCamera.WorldToViewportPoint(position);
        Ray canvasRay = canvas.worldCamera.ViewportPointToRay(viewport);
        return canvasRay.GetPoint(canvas.planeDistance);
    }
    public static Vector2 GetScreenPosition(Camera camera, Vector3 position)
    {
        Vector3 screenPos = camera.WorldToScreenPoint(position);
        return screenPos;
    }

    public static float ClockwiseAngleOf3Points(Vector2 A, Vector2 B, Vector2 C)
    {
        Vector2 v1 = A - B;
        Vector2 v2 = C - B;

        var sign = Mathf.Sign(v1.x * v2.y - v1.y * v2.x) * -1;
        float angle = Vector2.Angle(v1, v2) * sign;

        if (angle < 0)
        {
            angle = 360 + angle;
        }

        return angle;
    }
}


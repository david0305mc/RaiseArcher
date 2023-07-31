using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameUtil 
{
    public enum FacingDirection
    {
        UP = 270,
        DOWN = 90,
        LEFT = 180,
        RIGHT = 0
    }
    public static Quaternion LookAt2D(Vector2 startingPosition, Vector2 targetPosition, FacingDirection facing)
    {
        Vector2 direction = targetPosition - startingPosition;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        angle -= (float)facing;
        return Quaternion.AngleAxis(angle, Vector3.forward);
    }


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


public class WeaponGo : MonoBehaviour
{
    public SpriteRenderer WeaponSpriteRenderer;
    public void LateUpdate()
    {

        RotateByCursorPosition();
    }
    private void RotateByCursorPosition()
    {
        var cursorPositon = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        this.transform.rotation = GameUtil.LookAt2D(this.transform.position, cursorPositon, GameUtil.FacingDirection.RIGHT);
    }
}
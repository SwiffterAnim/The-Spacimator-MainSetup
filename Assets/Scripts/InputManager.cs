using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    private Vector2 mousePosition;
    private RaycastHit2D[] mouseHitAllArray;

    public Vector2 GetWorldMouseLocation2D()
    {
        mousePosition = Mouse.current.position.ReadValue();
        Vector3 mouseWorldPosition = Camera.main.ScreenToWorldPoint(mousePosition);
        mousePosition = mouseWorldPosition;
        return mousePosition;
    }

    public RaycastHit2D[] DetectALL_MouseRayCastHit2D()
    {
        mouseHitAllArray = Physics2D.RaycastAll(GetWorldMouseLocation2D(), Vector2.zero);
        return mouseHitAllArray;
    }

    public GameObject GetHoveredObject()
    {
        if (mouseHitAllArray.Length > 0)
        {
            return mouseHitAllArray[^1].transform.gameObject;
        }
        else
        {
            return null;
        }
    }
}

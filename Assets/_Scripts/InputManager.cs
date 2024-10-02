using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    private Vector2 mousePosition;
    private RaycastHit2D[] mouseHitAllArray;

    //Not sure if this isn't returning the same as Input.mousePosition lol
    public Vector2 GetWorldMouseLocation2D()
    {
        mousePosition = Mouse.current.position.ReadValue();
        Vector3 mouseWorldPosition = Camera.main.ScreenToWorldPoint(mousePosition);
        mousePosition = mouseWorldPosition;
        return mousePosition;
    }

    public RaycastHit2D[] DetectALL_MouseRayCastHit2D()
    {
        mouseHitAllArray = Physics2D.RaycastAll(GetWorldMouseLocation2D(), Vector3.forward);
        return mouseHitAllArray;
    }

    public RaycastHit[] DetectALL_MouseRaycastHit3D()
    {
        Vector3 mouseHit = GetWorldMouseLocation2D();

        RaycastHit[] mouseHitAllArray3D = Physics.RaycastAll(mouseHit, Vector3.forward);
        RaycastHit[] negativeMouseHitAllArray3D = Physics.RaycastAll(mouseHit, -Vector3.forward);
        if (mouseHitAllArray3D.Length != 0)
        {
            return mouseHitAllArray3D;
        }
        else if (negativeMouseHitAllArray3D.Length != 0)
        {
            return negativeMouseHitAllArray3D;
        }
        else
        {
            return null;
        }
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

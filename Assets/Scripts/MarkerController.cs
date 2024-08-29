using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MarkerController : MonoBehaviour
{
    [SerializeField]
    SpriteRenderer spriteRenderer;

    [SerializeField]
    MarkerEntity markerEntity;

    public void VisualOnHoveredMarker()
    {
        spriteRenderer.color = markerEntity.selectedColor;
        transform.localScale = markerEntity.selectedScale;
    }

    public void VisualOffHoveredMarker()
    {
        spriteRenderer.color = markerEntity.defaultColor;
        transform.localScale = markerEntity.defaultScale;
    }

    public void MoveMarkerWithMouse(Vector2 mouseWorldPosition2D)
    {
        //Used by Curve Manager. If selected, move this Marker.
        transform.position = mouseWorldPosition2D;
    }
}

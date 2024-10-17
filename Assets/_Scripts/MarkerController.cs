using TMPro;
using UnityEngine;

public class MarkerController : MonoBehaviour
{
    [SerializeField]
    SpriteRenderer spriteRenderer;

    [SerializeField]
    MarkerEntity markerEntity;

    [SerializeField]
    TextMeshProUGUI textMeshProUGUI;

    [SerializeField]
    Canvas canvas;

    private Vector2 offsetMouse_This;
    private bool leftMouseButtonIsPressed = false;

    //I need to subscribe this MoveMarker_performed to calcuate the offset between marker and mouse IF markerEntity.isSelected
    // Upon performed, you calculate the offset (only once).

    private void Awake()
    {
        canvas.worldCamera = Camera.main;
    }

    private void Update()
    {
        if (leftMouseButtonIsPressed && markerEntity.isSelected)
        {
            Vector2 mouseWorldPosition2D = (Vector2)
                Camera.main.ScreenToWorldPoint(Input.mousePosition);
            MoveMarkerWithMouse(mouseWorldPosition2D);
        }
        if (!markerEntity.isGhost)
        {
            canvas.enabled = true;
            textMeshProUGUI.text = "" + markerEntity.frameNumber;
        }
        else
        {
            canvas.enabled = false;
        }
        if (markerEntity.isHovered)
        {
            VisualOnHoveredMarker();
        }
        if (markerEntity.isSelected && !markerEntity.isHovered)
        {
            VisualOnSelectedMarker();
        }
        if (!markerEntity.isSelected && !markerEntity.isHovered)
        {
            VisualDefaultMarker();
        }
        if (!markerEntity.isSelected && !markerEntity.isHovered && markerEntity.isGhost)
        {
            VisualOnGhostMarker();
        }
        if (markerEntity.isPlaying)
        {
            VisualOnPlayingMarker();
        }
    }

    public void VisualOnPlayingMarker()
    {
        spriteRenderer.color = markerEntity.playingColor;
        transform.localScale = markerEntity.defaultScale;
    }

    public void VisualOnHoveredMarker()
    {
        spriteRenderer.color = markerEntity.hoveredColor;
        transform.localScale = markerEntity.hoveredScale;
    }

    public void VisualDefaultMarker()
    {
        spriteRenderer.color = markerEntity.defaultColor;
        transform.localScale = markerEntity.defaultScale;
    }

    public void VisualOnSelectedMarker()
    {
        spriteRenderer.color = markerEntity.selectedColor;
        transform.localScale = markerEntity.defaultScale;
    }

    public void VisualOnGhostMarker()
    {
        spriteRenderer.color = markerEntity.ghostColor;
        transform.localScale = markerEntity.defaultScale;
    }

    private void MoveMarkerWithMouse(Vector2 mouseWorldPosition2D)
    {
        //Used by Curve Manager. If selected, move this Marker.
        transform.position = mouseWorldPosition2D + offsetMouse_This;
    }

    public void MoveMarker_performed()
    {
        leftMouseButtonIsPressed = true;
        Vector2 mouseWorldPosition2D = (Vector2)Camera.main.ScreenToWorldPoint(Input.mousePosition);
        offsetMouse_This = (Vector2)transform.position - mouseWorldPosition2D; //I feel itchy about adding the inputSystem into this script.
    }

    public void MoveMarker_canceled()
    {
        leftMouseButtonIsPressed = false;
    }
}

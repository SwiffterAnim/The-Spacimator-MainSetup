using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

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
    private UserInputActions userInputActions;

    //I need to subscribe this MoveMarker_performed to calcuate the offset between marker and mouse IF markerEntity.isSelected
    // Upon performed, you calculate the offset (only once).

    private void OnEnable()
    {
        userInputActions = GameManager.Instance.GetInputAction();
        userInputActions.EditingCurve.MoveMarker.performed += MoveMarker_performed;
    }

    private void OnDisable()
    {
        userInputActions.EditingCurve.MoveMarker.performed -= MoveMarker_performed;
    }

    private void Awake()
    {
        canvas.worldCamera = Camera.main;
    }

    private void Update()
    {
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

    public void MoveMarkerWithMouse(Vector2 mouseWorldPosition2D)
    {
        //Used by Curve Manager. If selected, move this Marker.
        transform.position = mouseWorldPosition2D + offsetMouse_This;
    }

    private void MoveMarker_performed(InputAction.CallbackContext context)
    {
        //--------------------TODO - PAY ATTENTION TO THIS LATER--------------------
        // I did this this way because I don't know how to reference the instance of InputManager on a instance of a prefab.
        // Actually maybe this wasn't the issue before... perhaps I can just use the InputManager here and grab it from the prefab.
        Vector2 mouseWorldPosition2D = (Vector2)Camera.main.ScreenToWorldPoint(Input.mousePosition);
        offsetMouse_This = (Vector2)transform.position - mouseWorldPosition2D; //I feel itchy about adding the inputSystem into this script.
    }
}

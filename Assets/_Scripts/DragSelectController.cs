using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class DragSelectController : MonoBehaviour
{
    [SerializeField]
    private InputManager inputManager;

    [SerializeField]
    private CurveController curveController;

    [SerializeField]
    private MarkerSelection markerSelection;

    [SerializeField]
    private RectTransform selectionBox;

    [SerializeField]
    private float magnitude = 30;

    private bool isDragSelect = false;
    private bool mouseLeftButtonPressed = false;
    private Vector3 mousePositionInitial;
    private Vector3 mousePositionEnd;

    private RaycastHit2D[] mouseRayCastHitArray;
    private UserInputActions userInputActions;

    private void Start()
    {
        userInputActions = GameManager.Instance.GetInputAction();
        userInputActions.EditingCurve.MoveMarker.performed += MoveMarker_performed;
        userInputActions.EditingCurve.MoveMarker.canceled += MoveMarker_canceled;

        mouseRayCastHitArray = markerSelection.mouseRayCastHitArray;
    }

    private void OnDisable()
    {
        userInputActions.EditingCurve.MoveMarker.performed -= MoveMarker_performed;
        userInputActions.EditingCurve.MoveMarker.canceled -= MoveMarker_canceled;
    }

    void Update()
    {
        if (mouseRayCastHitArray.Length == 0 && mouseLeftButtonPressed)
        {
            if (!isDragSelect && (mousePositionInitial - Input.mousePosition).magnitude > magnitude)
            {
                isDragSelect = true;
            }

            if (isDragSelect)
            {
                mousePositionEnd = Input.mousePosition;
                ActivateSelectionBox();
            }
        }
    }

    private void SelectObjects()
    {
        Vector2 minValue = selectionBox.anchoredPosition - (selectionBox.sizeDelta / 2);
        Vector2 maxValue = selectionBox.anchoredPosition + (selectionBox.sizeDelta / 2);
        List<GameObject> selectedMarkers = new List<GameObject>();

        GameObject[] selectableMarkers = GameObject.FindGameObjectsWithTag("Marker");
        foreach (GameObject marker in selectableMarkers)
        {
            Vector3 markerScreenPosition = Camera.main.WorldToScreenPoint(
                marker.transform.position
            );
            if (
                markerScreenPosition.x > minValue.x
                && markerScreenPosition.x < maxValue.x
                && markerScreenPosition.y > minValue.y
                && markerScreenPosition.y < maxValue.y
            )
            {
                selectedMarkers.Add(marker);
            }
        }
        foreach (GameObject selectedMarker in selectedMarkers)
        {
            if (selectedMarker.TryGetComponent(out MarkerEntity markerEntity))
            {
                curveController.SelectMarker(markerEntity, selectedMarker);
            }
        }
        selectedMarkers.Clear();
    }

    private void ActivateSelectionBox()
    {
        selectionBox.gameObject.SetActive(isDragSelect);

        float width = mousePositionEnd.x - mousePositionInitial.x;
        float height = mousePositionEnd.y - mousePositionInitial.y;

        selectionBox.sizeDelta = new Vector2(Mathf.Abs(width), Mathf.Abs(height));

        selectionBox.anchoredPosition =
            new Vector2(mousePositionInitial.x, mousePositionInitial.y)
            + new Vector2(width / 2, height / 2);
    }

    private void DeactivateSelectionBox()
    {
        selectionBox.gameObject.SetActive(isDragSelect);
        selectionBox.sizeDelta = new Vector2(0.01f, 0.01f);
        selectionBox.position = new Vector3(-1000, -1000, 0);
    }

    private void MoveMarker_performed(InputAction.CallbackContext context)
    {
        mouseLeftButtonPressed = true;
        mousePositionInitial = Input.mousePosition;
        mouseRayCastHitArray = markerSelection.mouseRayCastHitArray; //Not sure this is going to work.
    }

    private void MoveMarker_canceled(InputAction.CallbackContext context)
    {
        mouseLeftButtonPressed = false;
        isDragSelect = false;

        SelectObjects();
        DeactivateSelectionBox();
    }
}

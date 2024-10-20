using System.Collections.Generic;
using UnityEngine;

public class DragSelectController : MonoBehaviour
{
    [SerializeField]
    private RectTransform selectionBox;

    [SerializeField]
    private float magnitude = 30;

    private bool isDragSelect = false;
    private bool mouseLeftButtonPressed = false;
    private Vector3 mousePositionInitial;
    private Vector3 mousePositionEnd;

    private RaycastHit2D[] mouseRayCastHitArray;

    private void Start()
    {
        GameEventSystem.Instance.RegisterListener<onLeftClickPerformed>(MoveSelect_performed);
        GameEventSystem.Instance.RegisterListener<onLeftClickCanceled>(MoveSelect_canceled);
    }

    private void OnDestroy()
    {
        GameEventSystem.Instance.UnregisterListener<onLeftClickPerformed>(MoveSelect_performed);
        GameEventSystem.Instance.UnregisterListener<onLeftClickCanceled>(MoveSelect_canceled);
    }

    void Update()
    {
        if (mouseRayCastHitArray != null)
        {
            if (mouseRayCastHitArray.Length == 0 && mouseLeftButtonPressed)
            {
                if (
                    !isDragSelect
                    && (mousePositionInitial - Input.mousePosition).magnitude > magnitude
                )
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
        if (selectedMarkers.Count > 0)
        {
            GameEventSystem.Instance.Raise<SelectionBoxEvent>(
                new SelectionBoxEvent(selectedMarkers)
            );
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

    private void MoveSelect_performed(onLeftClickPerformed onLeftClickPerformed)
    {
        mouseLeftButtonPressed = true;
        mousePositionInitial = Input.mousePosition;
        mouseRayCastHitArray = onLeftClickPerformed.mouseRayCastHit;
    }

    private void MoveSelect_canceled(onLeftClickCanceled onLeftClickCanceled)
    {
        mouseLeftButtonPressed = false;
        isDragSelect = false;

        SelectObjects();
        DeactivateSelectionBox();
    }
}

//====================    EVENTS    ====================

public struct SelectionBoxEvent
{
    public List<GameObject> selectedMarkers { get; private set; }

    public SelectionBoxEvent(List<GameObject> selectedMarkers)
    {
        this.selectedMarkers = selectedMarkers;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MarkerSelection : MonoBehaviour
{
    [SerializeField]
    InputManager inputManager;

    [SerializeField]
    CurveController curveController;

    private RaycastHit2D[] mouseRayCastHitArray = new RaycastHit2D[0];
    private List<GameObject> markersMouseRayCastHits = new List<GameObject>();
    private GameObject hoveredMarker;
    private bool leftMouseButtonIsPressed = true;
    private GameObject selectedObject;
    private GameObject selectedMarker;

    private List<GameObject> selectedMarkerList = new List<GameObject>();

    private void Start()
    {
        //=======   Registering Events.   =======
        GameEventSystem.Instance.RegisterListener<onLeftClickPerformed>(MoveSelect_performed);
        GameEventSystem.Instance.RegisterListener<onLeftClickCanceled>(MoveSelect_canceled);
        GameEventSystem.Instance.RegisterListener<onDeletePerformed>(Delete_performed);
        GameEventSystem.Instance.RegisterListener<SelectionBoxEvent>(SelectMultipleMarkers);
    }

    private void OnDestroy()
    {
        //=======   Unregistering Events.   =======
        GameEventSystem.Instance.UnregisterListener<onLeftClickPerformed>(MoveSelect_performed);
        GameEventSystem.Instance.UnregisterListener<onLeftClickCanceled>(MoveSelect_canceled);
        GameEventSystem.Instance.UnregisterListener<onDeletePerformed>(Delete_performed);
        GameEventSystem.Instance.UnregisterListener<SelectionBoxEvent>(SelectMultipleMarkers);
    }

    private void Update()
    {
        //Gets the array of all RaycastHit2D the mouse hits.
        mouseRayCastHitArray = inputManager.DetectALL_MouseRayCastHit2D();

        if (mouseRayCastHitArray.Length > 0)
        {
            CheckForMarkerOnMouseRaycastHit(mouseRayCastHitArray);
            UpdateMarkerHovered(markersMouseRayCastHits);
        }
        else if (hoveredMarker != null)
        {
            hoveredMarker.GetComponent<MarkerEntity>().isHovered = false;
        }
        //This is basically clearing in every frame. That's no good..
        markersMouseRayCastHits.Clear();
    }

    private void UpdateMarkerHovered(List<GameObject> markersList)
    {
        //Gets the top marker, important if there's more than one.
        hoveredMarker = markersList[^1];

        //Turning all the others off.
        for (int i = 0; i < curveController.markerList.Count; i++)
        {
            if (curveController.markerList[i].TryGetComponent(out MarkerEntity iMarkerEntity))
            {
                if (
                    iMarkerEntity.frameNumber
                    == hoveredMarker.GetComponent<MarkerEntity>().frameNumber
                )
                {
                    iMarkerEntity.isHovered = true;
                }
                else
                {
                    iMarkerEntity.isHovered = false;
                }
            }
        }
    }

    private void CheckForMarkerOnMouseRaycastHit(RaycastHit2D[] RayCastHit)
    {
        for (int i = 0; i < RayCastHit.Length; i++)
        {
            if (RayCastHit[i].transform.gameObject.CompareTag("Marker"))
            {
                markersMouseRayCastHits.Add(RayCastHit[i].transform.gameObject);
            }
        }
    }

    private List<int> GetSelectedMarkersIndices(List<GameObject> selectedMarkers)
    {
        List<int> selectedMarkersIndices = new List<int>();
        foreach (GameObject marker in selectedMarkers)
        {
            if (marker.TryGetComponent(out MarkerEntity markerEntity))
            {
                selectedMarkersIndices.Add(markerEntity.frameNumber - 1);
            }
        }
        return selectedMarkersIndices;
    }

    private void Delete_performed(onDeletePerformed onDeletePerformed)
    {
        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) //Check for SHIFT for GHOSTS.
        {
            GameEventSystem.Instance.Raise<ShiftDeleteSelectedEvent>(
                new ShiftDeleteSelectedEvent(GetSelectedMarkersIndices(selectedMarkerList))
            );
            selectedMarkerList.Clear();
        }
        else
        {
            GameEventSystem.Instance.Raise<DeleteSelectedEvent>(
                new DeleteSelectedEvent(GetSelectedMarkersIndices(selectedMarkerList))
            );
            selectedMarkerList.Clear();
        }
    }

    private void MoveSelect_performed(onLeftClickPerformed onLeftClickPerformed)
    {
        leftMouseButtonIsPressed = true;
        RaycastHit2D[] mouseRayCastHit = onLeftClickPerformed.mouseRayCastHit;

        if (mouseRayCastHit.Length > 0)
        {
            selectedObject = onLeftClickPerformed.hoveredObject;
            if (selectedObject.TryGetComponent(out MarkerEntity markerEntity)) //Checks if it's a marker.
            {
                selectedMarker = selectedObject;

                if (selectedMarkerList.Count == 0) //If the list is empty, select this marker.
                {
                    SelectMarker(markerEntity, selectedMarker);
                }
                else
                {
                    if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) //If it isn't empty check if shift is clicked.
                    {
                        if (selectedMarkerList.Contains(selectedMarker)) //If marker already in list, remove it.
                        {
                            DeselectMarker(markerEntity, selectedMarker);
                        }
                        else //otherwise, add it.
                        {
                            SelectMarker(markerEntity, selectedMarker);
                        }
                    }
                    else //if list is not empty and shift is NOT clicked
                    {
                        if (selectedMarkerList.Contains(selectedMarker)) //If this marker is selected.
                        {
                            //Do nothing. Basically will move all of them together.
                        }
                        else //If it isn't, deselect all, and select this one.
                        {
                            DeselectAllMarkers();
                            SelectMarker(markerEntity, selectedMarker);
                        }
                    }
                }
            }
        }
        else
        {
            DeselectAllMarkers();
        }
    }

    private void SelectMultipleMarkers(SelectionBoxEvent selectionBoxEvent)
    {
        foreach (GameObject selectedObject in selectionBoxEvent.selectedMarkers)
        {
            if (selectedObject.TryGetComponent(out MarkerEntity markerEntity))
            {
                SelectMarker(markerEntity, selectedObject);
            }
        }
    }

    private void SelectMarker(MarkerEntity markerEntity, GameObject selectedMarker)
    {
        int index = markerEntity.frameNumber - 1;
        markerEntity.isSelected = true;
        selectedMarkerList.Add(selectedMarker);

        GameEventSystem.Instance.Raise<MoveSelectedEvent>(
            new MoveSelectedEvent(selectedMarkerList)
        );
    }

    private void DeselectMarker(MarkerEntity markerEntity, GameObject selectedMarker)
    {
        markerEntity.isSelected = false;
        selectedMarkerList.Remove(selectedMarker);

        GameEventSystem.Instance.Raise<MoveSelectedEvent>(
            new MoveSelectedEvent(selectedMarkerList)
        );
    }

    private void DeselectAllMarkers()
    {
        for (int i = selectedMarkerList.Count - 1; i >= 0; i--)
        {
            selectedMarkerList[i].GetComponent<MarkerEntity>().isSelected = false;
            selectedMarkerList.RemoveAt(i);
        }
    }

    private void MoveSelect_canceled(onLeftClickCanceled onLeftClickCanceled)
    {
        leftMouseButtonIsPressed = false;

        selectedMarker = null;
        selectedObject = null;
    }
}

//====================    EVENTS    ====================

public struct DeleteSelectedEvent
{
    public List<int> selectedMarkersIndices { get; private set; }

    public DeleteSelectedEvent(List<int> selectedMarkersIndices)
    {
        this.selectedMarkersIndices = selectedMarkersIndices;
    }
}

public struct ShiftDeleteSelectedEvent
{
    public List<int> selectedMarkersIndices { get; private set; }

    public ShiftDeleteSelectedEvent(List<int> selectedMarkersIndices)
    {
        this.selectedMarkersIndices = selectedMarkersIndices;
    }
}

public struct MoveSelectedEvent
{
    public List<GameObject> selectedMarkers { get; private set; }

    public MoveSelectedEvent(List<GameObject> selectedMarkers)
    {
        this.selectedMarkers = selectedMarkers;
    }
}

public struct MoveCanceledEvent { }

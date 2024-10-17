using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;

public class CurveController : MonoBehaviour
{
    [SerializeField] private GameObject marker;

    [SerializeField] private InputManager inputManager;

    [SerializeField] private MarkerSelection markerSelection;

    [SerializeField] private GhostKnotController ghostKnotController;

    [SerializeField] private GameObject UI_FrameInputWindow;

    private bool leftMouseButtonIsPressed = false;
    private GameObject selectedMarker;
    private GameObject selectedObject;
    public List<GameObject> markerList = new List<GameObject>();
    public List<int> ghostIndices = new List<int>();

    private void Update()
    {
        if (markerSelection.selectedMarkerList != null && leftMouseButtonIsPressed)
        {
            var result = GameEventSystem.Intance.Raise<UpdateSplineEvent, bool>(new UpdateSplineEvent(markerList));
            UpdateGhostMarkers();
            //Update rotation of markers.
            UpdateALLMarkersRotation();
        }
    }

    public void AddMarker()
    {
        Vector2 markerPosition = Vector2.zero; //Initializing default markerPosition

        if (splineController.curveHovered) //Checks if will add a marker inbetween.
        {
            AddMarker_INBETWEEN();
        }
        else //If not, then add to the end.
        {
            AddMarker_TAIL(markerPosition);
        }
    }

    public void AddMarker_TAIL(Vector2 markerPosition)
    {
        DeselectAllMarkers();
        GameObject newMarker;
        // Vector2 markerPosition;

        if (markerPosition ==
            Vector2.zero) //This basically doesn't allow the user to put markers on (0,0) which he should be allowed. It's a dirty FIX.
        {
            markerPosition = inputManager.GetWorldMouseLocation2D();
        }

        newMarker = Instantiate(marker, markerPosition, Quaternion.identity, this.transform);

        markerList.Add(newMarker);
        if (newMarker.TryGetComponent(out MarkerEntity markerEntity))
        {
            markerEntity.frameNumber = markerList.Count;
        }

        splineController.AddKnot(newMarker.transform.position);
        UpdateGhostMarkers();
        UpdateALLMarkersRotation();
    }

    private void AddMarker_INBETWEEN()
    {
        DeselectAllMarkers();
        GameObject newMarker;
        Vector2 markerPosition;

        markerPosition = splineController.GetNearestPositionInSpline(out float ratio);
        newMarker = Instantiate(marker, markerPosition, Quaternion.identity, this.transform);

        //If curve.Hovered, Insert at specific index.
        int ratioIndex = splineController.GetKnotIndex(ratio);
        markerList.Insert(ratioIndex, newMarker);

        //Updating all frame numbers.
        for (int i = 0; i < markerList.Count; i++)
        {
            if (markerList[i].TryGetComponent(out MarkerEntity markerEntity))
            {
                markerEntity.frameNumber = i + 1;
            }
        }

        splineController.InsertKnot(markerPosition, ratioIndex);
        UpdateGhostMarkers();
        UpdateALLMarkersRotation();
    }

    public void MoveMarker_performed()
    {
        leftMouseButtonIsPressed = true;

        RaycastHit2D[] mouseRayCastHit = inputManager.DetectALL_MouseRayCastHit2D();
        if (mouseRayCastHit.Length > 0)
        {
            selectedObject = inputManager.GetHoveredObject();

            if (selectedObject.TryGetComponent(out MarkerEntity markerEntity)) //Checks if it's a marker.
            {
                //Old way of checking for double click.
                if (
                    markerSelection.selectedMarkerList.Contains(selectedObject)
                    && markerSelection.selectedMarkerList.Count < 2
                ) //This checks if this marker is ALREADY selected.
                {
                    // Update Frame Number.
                    //Vector3 offsetPosition = selectedObject.transform.position;
                    //offsetPosition.y += 0.5f;
                    // Instantiate(UI_FrameInputWindow,selectedObject.transform.position,Quaternion.identity);
                }

                selectedMarker = selectedObject;

                if (markerSelection.selectedMarkerList.Count == 0) //If the list is empty, select this marker.
                {
                    SelectMarker(markerEntity, selectedMarker);
                }
                else
                {
                    if (Input.GetKey(KeyCode.LeftShift) ||
                        Input.GetKey(KeyCode.RightShift)) //If it isn't empty check if shift is clicked.
                    {
                        if (markerSelection.selectedMarkerList
                            .Contains(selectedMarker)) //If marker already in list, remove it.
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
                        if (markerSelection.selectedMarkerList.Contains(selectedMarker)) //If this marker is selected.
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

    public void MoveMarker_canceled()
    {
        leftMouseButtonIsPressed = false;

        selectedMarker = null;
        selectedObject = null;
    }

    private void DeselectAllMarkers()
    {
        for (int i = markerSelection.selectedMarkerList.Count - 1; i >= 0; i--)
        {
            markerSelection.selectedMarkerList[i].GetComponent<MarkerEntity>().isSelected = false;
            markerSelection.selectedMarkerList.RemoveAt(i);
        }
    }

    public void SelectMarker(MarkerEntity markerEntity, GameObject selectedMarker)
    {
        int index = markerEntity.frameNumber - 1;
        markerEntity.isSelected = true;
        markerSelection.selectedMarkerList.Add(selectedMarker);
        TurnGhostIntoKeyMarker(index);
    }

    private void DeselectMarker(MarkerEntity markerEntity, GameObject selectedMarker)
    {
        markerEntity.isSelected = false;
        markerSelection.selectedMarkerList.Remove(selectedMarker);
    }

    public void DeleteMarker_performed()
    {
        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) //Check for SHIFT for GHOSTS.
        {
            foreach (GameObject marker in markerSelection.selectedMarkerList)
            {
                TurnMarkerToGhost(marker);
            }

            UpdateGhostMarkersPosition();
            UpdateALLMarkersRotation();
            DeselectAllMarkers();
        }
        else
        {
            //This was the way I found to delete the markers in their correct indices, before I was getting errors.
            List<int> indexToDelete = new List<int>();
            for (int i = 0; i < markerSelection.selectedMarkerList.Count; i++)
            {
                if (
                    markerSelection
                    .selectedMarkerList[i]
                    .TryGetComponent(out MarkerEntity markerEntity)
                )
                {
                    indexToDelete.Add(markerEntity.frameNumber - 1);
                }
            }

            indexToDelete.Sort();
            indexToDelete.Reverse();

            foreach (int index in indexToDelete)
            {
                //Checking if we're deleting the last marker. If yes, make second to last, if there is one, a KEY marker.
                if (index == markerList.Count - 1)
                {
                    if (markerList.Count > 1)
                    {
                        TurnGhostIntoKeyMarker(index - 1);
                    }
                }
                //Checking if we're deleting the first marker. If yes, make second, if there is one, a KEY marker.
                else if (index == 0)
                {
                    if (markerList.Count > 1)
                    {
                        TurnGhostIntoKeyMarker(index + 1);
                    }
                }

                GameObject markerToDelete = markerList[index];
                splineController.RemoveKnot(index);
                markerList.RemoveAt(index);
                Destroy(markerToDelete);
            }

            markerSelection.selectedMarkerList.Clear();
            UpdateFrameNumber();
            UpdateGhostMarkers();
            UpdateALLMarkersRotation();
        }
    }

    private void UpdateFrameNumber()
    {
        for (int i = 0; i < markerList.Count; i++)
        {
            if (markerList[i].TryGetComponent(out MarkerEntity iMarkerEntity))
            {
                iMarkerEntity.frameNumber = i + 1;
            }
        }
    }

    private void UpdateALLMarkersRotation()
    {
        //Update marker's rotation taken from knot at same index.
        for (int i = 0; i < markerList.Count; i++)
        {
            markerList[i].gameObject.transform.rotation = splineController.GetKnotRotation(i);
        }

        //I'll use this now to update the rotation of the markers, but this will also update the position/creation of ghost markers maybe.
    }

    private void
        UpdateGhostMarkers() //Need to call this every time a marker is deleted, and when a marker is added on the curve.
    {
        UpdateGhostIndices();
        splineController.UpdateGhostKnots(ghostIndices);
        UpdateGhostMarkersPosition();
    }

    private void UpdateGhostMarkersPosition()
    {
        if (ghostIndices.Count > 0)
        {
            Dictionary<int, Vector3> ghostPositions = new Dictionary<int, Vector3>(
                ghostKnotController.GetGhostPositions()
            );

            for (int i = 0; i < markerList.Count; i++)
            {
                if (ghostIndices.Contains(i))
                {
                    markerList[i].transform.position = ghostPositions[i];
                }
            }
        }
    }

    //Helper method to Update ghostIndices list.
    private void UpdateGhostIndices()
    {
        ghostIndices.Clear();
        for (int i = 0; i < markerList.Count; i++)
        {
            if (markerList[i].TryGetComponent(out MarkerEntity markerEntity))
            {
                if (markerEntity.isGhost)
                {
                    ghostIndices.Add(i);
                }
            }
        }
    }

    public void TurnMarkerToGhost(GameObject marker) //Call this when selected markers are SHIFT-Deleted
    {
        if (marker.TryGetComponent(out MarkerEntity markerEntity))
        {
            int markerIndex = markerEntity.frameNumber - 1;
            if (markerIndex > 0 &&
                markerIndex < markerList.Count - 1) //Checking if it's not the first or last marker index.
            {
                //This turns on isGhost and adds that index to the ghostIndices list.
                markerEntity.isGhost = true;
                if (!ghostIndices.Contains(markerIndex))
                {
                    ghostIndices.Add(markerIndex);
                }
            }
        }

        // Passes the ghostIndices list to Spline Controller for it to update the Ghosts position.
        splineController.UpdateGhostKnots(ghostIndices);
    }

    private void TurnGhostIntoKeyMarker(int index)
    {
        if (markerList[index].TryGetComponent(out MarkerEntity markerEntity))
        {
            markerEntity.isGhost = false;
        }

        if (ghostIndices.Contains(index))
        {
            ghostIndices.Remove(index);
        }

        splineController.UpdateGhostKnots(ghostIndices);
        UpdateGhostMarkersPosition();
    }

    internal void UpdateFrameNumber(int currentFrameNumber, int updatedFrameNumber)
    {
        //
    }
}
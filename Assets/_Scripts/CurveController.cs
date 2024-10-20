using System.Collections.Generic;
using UnityEngine;

public class CurveController : MonoBehaviour
{
    [SerializeField]
    private GameObject marker;

    //[SerializeField] private InputManager inputManager;

    [SerializeField]
    private GhostKnotController ghostKnotController;

    [SerializeField]
    private GameObject UI_FrameInputWindow;

    public List<GameObject> markerList = new List<GameObject>();
    public List<int> ghostIndices = new List<int>();

    private void Start()
    {
        //=======   Registering Events.   =======
        GameEventSystem.Instance.RegisterListener<onRightClickPerformed>(AddMarkerHandler);
        GameEventSystem.Instance.RegisterListener<MoveSelectedEvent>(MoveSelectMarker);
        GameEventSystem.Instance.RegisterListener<DeleteSelectedEvent>(DeleteMarker_performed);
        GameEventSystem.Instance.RegisterListener<ShiftDeleteSelectedEvent>(
            ShiftDeleteMarker_performed
        );
        GameEventSystem.Instance.RegisterListener<onRecordingFinishedEvent>(AddRecordedMarkers);
    }

    private void OnDestroy()
    {
        //=======   Unregistering Events.   =======
        GameEventSystem.Instance.UnregisterListener<onRightClickPerformed>(AddMarkerHandler);
        GameEventSystem.Instance.UnregisterListener<MoveSelectedEvent>(MoveSelectMarker);
        GameEventSystem.Instance.RegisterListener<DeleteSelectedEvent>(DeleteMarker_performed);
        GameEventSystem.Instance.RegisterListener<ShiftDeleteSelectedEvent>(
            ShiftDeleteMarker_performed
        );
        GameEventSystem.Instance.RegisterListener<onRecordingFinishedEvent>(AddRecordedMarkers);
    }

    private void Update()
    {
        //TODO What was this meant to be anyway?
        GameEventSystem.Instance.Raise<UpdateSplineEvent, bool>(new UpdateSplineEvent(markerList));
    }

    private void AddMarkerHandler(onRightClickPerformed onRightClickPerformed)
    {
        Vector2 markerPosition = onRightClickPerformed.mousePosition;
        List<int> affectedMarkersRotationIndex = new List<int>();

        if (
            onRightClickPerformed.mouseHitAllArray3D != null
            && onRightClickPerformed.mouseHitAllArray3D.Length != 0
        ) //Checks if will add a marker inbetween.
        {
            //Raising insert Marker event to get info to where the marker needs to be on the spline.
            MarkerDataStruct markerData = GameEventSystem.Instance.Raise<
                InsertMarkerEvent,
                MarkerDataStruct
            >(new InsertMarkerEvent(onRightClickPerformed.mousePosition));
            InsertMarker(markerData);

            affectedMarkersRotationIndex.Add(markerData.markerIndex);
            affectedMarkersRotationIndex.Add(markerData.markerIndex - 1);
            affectedMarkersRotationIndex.Add(markerData.markerIndex + 1);

            //AddMarkerEvent with markerData index and position.
            GameEventSystem.Instance.Raise<AddMarkerEvent>(
                new AddMarkerEvent(markerData.markerPosition, markerData.markerIndex)
            );
        }
        else //If not, then add to the end.
        {
            AddMarker(markerPosition);

            //AddMarkerEvent with last index and markerPosition.
            int lastIndex = markerList.Count - 1;
            affectedMarkersRotationIndex.Add(lastIndex);
            if (lastIndex > 0)
            {
                affectedMarkersRotationIndex.Add(lastIndex - 1);
            }
            GameEventSystem.Instance.Raise<AddMarkerEvent>(
                new AddMarkerEvent(markerPosition, lastIndex)
            );
        }

        UpdateMarkersRotation(affectedMarkersRotationIndex);
    }

    private void AddMarker(Vector2 markerPosition)
    {
        GameObject newMarker = Instantiate(
            marker,
            markerPosition,
            Quaternion.identity,
            this.transform
        );

        markerList.Add(newMarker);
        if (newMarker.TryGetComponent(out MarkerEntity markerEntity))
        {
            markerEntity.frameNumber = markerList.Count;
        }

        UpdateGhostMarkers();
    }

    private void AddRecordedMarkers(onRecordingFinishedEvent onRecordingFinishedEvent)
    {
        List<int> affectedMarkersRotationIndex = new(markerList.Count - 1); //Adding the last before starting adding more markers.

        foreach (MarkerDataStruct marker in onRecordingFinishedEvent.recordedMarkers)
        {
            AddMarker(marker.markerPosition);
            affectedMarkersRotationIndex.Add(markerList.Count - 1); //Adding the new last marker.
        }

        UpdateMarkersRotation(affectedMarkersRotationIndex);
    }

    private void InsertMarker(MarkerDataStruct markerData)
    {
        Vector2 markerPosition = markerData.markerPosition;
        int ratioIndex = markerData.markerIndex;
        GameObject newMarker = Instantiate(marker, markerPosition, Quaternion.identity, transform);

        markerList.Insert(ratioIndex, newMarker);

        //Updating all frame numbers.
        //TODO You can just update from this marker onwards, and not all of them.
        for (int i = 0; i < markerList.Count; i++)
        {
            if (markerList[i].TryGetComponent(out MarkerEntity markerEntity))
            {
                markerEntity.frameNumber = i + 1;
            }
        }

        UpdateGhostMarkers();
    }

    private void MoveSelectMarker(MoveSelectedEvent moveSelectedEvent)
    {
        foreach (GameObject marker in moveSelectedEvent.selectedMarkers)
        {
            if (marker.TryGetComponent(out MarkerEntity markerEntity))
            {
                if (markerEntity.isGhost)
                {
                    TurnGhostIntoKeyMarker(markerEntity.frameNumber - 1);
                }
            }
        }
        UpdateGhostMarkers();
    }

    private void DeleteMarker_performed(DeleteSelectedEvent deleteSelectedEvent)
    {
        //This was the way I found to delete the markers in their correct indices, before I was getting errors.
        List<int> indexToDelete = new(deleteSelectedEvent.selectedMarkersIndices);
        List<int> affectedMarkersRotationIndex = new List<int>();

        indexToDelete.Sort();
        indexToDelete.Reverse();

        foreach (int index in indexToDelete)
        {
            //Checking if we're deleting the last marker. If yes, make second to last, if there is one, a KEY marker.
            if (index == markerList.Count - 1)
            {
                //We're starting the deletetion from the end. So if we're deleting the last, the first affected one is the last - 1.
                affectedMarkersRotationIndex.Clear();
                affectedMarkersRotationIndex.Add(index - 1);

                if (markerList.Count > 1)
                {
                    TurnGhostIntoKeyMarker(index - 1);
                }
            }
            else
            {
                if (index - 1 >= 0)
                {
                    affectedMarkersRotationIndex.Add(index - 1);
                }
                if (!affectedMarkersRotationIndex.Contains(index))
                {
                    affectedMarkersRotationIndex.Add(index);
                }
                //Checking if we're deleting the first marker. If yes, make second, if there is one, a KEY marker.
                if (index == 0)
                {
                    if (markerList.Count > 1)
                    {
                        TurnGhostIntoKeyMarker(index + 1);
                    }
                }
            }

            GameObject markerToDelete = markerList[index];
            markerList.RemoveAt(index);
            Destroy(markerToDelete);
        }

        UpdateMarkersRotation(affectedMarkersRotationIndex);

        UpdateFrameNumber();
        UpdateGhostMarkers();
    }

    private void ShiftDeleteMarker_performed(ShiftDeleteSelectedEvent shiftDeleteSelectedEvent)
    {
        List<int> selectedMarkers = shiftDeleteSelectedEvent.selectedMarkersIndices;
        List<int> affectedMarkersRotationIndex = new List<int>();

        selectedMarkers.Sort();
        selectedMarkers.Reverse();

        foreach (int index in selectedMarkers)
        {
            TurnMarkerToGhost(markerList[index]);

            //Figuring out the index of the affected markers by the ghosting.
            affectedMarkersRotationIndex.Add(index);
            if (index - 1 >= 0 && !affectedMarkersRotationIndex.Contains(index - 1))
            {
                affectedMarkersRotationIndex.Add(index - 1);
            }
            if (
                index + 1 <= markerList.Count - 1
                && !affectedMarkersRotationIndex.Contains(index + 1)
            )
            {
                affectedMarkersRotationIndex.Add(index + 1);
            }
        }

        UpdateGhostMarkersPosition();

        UpdateMarkersRotation(affectedMarkersRotationIndex);
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

    private void UpdateMarkersRotation(List<int> affectedMarkers)
    {
        Dictionary<int, Quaternion> affectedMarkersRotation = new();

        affectedMarkersRotation = GameEventSystem.Instance.Raise<
            UpdateMarkerRotationEvent,
            Dictionary<int, Quaternion>
        >(new UpdateMarkerRotationEvent(affectedMarkers));

        foreach (KeyValuePair<int, Quaternion> marker in affectedMarkersRotation)
        {
            markerList[marker.Key].gameObject.transform.rotation = marker.Value;
        }
    }

    //Need to call this every time a marker is deleted, added or moved?
    private void UpdateGhostMarkers()
    {
        UpdateGhostIndices();
        GameEventSystem.Instance.Raise<UpdateGhostsEvent>(new UpdateGhostsEvent(ghostIndices));
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
            if (markerIndex > 0 && markerIndex < markerList.Count - 1) //Checking if it's not the first or last marker index.
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
        GameEventSystem.Instance.Raise<UpdateGhostsEvent>(new UpdateGhostsEvent(ghostIndices));
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

        GameEventSystem.Instance.Raise<UpdateGhostsEvent>(new UpdateGhostsEvent(ghostIndices));
        UpdateGhostMarkersPosition();
    }

    internal void UpdateFrameNumber(int currentFrameNumber, int updatedFrameNumber)
    {
        //
    }
}

// Structs to send or return as data
public struct MarkerDataStruct
{
    public Vector3 markerPosition { get; private set; }
    public int markerIndex { get; private set; }
    public bool isKey { get; private set; }

    public MarkerDataStruct(
        Vector3 markerPosition = default,
        int markerIndex = -1,
        bool isKey = true
    )
    {
        this.markerPosition = markerPosition;
        this.markerIndex = markerIndex;
        this.isKey = isKey;
    }
}

//====================    EVENTS    ====================
public struct AddMarkerEvent
{
    public Vector3 position { get; private set; }
    public int index { get; private set; }

    public AddMarkerEvent(Vector3 position, int index)
    {
        this.position = position;
        this.index = index;
    }
}

public struct InsertMarkerEvent
{
    public Vector3 mousePosition { get; private set; }

    public InsertMarkerEvent(Vector3 mousePosition)
    {
        this.mousePosition = mousePosition;
    }
}

public struct DeleteMarkersEvent
{
    public int deletedMarkerIndex { get; private set; }

    public DeleteMarkersEvent(int deletedMarkerIndex)
    {
        this.deletedMarkerIndex = deletedMarkerIndex;
    }
}

public struct MoveMarkersEvent
{
    public Dictionary<int, Vector3> movedMarkers { get; private set; }
}

public struct UpdateMarkerRotationEvent
{
    public List<int> affectedIndices { get; private set; }

    public UpdateMarkerRotationEvent(List<int> affectedIndices)
    {
        this.affectedIndices = affectedIndices;
    }
}

public struct UpdateGhostsEvent
{
    public List<int> ghostMarkersIndices { get; private set; }

    public UpdateGhostsEvent(List<int> ghostMarkersIndices)
    {
        this.ghostMarkersIndices = ghostMarkersIndices;
    }
}

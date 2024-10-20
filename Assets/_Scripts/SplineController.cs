using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;

public class SplineController : MonoBehaviour
{
    [SerializeField]
    private InputManager inputManager;

    [SerializeField]
    private SplineMeshController splineMeshController;

    [SerializeField]
    private GameObject hoverPointerPrefab;

    [SerializeField]
    private float tension;

    [SerializeField]
    private GhostKnotController ghostKnotController;

    private Spline spline;
    private GameObject hoverPointer;
    private List<float> knotsRatios = new List<float>(); //TODO turn this into a dictionary?

    private void Start()
    {
        spline = GetComponent<SplineContainer>().Spline;

        //=======   Registering Events.   =======
        GameEventSystem.Instance.RegisterListener<UpdateSplineEvent, bool>(UpdateSpline);
        GameEventSystem.Instance.RegisterListener<InsertMarkerEvent, MarkerDataStruct>(
            InsertKnotHandler
        );
        GameEventSystem.Instance.RegisterListener<AddMarkerEvent>(AddKnot);
        GameEventSystem.Instance.RegisterListener<
            UpdateMarkerRotationEvent,
            Dictionary<int, Quaternion>
        >(GetKnotsRotation);
        GameEventSystem.Instance.RegisterListener<DeleteSelectedEvent>(RemoveKnots);
        GameEventSystem.Instance.RegisterListener<UpdateGhostsEvent>(UpdateGhostKnots);
        GameEventSystem.Instance.RegisterListener<OnHoveringCurveEvent>(SplinePointerController);
        GameEventSystem.Instance.RegisterListener<OnHoveringCurveCanceledEvent>(
            SplinePointerDestroyer
        );
    }

    private void OnDestroy()
    {
        //=======   Unregistering Events.   =======
        GameEventSystem.Instance.UnregisterListener<UpdateSplineEvent, bool>(UpdateSpline);
        GameEventSystem.Instance.UnregisterListener<InsertMarkerEvent, MarkerDataStruct>(
            InsertKnotHandler
        );
        GameEventSystem.Instance.UnregisterListener<AddMarkerEvent>(AddKnot);
        GameEventSystem.Instance.UnregisterListener<
            UpdateMarkerRotationEvent,
            Dictionary<int, Quaternion>
        >(GetKnotsRotation);
        GameEventSystem.Instance.UnregisterListener<DeleteSelectedEvent>(RemoveKnots);
        GameEventSystem.Instance.UnregisterListener<UpdateGhostsEvent>(UpdateGhostKnots);
        GameEventSystem.Instance.UnregisterListener<OnHoveringCurveEvent>(SplinePointerController);
        GameEventSystem.Instance.UnregisterListener<OnHoveringCurveCanceledEvent>(
            SplinePointerDestroyer
        );
    }

    public int GetKnotIndex(float ratio)
    {
        //This checks to see if the value ratio is found in the list and gives you back the index if it does.
        //If not, it gives you a negative number of what is the index the number should be inserted in order to maintain order.
        int index = knotsRatios.BinarySearch(ratio);

        if (index < 0)
        {
            index = ~index; // This is where the value should be inserted to maintain order.
        }

        return index;
    }

    public Vector3 GetNearestPositionInSpline(Vector3 mouseWorldPosition, out float ratio)
    {
        SplineUtility.GetNearestPoint(
            spline,
            (float3)mouseWorldPosition,
            out float3 hoverPosition,
            out ratio
        );
        return (Vector3)hoverPosition;
    }

    //Handles both Adding and Inserting knots in any position.
    public void AddKnot(AddMarkerEvent addMarkerEvent)
    {
        float3 knotPosition = addMarkerEvent.position;
        int knotIndex = addMarkerEvent.index;

        BezierKnot knot = new(knotPosition);
        spline.Insert(knotIndex, knot, TangentMode.AutoSmooth, tension);
        UpdateKnotsRatiosList();

        splineMeshController.BuildMesh();
    }

    //This is not inserting a marker, it's just finding it's position and index in case of insert event.
    public MarkerDataStruct InsertKnotHandler(InsertMarkerEvent insertMarkerEvent)
    {
        Vector3 mousePosition = insertMarkerEvent.mousePosition;
        Vector3 splinePosition = GetNearestPositionInSpline(mousePosition, out float ratio);
        int index = GetKnotIndex(ratio);
        MarkerDataStruct markerDataStruct = new MarkerDataStruct(splinePosition, index);

        return markerDataStruct;
    }

    public bool UpdateSpline(UpdateSplineEvent updateSplineEvent)
    {
        var markerList = updateSplineEvent.MarkersList;
        for (int i = 0; i < markerList.Count; i++)
        {
            //Not sure this is the way..
            //This is rebuilding the whole spline every frame while mouse left button is pressed.
            BezierKnot iKnot = new BezierKnot((float3)markerList[i].transform.position);
            spline.SetKnot(i, iKnot);
            spline.SetAutoSmoothTension(i, tension);
        }

        UpdateKnotsRatiosList();
        splineMeshController.BuildMesh();
        return true;
    }

    private void RemoveKnots(DeleteSelectedEvent deleteSelectedEvent)
    {
        List<int> knotIndicesToRemove = new(deleteSelectedEvent.selectedMarkersIndices);
        knotIndicesToRemove.Sort();
        knotIndicesToRemove.Reverse();

        foreach (int index in knotIndicesToRemove)
        {
            spline.RemoveAt(index);
        }

        UpdateKnotsRatiosList();
        splineMeshController.BuildMesh();
    }

    // This is the method to get the rotation given an index.
    public Dictionary<int, Quaternion> GetKnotsRotation(
        UpdateMarkerRotationEvent updateMarkerRotationEvent
    )
    {
        Dictionary<int, Quaternion> affectedMarkersRotation = new();
        foreach (int knotIndex in updateMarkerRotationEvent.affectedIndices)
        {
            float ratio = GetKnotRatioInSpline(knotIndex);

            Quaternion zRotation = GetRotation(ratio);
            affectedMarkersRotation.Add(knotIndex, zRotation);
        }

        // Return the rotation that only affects the Z axis of the game object
        return affectedMarkersRotation;
    }

    // This is the method to get the rotation given a ratio.
    private Quaternion GetRotation(float ratio)
    {
        // Evaluate tangent at this t (ratio)
        float3 tangent = spline.EvaluateTangent(ratio);

        // Calculate the angle in degrees from the tangent's X component
        float angleAroundX = Mathf.Atan2(tangent.y, tangent.x) * Mathf.Rad2Deg;

        // We want to apply this angle to the Z rotation of the game object
        Quaternion zRotation = Quaternion.Euler(0, 0, angleAroundX);

        // Return the rotation that only affects the Z axis of the game object
        return zRotation;
    }

    public float GetKnotRatioInSpline(int knotIndex)
    {
        //Thanks ChatGPT lol - I didn't find any good way on the documentation to get the knot ratio.
        // Calculate total spline length
        float totalLength = spline.GetLength();

        // Handle edge case where there's only one knot or spline length is zero
        if (totalLength == 0 || spline.Count <= 1)
        {
            return 0f; // Ratio for the first knot should be zero
        }

        // Calculate cumulative length up to the given knot
        float cumulativeLength = 0f;

        // We stop accumulating the length if we're at the first knot (index 0), otherwise, we accumulate lengths of segments
        for (int i = 0; i < knotIndex; i++)
        {
            // Add the length of each curve (segment between knots)
            cumulativeLength += spline.GetCurveLength(i);
        }

        // Ensure the cumulative length doesn't exceed total length (in case of floating-point inaccuracies)
        cumulativeLength = Mathf.Min(cumulativeLength, totalLength);

        // Calculate the normalized t value for this knot
        float t = cumulativeLength / totalLength;

        return t;
    }

    public void UpdateKnotsRatiosList()
    {
        if (knotsRatios.Count != 0)
        {
            knotsRatios.Clear();
        }

        for (int i = 0; i < spline.Count; i++)
        {
            float ratio = GetKnotRatioInSpline(i);
            knotsRatios.Add(ratio);
        }
    }

    public void UpdateGhostKnots(UpdateGhostsEvent updateGhostsEvent)
    {
        List<int> ghostMarkersIndices = updateGhostsEvent.ghostMarkersIndices;

        if (ghostMarkersIndices.Count > 0)
        {
            // Method to delete knots and reindex the remaining knots
            ghostKnotController.DeleteKnotsAndTrackIndices(
                ghostMarkersIndices,
                out Dictionary<int, int> newIndexMapping
            );

            // Method to reinsert deleted knots
            ghostKnotController.ReInsertDeletedKnots(ghostMarkersIndices, newIndexMapping);
        }

        splineMeshController.BuildMesh();
    }

    public void SplinePointerController(OnHoveringCurveEvent onHoveringCurveEvent)
    {
        Vector3 hoverPosition = GetNearestPositionInSpline(Input.mousePosition, out float ratio);
        if (hoverPointer == null)
        {
            hoverPointer = Instantiate(hoverPointerPrefab, hoverPosition, Quaternion.identity);
        }

        hoverPointer.transform.position = hoverPosition;
    }

    public void SplinePointerDestroyer(OnHoveringCurveCanceledEvent onHoveringCurveCanceledEvent)
    {
        if (hoverPointer != null)
        {
            Destroy(hoverPointer.gameObject);
        }
    }
}

//========== EVENTS =============
public struct UpdateSplineEvent
{
    public List<GameObject> MarkersList { get; private set; }

    public UpdateSplineEvent(List<GameObject> markersList)
    {
        MarkersList = markersList;
    }
}

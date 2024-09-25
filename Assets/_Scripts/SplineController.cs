using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    private List<float> knotsRatios = new List<float>();
    public bool curveHovered;

    private void Start()
    {
        spline = GetComponent<SplineContainer>().Spline;
    }

    private void Update()
    {
        //Check for mouseRayCastHits in 3D, because Mesh is a 3D object.
        RaycastHit[] mouseRayCastHit = inputManager.DetectALL_MouseRaycastHit3D();
        if (mouseRayCastHit != null)
        {
            //Checking if for some reason there's more than one 3D object detected.
            if (mouseRayCastHit.Length > 1)
            {
                Debug.LogError("Two different 3D objects detected.");
            }
            //Checking if it's a curve mesh.
            else if (
                mouseRayCastHit[0].collider.gameObject.TryGetComponent(out SplineController SC)
            )
            {
                //Instantiating a pointer for the player to know visually where the nearest point in the spline is.
                Vector3 hoverPosition = GetNearestPositionInSpline(out float ratio);
                if (hoverPointer == null)
                {
                    hoverPointer = Instantiate(
                        hoverPointerPrefab,
                        hoverPosition,
                        Quaternion.identity
                    );
                }
                hoverPointer.transform.position = hoverPosition;
                curveHovered = true;
            }
        }
        else //Deleting the pointer object and clearing the bool.
        {
            curveHovered = false;
            if (hoverPointer != null)
            {
                Destroy(hoverPointer.gameObject);
            }
        }
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

    public Vector3 GetNearestPositionInSpline(out float ratio)
    {
        Vector3 mouseWorldPosition = inputManager.GetWorldMouseLocation2D();
        float3 hoverPosition;
        SplineUtility.GetNearestPoint(
            spline,
            (float3)mouseWorldPosition,
            out hoverPosition,
            out ratio
        );
        return (Vector3)hoverPosition;
    }

    public void AddKnot(Vector3 position)
    {
        //Used by Curve Controller to add a knot at the end.
        BezierKnot knot = new BezierKnot((float3)position);
        spline.Add(knot, TangentMode.AutoSmooth, tension);
        UpdateKnotsRatiosList();
        splineMeshController.BuildMesh();
    }

    public void InsertKnot(Vector3 position, int index)
    {
        //Used by Curve Controller to add a knot inbetween other knots.
        BezierKnot knot = new BezierKnot((float3)position);
        spline.Insert(index, knot, TangentMode.AutoSmooth, tension);
        UpdateKnotsRatiosList();
        splineMeshController.BuildMesh();
    }

    public void UpdateSpline(List<GameObject> markerList)
    {
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
    }

    public void RemoveKnot(int index)
    {
        spline.RemoveAt(index);
        UpdateKnotsRatiosList();
        splineMeshController.BuildMesh();
    }

    // This is the method to get the rotation given an index.
    public Quaternion GetKnotRotation(int knotIndex)
    {
        float ratio = GetKnotRatioInSpline(knotIndex);

        Quaternion zRotation = GetRotation(ratio);

        // Return the rotation that only affects the Z axis of the game object
        return zRotation;
    }

    // This is the method to get the rotation given a ratio.
    public Quaternion GetRotation(float ratio)
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
        //Thanks ChatGPT lol - I didn't find any good way to get the knot ratio.
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

    public void UpdateGhostKnots(List<int> ghostMarkersIndices)
    {
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
}

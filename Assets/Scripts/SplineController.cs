using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;

public class SplineController : MonoBehaviour
{
    [SerializeField]
    private SplineContainer splineContainer;

    [SerializeField]
    private float tension;

    private Spline spline;

    private void Start()
    {
        spline = GetComponent<SplineContainer>().Spline;
    }

    public void AddKnot(Vector3 position)
    {
        //Used by Curve Controller to add a knot at the end.
        BezierKnot knot = new BezierKnot((float3)position);
        spline.Add(knot, TangentMode.AutoSmooth, tension);
    }

    public void UpdateSpline(List<GameObject> markerList)
    {
        for (int i = 0; i < markerList.Count; i++)
        {
            //Not sure this is the way..
            BezierKnot iKnot = new BezierKnot((float3)markerList[i].transform.position);
            spline.SetKnot(i, iKnot);
            spline.SetAutoSmoothTension(i, tension);
        }
    }

    public void RemoveKnot(int index)
    {
        spline.RemoveAt(index);
    }

    public Quaternion GetKnotRotation(int knotIndex)
    {
        float ratio = GetKnotRatioInSpline(knotIndex);

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
        //Thanks ChatGPT lol
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
}

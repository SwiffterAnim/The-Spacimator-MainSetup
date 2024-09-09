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

    public void AddKnot(Vector3 position)
    {
        //Used by Curve Controller to add a knot at the end.
        BezierKnot knot = new BezierKnot((float3)position);
        splineContainer.Spline.Add(knot, TangentMode.AutoSmooth, tension);
    }

    public void UpdateSpline(List<GameObject> markerList)
    {
        for (int i = 0; i < markerList.Count; i++)
        {
            //Not sure this is the way..
            BezierKnot iKnot = new BezierKnot((float3)markerList[i].transform.position);
            splineContainer.Spline.SetKnot(i, iKnot);
            splineContainer.Spline.SetAutoSmoothTension(i, tension);
        }
    }

    public void RemoveKnot(int index)
    {
        splineContainer.Spline.RemoveAt(index);
    }
}

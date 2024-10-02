using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MarkerSelection : MonoBehaviour
{
    [SerializeField]
    InputManager inputManager;

    [SerializeField]
    CurveController curveController;

    public RaycastHit2D[] mouseRayCastHitArray = new RaycastHit2D[0];
    private List<GameObject> markersMouseRayCastHits = new List<GameObject>();
    private GameObject hoveredMarker;

    public List<GameObject> selectedMarkerList = new List<GameObject>();

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
                //Exactly because I'm checking the frame number, I can use this to highligh both markers in the curve and timeline.
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
}

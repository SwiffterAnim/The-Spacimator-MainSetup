using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MarkerManager : MonoBehaviour
{
    [SerializeField]
    InputManager inputManager;

    private RaycastHit2D[] mouseRayCastHitArray;
    private List<GameObject> markersMouseRayCastHits = new List<GameObject>();
    private GameObject hoveredMarker;
    public List<GameObject> markerList = new List<GameObject>();

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
            hoveredMarker.GetComponent<MarkerController>().VisualOffHoveredMarker();
        }
        //This is basically clearing in every frame. That's no good..
        markersMouseRayCastHits.Clear();
    }

    private void UpdateMarkerHovered(List<GameObject> markersList)
    {
        //Gets the top marker, important if there's more than one.
        hoveredMarker = markersList[^1];

        // hoveredMarker.GetComponent<MarkerController>().VisualOnHoveredMarker();

        //Turning all the others off.
        for (int i = 0; i < markerList.Count; i++)
        {
            if (
                markerList[i].GetComponent<MarkerEntity>().frameNumber
                == hoveredMarker.GetComponent<MarkerEntity>().frameNumber
            )
            {
                hoveredMarker.GetComponent<MarkerController>().VisualOnHoveredMarker();
            }
            else
            {
                markerList[i].GetComponent<MarkerController>().VisualOffHoveredMarker();
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

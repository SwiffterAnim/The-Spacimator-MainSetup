using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CurveController : MonoBehaviour
{
    [SerializeField]
    GameObject marker;

    [SerializeField]
    InputManager inputManager;

    [SerializeField]
    MarkerSelection markerSelection;

    [SerializeField]
    SplineController splineController;

    private bool leftMouseButtonIsPressed = false;
    private UserInputActions userInputActions;
    private GameObject selectedMarker;
    private GameObject selectedObject;
    public List<GameObject> markerList = new List<GameObject>();

    // private MarkerController markerController;

    private void OnEnable()
    {
        userInputActions = GameManager.Instance.GetInputAction();
        userInputActions.EditingCurve.AddMarker.performed += AddMarker_performed;
        userInputActions.EditingCurve.MoveMarker.performed += MoveMarker_performed;
        userInputActions.EditingCurve.MoveMarker.canceled += MoveMarker_canceled;
        userInputActions.EditingCurve.DeleteMarker.performed += DeleteMarker_performed;
    }

    private void OnDisable()
    {
        userInputActions.EditingCurve.AddMarker.performed -= AddMarker_performed;
        userInputActions.EditingCurve.MoveMarker.performed -= MoveMarker_performed;
        userInputActions.EditingCurve.MoveMarker.canceled -= MoveMarker_canceled;
        userInputActions.EditingCurve.DeleteMarker.performed -= DeleteMarker_performed;
    }

    private void Update()
    {
        if (markerSelection.selectedMarkerList != null && leftMouseButtonIsPressed)
        {
            for (int i = 0; i < markerSelection.selectedMarkerList.Count; i++)
            {
                if (
                    markerSelection
                        .selectedMarkerList[i]
                        .TryGetComponent(out MarkerController markerController)
                )
                {
                    markerController.MoveMarkerWithMouse(inputManager.GetWorldMouseLocation2D());
                    splineController.UpdateSpline(markerList);
                }
            }
            //Update rotation of markers.
            UpdateMarkers();
        }
    }

    private void AddMarker_performed(InputAction.CallbackContext context)
    {
        DeselectAllMarkers();

        Vector2 mouseWorldPosition2D = inputManager.GetWorldMouseLocation2D();
        GameObject newMarker = Instantiate(
            marker,
            mouseWorldPosition2D,
            Quaternion.identity,
            this.transform
        );
        markerList.Add(newMarker);
        if (newMarker.TryGetComponent(out MarkerEntity markerEntity))
        {
            //--------------------TODO - PAY ATTENTION TO THIS LATER--------------------
            //I'm sure I'll have to make this better, because when I started removing markers, or adding markers in between other markers in the curve, this will have to be better.
            markerEntity.frameNumber = markerList.Count;
        }
        splineController.AddKnot(newMarker.transform.position);
        UpdateMarkers();
    }

    private void MoveMarker_performed(InputAction.CallbackContext context)
    {
        leftMouseButtonIsPressed = true;

        RaycastHit2D[] mouseRayCastHit = inputManager.DetectALL_MouseRayCastHit2D();
        if (mouseRayCastHit.Length > 0)
        {
            selectedObject = inputManager.GetHoveredObject();

            if (selectedObject.TryGetComponent(out MarkerEntity markerEntity)) //Checks if it's a marker.
            {
                selectedMarker = selectedObject;

                if (markerSelection.selectedMarkerList.Count == 0) //If the list is empty, select this marker.
                {
                    SelectMarker(markerEntity, selectedMarker);
                }
                else
                {
                    if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) //If it isn't empty check if shift is clicked.
                    {
                        if (markerSelection.selectedMarkerList.Contains(selectedMarker)) //If marker already in list, remove it.
                        {
                            DeselectMarker(markerEntity, selectedMarker);
                        }
                        else //otherwise, add it.
                        {
                            SelectMarker(markerEntity, selectedMarker);
                        }
                    }
                    else //if list is not empty and shit is NOT clicked
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

    private void DeselectAllMarkers()
    {
        for (int i = markerSelection.selectedMarkerList.Count - 1; i >= 0; i--)
        {
            markerSelection.selectedMarkerList[i].GetComponent<MarkerEntity>().isSelected = false;
            markerSelection.selectedMarkerList.RemoveAt(i);
        }
    }

    private void SelectMarker(MarkerEntity markerEntity, GameObject selectedMarker)
    {
        markerEntity.isSelected = true;
        markerSelection.selectedMarkerList.Add(selectedMarker);
    }

    private void DeselectMarker(MarkerEntity markerEntity, GameObject selectedMarker)
    {
        markerEntity.isSelected = false;
        markerSelection.selectedMarkerList.Remove(selectedMarker);
    }

    private void MoveMarker_canceled(InputAction.CallbackContext context)
    {
        leftMouseButtonIsPressed = false;

        selectedMarker = null;
        selectedObject = null;
    }

    private void DeleteMarker_performed(InputAction.CallbackContext context)
    {
        //This was the way I found to delete the markers in their correct indexes, before I was getting errors.
        List<int> indexToDelete = new List<int>();
        for (int i = 0; i < markerSelection.selectedMarkerList.Count; i++)
        {
            if (
                markerSelection.selectedMarkerList[i].TryGetComponent(out MarkerEntity markerEntity)
            )
            {
                indexToDelete.Add(markerEntity.frameNumber - 1);
            }
        }
        indexToDelete.Sort();
        indexToDelete.Reverse();

        foreach (int index in indexToDelete)
        {
            GameObject markerToDelete = markerList[index];
            splineController.RemoveKnot(index);
            markerList.RemoveAt(index);
            Destroy(markerToDelete);
        }

        markerSelection.selectedMarkerList.Clear();
        UpdateFrameNumber();
        UpdateMarkers();
        //--------------------TODO - PAY ATTENTION TO THIS LATER--------------------
        //Right now I'm just deleting and updating the frame number. I'm not deleting and creating "ghost" markers.
        //I think for this to be nice to have the option to add ghost markers.
        //1- If you delete normally, it updates frame numbers and timeline.
        //2- If you delete with SHIFT, then it creates ghost markers. I think that's better.
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

    private void UpdateMarkers()
    {
        //Update marker's rotation taken from knot at same index.
        for (int i = 0; i < markerList.Count; i++)
        {
            markerList[i].gameObject.transform.rotation = splineController.GetKnotRotation(i);
        }

        //I'll use this now to update the rotation of the markers, but this will also update the position/creation of ghost markers maybe.
    }
}

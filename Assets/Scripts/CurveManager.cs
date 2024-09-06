using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CurveManager : MonoBehaviour
{
    [SerializeField]
    GameObject marker;

    [SerializeField]
    InputManager inputManager;

    [SerializeField]
    MarkerManager markerManager;

    private bool leftMouseButtonIsPressed = false;
    private UserInputActions userInputActions;
    private GameObject selectedMarker;
    private GameObject selectedObject;

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
        if (markerManager.selectedMarkerList != null && leftMouseButtonIsPressed)
        {
            for (int i = 0; i < markerManager.selectedMarkerList.Count; i++)
            {
                if (
                    markerManager
                        .selectedMarkerList[i]
                        .TryGetComponent(out MarkerController markerController)
                )
                {
                    markerController.MoveMarkerWithMouse(inputManager.GetWorldMouseLocation2D());
                }
            }
        }
    }

    private void AddMarker_performed(InputAction.CallbackContext context)
    {
        DeselectAllMarkers();

        Vector2 mouseWorldPosition2D = inputManager.GetWorldMouseLocation2D();
        GameObject newMarker = Instantiate(marker, mouseWorldPosition2D, Quaternion.identity);
        markerManager.markerList.Add(newMarker);
        if (newMarker.TryGetComponent(out MarkerEntity markerEntity))
        {
            //--------------------TODO - PAY ATTENTION TO THIS LATER--------------------
            //I'm sure I'll have to make this better, because when I started removing markers, or adding markers in between other markers in the curve, this will have to be better.
            markerEntity.frameNumber = markerManager.markerList.Count;
        }
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

                if (markerManager.selectedMarkerList.Count == 0) //If the list is empty, select this marker.
                {
                    SelectMarker(markerEntity, selectedMarker);
                }
                else
                {
                    if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) //If it isn't empty check if shift is clicked.
                    {
                        if (markerManager.selectedMarkerList.Contains(selectedMarker)) //If marker already in list, remove it.
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
                        if (markerManager.selectedMarkerList.Contains(selectedMarker)) //If this marker is selected.
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
        for (int i = markerManager.selectedMarkerList.Count - 1; i >= 0; i--)
        {
            markerManager.selectedMarkerList[i].GetComponent<MarkerEntity>().isSelected = false;
            markerManager.selectedMarkerList.RemoveAt(i);
        }
    }

    private void SelectMarker(MarkerEntity markerEntity, GameObject selectedMarker)
    {
        markerEntity.isSelected = true;
        markerManager.selectedMarkerList.Add(selectedMarker);
    }

    private void DeselectMarker(MarkerEntity markerEntity, GameObject selectedMarker)
    {
        markerEntity.isSelected = false;
        markerManager.selectedMarkerList.Remove(selectedMarker);
    }

    private void MoveMarker_canceled(InputAction.CallbackContext context)
    {
        leftMouseButtonIsPressed = false;

        selectedMarker = null;
        selectedObject = null;
    }

    private void DeleteMarker_performed(InputAction.CallbackContext context)
    {
        for (int i = 0; i < markerManager.selectedMarkerList.Count; i++)
        {
            markerManager.markerList.Remove(markerManager.selectedMarkerList[i]);
            Destroy(markerManager.selectedMarkerList[i]);
        }
        markerManager.selectedMarkerList.Clear();
        UpdateFrameNumber();
        //--------------------TODO - PAY ATTENTION TO THIS LATER--------------------
        //Right now I'm just deleting and updating the frame number. I'm not deleting and creating "ghost" markers.
        //I think for this to be nice to have the option to add ghost markers.
        //1- If you delete normally, it updates frame numbers and timeline.
        //2- If you delete with SHIFT, then it creates ghost markers. I think that's better.
    }

    private void UpdateFrameNumber()
    {
        //--------------------TODO - PAY ATTENTION TO THIS LATER--------------------
        //Right now I'm just deleting
        for (int i = 0; i < markerManager.markerList.Count; i++)
        {
            if (markerManager.markerList[i].TryGetComponent(out MarkerEntity iMarkerEntity))
            {
                iMarkerEntity.frameNumber = i + 1;
            }
        }
    }
}

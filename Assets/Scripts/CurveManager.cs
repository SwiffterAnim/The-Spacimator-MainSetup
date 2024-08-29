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

    private UserInputActions userInputActions;
    private bool leftMouseButtonPress = false;
    private GameObject selectedMarker;

    // private MarkerController markerController;

    private void OnEnable()
    {
        userInputActions = GameManager.Instance.GetInputAction();
        userInputActions.EditingCurve.AddMarker.performed += AddMarker_performed;
        userInputActions.EditingCurve.MoveMarker.performed += MoveMarker_performed;
        userInputActions.EditingCurve.MoveMarker.canceled += MoveMarker_canceled;
    }

    private void OnDisable()
    {
        userInputActions.EditingCurve.AddMarker.performed -= AddMarker_performed;
        userInputActions.EditingCurve.MoveMarker.performed -= MoveMarker_performed;
        userInputActions.EditingCurve.MoveMarker.canceled -= MoveMarker_canceled;
    }

    private void Update()
    {
        if (selectedMarker != null)
        {
            if (
                selectedMarker.transform.gameObject.TryGetComponent(
                    out MarkerController markerController
                )
            )
            {
                markerController.MoveMarkerWithMouse(inputManager.GetWorldMouseLocation2D());
            }
        }
    }

    private void AddMarker_performed(InputAction.CallbackContext context)
    {
        Vector2 mouseWorldPosition2D = inputManager.GetWorldMouseLocation2D();
        GameObject newMarker = Instantiate(marker, mouseWorldPosition2D, Quaternion.identity);
        markerManager.markerList.Add(newMarker);
        if (newMarker.TryGetComponent(out MarkerEntity markerEntity))
        {
            //I'm sure I'll have to make this better, because when I started removing markers, or adding markers in between other markers in the curve, this will have to be better.
            markerEntity.frameNumber = markerManager.markerList.Count;
        }
    }

    private void MoveMarker_performed(InputAction.CallbackContext context)
    {
        //Do I need this?
        leftMouseButtonPress = true;

        RaycastHit2D[] mouseRayCastHit = inputManager.DetectALL_MouseRayCastHit2D();
        if (mouseRayCastHit.Length > 0)
        {
            selectedMarker = inputManager.GetHoveredObject();
        }
    }

    private void MoveMarker_canceled(InputAction.CallbackContext context)
    {
        leftMouseButtonPress = false;
        selectedMarker = null;
    }
}

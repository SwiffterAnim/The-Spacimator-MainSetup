using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Timeline;

public class CurveManager : MonoBehaviour
{
    [SerializeField]
    GameObject marker;

    [SerializeField]
    InputManager inputManager;

    [SerializeField]
    MarkerEntity markerEntity;

    private List<GameObject> markerList = new List<GameObject>();

    private UserInputActions userInputActions;
    private bool leftMouseButtonPress = false;
    public GameObject selectedMarker;
    public GameObject hoveredMarker;
    private RaycastHit2D[] mouseRayCastHit;

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
        //Gets the array of all objects the mouse hits.
        mouseRayCastHit = inputManager.DetectALL_MouseRayCastHit2D();

        UpdateMarkerHovered(mouseRayCastHit);

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

    //Not sure this is the nicest way to do it. I wish I could just get a number and give that number for a function that updates the hovered marker.
    private void UpdateMarkerHovered(RaycastHit2D[] RayCastHit)
    {
        if (RayCastHit.Length > 0)
        {
            //Gets the top marker, important if there's more than one.
            hoveredMarker = RayCastHit[^1].transform.gameObject;

            if (hoveredMarker.TryGetComponent<MarkerEntity>(out markerEntity))
            {
                markerEntity.isHovered = true;
            }
            //This makes sure to turn off isHovered for all the ones that are NOT on top.
            if (RayCastHit.Length > 1)
            {
                for (int i = 0; i < RayCastHit.Length - 1; i++)
                {
                    if (RayCastHit[i].transform.gameObject.TryGetComponent(out MarkerEntity entity))
                    {
                        entity.isHovered = false;
                    }
                }
            }
        }
        else
        {
            if (markerEntity != null)
            {
                markerEntity.isHovered = false;
            }
        }
    }

    private void AddMarker_performed(InputAction.CallbackContext context)
    {
        Vector2 mouseWorldPosition2D = inputManager.GetWorldMouseLocation2D();
        GameObject newMarker = Instantiate(marker, mouseWorldPosition2D, Quaternion.identity);
        markerList.Add(newMarker);
        if (newMarker.TryGetComponent(out MarkerEntity markerEntity))
        {
            //I'm sure I'll have to make this better, because when I started removing markers, or adding markers in between other markers in the curve, this will have to be better.
            markerEntity.frameNumber = markerList.Count;
        }
    }

    private void MoveMarker_performed(InputAction.CallbackContext context)
    {
        //Do I need this?
        leftMouseButtonPress = true;
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

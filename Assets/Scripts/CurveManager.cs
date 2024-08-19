using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CurveManager : MonoBehaviour
{
    [SerializeField]
    GameObject marker;

    private List<GameObject> markerList = new List<GameObject>();

    private UserInputActions userInputActions;

    private void OnEnable()
    {
        userInputActions = GameManager.Instance.GetInputAction();
        userInputActions.EditingCurve.AddMarker.performed += AddMarker_performed;
    }

    private void OnDisable()
    {
        userInputActions.EditingCurve.AddMarker.performed -= AddMarker_performed;
    }

    private void AddMarker_performed(InputAction.CallbackContext context)
    {
        Vector3 mousePosition = Mouse.current.position.ReadValue();
        Vector3 mouseWorldPosition = Camera.main.ScreenToWorldPoint(mousePosition);
        mouseWorldPosition.z = 0;
        GameObject newMarker = Instantiate(marker, mouseWorldPosition, Quaternion.identity);
        markerList.Add(newMarker);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CurveManager : MonoBehaviour
{
    [SerializeField]
    GameObject marker;

    private UserInputActions userInputActions;
    private bool leftMouseButtonPress;

    private void Awake()
    {
        //I'm assuming the "creating curve" mode is on as default. If later we want to go to "record" mode, these might need to change.
        userInputActions = new UserInputActions();
        userInputActions.EditingCurve.Enable();

        userInputActions.EditingCurve.AddMarker.performed += AddMarker_performed;
        userInputActions.EditingCurve.MoveMarker.performed += MoveMarker_performed;
        userInputActions.EditingCurve.MoveMarker.canceled += MoveMarker_canceled;
    }

    private void Update()
    {
        if (leftMouseButtonPress)
        {
            //selected marker follows mouse
        }
    }

    private void AddMarker_performed(InputAction.CallbackContext context)
    {
        Vector3 mousePosition = Mouse.current.position.ReadValue();
        Vector3 mouseWorldPosition = Camera.main.ScreenToWorldPoint(mousePosition);
        mouseWorldPosition.z = 0;
        Instantiate(marker, mouseWorldPosition, Quaternion.identity);
    }

    private void MoveMarker_performed(InputAction.CallbackContext context)
    {
        leftMouseButtonPress = context.ReadValueAsButton();
        Debug.Log(context.ReadValueAsButton());
    }

    private void MoveMarker_canceled(InputAction.CallbackContext context)
    {
        leftMouseButtonPress = context.ReadValueAsButton();
        Debug.Log(context.ReadValueAsButton());
    }
}

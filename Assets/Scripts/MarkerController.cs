using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class MarkerController : MonoBehaviour
{
    private bool leftMouseButtonPress;

    private void Update()
    {
        if (leftMouseButtonPress)
        {
            Vector3 mouseWorldPosition = Camera.main.ScreenToWorldPoint(
                Mouse.current.position.ReadValue()
            );
            mouseWorldPosition.z = 0;
            transform.position = mouseWorldPosition;
        }
    }

    //So this works, on Mouse down I guess it's a unity way of controlling things with colliders.
    //but if I would like to not have this work at some point, like if I am with a menu on top of the window
    //this is going to still move the keys underneath.
    //The point of creating the input system is that I can change the Action Package thingy when I don't want the click of the mouse to do this.

    private void OnMouseDown()
    {
        leftMouseButtonPress = true;
    }

    private void OnMouseUp()
    {
        leftMouseButtonPress = false;
    }

    //I needed to make this work with this method below.
    //The problem with it is that it takes ALL of the markers in the scene, and not just the selected one.
    //TODO:
    /*
        private UserInputActions userInputActions;
        private void OnEnable()
        {
            userInputActions = GameManager.Instance.GetInputAction();
            userInputActions.EditingCurve.MoveMarker.performed += MoveMarker_performed;
            userInputActions.EditingCurve.MoveMarker.canceled += MoveMarker_canceled;
        }
        private void MoveMarker_performed(InputAction.CallbackContext context)
        {
            leftMouseButtonPress = true;
        }
        private void MoveMarker_canceled(InputAction.CallbackContext context)
        {
            leftMouseButtonPress = false;
        }
    */
}

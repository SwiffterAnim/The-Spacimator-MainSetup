using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    private Vector2 mousePosition;
    private RaycastHit2D[] mouseHitAllArray;
    private UserInputActions userInputActions;

    private bool firstOfTwoClicked = false;
    private bool secondOfTwoClicked = false;
    private float timer;
    private float timeLimitToActivateDoubleClick = 0.45f;

    [Header("Events")]
    public GameEvent onDoubleClickPerformed;
    public GameEvent onRightClickPerformed;
    public GameEvent onLeftClickPerformed;
    public GameEvent onLeftClickCanceled;
    public GameEvent onDeletePerformed;

    private void Start()
    {
        userInputActions = GameManager.Instance.GetInputAction();
        userInputActions.EditingCurve.MoveMarker.performed += MoveMarker_performed;
        userInputActions.EditingCurve.MoveMarker.canceled += MoveMarker_canceled;
        userInputActions.EditingCurve.AddMarker.performed += AddMarker_performed;
        userInputActions.EditingCurve.DeleteMarker.performed += DeleteMarker_performed;
    }

    private void OnDisable()
    {
        userInputActions.EditingCurve.MoveMarker.performed -= MoveMarker_performed;
        userInputActions.EditingCurve.MoveMarker.canceled -= MoveMarker_canceled;
        userInputActions.EditingCurve.AddMarker.performed -= AddMarker_performed;
        userInputActions.EditingCurve.DeleteMarker.performed -= DeleteMarker_performed;
    }

    private void Update()
    {
        //Checking for Double Click.
        if (firstOfTwoClicked)
        {
            timer += Time.unscaledDeltaTime;
            if (timer < timeLimitToActivateDoubleClick && secondOfTwoClicked)
            {
                onDoubleClickPerformed.Raise(this, secondOfTwoClicked);
                firstOfTwoClicked = false;
                secondOfTwoClicked = false;
                timer = 0f;
            }
            else if (timer >= timeLimitToActivateDoubleClick && !secondOfTwoClicked)
            {
                firstOfTwoClicked = false;
                timer = 0f;
            }
        }
    }

    private void MoveMarker_performed(InputAction.CallbackContext context)
    {
        onLeftClickPerformed.Raise(this, true);

        if (!firstOfTwoClicked)
        {
            firstOfTwoClicked = true;
        }
        else
        {
            secondOfTwoClicked = true;
        }
    }

    private void MoveMarker_canceled(InputAction.CallbackContext context)
    {
        onLeftClickCanceled.Raise(this, true);
    }

    private void AddMarker_performed(InputAction.CallbackContext context)
    {
        onRightClickPerformed.Raise(this, true);
    }

    private void DeleteMarker_performed(InputAction.CallbackContext context)
    {
        onDeletePerformed.Raise(this, true);
    }

    //Not sure if this isn't returning the same as Input.mousePosition lol
    public Vector2 GetWorldMouseLocation2D()
    {
        mousePosition = Mouse.current.position.ReadValue();
        Vector3 mouseWorldPosition = Camera.main.ScreenToWorldPoint(mousePosition);
        mousePosition = mouseWorldPosition;
        return mousePosition;
    }

    public RaycastHit2D[] DetectALL_MouseRayCastHit2D()
    {
        mouseHitAllArray = Physics2D.RaycastAll(GetWorldMouseLocation2D(), Vector3.forward);
        return mouseHitAllArray;
    }

    public RaycastHit[] DetectALL_MouseRaycastHit3D()
    {
        Vector3 mouseHit = GetWorldMouseLocation2D();

        RaycastHit[] mouseHitAllArray3D = Physics.RaycastAll(mouseHit, Vector3.forward);
        RaycastHit[] negativeMouseHitAllArray3D = Physics.RaycastAll(mouseHit, -Vector3.forward);
        if (mouseHitAllArray3D.Length != 0)
        {
            return mouseHitAllArray3D;
        }
        else if (negativeMouseHitAllArray3D.Length != 0)
        {
            return negativeMouseHitAllArray3D;
        }
        else
        {
            return null;
        }
    }

    public GameObject GetHoveredObject()
    {
        if (mouseHitAllArray.Length > 0)
        {
            return mouseHitAllArray[^1].transform.gameObject;
        }
        else
        {
            return null;
        }
    }
}
